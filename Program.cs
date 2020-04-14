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
            Console.WriteLine("Starting file parser...");
            Console.WriteLine("Loading file...");

            if (FileParserHelper.IsMicroLIFFile())
                Console.WriteLine("Yes it is MicroLIF!");
            else
                Console.WriteLine("No it is not MicroLIF!");

            Console.ReadLine();

            var records = new List<MarcRecord>();
            var lifParser = new MicroLIFFormatParser();

            while (lifParser.MoveNext())
            {
                records.Add(lifParser.Current);
            }

            Console.WriteLine(records.Count);
            Console.ReadLine();

        }
    }
}
