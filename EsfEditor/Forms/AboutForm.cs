namespace EsfEditor.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    public class AboutForm : Form
    {
        private Label label1;
        private Label label2;
        private Label label3;
        private Label lblVersion;

        public AboutForm()
        {
            this.InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            this.lblVersion.Text = this.lblVersion.Text + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblVersion = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.label1 = new Label();
            base.SuspendLayout();
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new Point(0x7d, 0x2a);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new Size(0x37, 13);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "Esf Editor ";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0x1b, 0x11);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x11c, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Viewer and Editor for Empire: Total War ESF container files";
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0x89, 0x43);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x41, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "1.4.4 by erasmus777";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x67, 0x5c);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x84, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "previous permutations by just and koras321";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x153, 0x7a);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.label3);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.lblVersion);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "AboutForm";
            base.ShowIcon = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About";
            base.Load += new EventHandler(this.AboutForm_Load);
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

