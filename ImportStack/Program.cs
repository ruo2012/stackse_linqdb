using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using LinqDb;
using System.Xml.Linq;
using System.Xml;
using StackData;

namespace ImportStack
{
    class ImportStackoverflow
    {
        static void Main(string[] args)
        {
            DateTime start_time = DateTime.Now;
            //step 1: import data from xml files
            ImportData.Import(@"C:\Users\Administrator\Documents\stackoverflow\");
        
            ////step2: prepare searchable data
            string path = @"C:\Users\Administrator\Documents\stackoverflow\WHOLE_DATA";
            string p1 = @"C:\Users\Administrator\Documents\stackoverflow\p1";
            string p2 = @"C:\Users\Administrator\Documents\stackoverflow\p2";
            string p3 = @"C:\Users\Administrator\Documents\stackoverflow\p3";
            string p4 = @"C:\Users\Administrator\Documents\stackoverflow\p4";

            int start = 6000000;
            int total = 2000000;

            DataPreparation.MakeSearchableData(path, p1, p2, p3, p4, start, total);
            DataPreparation.MakeFragments(p1, p2, p3, p4);
            DataPreparation.MakeFragmentWords(p1, p2, p3, p4);

            Console.WriteLine("Time: {0} min", (DateTime.Now - start_time).TotalMinutes);
        }
    }
}
