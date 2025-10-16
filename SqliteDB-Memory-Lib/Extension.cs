using System.Text;

namespace SqliteDB_Memory_Lib
{
    /// <summary>
    /// Provides utility extension methods for working with arrays and strings within the library.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extracts a two-dimensional subarray from the provided matrix.
        /// </summary>
        /// <typeparam name="T">Type of the elements inside the array.</typeparam>
        /// <param name="values">Source matrix.</param>
        /// <param name="rowMin">Inclusive starting row index.</param>
        /// <param name="rowMax">Exclusive ending row index.</param>
        /// <param name="colMin">Inclusive starting column index.</param>
        /// <param name="colMax">Exclusive ending column index.</param>
        /// <returns>A new matrix containing the requested slice.</returns>
        public static T[,] SubArray<T>(this T[,] values, int rowMin, int rowMax, int colMin, int colMax)
        {
            // Allocate the result array.
            var numRows = rowMax - rowMin;
            var numCols = colMax - colMin;
            T[,] result = new T[numRows, numCols];

            // Get the number of columns in the values array.
            var totalCols = values.GetUpperBound(1) + 1;
            var fromIndex = rowMin * totalCols + colMin;
            var toIndex = 0;
            for (int row = 0; row < numRows; row++)
            {
                Array.Copy(values, fromIndex, result, toIndex, numCols);
                fromIndex += totalCols;
                toIndex += numCols;
            }

            return result;
        }

        /// <summary>
        /// Extracts a two-dimensional slice from the provided matrix and converts it into a list of lists.
        /// </summary>
        /// <typeparam name="T">Type of the elements inside the array.</typeparam>
        /// <param name="values">Source matrix.</param>
        /// <param name="rowMin">Inclusive starting row index.</param>
        /// <param name="rowMax">Exclusive ending row index.</param>
        /// <param name="colMin">Inclusive starting column index.</param>
        /// <param name="colMax">Exclusive ending column index.</param>
        /// <returns>A list of rows representing the selected slice.</returns>
        public static List<List<T>> SubArrayToList<T>(this T[,] values, int rowMin, int rowMax, int colMin, int colMax)
        {
            var result = new List<List<T>>();
            var rowCounter = 0;

            for (var row = rowMin; row < rowMax; row++)
            {
                result.Add(new List<T>());
                for (var col = colMin; col < colMax; col++)
                {
                    result[rowCounter].Add(values[row, col]);
                }

                rowCounter++;
            }

            return result;
        }
       
        /// <summary>
        /// Converts a one-dimensional array into a two-dimensional column matrix.
        /// </summary>
        /// <typeparam name="T">Type of the elements inside the array.</typeparam>
        /// <param name="array">Source array.</param>
        /// <returns>A two-dimensional representation of the array.</returns>
        public static T[,] To2D<T>(this T[] array)
        {
            var noElements = array.Length;
            T[,] output = new T[noElements,1];
            for (var i = 0; i < noElements; i++)
            {
                output[i, 0] = array[i];
            }

            return output;
        }

        /// <summary>
        /// Surrounds a string with the provided left and right tokens.
        /// </summary>
        /// <param name="inputStr">Original string.</param>
        /// <param name="strToAddLeft">Token to prepend.</param>
        /// <param name="strToAddRight">Token to append.</param>
        /// <returns>The decorated string.</returns>
        public static string ToDbString(this string inputStr, string strToAddLeft, string strToAddRight)
        {
            return strToAddLeft + inputStr + strToAddRight;
        }

        /// <summary>
        /// Replaces all occurrences of a string using the specified comparison rules.
        /// </summary>
        /// <param name="s">Original string.</param>
        /// <param name="oldValue">Text to replace.</param>
        /// <param name="newValue">Replacement text.</param>
        /// <param name="comparisonType">Comparison mode applied during the search.</param>
        /// <returns>The transformed string or <c>null</c> when the source is <c>null</c>.</returns>
        public static string? Replace(this string? s, string oldValue, string newValue,
            StringComparison comparisonType)
        {
            if (s == null)
                return null;

            if (string.IsNullOrEmpty(oldValue))
                return s;

            var result = new StringBuilder(Math.Min(4096, s.Length));
            var pos = 0;

            while (true)
            {
                var i = s.IndexOf(oldValue, pos, comparisonType);
                if (i < 0)
                    break;

                result.Append(s, pos, i - pos);
                result.Append(newValue);

                pos = i + oldValue.Length;
            }
            result.Append(s, pos, s.Length - pos);

            return result.ToString();
        }
       
    }

}

