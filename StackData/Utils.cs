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
    }
}
