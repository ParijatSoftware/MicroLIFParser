using System.IO;

namespace MicroLIFParser
{
    public static class FileParserHelper
    {
        public static bool IsMicroLIFFile()
        {
            var filePath = Directory.GetCurrentDirectory() + @"\LIFs\SampleLif";
            using (var reader = new StreamReader(File.Open(filePath, FileMode.Open)))
            {
                var firstLine = reader.ReadLine();
                var secondLine = reader.ReadLine();

                if (string.IsNullOrEmpty(firstLine) || string.IsNullOrEmpty(secondLine))
                    return false;

                if ((firstLine.StartsWith("LDR") || secondLine.StartsWith("LDR")) && !string.IsNullOrEmpty(secondLine))
                    return true;

                return false;
            }
        }
    }
}
