using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentGenerator3.DocumentAssembly
{
    public class CreateArray
    {
        public string[,] LoadCsv(string csvString)
        {
            // Split into lines.
            csvString = csvString.Replace('\n', '\r');
            string[] lines = csvString.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            NumberOfRows = num_rows;
            int num_cols = lines[0].Split(',').Length;
            numberOfColumns = num_cols;

            // Allocate the data array.
            string[,] values = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }

            // Return the values.
            return values;
        }

        public int NumberOfRows = 0;
        public int numberOfColumns = 0;

    }
}
