using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MicroLIFParser
{
    public class MicroLIFFormatParser
    {
        readonly StreamReader reader;
        public MicroLIFFormatParser()
        {
            var filePath = Directory.GetCurrentDirectory() + @"\LIFs\22691407.mrc";
            reader = new StreamReader(File.Open(filePath, FileMode.Open));
        }

        public MarcRecord Current
        {
            get;
            private set;
        }


        private MarcRecord ParseNextRecord()
        {

            try
            {
                string currentLine;
                var currentMarc = new MarcRecord();
                bool eor;
                var firstLine = reader.ReadLine();
                var secondLine = reader.ReadLine();

                do
                {
                    if (firstLine != null && !firstLine.StartsWith("LDR")) //If not LDR then probably it Software name and version so need to skip
                    {
                        eor = false;
                        firstLine = null;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(firstLine))
                        currentLine = firstLine;
                    else
                        currentLine = reader.ReadLine();

                    if (currentLine == null)
                    {
                        currentMarc = null;
                        break;
                    }

                    eor = currentLine.EndsWith('`'); //end of record
                    if (currentLine.StartsWith("LDR"))
                        currentMarc.Leader = currentLine.Substring(3).TrimEnd('`').TrimEnd('^');
                    else //control or MARC fields
                    {
                        var currentTag = currentLine.Substring(0, 3);
                        if (int.Parse(currentTag) < 10) //control fields
                            currentMarc.ControlFields.Add(currentTag, currentLine.Substring(3).TrimEnd('`').TrimEnd('^'));
                        else //Marc fields
                        {
                            var subFieldValues = currentLine.Substring(3).TrimEnd('`').TrimEnd('^');
                            var indicators = subFieldValues.Substring(0, 2);

                            var marcField = new MarcField();
                            marcField.Tag = int.Parse(currentTag);
                            marcField.Ind1 = indicators[0];
                            marcField.Ind2 = indicators[1];

                            var subFields = subFieldValues.Substring(2).Split('_').Where(x => !string.IsNullOrEmpty(x)).ToList();
                            foreach (var subfield in subFields)
                                marcField.Subfields.Add(new MarcSubfield { Code = subfield.First(), Data = subfield.Substring(1).Trim() });

                            //currentMarc.MarcFields[marcField.Tag].Add(marcField);

                            if (currentMarc.MarcFields.ContainsKey(marcField.Tag))
                                currentMarc.MarcFields[marcField.Tag].Add(marcField);
                            else
                            {
                                List<MarcField> newTagCollection = new List<MarcField> { marcField };
                                currentMarc.MarcFields[marcField.Tag] = newTagCollection;
                            }
                        }
                    }

                    if (firstLine != null) //making sure that first line is clean
                        firstLine = null;

                } while (!eor);

                return currentMarc;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public bool MoveNext()
        {
            Current = ParseNextRecord();
            if (Current == null)
                return false;
            return true;
        }
    }
}
