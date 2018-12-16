using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePOS3.Utils
{
    public class PhoneNumber
    {

        public static string [] ProcessProvider(Dictionary<string, string> dict)
        {
            string[] array = new string[dict.Count()];
            for (int i = 0; i < dict.Count(); i++)
                array[i] = dict.ElementAt(i).Key.Trim();
            return array;
        }

        public static string[] ProcessCustomerGroup(string v)
        {
            // 1
            // Keep track of words found in this Dictionary.
            var d = new Dictionary<string, bool>();

            v = v.ToUpper();
            // 3
            // Split the input and handle spaces and punctuation.
            string[] a = v.Split(new char[] { ' ', ',', ';', ':', '.', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            List<string> b = new List<string>();


            // 4
            // Loop over each word
            int i = 0;
            foreach (string current in a)
            {
                // 5
                // Lowercase each word
                string upper = current;

                // 6
                // If we haven't already encountered the word,
                // append it to the result.
                if (!string.IsNullOrEmpty(upper) && !d.ContainsKey(upper))
                {
                    b.Add(current);
                    d.Add(upper, true);
                    i++;
                }
            }
            // 7
            // Return the duplicate words removed
            return b.ToArray();
        }

        public static string[] ProcessCustomerGroupwithPrefix(string prefix, string v)
        {
            // 1
            // Keep track of words found in this Dictionary.
            var d = new Dictionary<string, bool>();
            prefix = prefix.ToUpper();
            v = v.ToUpper();
            // 3
            // Split the input and handle spaces and punctuation.
            string[] a = v.Split(new char[] { ' ', ',', ';', ':', '.', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            List<string> b = new List<string>();


            // 4
            // Loop over each word
            int i = 0;
            string upper = string.Empty;
            foreach (string current in a)
            {
                // 5
                // Lowercase each word
                upper = current.IndexOf(prefix) < 0 ? prefix.Trim() + current : current;

                // 6
                // If we haven't already encountered the word,
                // append it to the result.
                if (!string.IsNullOrEmpty(upper) && !d.ContainsKey(upper))
                {
                    b.Add(current.IndexOf(prefix) < 0 ? prefix.Trim() + current : current);
                    d.Add(upper, true);
                    i++;
                }
            }
            // 7
            // Return the duplicate words removed
            return b.ToArray();
        }

        public static string ProcessReplace(string v)
        {
            string b = null;
            string[] a = ProcessCustomerGroup(v);
            foreach (string item in a)
            {
                if (string.IsNullOrEmpty(b))
                    b = "'" + item + "'";
                else
                    b = b + "," + "'" + item + "'";
            }

            return b;
        }

        public static string GetFirstDayOfMonth()
        {
            DateTime dtResult = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtResult = dtResult.AddDays((-dtResult.Day) + 1);
            return dtResult.ToString("dd/MM/yyyy");
        }
    }
}