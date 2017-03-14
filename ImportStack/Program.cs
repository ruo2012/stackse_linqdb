using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
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
        
            //step2: prepare searchable data
            //string whole_path = @"C:\Users\Administrator\Documents\stackoverflow\WHOLE_DATA";
            //string interm_path = @"C:\Users\Administrator\Documents\stackoverflow\INTERM_DATA";
            //string final_path = @"C:\Users\Administrator\Documents\stackoverflow\FINAL_DATA";

            //int start = 9600000;
            //int total = 2400000;

            //DataPreparation.MakeSearchableData(whole_path, interm_path, final_path, start, total);
            //DataPreparation.MakeFragments(interm_path, final_path);

            Console.WriteLine("Time: {0} min", (DateTime.Now - start_time).TotalMinutes);
        }
    }
}
