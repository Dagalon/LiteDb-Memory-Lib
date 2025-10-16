using System.Text;

namespace SqliteDB_Memory_Lib
{
    /// <summary>
    /// Extension helpers that simplify manipulating in-memory tabular data.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extracts a 2D slice from the provided matrix.
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the array.</typeparam>
        /// <param name="values">Source matrix.</param>
        /// <param name="rowMin">Inclusive start row.</param>
        /// <param name="rowMax">Exclusive end row.</param>
        /// <param name="colMin">Inclusive start column.</param>
        /// <param name="colMax">Exclusive end column.</param>
        /// <returns>A new matrix that contains the requested slice.</returns>
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
        /// Converts the requested 2D slice into a list of lists.
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the array.</typeparam>
        /// <param name="values">Source matrix.</param>
        /// <param name="rowMin">Inclusive start row.</param>
        /// <param name="rowMax">Exclusive end row.</param>
        /// <param name="colMin">Inclusive start column.</param>
        /// <param name="colMax">Exclusive end column.</param>
        /// <returns>List representation of the requested slice.</returns>
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
        /// Converts a single dimensional array into a column-based 2D matrix.
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the array.</typeparam>
        /// <param name="array">Source array.</param>
        /// <returns>A matrix with one column that contains the original elements.</returns>
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
        /// Surrounds the provided string with the specified prefixes and suffixes.
        /// </summary>
        /// <param name="inputStr">Value to decorate.</param>
        /// <param name="strToAddLeft">Prefix added to the left of the string.</param>
        /// <param name="strToAddRight">Suffix added to the right of the string.</param>
        /// <returns>The decorated string.</returns>
        public static string ToDbString(this string inputStr, string strToAddLeft, string strToAddRight)
        {
            return strToAddLeft + inputStr + strToAddRight;
        }

        /// <summary>
        /// Performs a case-aware string replacement that mirrors the overload introduced in newer frameworks.
        /// </summary>
        /// <param name="s">Source string.</param>
        /// <param name="oldValue">Value that will be replaced.</param>
        /// <param name="newValue">Replacement value.</param>
        /// <param name="comparisonType">Comparison type used to match <paramref name="oldValue"/>.</param>
        /// <returns>A string with the replacements applied or <c>null</c> when the source string is <c>null</c>.</returns>
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

