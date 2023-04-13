using System.Data;
using System.Xml.Serialization;

namespace WordMergeEngine
{
    public static class Extensions
    {
        public static bool Equals(this string text, StringComparison comparison, params string[] toCompare)
        {
            return toCompare.Any(s => text.Equals(s, comparison));
        }

        public static void ChangeQuotes(this DataSet dataset)
        {
            for (var i = 0; i < dataset.Tables.Count; i++)
            {
                for (int j = 0; j < dataset.Tables[i].Rows.Count; j++)
                {
                    for (int m = 0; m < dataset.Tables[i].Columns.Count; m++)
                    {
                        if (dataset.Tables[i].Columns[m].DataType != typeof(string) || dataset.Tables[i].Rows[j][m] == DBNull.Value)
                            continue;

                        dataset.Tables[i].Columns[m].ReadOnly = false;
                        var resultString = ((string)dataset.Tables[i].Rows[j][m]).Replace("\"", "\'\'");

                        int? maxLength = 0;

                        for (var sl = 0; sl < dataset.Tables[i].Rows.Count; sl++)
                            maxLength = dataset.Tables[i].Rows[sl][m]?.ToString()?.Length > maxLength ? dataset.Tables[i].Rows[sl][m]?.ToString()?.Length : maxLength;

                        if (maxLength.HasValue && maxLength.Value < resultString.Length)
                            maxLength = resultString.Length;

                        if (maxLength.HasValue)
                            dataset.Tables[i].Columns[m].MaxLength = maxLength.Value;

                        dataset.Tables[i].Rows[j][m] = resultString;

                    }
                }
            }
        }

        public static string Serialize<T>(T data)
        {
            using (var stringwriter = new StringWriter())
            {
                new XmlSerializer(typeof(T)).Serialize(stringwriter, data);

                return stringwriter.ToString();
            }
        }

        public static T Deserialize<T>(string xml)
        {
            using (TextReader reader = new StringReader(xml))
            {
                var result = (T)new XmlSerializer(typeof(T)).Deserialize(reader);

                return result;
            }
        }

        public static string ReverseString(this string str)
        {
            var strArray = str.ToCharArray();

            Array.Reverse(strArray);

            return new string(strArray);
        }
    }
}
