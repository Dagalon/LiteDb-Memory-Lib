using System.Text;

namespace SqliteDB_Memory_Lib
{
    public static class Extensions
    {
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

        public static string ToDbString(this string inputStr, string strToAddLeft, string strToAddRight)
        {
            return strToAddLeft + inputStr + strToAddRight;
        }
       
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

