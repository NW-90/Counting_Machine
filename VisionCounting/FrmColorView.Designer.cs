namespace VisionCounting
{
    partial class FrmColorView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmColorView));
            this.ckROI = new MetroFramework.Controls.MetroCheckBox();
            this.btRecheckColor = new System.Windows.Forms.Button();
            this.lbMaxRColor = new System.Windows.Forms.Label();
            this.lbMaxGColor = new System.Windows.Forms.Label();
            this.lbMaxBColor = new System.Windows.Forms.Label();
            this.lbMinRColor = new System.Windows.Forms.Label();
            this.lbMinGColor = new System.Windows.Forms.Label();
            this.lbMinBColor = new System.Windows.Forms.Label();
            this.lbMaxVColor = new System.Windows.Forms.Label();
            this.lbMaxSColor = new System.Windows.Forms.Label();
            this.lbMaxHColor = new System.Windows.Forms.Label();
            this.lbMinVColor = new System.Windows.Forms.Label();
            this.lbMinSColor = new System.Windows.Forms.Label();
            this.lbMinHColor = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.lbimageROIHSV = new System.Windows.Forms.Label();
            this.imageROIBGR = new Emgu.CV.UI.ImageBox();
            this.lbimageROIRGB = new System.Windows.Forms.Label();
            this.imageROIHSV = new Emgu.CV.UI.ImageBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btDoneBlob = new Bunifu.Framework.UI.BunifuImageButton();
            this.ckTopMost = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageROIBGR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageROIHSV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btDoneBlob)).BeginInit();
            this.SuspendLayout();
            // 
            // ckROI
            // 
            this.ckROI.AutoSize = true;
            this.ckROI.FontSize = MetroFramework.MetroCheckBoxSize.Medium;
            this.ckROI.Location = new System.Drawing.Point(26, 433);
            this.ckROI.Name = "ckROI";
            this.ckROI.Size = new System.Drawing.Size(87, 19);
            this.ckROI.TabIndex = 558;
            this.ckROI.Text = "Select ROI";
            this.ckROI.UseSelectable = true;
            this.ckROI.CheckedChanged += new System.EventHandler(this.CkROI_CheckedChanged);
            // 
            // btRecheckColor
            // 
            this.btRecheckColor.Image = ((System.Drawing.Image)(resources.GetObject("btRecheckColor.Image")));
            this.btRecheckColor.Location = new System.Drawing.Point(26, 470);
            this.btRecheckColor.Name = "btRecheckColor";
            this.btRecheckColor.Size = new System.Drawing.Size(70, 60);
            this.btRecheckColor.TabIndex = 636;
            this.btRecheckColor.UseVisualStyleBackColor = true;
            this.btRecheckColor.Click += new System.EventHandler(this.BtRecheckColor_Click);
            // 
            // lbMaxRColor
            // 
            this.lbMaxRColor.BackColor = System.Drawing.Color.White;
            this.lbMaxRColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMaxRColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxRColor.Location = new System.Drawing.Point(362, 382);
            this.lbMaxRColor.Name = "lbMaxRColor";
            this.lbMaxRColor.Size = new System.Drawing.Size(61, 20);
            this.lbMaxRColor.TabIndex = 635;
            this.lbMaxRColor.Text = "0";
            this.lbMaxRColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMaxGColor
            // 
            this.lbMaxGColor.BackColor = System.Drawing.Color.White;
            this.lbMaxGColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMaxGColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxGColor.Location = new System.Drawing.Point(362, 356);
            this.lbMaxGColor.Name = "lbMaxGColor";
            this.lbMaxGColor.Size = new System.Drawing.Size(61, 20);
            this.lbMaxGColor.TabIndex = 634;
            this.lbMaxGColor.Text = "0";
            this.lbMaxGColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMaxBColor
            // 
            this.lbMaxBColor.BackColor = System.Drawing.Color.White;
            this.lbMaxBColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMaxBColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxBColor.Location = new System.Drawing.Point(362, 330);
            this.lbMaxBColor.Name = "lbMaxBColor";
            this.lbMaxBColor.Size = new System.Drawing.Size(61, 20);
            this.lbMaxBColor.TabIndex = 633;
            this.lbMaxBColor.Text = "0";
            this.lbMaxBColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinRColor
            // 
            this.lbMinRColor.BackColor = System.Drawing.Color.White;
            this.lbMinRColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinRColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinRColor.Location = new System.Drawing.Point(280, 382);
            this.lbMinRColor.Name = "lbMinRColor";
            this.lbMinRColor.Size = new System.Drawing.Size(61, 20);
            this.lbMinRColor.TabIndex = 632;
            this.lbMinRColor.Text = "0";
            this.lbMinRColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinGColor
            // 
            this.lbMinGColor.BackColor = System.Drawing.Color.White;
            this.lbMinGColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinGColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinGColor.Location = new System.Drawing.Point(280, 356);
            this.lbMinGColor.Name = "lbMinGColor";
            this.lbMinGColor.Size = new System.Drawing.Size(61, 20);
            this.lbMinGColor.TabIndex = 631;
            this.lbMinGColor.Text = "0";
            this.lbMinGColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinBColor
            // 
            this.lbMinBColor.BackColor = System.Drawing.Color.White;
            this.lbMinBColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinBColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinBColor.Location = new System.Drawing.Point(280, 330);
            this.lbMinBColor.Name = "lbMinBColor";
            this.lbMinBColor.Size = new System.Drawing.Size(61, 20);
            this.lbMinBColor.TabIndex = 630;
            this.lbMinBColor.Text = "0";
            this.lbMinBColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMaxVColor
            // 
            this.lbMaxVColor.BackColor = System.Drawing.Color.White;
            this.lbMaxVColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMaxVColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxVColor.Location = new System.Drawing.Point(128, 382);
            this.lbMaxVColor.Name = "lbMaxVColor";
            this.lbMaxVColor.Size = new System.Drawing.Size(61, 20);
            this.lbMaxVColor.TabIndex = 629;
            this.lbMaxVColor.Text = "0";
            this.lbMaxVColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMaxSColor
            // 
            this.lbMaxSColor.BackColor = System.Drawing.Color.White;
            this.lbMaxSColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMaxSColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxSColor.Location = new System.Drawing.Point(128, 356);
            this.lbMaxSColor.Name = "lbMaxSColor";
            this.lbMaxSColor.Size = new System.Drawing.Size(61, 20);
            this.lbMaxSColor.TabIndex = 628;
            this.lbMaxSColor.Text = "0";
            this.lbMaxSColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMaxHColor
            // 
            this.lbMaxHColor.BackColor = System.Drawing.Color.White;
            this.lbMaxHColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMaxHColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxHColor.Location = new System.Drawing.Point(128, 330);
            this.lbMaxHColor.Name = "lbMaxHColor";
            this.lbMaxHColor.Size = new System.Drawing.Size(61, 20);
            this.lbMaxHColor.TabIndex = 627;
            this.lbMaxHColor.Text = "0";
            this.lbMaxHColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinVColor
            // 
            this.lbMinVColor.BackColor = System.Drawing.Color.White;
            this.lbMinVColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinVColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinVColor.Location = new System.Drawing.Point(49, 382);
            this.lbMinVColor.Name = "lbMinVColor";
            this.lbMinVColor.Size = new System.Drawing.Size(61, 20);
            this.lbMinVColor.TabIndex = 626;
            this.lbMinVColor.Text = "0";
            this.lbMinVColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinSColor
            // 
            this.lbMinSColor.BackColor = System.Drawing.Color.White;
            this.lbMinSColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinSColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinSColor.Location = new System.Drawing.Point(49, 356);
            this.lbMinSColor.Name = "lbMinSColor";
            this.lbMinSColor.Size = new System.Drawing.Size(61, 20);
            this.lbMinSColor.TabIndex = 625;
            this.lbMinSColor.Text = "0";
            this.lbMinSColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMinHColor
            // 
            this.lbMinHColor.BackColor = System.Drawing.Color.White;
            this.lbMinHColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMinHColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinHColor.Location = new System.Drawing.Point(49, 330);
            this.lbMinHColor.Name = "lbMinHColor";
            this.lbMinHColor.Size = new System.Drawing.Size(61, 20);
            this.lbMinHColor.TabIndex = 624;
            this.lbMinHColor.Text = "0";
            this.lbMinHColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.Location = new System.Drawing.Point(255, 385);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(24, 16);
            this.label25.TabIndex = 623;
            this.label25.Text = "R :";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(255, 358);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(24, 16);
            this.label26.TabIndex = 622;
            this.label26.Text = "G :";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(255, 333);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(23, 16);
            this.label27.TabIndex = 621;
            this.label27.Text = "B :";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(61, 309);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(29, 16);
            this.label28.TabIndex = 617;
            this.label28.Text = "Min";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(298, 309);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(29, 16);
            this.label30.TabIndex = 620;
            this.label30.Text = "Min";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(23, 333);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(24, 16);
            this.label31.TabIndex = 615;
            this.label31.Text = "H :";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.Location = new System.Drawing.Point(23, 358);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(26, 16);
            this.label32.TabIndex = 616;
            this.label32.Text = "S  :";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(23, 385);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(26, 16);
            this.label33.TabIndex = 618;
            this.label33.Text = "V  :";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(141, 309);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(33, 16);
            this.label34.TabIndex = 619;
            this.label34.Text = "Max";
            // 
            // lbimageROIHSV
            // 
            this.lbimageROIHSV.BackColor = System.Drawing.Color.White;
            this.lbimageROIHSV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbimageROIHSV.Location = new System.Drawing.Point(14, 73);
            this.lbimageROIHSV.Name = "lbimageROIHSV";
            this.lbimageROIHSV.Size = new System.Drawing.Size(225, 28);
            this.lbimageROIHSV.TabIndex = 614;
            this.lbimageROIHSV.Text = "ROI HSV";
            this.lbimageROIHSV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imageROIBGR
            // 
            this.imageROIBGR.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.imageROIBGR.Location = new System.Drawing.Point(245, 105);
            this.imageROIBGR.Name = "imageROIBGR";
            this.imageROIBGR.Size = new System.Drawing.Size(225, 195);
            this.imageROIBGR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageROIBGR.TabIndex = 613;
            this.imageROIBGR.TabStop = false;
            // 
            // lbimageROIRGB
            // 
            this.lbimageROIRGB.BackColor = System.Drawing.Color.White;
            this.lbimageROIRGB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbimageROIRGB.Location = new System.Drawing.Point(245, 73);
            this.lbimageROIRGB.Name = "lbimageROIRGB";
            this.lbimageROIRGB.Size = new System.Drawing.Size(225, 28);
            this.lbimageROIRGB.TabIndex = 612;
            this.lbimageROIRGB.Text = "ROI RGB";
            this.lbimageROIRGB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imageROIHSV
            // 
            this.imageROIHSV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.imageROIHSV.Location = new System.Drawing.Point(14, 105);
            this.imageROIHSV.Name = "imageROIHSV";
            this.imageROIHSV.Size = new System.Drawing.Size(225, 195);
            this.imageROIHSV.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageROIHSV.TabIndex = 611;
            this.imageROIHSV.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(377, 309);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 16);
            this.label1.TabIndex = 638;
            this.label1.Text = "Max";
            // 
            // btDoneBlob
            // 
            this.btDoneBlob.BackColor = System.Drawing.Color.Transparent;
            this.btDoneBlob.Image = ((System.Drawing.Image)(resources.GetObject("btDoneBlob.Image")));
            this.btDoneBlob.ImageActive = null;
            this.btDoneBlob.Location = new System.Drawing.Point(447, 9);
            this.btDoneBlob.Name = "btDoneBlob";
            this.btDoneBlob.Size = new System.Drawing.Size(36, 31);
            this.btDoneBlob.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btDoneBlob.TabIndex = 643;
            this.btDoneBlob.TabStop = false;
            this.btDoneBlob.Zoom = 20;
            this.btDoneBlob.Click += new System.EventHandler(this.BtDoneBlob_Click);
            // 
            // ckTopMost
            // 
            this.ckTopMost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ckTopMost.AutoSize = true;
            this.ckTopMost.Location = new System.Drawing.Point(370, 9);
            this.ckTopMost.Name = "ckTopMost";
            this.ckTopMost.Size = new System.Drawing.Size(71, 17);
            this.ckTopMost.TabIndex = 1059;
            this.ckTopMost.Text = "Top Most";
            this.ckTopMost.UseVisualStyleBackColor = true;
            this.ckTopMost.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // FrmColorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 543);
            this.ControlBox = false;
            this.Controls.Add(this.ckTopMost);
            this.Controls.Add(this.btDoneBlob);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btRecheckColor);
            this.Controls.Add(this.lbMaxRColor);
            this.Controls.Add(this.lbMaxGColor);
            this.Controls.Add(this.lbMaxBColor);
            this.Controls.Add(this.lbMinRColor);
            this.Controls.Add(this.lbMinGColor);
            this.Controls.Add(this.lbMinBColor);
            this.Controls.Add(this.lbMaxVColor);
            this.Controls.Add(this.lbMaxSColor);
            this.Controls.Add(this.lbMaxHColor);
            this.Controls.Add(this.lbMinVColor);
            this.Controls.Add(this.lbMinSColor);
            this.Controls.Add(this.lbMinHColor);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.label31);
            this.Controls.Add(this.label32);
            this.Controls.Add(this.label33);
            this.Controls.Add(this.label34);
            this.Controls.Add(this.lbimageROIHSV);
            this.Controls.Add(this.imageROIBGR);
            this.Controls.Add(this.lbimageROIRGB);
            this.Controls.Add(this.imageROIHSV);
            this.Controls.Add(this.ckROI);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmColorView";
            this.Text = "Color ROI View";
            this.Load += new System.EventHandler(this.FrmColorView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imageROIBGR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageROIHSV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btDoneBlob)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btRecheckColor;
        public System.Windows.Forms.Label lbMaxRColor;
        public System.Windows.Forms.Label lbMaxGColor;
        public System.Windows.Forms.Label lbMaxBColor;
        public System.Windows.Forms.Label lbMinRColor;
        public System.Windows.Forms.Label lbMinGColor;
        public System.Windows.Forms.Label lbMinBColor;
        public System.Windows.Forms.Label lbMaxVColor;
        public System.Windows.Forms.Label lbMaxSColor;
        public System.Windows.Forms.Label lbMaxHColor;
        public System.Windows.Forms.Label lbMinVColor;
        public System.Windows.Forms.Label lbMinSColor;
        public System.Windows.Forms.Label lbMinHColor;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label lbimageROIHSV;
        public Emgu.CV.UI.ImageBox imageROIBGR;
        private System.Windows.Forms.Label lbimageROIRGB;
        public Emgu.CV.UI.ImageBox imageROIHSV;
        private System.Windows.Forms.Label label1;
        private MetroFramework.Controls.MetroCheckBox ckROI;
        private Bunifu.Framework.UI.BunifuImageButton btDoneBlob;
        private System.Windows.Forms.CheckBox ckTopMost;
    }
}