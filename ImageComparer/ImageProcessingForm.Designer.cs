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
            btnEdgeDetection = new Button();
            pbNewImagePreview = new PictureBox();
            pbBaseleinePreview = new PictureBox();
            lblBaseLineR = new Label();
            lblBaseLineG = new Label();
            lblBaseLineB = new Label();
            lblNewImageB = new Label();
            lblNewImageG = new Label();
            lblNewImageR = new Label();
            txtNewImageCoordinate = new TextBox();
            txtBaselineImageCoordinate = new TextBox();
            button1 = new Button();
            btnCompareWithComparableImage = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbNewImagePreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbBaseleinePreview).BeginInit();
            SuspendLayout();
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new Point(14, 16);
            btnSelectImage.Margin = new Padding(3, 4, 3, 4);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new Size(115, 31);
            btnSelectImage.TabIndex = 0;
            btnSelectImage.Text = "Select Image";
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += btnSelectImage_Click;
            // 
            // lblImage1
            // 
            lblImage1.AutoSize = true;
            lblImage1.Location = new Point(136, 21);
            lblImage1.Name = "lblImage1";
            lblImage1.Size = new Size(54, 20);
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
            label1.Location = new Point(136, 60);
            label1.Name = "label1";
            label1.Size = new Size(54, 20);
            label1.TabIndex = 3;
            label1.Text = "Image:";
            // 
            // btnSelectBaseline
            // 
            btnSelectBaseline.Location = new Point(14, 55);
            btnSelectBaseline.Margin = new Padding(3, 4, 3, 4);
            btnSelectBaseline.Name = "btnSelectBaseline";
            btnSelectBaseline.Size = new Size(115, 31);
            btnSelectBaseline.TabIndex = 2;
            btnSelectBaseline.Text = "Select Baseline";
            btnSelectBaseline.UseVisualStyleBackColor = true;
            btnSelectBaseline.Click += btnSelectBaseline_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(3, 4);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1714, 2000);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // btnShowAsGreyScale
            // 
            btnShowAsGreyScale.Location = new Point(239, 16);
            btnShowAsGreyScale.Margin = new Padding(3, 4, 3, 4);
            btnShowAsGreyScale.Name = "btnShowAsGreyScale";
            btnShowAsGreyScale.Size = new Size(86, 31);
            btnShowAsGreyScale.TabIndex = 5;
            btnShowAsGreyScale.Text = "ShowGrey";
            btnShowAsGreyScale.UseVisualStyleBackColor = true;
            btnShowAsGreyScale.Click += btnShowAsGreyScale_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Location = new Point(1817, 4);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(1714, 2000);
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // btnTestTransparency
            // 
            btnTestTransparency.Location = new Point(331, 16);
            btnTestTransparency.Margin = new Padding(3, 4, 3, 4);
            btnTestTransparency.Name = "btnTestTransparency";
            btnTestTransparency.Size = new Size(154, 31);
            btnTestTransparency.TabIndex = 7;
            btnTestTransparency.Text = "Test Transparency";
            btnTestTransparency.UseVisualStyleBackColor = true;
            btnTestTransparency.Click += btnTestTransparency_Click;
            // 
            // btnViewSubtraction
            // 
            btnViewSubtraction.Location = new Point(493, 16);
            btnViewSubtraction.Margin = new Padding(3, 4, 3, 4);
            btnViewSubtraction.Name = "btnViewSubtraction";
            btnViewSubtraction.Size = new Size(122, 31);
            btnViewSubtraction.TabIndex = 8;
            btnViewSubtraction.Text = "View Subtraction";
            btnViewSubtraction.UseVisualStyleBackColor = true;
            btnViewSubtraction.Click += btnViewSubtraction_Click;
            // 
            // btnDoCompare
            // 
            btnDoCompare.Location = new Point(622, 16);
            btnDoCompare.Margin = new Padding(3, 4, 3, 4);
            btnDoCompare.Name = "btnDoCompare";
            btnDoCompare.Size = new Size(149, 31);
            btnDoCompare.TabIndex = 9;
            btnDoCompare.Text = "Do Compare";
            btnDoCompare.UseVisualStyleBackColor = true;
            btnDoCompare.Click += btnDoCompare_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(493, 51);
            label2.Name = "label2";
            label2.Size = new Size(49, 20);
            label2.TabIndex = 10;
            label2.Text = "Offset";
            // 
            // txtOffset
            // 
            txtOffset.Location = new Point(543, 47);
            txtOffset.Margin = new Padding(3, 4, 3, 4);
            txtOffset.Name = "txtOffset";
            txtOffset.Size = new Size(57, 27);
            txtOffset.TabIndex = 11;
            txtOffset.Text = "0,0";
            // 
            // lblDuration
            // 
            lblDuration.AutoSize = true;
            lblDuration.Location = new Point(14, 92);
            lblDuration.Name = "lblDuration";
            lblDuration.Size = new Size(0, 20);
            lblDuration.TabIndex = 12;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(pictureBox2);
            panel1.Location = new Point(14, 116);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(3613, 4667);
            panel1.TabIndex = 13;
            // 
            // btnEdgeDetection
            // 
            btnEdgeDetection.Location = new Point(777, 17);
            btnEdgeDetection.Name = "btnEdgeDetection";
            btnEdgeDetection.Size = new Size(139, 29);
            btnEdgeDetection.TabIndex = 14;
            btnEdgeDetection.Text = "EdgeDeteection";
            btnEdgeDetection.UseVisualStyleBackColor = true;
            btnEdgeDetection.Click += btnEdgeDetection_Click;
            // 
            // pbNewImagePreview
            // 
            pbNewImagePreview.Location = new Point(1674, 45);
            pbNewImagePreview.Margin = new Padding(3, 4, 3, 4);
            pbNewImagePreview.Name = "pbNewImagePreview";
            pbNewImagePreview.Size = new Size(57, 67);
            pbNewImagePreview.TabIndex = 15;
            pbNewImagePreview.TabStop = false;
            // 
            // pbBaseleinePreview
            // 
            pbBaseleinePreview.Location = new Point(1831, 47);
            pbBaseleinePreview.Margin = new Padding(3, 4, 3, 4);
            pbBaseleinePreview.Name = "pbBaseleinePreview";
            pbBaseleinePreview.Size = new Size(57, 67);
            pbBaseleinePreview.TabIndex = 16;
            pbBaseleinePreview.TabStop = false;
            // 
            // lblBaseLineR
            // 
            lblBaseLineR.AutoSize = true;
            lblBaseLineR.Location = new Point(1895, 48);
            lblBaseLineR.Name = "lblBaseLineR";
            lblBaseLineR.Size = new Size(50, 20);
            lblBaseLineR.TabIndex = 17;
            lblBaseLineR.Text = "label3";
            // 
            // lblBaseLineG
            // 
            lblBaseLineG.AutoSize = true;
            lblBaseLineG.Location = new Point(1895, 68);
            lblBaseLineG.Name = "lblBaseLineG";
            lblBaseLineG.Size = new Size(50, 20);
            lblBaseLineG.TabIndex = 18;
            lblBaseLineG.Text = "label4";
            // 
            // lblBaseLineB
            // 
            lblBaseLineB.AutoSize = true;
            lblBaseLineB.Location = new Point(1895, 88);
            lblBaseLineB.Name = "lblBaseLineB";
            lblBaseLineB.Size = new Size(50, 20);
            lblBaseLineB.TabIndex = 19;
            lblBaseLineB.Text = "label5";
            // 
            // lblNewImageB
            // 
            lblNewImageB.AutoSize = true;
            lblNewImageB.Location = new Point(1624, 91);
            lblNewImageB.Name = "lblNewImageB";
            lblNewImageB.Size = new Size(25, 20);
            lblNewImageB.TabIndex = 22;
            lblNewImageB.Text = "B: ";
            // 
            // lblNewImageG
            // 
            lblNewImageG.AutoSize = true;
            lblNewImageG.Location = new Point(1624, 71);
            lblNewImageG.Name = "lblNewImageG";
            lblNewImageG.Size = new Size(50, 20);
            lblNewImageG.TabIndex = 21;
            lblNewImageG.Text = "label7";
            // 
            // lblNewImageR
            // 
            lblNewImageR.AutoSize = true;
            lblNewImageR.Location = new Point(1624, 51);
            lblNewImageR.Name = "lblNewImageR";
            lblNewImageR.Size = new Size(50, 20);
            lblNewImageR.TabIndex = 20;
            lblNewImageR.Text = "label8";
            // 
            // txtNewImageCoordinate
            // 
            txtNewImageCoordinate.Location = new Point(1674, 11);
            txtNewImageCoordinate.Margin = new Padding(3, 4, 3, 4);
            txtNewImageCoordinate.Name = "txtNewImageCoordinate";
            txtNewImageCoordinate.Size = new Size(57, 27);
            txtNewImageCoordinate.TabIndex = 23;
            txtNewImageCoordinate.TextChanged += txtNewImageCoordinate_TextChanged;
            txtNewImageCoordinate.KeyPress += txtNewImageCoordinate_KeyPress;
            // 
            // txtBaselineImageCoordinate
            // 
            txtBaselineImageCoordinate.Location = new Point(1831, 12);
            txtBaselineImageCoordinate.Margin = new Padding(3, 4, 3, 4);
            txtBaselineImageCoordinate.Name = "txtBaselineImageCoordinate";
            txtBaselineImageCoordinate.Size = new Size(57, 27);
            txtBaselineImageCoordinate.TabIndex = 24;
            txtBaselineImageCoordinate.KeyPress += txtBaselineImageCoordinate_KeyPress;
            // 
            // button1
            // 
            button1.Location = new Point(949, 17);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(86, 31);
            button1.TabIndex = 25;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnCompareWithComparableImage
            // 
            btnCompareWithComparableImage.Location = new Point(622, 51);
            btnCompareWithComparableImage.Name = "btnCompareWithComparableImage";
            btnCompareWithComparableImage.Size = new Size(294, 29);
            btnCompareWithComparableImage.TabIndex = 26;
            btnCompareWithComparableImage.Text = "Compare With Comparable Image";
            btnCompareWithComparableImage.UseVisualStyleBackColor = true;
            btnCompareWithComparableImage.Click += btnCompareWithComparableImage_Click;
            // 
            // ImageProcessingForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1924, 1055);
            Controls.Add(btnCompareWithComparableImage);
            Controls.Add(button1);
            Controls.Add(txtBaselineImageCoordinate);
            Controls.Add(txtNewImageCoordinate);
            Controls.Add(lblNewImageB);
            Controls.Add(lblNewImageG);
            Controls.Add(lblNewImageR);
            Controls.Add(lblBaseLineB);
            Controls.Add(lblBaseLineG);
            Controls.Add(lblBaseLineR);
            Controls.Add(pbBaseleinePreview);
            Controls.Add(pbNewImagePreview);
            Controls.Add(btnEdgeDetection);
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
            Margin = new Padding(3, 4, 3, 4);
            Name = "ImageProcessingForm";
            Text = "ImageProcessingForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbNewImagePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbBaseleinePreview).EndInit();
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
        private Button btnEdgeDetection;
        private PictureBox pbNewImagePreview;
        private PictureBox pbBaseleinePreview;
        private Label lblBaseLineR;
        private Label lblBaseLineG;
        private Label lblBaseLineB;
        private Label lblNewImageB;
        private Label lblNewImageG;
        private Label lblNewImageR;
        private TextBox txtNewImageCoordinate;
        private TextBox txtBaselineImageCoordinate;
        private Button button1;
        private Button btnCompareWithComparableImage;
    }
}