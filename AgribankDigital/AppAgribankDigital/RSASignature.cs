using System;
using System.Security.Cryptography;
using System.IO;

namespace AppAgribankDigital
{
    public class RSASignature
    {


        public static string signature(string partKey, string messege)
        {

            byte[] SignedHash = null;
            //   string sPrivateKeyPEM = File.ReadAllText(partKey);
            byte[] oraiginalDate = Convert.FromBase64String(messege);
            try
            {
                Logger.LogFingrprint(">1 ");
                using (RSACryptoServiceProvider rsa = GetRSA(partKey))
                {
                    Logger.LogFingrprint("rsa "+ rsa);
                    if (rsa != null)
                    {
                        Logger.LogFingrprint("rsa1 " + rsa);
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            Logger.LogFingrprint("sha256 " + sha256);
                            byte[] hash;
                            hash = sha256.ComputeHash(oraiginalDate);

                            RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);
                            Logger.LogFingrprint("RSAFormatter " + RSAFormatter);
                            RSAFormatter.SetHashAlgorithm("SHA256");
                            SignedHash = RSAFormatter.CreateSignature(hash);
                            Logger.LogFingrprint("SignedHash " + SignedHash);
                        }
                    }
                    else
                    {
                        return null;
                    }

                }
            }
            catch (CryptographicException e)
            {
                Logger.LogFingrprint(">loi");
                Logger.Log(e.Message);
            }

            Logger.LogFingrprint("get signature base64 ");
            return Convert.ToBase64String(SignedHash);
        }

        const string KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----";
        const string KEY_FOOTER = "-----END RSA PRIVATE KEY-----";

        public static RSACryptoServiceProvider GetRSA(string pem)
        {
            RSACryptoServiceProvider rsa = null;

            if (IsPrivateKeyAvailable(pem))
            {

                string keyFormatted = pem;

                int cutIndex = keyFormatted.IndexOf(KEY_HEADER);
                keyFormatted = keyFormatted.Substring(cutIndex, keyFormatted.Length - cutIndex);
                cutIndex = keyFormatted.IndexOf(KEY_FOOTER);
                keyFormatted = keyFormatted.Substring(0, cutIndex + KEY_FOOTER.Length);
                keyFormatted = keyFormatted.Replace(KEY_HEADER, "");
                keyFormatted = keyFormatted.Replace(KEY_FOOTER, "");
                keyFormatted = keyFormatted.Replace("\r", "");
                keyFormatted = keyFormatted.Replace("\n", "");
                keyFormatted = keyFormatted.Trim();

                byte[] privateKeyInDER = System.Convert.FromBase64String(keyFormatted);

                rsa = DecodeRSAPrivateKey(privateKeyInDER);
                Logger.LogFingrprint("Key " + rsa);
            }

            return rsa;
        }

        private static bool IsPrivateKeyAvailable(string privateKeyInPEM)
        {
            return (privateKeyInPEM != null && privateKeyInPEM.Contains(KEY_HEADER)
                && privateKeyInPEM.Contains(KEY_FOOTER));
        }
        public static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;


            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;



                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);


                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                binr.Close();
            }
        }

        private static int GetIntegerSize(BinaryReader binary)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;

            bt = binary.ReadByte();

            if (bt != 0x02)
                return 0;

            bt = binary.ReadByte();

            if (bt == 0x81)
                count = binary.ReadByte();
            else if (bt == 0x82)
            {
                highbyte = binary.ReadByte();
                lowbyte = binary.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
                count = bt;

            while (binary.ReadByte() == 0x00)
                count -= 1;

            binary.BaseStream.Seek(-1, SeekOrigin.Current);

            return count;
        }
    }

}

