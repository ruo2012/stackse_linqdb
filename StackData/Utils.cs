using Iveonik.Stemmers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackData
{
    public class Utils
    {
        public static string GetStemFromWord(string word)
        {
            var stemm = new EnglishStemmer();
            return stemm.Stem(word);
        }
        public static byte[] TfIdfToData(List<WordTfIdf> words)
        {
            var list = new List<byte>();
            foreach (var w in words)
            {
                list.AddRange(BitConverter.GetBytes(w.QuestionId));
                int val = (int)Math.Round(w.TfIdf);
                if (val > Int16.MaxValue)
                {
                    val = Int16.MaxValue - 10;
                }
                list.AddRange(BitConverter.GetBytes((short)val));
            }
            return list.ToArray();
        }
        public static Tuple<Dictionary<int, short>, HashSet<int>> TfIdfFromData(byte[] data)
        {
            int total = data.Length / 6;
            var dic = new Dictionary<int, short>(total+10);
            var set = new HashSet<int>();
            var res = new Tuple<Dictionary<int, short>, HashSet<int>>(dic, set);

            int current = 0;
            for (int i = 0; i < total; i++)
            {
                int id = BitConverter.ToInt32(new byte[4] { data[current], data[current+1], data[current+2], data[current+3] }, 0);
                current += 4;
                short tfidf = BitConverter.ToInt16(new byte[2] { data[current], data[current + 1] }, 0);
                current += 2;
                dic[id] = tfidf;
                set.Add(id);
            }
            return res;
        }
    }
}
