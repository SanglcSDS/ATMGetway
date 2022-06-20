
namespace AppAgribankDigital
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bt_start_atm = new System.Windows.Forms.Button();
            this.bt_start_host = new System.Windows.Forms.Button();
            this.bt_close_atm = new System.Windows.Forms.Button();
            this.bt_close_host = new System.Windows.Forms.Button();
            this.bt_start_zf1 = new System.Windows.Forms.Button();
            this.lb_zf1 = new System.Windows.Forms.Label();
            this.lb_atm = new System.Windows.Forms.Label();
            this.lb_host = new System.Windows.Forms.Label();
            this.bt_Connec = new System.Windows.Forms.Button();
            this.lb_connec = new System.Windows.Forms.Label();
            this.btn_close_Thread = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bt_start_atm
            // 
            this.bt_start_atm.Location = new System.Drawing.Point(75, 42);
            this.bt_start_atm.Name = "bt_start_atm";
            this.bt_start_atm.Size = new System.Drawing.Size(98, 23);
            this.bt_start_atm.TabIndex = 0;
            this.bt_start_atm.Text = "Statrt ATM";
            this.bt_start_atm.UseVisualStyleBackColor = true;
            this.bt_start_atm.Click += new System.EventHandler(this.bt_start_atm_Click);
            // 
            // bt_start_host
            // 
            this.bt_start_host.Location = new System.Drawing.Point(75, 100);
            this.bt_start_host.Name = "bt_start_host";
            this.bt_start_host.Size = new System.Drawing.Size(115, 23);
            this.bt_start_host.TabIndex = 1;
            this.bt_start_host.Text = "Start Host";
            this.bt_start_host.UseVisualStyleBackColor = true;
            this.bt_start_host.Click += new System.EventHandler(this.bt_start_host_Click);
            // 
            // bt_close_atm
            // 
            this.bt_close_atm.Location = new System.Drawing.Point(243, 42);
            this.bt_close_atm.Name = "bt_close_atm";
            this.bt_close_atm.Size = new System.Drawing.Size(148, 23);
            this.bt_close_atm.TabIndex = 2;
            this.bt_close_atm.Text = "Close ATM";
            this.bt_close_atm.UseVisualStyleBackColor = true;
            this.bt_close_atm.Click += new System.EventHandler(this.bt_close_atm_Click);
            // 
            // bt_close_host
            // 
            this.bt_close_host.Location = new System.Drawing.Point(243, 100);
            this.bt_close_host.Name = "bt_close_host";
            this.bt_close_host.Size = new System.Drawing.Size(148, 23);
            this.bt_close_host.TabIndex = 3;
            this.bt_close_host.Text = "Close Host";
            this.bt_close_host.UseVisualStyleBackColor = true;
            this.bt_close_host.Click += new System.EventHandler(this.bt_close_host_Click);
            // 
            // bt_start_zf1
            // 
            this.bt_start_zf1.Location = new System.Drawing.Point(94, 190);
            this.bt_start_zf1.Name = "bt_start_zf1";
            this.bt_start_zf1.Size = new System.Drawing.Size(148, 23);
            this.bt_start_zf1.TabIndex = 4;
            this.bt_start_zf1.Text = "Start zf1";
            this.bt_start_zf1.UseVisualStyleBackColor = true;
            this.bt_start_zf1.Click += new System.EventHandler(this.bt_start_zf1_Click);
            // 
            // lb_zf1
            // 
            this.lb_zf1.AutoSize = true;
            this.lb_zf1.Location = new System.Drawing.Point(342, 195);
            this.lb_zf1.Name = "lb_zf1";
            this.lb_zf1.Size = new System.Drawing.Size(46, 17);
            this.lb_zf1.TabIndex = 5;
            this.lb_zf1.Text = "label1";
            // 
            // lb_atm
            // 
            this.lb_atm.AutoSize = true;
            this.lb_atm.Location = new System.Drawing.Point(444, 45);
            this.lb_atm.Name = "lb_atm";
            this.lb_atm.Size = new System.Drawing.Size(46, 17);
            this.lb_atm.TabIndex = 6;
            this.lb_atm.Text = "label1";
            // 
            // lb_host
            // 
            this.lb_host.AutoSize = true;
            this.lb_host.Location = new System.Drawing.Point(456, 106);
            this.lb_host.Name = "lb_host";
            this.lb_host.Size = new System.Drawing.Size(46, 17);
            this.lb_host.TabIndex = 7;
            this.lb_host.Text = "label1";
            // 
            // bt_Connec
            // 
            this.bt_Connec.Location = new System.Drawing.Point(75, 309);
            this.bt_Connec.Name = "bt_Connec";
            this.bt_Connec.Size = new System.Drawing.Size(365, 66);
            this.bt_Connec.TabIndex = 8;
            this.bt_Connec.Text = "Start Thread";
            this.bt_Connec.UseVisualStyleBackColor = true;
            this.bt_Connec.Click += new System.EventHandler(this.bt_Connec_Click);
            // 
            // lb_connec
            // 
            this.lb_connec.AutoSize = true;
            this.lb_connec.Location = new System.Drawing.Point(496, 334);
            this.lb_connec.Name = "lb_connec";
            this.lb_connec.Size = new System.Drawing.Size(46, 17);
            this.lb_connec.TabIndex = 9;
            this.lb_connec.Text = "label1";
            // 
            // btn_close_Thread
            // 
            this.btn_close_Thread.Location = new System.Drawing.Point(75, 381);
            this.btn_close_Thread.Name = "btn_close_Thread";
            this.btn_close_Thread.Size = new System.Drawing.Size(213, 66);
            this.btn_close_Thread.TabIndex = 10;
            this.btn_close_Thread.Text = "Close Thread";
            this.btn_close_Thread.UseVisualStyleBackColor = true;
            this.btn_close_Thread.Click += new System.EventHandler(this.btn_close_Thread_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_close_Thread);
            this.Controls.Add(this.lb_connec);
            this.Controls.Add(this.bt_Connec);
            this.Controls.Add(this.lb_host);
            this.Controls.Add(this.lb_atm);
            this.Controls.Add(this.lb_zf1);
            this.Controls.Add(this.bt_start_zf1);
            this.Controls.Add(this.bt_close_host);
            this.Controls.Add(this.bt_close_atm);
            this.Controls.Add(this.bt_start_host);
            this.Controls.Add(this.bt_start_atm);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_start_atm;
        private System.Windows.Forms.Button bt_start_host;
        private System.Windows.Forms.Button bt_close_atm;
        private System.Windows.Forms.Button bt_close_host;
        private System.Windows.Forms.Button bt_start_zf1;
        private System.Windows.Forms.Label lb_zf1;
        private System.Windows.Forms.Label lb_atm;
        private System.Windows.Forms.Label lb_host;
        private System.Windows.Forms.Button bt_Connec;
        private System.Windows.Forms.Label lb_connec;
        private System.Windows.Forms.Button btn_close_Thread;
    }
}

