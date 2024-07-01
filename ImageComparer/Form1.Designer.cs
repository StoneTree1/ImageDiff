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
        components = new System.ComponentModel.Container();
        btnSelectBaselineFolder = new Button();
        btnSelectComparisonFolder = new Button();
        lblBaselineFoilder = new Label();
        lblComparisonFolder = new Label();
        pbAlternaitingComparison = new PictureBox();
        pbCompareResult = new PictureBox();
        timer1 = new System.Windows.Forms.Timer(components);
        btnPerformDiffs = new Button();
        cmbImagesForReview = new ComboBox();
        btnRefresh = new Button();
        openFileDialog1 = new OpenFileDialog();
        folderBrowserDialog1 = new FolderBrowserDialog();
        button1 = new Button();
        button2 = new Button();
        button3 = new Button();
        ((System.ComponentModel.ISupportInitialize)pbAlternaitingComparison).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pbCompareResult).BeginInit();
        SuspendLayout();
        // 
        // btnSelectBaselineFolder
        // 
        btnSelectBaselineFolder.Location = new Point(10, 9);
        btnSelectBaselineFolder.Margin = new Padding(3, 2, 3, 2);
        btnSelectBaselineFolder.Name = "btnSelectBaselineFolder";
        btnSelectBaselineFolder.Size = new Size(156, 22);
        btnSelectBaselineFolder.TabIndex = 0;
        btnSelectBaselineFolder.Text = "Select Baseline Folder";
        btnSelectBaselineFolder.UseVisualStyleBackColor = true;
        btnSelectBaselineFolder.Click += btnSelectBaselineFolder_Click;
        // 
        // btnSelectComparisonFolder
        // 
        btnSelectComparisonFolder.Location = new Point(172, 9);
        btnSelectComparisonFolder.Margin = new Padding(3, 2, 3, 2);
        btnSelectComparisonFolder.Name = "btnSelectComparisonFolder";
        btnSelectComparisonFolder.Size = new Size(168, 22);
        btnSelectComparisonFolder.TabIndex = 1;
        btnSelectComparisonFolder.Text = "Select Comparison Folder";
        btnSelectComparisonFolder.UseVisualStyleBackColor = true;
        btnSelectComparisonFolder.Click += btnSelectComparisonFolder_Click;
        // 
        // lblBaselineFoilder
        // 
        lblBaselineFoilder.AutoSize = true;
        lblBaselineFoilder.Location = new Point(10, 33);
        lblBaselineFoilder.Name = "lblBaselineFoilder";
        lblBaselineFoilder.Size = new Size(38, 15);
        lblBaselineFoilder.TabIndex = 2;
        lblBaselineFoilder.Text = "label1";
        // 
        // lblComparisonFolder
        // 
        lblComparisonFolder.AutoSize = true;
        lblComparisonFolder.Location = new Point(10, 48);
        lblComparisonFolder.Name = "lblComparisonFolder";
        lblComparisonFolder.Size = new Size(38, 15);
        lblComparisonFolder.TabIndex = 3;
        lblComparisonFolder.Text = "label1";
        // 
        // pbAlternaitingComparison
        // 
        pbAlternaitingComparison.Location = new Point(12, 106);
        pbAlternaitingComparison.Margin = new Padding(3, 2, 3, 2);
        pbAlternaitingComparison.Name = "pbAlternaitingComparison";
        pbAlternaitingComparison.Size = new Size(729, 814);
        pbAlternaitingComparison.TabIndex = 4;
        pbAlternaitingComparison.TabStop = false;
        // 
        // pbCompareResult
        // 
        pbCompareResult.Location = new Point(791, 106);
        pbCompareResult.Margin = new Padding(3, 2, 3, 2);
        pbCompareResult.Name = "pbCompareResult";
        pbCompareResult.Size = new Size(729, 814);
        pbCompareResult.TabIndex = 5;
        pbCompareResult.TabStop = false;
        // 
        // timer1
        // 
        timer1.Tick += timer1_Tick;
        // 
        // btnPerformDiffs
        // 
        btnPerformDiffs.Location = new Point(345, 9);
        btnPerformDiffs.Margin = new Padding(3, 2, 3, 2);
        btnPerformDiffs.Name = "btnPerformDiffs";
        btnPerformDiffs.Size = new Size(168, 22);
        btnPerformDiffs.TabIndex = 6;
        btnPerformDiffs.Text = "Perform Diffs";
        btnPerformDiffs.UseVisualStyleBackColor = true;
        btnPerformDiffs.Click += btnPerformDiffs_Click;
        // 
        // cmbImagesForReview
        // 
        cmbImagesForReview.FormattingEnabled = true;
        cmbImagesForReview.Location = new Point(10, 62);
        cmbImagesForReview.Margin = new Padding(3, 2, 3, 2);
        cmbImagesForReview.Name = "cmbImagesForReview";
        cmbImagesForReview.Size = new Size(178, 23);
        cmbImagesForReview.TabIndex = 7;
        cmbImagesForReview.SelectedIndexChanged += cmbImagesForReview_SelectedIndexChanged;
        // 
        // btnRefresh
        // 
        btnRefresh.Location = new Point(193, 62);
        btnRefresh.Margin = new Padding(3, 2, 3, 2);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(73, 21);
        btnRefresh.TabIndex = 8;
        btnRefresh.Text = "Refresh";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;
        // 
        // openFileDialog1
        // 
        openFileDialog1.FileName = "openFileDialog1";
        // 
        // button1
        // 
        button1.Location = new Point(519, 8);
        button1.Name = "button1";
        button1.Size = new Size(75, 23);
        button1.TabIndex = 9;
        button1.Text = "button1";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // button2
        // 
        button2.Location = new Point(600, 8);
        button2.Name = "button2";
        button2.Size = new Size(75, 23);
        button2.TabIndex = 10;
        button2.Text = "button2";
        button2.UseVisualStyleBackColor = true;
        button2.Click += button2_Click;
        // 
        // button3
        // 
        button3.Location = new Point(681, 8);
        button3.Name = "button3";
        button3.Size = new Size(75, 23);
        button3.TabIndex = 11;
        button3.Text = "button3";
        button3.UseVisualStyleBackColor = true;
        button3.Click += button3_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(2018, 1066);
        Controls.Add(button3);
        Controls.Add(button2);
        Controls.Add(button1);
        Controls.Add(btnRefresh);
        Controls.Add(cmbImagesForReview);
        Controls.Add(btnPerformDiffs);
        Controls.Add(pbCompareResult);
        Controls.Add(pbAlternaitingComparison);
        Controls.Add(lblComparisonFolder);
        Controls.Add(lblBaselineFoilder);
        Controls.Add(btnSelectComparisonFolder);
        Controls.Add(btnSelectBaselineFolder);
        Margin = new Padding(3, 2, 3, 2);
        Name = "Form1";
        Text = "Form1";
        ((System.ComponentModel.ISupportInitialize)pbAlternaitingComparison).EndInit();
        ((System.ComponentModel.ISupportInitialize)pbCompareResult).EndInit();
        ResumeLayout(false);
        PerformLayout();
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
    private Button button1;
    private Button button2;
    private Button button3;
}
