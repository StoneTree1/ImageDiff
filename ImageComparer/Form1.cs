using ImageDiff;
using SixLabors.ImageSharp;
using System.Reflection;

namespace ImageComparer;

public partial class Form1 : Form
{
    string baselineFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\BaselineImages";
    string comparisonFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\ComparisonImages";
    string resultsFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\CompareResults";
    string imageA = "";
    string imageB = "";
    string imageC = "";
    bool displayImageA;
    public Form1()
    {
        InitializeComponent();
        folderBrowserDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        lblBaselineFoilder.Text = baselineFolder;
        lblComparisonFolder.Text = comparisonFolder;
    }

    private void btnSelectBaselineFolder_Click(object sender, EventArgs e)
    {

        if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
        {
            baselineFolder = folderBrowserDialog1.SelectedPath;
            lblBaselineFoilder.Text = baselineFolder;
        }
    }

    private void btnSelectComparisonFolder_Click(object sender, EventArgs e)
    {
        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
        {
            comparisonFolder = folderBrowserDialog1.SelectedPath;
            lblComparisonFolder.Text = comparisonFolder;
        }
    }

    private void btnPerformDiffs_Click(object sender, EventArgs e)
    {       
        this.SuspendLayout();
        DoCompares();
        this.ResumeLayout();
    }

    //Todo: add threading support to speed things up
    private void DoCompares()
    {
        var comparer = new ImageDiff.ImageDiff();
        comparer.searchWidth = 75;
        comparer.searchHeight = 75;
        comparer.traversalBorder = 1;
        var comparisonImages = Directory.GetFiles(comparisonFolder);
        var baselineImages = Directory.GetFiles(baselineFolder);
        foreach (var file in baselineImages)
        {
            var datetime = DateTime.Now;
            var fileName = Path.GetFileName(file);
            if (comparisonImages.Any(x => x.Contains(fileName))){
                var comparisonImage = File.ReadAllBytes($"{comparisonFolder}\\{Path.GetFileName(fileName)}");
                var baselineImage = File.ReadAllBytes($"{baselineFolder}\\{Path.GetFileName(fileName)}");
                bool isDifferent;
                var result = comparer.DoCompare(comparisonImage, baselineImage, out isDifferent);
                result.SaveAsPng($"{resultsFolder}\\{Path.GetFileNameWithoutExtension(file)}.png");
            }
            var endTime = DateTime.Now;
            FormLogger.Instance.Log($"Image {fileName} took {(endTime - datetime).TotalSeconds} seconds to process\n");
        }
        Refresh();
        cmbImagesForReview.SelectedIndex = 0;
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        cmbImagesForReview.Items.Clear();
        foreach (var file in Directory.GetFiles(resultsFolder))
        {
            cmbImagesForReview.Items.Add(Path.GetFileName(file));
        }
    }

    private void cmbImagesForReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        var comparisonImages = Directory.GetFiles(comparisonFolder);
        var baselineImages = Directory.GetFiles(baselineFolder);
        var resultImages = Directory.GetFiles(resultsFolder);
        var selectedFile = cmbImagesForReview.Text;
        var baseline = baselineImages.First(x => x.Contains(selectedFile));
        var comparison = comparisonImages.First(x => x.Contains(selectedFile));
        var result = resultImages.First(x => x.Contains(selectedFile));
        pbCompareResult.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(result)));
        pbCompareResult.SizeMode = PictureBoxSizeMode.Zoom;
        imageA = baseline;
        imageB = comparison;
        imageC = result;
        timer1.Interval=1000;
        displayImageA = true;
        timer1.Start();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        timer1.Interval = 500;
        if (displayImageA)
        {
            pbAlternaitingComparison.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(imageA)));
            pbAlternaitingComparison.SizeMode = PictureBoxSizeMode.Zoom;
        }
        else
        {
            pbAlternaitingComparison.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(imageB)));
            pbAlternaitingComparison.SizeMode = PictureBoxSizeMode.Zoom;
        }
        displayImageA = !displayImageA;
    }
}
