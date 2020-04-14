using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroLIFParser
{
    public class MarcRecord
    {
        public string Leader { get; set; }

        public string Title
        {
            get
            {
                return RemoveEndingPunctuation(GetDataSubfield(245, 'a') + " " + GetDataSubfield(245, 'b') + " " + GetDataSubfield(245, 'c')).Trim();
            }
        }

        public string ShortTitle
        {
            get
            {
                return RemoveEndingPunctuation(GetDataSubfield(245, 'a'));
            }
        }

        public string Author
        {
            get
            {
                return RemoveEndingPunctuation(GetDataSubfield(100, 'a'));
            }
        }

        public string ISBN
        {
            get
            {
                return CleanISBN(GetDataSubfield(020, 'a'));
            }
        }

        public string LCCN
        {
            get
            {
                return DigitsOnly(GetDataSubfield(010, 'a'));
            }
        }

        public string ISSN
        {
            get
            {
                return DigitsOnly(GetDataSubfield(022, 'a'));
            }
        }

        public string UPC
        {
            get
            {
                if (MarcFields.ContainsKey(024))
                {
                    foreach (var field in MarcFields[024].Where(x => x.Ind1 == 1))
                    {
                        var subfield = field.Subfields.FirstOrDefault(x => x.Code == 'a');
                        if (subfield != null)
                            return DigitsOnly(subfield.Data);
                    }
                }
                return string.Empty;
            }
        }

        public Dictionary<string, string> ControlFields { get; set; } = new Dictionary<string, string>();

        public SortedList<int, List<MarcField>> MarcFields { get; set; } = new SortedList<int, List<MarcField>>();

        public string GetDataSubfield(int Tag, char Subfield)
        {
            if ((MarcFields.ContainsKey(Tag)) && (MarcFields[Tag][0].HasSubfield(Subfield)))
                return MarcFields[Tag][0][Subfield];
            return string.Empty;
        }

        public string GetDataSubfield(int Tag, char Subfield, int fieldCount)
        {
            if ((MarcFields.ContainsKey(Tag)) && (MarcFields[Tag][fieldCount].HasSubfield(Subfield)))
                return MarcFields[Tag][fieldCount][Subfield];
            return string.Empty;
        }


        #region Helpers

        public static string DigitsOnly(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            return new String(s.Where(Char.IsDigit).ToArray());
        }

        private string RemoveEndingPunctuation(string s)
        {
            if (s != null)
                while (s.Length > 0 && (Char.IsPunctuation(s[s.Length - 1]) || Char.IsWhiteSpace(s[s.Length - 1])))
                    s = s.Substring(0, s.Length - 1);
            return s;
        }

        private string CleanISBN(string isbn, bool isbn13 = true)
        {
            if (isbn == null)
                return null;

            // Remove everything except digits 
            isbn = DigitsOnly(isbn);

            // Fix if EAN is missing leading 9
            if (isbn.Length >= 11 && isbn.Length <= 12 && (isbn.Substring(0, 2) == "78" || isbn.Substring(0, 2) == "79"))
                isbn = "9" + isbn;

            if (ISBNisEAN13(isbn))
            {
                // ISBN appears to be EAN 13 (or 12 digits without check digit)
                if (isbn13 || isbn[2] == '9')
                {
                    // Return EAN 13 if requested or if 979 prefix, which must be EAN 13
                    isbn = isbn.Substring(0, 12);
                    return isbn + EAN13CheckDigit(isbn);
                }
                else
                {
                    // Return 10-digit ISBN
                    isbn = isbn.Substring(3, 9); // Remove 978 or 979 prefix and check digit
                    return isbn + ISBN10CheckDigit(isbn);
                }
            }
            else if (isbn.Length >= 9)
            {
                // ISBN is 10-digits (or 9 wihtout check digit)
                if (isbn13)
                {
                    // Return EAN if requested (must be 978 prefix when coming from legacy ISBN)
                    isbn = "978" + isbn.Substring(0, 9);
                    return isbn + EAN13CheckDigit(isbn);
                }
                else
                {
                    // Return legacy 10-digit ISBN
                    isbn = isbn.Substring(0, 9);
                    return isbn + ISBN10CheckDigit(isbn);
                }
            }
            else
                return string.Empty;
        }

        private bool ISBNisEAN13(string isbn, bool checkDigitOptional = true)
        {
            // Must be 13 digits and begin with 978 or 979
            return (isbn != null && isbn.Length >= 12 && (checkDigitOptional || isbn.Length == 13) && isbn.Substring(0, 2) == "97" && (isbn[2] == '8' || isbn[2] == '9'));
        }

        public char EAN13CheckDigit(string isbn)
        {
            if (!ISBNisEAN13(isbn))
                return ' ';

            int sum = 0;
            for (var i = 1; i <= 12; i++)
            {
                int digit = 0;
                if (!int.TryParse(isbn.Substring(i - 1, 1), out digit))
                    return ' ';
                if (i % 2 == 0)
                    sum += (digit * 3);
                else
                    sum += digit;
            }

            return (10 - (sum % 10)).ToString()[0];
        }

        private char ISBN10CheckDigit(string isbn)
        {
            if (isbn.Length < 9)
                return ' ';

            int sum = 0;
            for (var i = 1; i <= 9; i++)
            {
                int digit = 0;
                if (!int.TryParse(isbn.Substring(i - 1, 1), out digit))
                    return ' ';
                sum += (digit * (11 - i));
            }

            sum = 11 - (sum % 11);
            if (sum == 11)
                return '0';
            else if (sum == 10)
                return 'X';
            else
                return sum.ToString()[0];
        }

        #endregion
    }

    public class MarcField
    {
        #region Constructors

        public MarcField()
        {
            Tag = -1;
            Ind1 = ' ';
            Ind2 = ' ';
        }

        #endregion

        public int Tag { get; set; }

        public char Ind1 { get; set; }

        public char Ind2 { get; set; }

        public List<MarcSubfield> Subfields { get; set; } = new List<MarcSubfield>();

        public string this[char Subfield_Code]
        {
            get
            {
                string returnValue = string.Empty;
                foreach (MarcSubfield subfield in Subfields)
                {
                    if (subfield.Code == Subfield_Code)
                    {
                        if (returnValue.Length == 0)
                            returnValue = subfield.Data;
                        else
                            returnValue = returnValue + "|" + subfield.Data;
                    }
                }
                return returnValue;
            }
        }

        public bool HasSubfield(char Subfield_Code)
        {
            return Subfields.Any(subfield => subfield.Code == Subfield_Code);
        }

    }

    public class MarcSubfield
    {
        public char Code { get; set; }

        public string Data { get; set; }

        public override string ToString()
        {
            return "|" + Code + " " + Data;
        }
    }
}
