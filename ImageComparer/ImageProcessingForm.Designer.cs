namespace ImageComparer
{
    partial class ImageProcessingForm
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
            btnSelectImage = new Button();
            lblImage1 = new Label();
            openFileDialog1 = new OpenFileDialog();
            label1 = new Label();
            btnSelectBaseline = new Button();
            pictureBox1 = new PictureBox();
            btnShowAsGreyScale = new Button();
            pictureBox2 = new PictureBox();
            btnTestTransparency = new Button();
            btnViewSubtraction = new Button();
            btnDoCompare = new Button();
            label2 = new Label();
            txtOffset = new TextBox();
            lblDuration = new Label();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new Point(12, 12);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new Size(101, 23);
            btnSelectImage.TabIndex = 0;
            btnSelectImage.Text = "Select Image";
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += btnSelectImage_Click;
            // 
            // lblImage1
            // 
            lblImage1.AutoSize = true;
            lblImage1.Location = new Point(119, 16);
            lblImage1.Name = "lblImage1";
            lblImage1.Size = new Size(43, 15);
            lblImage1.TabIndex = 1;
            lblImage1.Text = "Image:";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(119, 45);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 3;
            label1.Text = "Image:";
            // 
            // btnSelectBaseline
            // 
            btnSelectBaseline.Location = new Point(12, 41);
            btnSelectBaseline.Name = "btnSelectBaseline";
            btnSelectBaseline.Size = new Size(101, 23);
            btnSelectBaseline.TabIndex = 2;
            btnSelectBaseline.Text = "Select Baseline";
            btnSelectBaseline.UseVisualStyleBackColor = true;
            btnSelectBaseline.Click += btnSelectBaseline_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(11, 16);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(750, 1500);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // btnShowAsGreyScale
            // 
            btnShowAsGreyScale.Location = new Point(209, 12);
            btnShowAsGreyScale.Name = "btnShowAsGreyScale";
            btnShowAsGreyScale.Size = new Size(75, 23);
            btnShowAsGreyScale.TabIndex = 5;
            btnShowAsGreyScale.Text = "ShowGrey";
            btnShowAsGreyScale.UseVisualStyleBackColor = true;
            btnShowAsGreyScale.Click += btnShowAsGreyScale_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Location = new Point(806, 16);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(750, 1500);
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            // 
            // btnTestTransparency
            // 
            btnTestTransparency.Location = new Point(290, 12);
            btnTestTransparency.Name = "btnTestTransparency";
            btnTestTransparency.Size = new Size(135, 23);
            btnTestTransparency.TabIndex = 7;
            btnTestTransparency.Text = "Test Transparency";
            btnTestTransparency.UseVisualStyleBackColor = true;
            btnTestTransparency.Click += btnTestTransparency_Click;
            // 
            // btnViewSubtraction
            // 
            btnViewSubtraction.Location = new Point(431, 12);
            btnViewSubtraction.Name = "btnViewSubtraction";
            btnViewSubtraction.Size = new Size(107, 23);
            btnViewSubtraction.TabIndex = 8;
            btnViewSubtraction.Text = "View Subtraction";
            btnViewSubtraction.UseVisualStyleBackColor = true;
            btnViewSubtraction.Click += btnViewSubtraction_Click;
            // 
            // btnDoCompare
            // 
            btnDoCompare.Location = new Point(544, 12);
            btnDoCompare.Name = "btnDoCompare";
            btnDoCompare.Size = new Size(87, 23);
            btnDoCompare.TabIndex = 9;
            btnDoCompare.Text = "Do Compare";
            btnDoCompare.UseVisualStyleBackColor = true;
            btnDoCompare.Click += btnDoCompare_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(431, 38);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 10;
            label2.Text = "Offset";
            // 
            // txtOffset
            // 
            txtOffset.Location = new Point(475, 35);
            txtOffset.Name = "txtOffset";
            txtOffset.Size = new Size(50, 23);
            txtOffset.TabIndex = 11;
            txtOffset.Text = "0,0";
            // 
            // lblDuration
            // 
            lblDuration.AutoSize = true;
            lblDuration.Location = new Point(12, 69);
            lblDuration.Name = "lblDuration";
            lblDuration.Size = new Size(0, 15);
            lblDuration.TabIndex = 12;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(pictureBox2);
            panel1.Location = new Point(12, 87);
            panel1.Name = "panel1";
            panel1.Size = new Size(1760, 1700);
            panel1.TabIndex = 13;
            // 
            // ImageProcessingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1801, 1061);
            Controls.Add(panel1);
            Controls.Add(lblDuration);
            Controls.Add(txtOffset);
            Controls.Add(label2);
            Controls.Add(btnDoCompare);
            Controls.Add(btnViewSubtraction);
            Controls.Add(btnTestTransparency);
            Controls.Add(btnShowAsGreyScale);
            Controls.Add(label1);
            Controls.Add(btnSelectBaseline);
            Controls.Add(lblImage1);
            Controls.Add(btnSelectImage);
            Name = "ImageProcessingForm";
            Text = "ImageProcessingForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectImage;
        private Label lblImage1;
        private OpenFileDialog openFileDialog1;
        private Label label1;
        private Button btnSelectBaseline;
        private PictureBox pictureBox1;
        private Button btnShowAsGreyScale;
        private PictureBox pictureBox2;
        private Button btnTestTransparency;
        private Button btnViewSubtraction;
        private Button btnDoCompare;
        private Label label2;
        private TextBox txtOffset;
        private Label lblDuration;
        private Panel panel1;
    }
}