namespace ImageComparer;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.btnSelectBaselineFolder = new System.Windows.Forms.Button();
            this.btnSelectComparisonFolder = new System.Windows.Forms.Button();
            this.lblBaselineFoilder = new System.Windows.Forms.Label();
            this.lblComparisonFolder = new System.Windows.Forms.Label();
            this.pbAlternaitingComparison = new System.Windows.Forms.PictureBox();
            this.pbCompareResult = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnPerformDiffs = new System.Windows.Forms.Button();
            this.cmbImagesForReview = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pbAlternaitingComparison)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCompareResult)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectBaselineFolder
            // 
            this.btnSelectBaselineFolder.Location = new System.Drawing.Point(12, 12);
            this.btnSelectBaselineFolder.Name = "btnSelectBaselineFolder";
            this.btnSelectBaselineFolder.Size = new System.Drawing.Size(178, 29);
            this.btnSelectBaselineFolder.TabIndex = 0;
            this.btnSelectBaselineFolder.Text = "Select Baseline Folder";
            this.btnSelectBaselineFolder.UseVisualStyleBackColor = true;
            this.btnSelectBaselineFolder.Click += new System.EventHandler(this.btnSelectBaselineFolder_Click);
            // 
            // btnSelectComparisonFolder
            // 
            this.btnSelectComparisonFolder.Location = new System.Drawing.Point(196, 12);
            this.btnSelectComparisonFolder.Name = "btnSelectComparisonFolder";
            this.btnSelectComparisonFolder.Size = new System.Drawing.Size(192, 29);
            this.btnSelectComparisonFolder.TabIndex = 1;
            this.btnSelectComparisonFolder.Text = "Select Comparison Folder";
            this.btnSelectComparisonFolder.UseVisualStyleBackColor = true;
            this.btnSelectComparisonFolder.Click += new System.EventHandler(this.btnSelectComparisonFolder_Click);
            // 
            // lblBaselineFoilder
            // 
            this.lblBaselineFoilder.AutoSize = true;
            this.lblBaselineFoilder.Location = new System.Drawing.Point(12, 44);
            this.lblBaselineFoilder.Name = "lblBaselineFoilder";
            this.lblBaselineFoilder.Size = new System.Drawing.Size(50, 20);
            this.lblBaselineFoilder.TabIndex = 2;
            this.lblBaselineFoilder.Text = "label1";
            // 
            // lblComparisonFolder
            // 
            this.lblComparisonFolder.AutoSize = true;
            this.lblComparisonFolder.Location = new System.Drawing.Point(12, 64);
            this.lblComparisonFolder.Name = "lblComparisonFolder";
            this.lblComparisonFolder.Size = new System.Drawing.Size(50, 20);
            this.lblComparisonFolder.TabIndex = 3;
            this.lblComparisonFolder.Text = "label1";
            // 
            // pbAlternaitingComparison
            // 
            this.pbAlternaitingComparison.Location = new System.Drawing.Point(12, 117);
            this.pbAlternaitingComparison.Name = "pbAlternaitingComparison";
            this.pbAlternaitingComparison.Size = new System.Drawing.Size(833, 1086);
            this.pbAlternaitingComparison.TabIndex = 4;
            this.pbAlternaitingComparison.TabStop = false;
            // 
            // pbCompareResult
            // 
            this.pbCompareResult.Location = new System.Drawing.Point(880, 117);
            this.pbCompareResult.Name = "pbCompareResult";
            this.pbCompareResult.Size = new System.Drawing.Size(833, 1086);
            this.pbCompareResult.TabIndex = 5;
            this.pbCompareResult.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnPerformDiffs
            // 
            this.btnPerformDiffs.Location = new System.Drawing.Point(394, 12);
            this.btnPerformDiffs.Name = "btnPerformDiffs";
            this.btnPerformDiffs.Size = new System.Drawing.Size(192, 29);
            this.btnPerformDiffs.TabIndex = 6;
            this.btnPerformDiffs.Text = "Perform Diffs";
            this.btnPerformDiffs.UseVisualStyleBackColor = true;
            this.btnPerformDiffs.Click += new System.EventHandler(this.btnPerformDiffs_Click);
            // 
            // cmbImagesForReview
            // 
            this.cmbImagesForReview.FormattingEnabled = true;
            this.cmbImagesForReview.Location = new System.Drawing.Point(12, 83);
            this.cmbImagesForReview.Name = "cmbImagesForReview";
            this.cmbImagesForReview.Size = new System.Drawing.Size(203, 28);
            this.cmbImagesForReview.TabIndex = 7;
            this.cmbImagesForReview.SelectedIndexChanged += new System.EventHandler(this.cmbImagesForReview_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(221, 83);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(83, 28);
            this.btnRefresh.TabIndex = 8;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2306, 1493);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cmbImagesForReview);
            this.Controls.Add(this.btnPerformDiffs);
            this.Controls.Add(this.pbCompareResult);
            this.Controls.Add(this.pbAlternaitingComparison);
            this.Controls.Add(this.lblComparisonFolder);
            this.Controls.Add(this.lblBaselineFoilder);
            this.Controls.Add(this.btnSelectComparisonFolder);
            this.Controls.Add(this.btnSelectBaselineFolder);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pbAlternaitingComparison)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCompareResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private Button btnSelectBaselineFolder;
    private Button btnSelectComparisonFolder;
    private Label lblBaselineFoilder;
    private Label lblComparisonFolder;
    private PictureBox pbAlternaitingComparison;
    private PictureBox pbCompareResult;
    private System.Windows.Forms.Timer timer1;
    private Button btnPerformDiffs;
    private ComboBox cmbImagesForReview;
    private Button btnRefresh;
    private OpenFileDialog openFileDialog1;
    private FolderBrowserDialog folderBrowserDialog1;
}
