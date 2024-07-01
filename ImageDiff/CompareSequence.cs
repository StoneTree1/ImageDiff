using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{

    public class CompareSequence
    {
        // Define scoring parameters (you can adjust these)
        private const int MatchScore = 1;
        private const int MismatchScore = -1;
        private const int GapPenalty = -2;

        public double CompareImages(int[,] image1, int[,] image2)
        {
            int rows = image1.GetLength(0);
            int cols = image1.GetLength(1);

            // Initialize the scoring matrix
            int[,] scoreMatrix = new int[rows + 1, cols + 1];

            // Fill in the scoring matrix using dynamic programming
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    int matchScore = image1[i - 1, j - 1] == image2[i - 1, j - 1] ? MatchScore : MismatchScore;

                    int diagonalScore = scoreMatrix[i - 1, j - 1] + matchScore;
                    int upScore = scoreMatrix[i - 1, j] + GapPenalty;
                    int leftScore = scoreMatrix[i, j - 1] + GapPenalty;

                    scoreMatrix[i, j] = Math.Max(0, Math.Max(diagonalScore, Math.Max(upScore, leftScore)));
                }
            }

            // Find the maximum score and its position
            int maxScore = 0;
            int maxI = 0, maxJ = 0;
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    if (scoreMatrix[i, j] > maxScore)
                    {
                        maxScore = scoreMatrix[i, j];
                        maxI = i;
                        maxJ = j;
                    }
                }
            }

            // Trace back to find aligned pixels (optional)
            // You can adapt this part to extract the aligned regions

            // Compute similarity score (normalized by image size)
            double similarity = (double)maxScore / (rows * cols);

            return similarity;
        }
    }

}
