using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parserjutuba
{
    class TextDataParser
    {
        public void Parse(string dest, string fileContent, string label)
        {
            var list = new List<string>();

            char[] delimiters = new char[] { ' ' };
            var arr = fileContent.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            int length = arr.Count();
            int groups = length / 30;

            for (int i = 0; i < groups; i++)
            {
                string content = "__label__" + label;
                for (int j = 0; j < 30; j++)
                {
                    content = content + " " + arr[j + (i * 30)];
                }
                list.Add(content);
            }

            var last = list.Last();
            int remainder = length % 30;
            for (int i = length - remainder; i < length; i++)
            {
                last = last + " " + arr[i];
            }

            list[groups - 1] = last;

            using (StreamWriter w = new StreamWriter(dest + "data.txt", true, Encoding.UTF8))
            {
                foreach (var item in list)
                {
                    w.WriteLine(item);
                }
            }
        }
    }
}
