namespace waxPrecipitationFinalFormat
{
    partial class FirstPage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstPage));
            this.ButtonBinary = new System.Windows.Forms.Button();
            this.ButtonMultiComponent = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonBinary
            // 
            this.ButtonBinary.Font = new System.Drawing.Font("Times New Roman", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.ButtonBinary.Location = new System.Drawing.Point(406, 408);
            this.ButtonBinary.Name = "ButtonBinary";
            this.ButtonBinary.Size = new System.Drawing.Size(259, 55);
            this.ButtonBinary.TabIndex = 2;
            this.ButtonBinary.Text = "Binary System";
            this.toolTip1.SetToolTip(this.ButtonBinary, "Solubility of binary systems can also be calculated.");
            this.ButtonBinary.UseVisualStyleBackColor = true;
            this.ButtonBinary.Click += new System.EventHandler(this.ButtonBinary_Click);
            // 
            // ButtonMultiComponent
            // 
            this.ButtonMultiComponent.Font = new System.Drawing.Font("Times New Roman", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.ButtonMultiComponent.Location = new System.Drawing.Point(24, 408);
            this.ButtonMultiComponent.Name = "ButtonMultiComponent";
            this.ButtonMultiComponent.Size = new System.Drawing.Size(259, 55);
            this.ButtonMultiComponent.TabIndex = 3;
            this.ButtonMultiComponent.Text = "Multi-Component System";
            this.toolTip1.SetToolTip(this.ButtonMultiComponent, "Paraffin characteristics of multi-component systems \r\ncan also be calculated. ");
            this.ButtonMultiComponent.UseVisualStyleBackColor = true;
            this.ButtonMultiComponent.Click += new System.EventHandler(this.ButtonMultiComponent_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(248, 354);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Program Options";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.InitialImage")));
            this.pictureBox2.Location = new System.Drawing.Point(3, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(859, 502);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox2, "Paraffin deposition in pipe");
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // FirstPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 505);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonMultiComponent);
            this.Controls.Add(this.ButtonBinary);
            this.Controls.Add(this.pictureBox2);
            this.Name = "FirstPage";
            this.ShowIcon = false;
            this.Text = "SP-Wax";
            this.Load += new System.EventHandler(this.FirstPage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ButtonBinary;
        private System.Windows.Forms.Button ButtonMultiComponent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}