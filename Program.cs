using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroLIFParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Operation started...");

            var records = new List<MarcRecord>();
            var lifParser = new MicroLIFFormatParser();

            while (lifParser.MoveNext())
                records.Add(lifParser.Current);

            Console.WriteLine($"Total Records: {records.Count}");
            Console.WriteLine("Operation Complete.");
            Console.ReadLine();
        }
    }
}
