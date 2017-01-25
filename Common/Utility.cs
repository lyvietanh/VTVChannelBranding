using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Xml;

namespace Common
{
    public static class Utility
    {
        public static string ConvertCompositeToPrecomposed(string compositeString)
        {
            string precomposedString = "";
            if (string.IsNullOrEmpty(compositeString) == false)
            {
                precomposedString = compositeString.Normalize(NormalizationForm.FormC);
            }
            return precomposedString;
        }

        public static string ConvertUTF16ToUTF8(string utf16String)
        {
            if (string.IsNullOrEmpty(utf16String)) return string.Empty;
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
            return Encoding.UTF8.GetString(utf8Bytes);
        }

        public static string ConvertTextToUpper(string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                return text.Trim().ToUpper();
            }
            return text;
        }

        public static string TrimText(string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                return text.Trim();
            }
            return text;
        }

        public static string ConvertToValidString(string text, bool enableAsciiCharacter, string allowUnicodeCharacters, string fixedString)
        {
            string result = "";

            if (!string.IsNullOrEmpty(text))
            {
                text = text.ToLower().Trim();
                for (int i = 0; i < text.Length; i++)
                {

                    bool isValid = false;

                    //kiểm tra trong bangr ascii hoặc trong dãy allowUnicodeCharacters
                    if (string.IsNullOrEmpty(allowUnicodeCharacters))
                    {
                        isValid = (int)text[i] >= 32 && (int)text[i] <= 127;
                    }
                    else
                    {
                        allowUnicodeCharacters = allowUnicodeCharacters.ToLower();
                        isValid = ((int)text[i] >= 32 && (int)text[i] <= 127) || allowUnicodeCharacters.Contains(text[i]);
                    }

                    if (isValid)
                    {
                        result += text[i];
                    }
                    else
                    {
                        result += fixedString;
                    }
                }


            }

            return result;
        }

        public static bool IsValidStringLength(string text, int maximumLength)
        {
            return string.IsNullOrEmpty(text) || text.Trim().Length <= maximumLength;
        }

        public static bool WordTextContainsInPercent(string text1, string text2, int equalOrMoreThanPercents)
        {
            try
            {
                text1 = text1.ToUpper().Trim();
                text2 = text2.ToUpper().Trim();
                int trueCount = 0;
                string[] t2 = text1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (t2 != null && t2.Length > 0)
                {
                    for (int i = 0; i < t2.Length; i++)
                    {
                        if (text1.Contains(t2[i]))
                        {
                            trueCount++;
                        }
                    }
                    int percents = (trueCount / t2.Length) * 100;
                    return (percents >= equalOrMoreThanPercents);
                }
            }
            catch (Exception ex) { }
            return false;
        }

        public static bool TextContainsInPercent(string text1, string text2, int equalOrMoreThanPercents)
        {
            try
            {
                text1 = text1.ToUpper().Trim();
                text2 = text2.ToUpper().Trim();

                int minCount = text1.Length <= text2.Length ? text1.Length : text2.Length;
                int maxCount = text1.Length >= text2.Length ? text1.Length : text2.Length;
                int trueCount = 0;
                for (int i = 0; i < minCount; i++)
                {
                    if (text1[i] == text2[i])
                    {
                        trueCount++;
                    }
                }
                int percents = (trueCount / maxCount) * 100;
                return (percents >= equalOrMoreThanPercents);
            }
            catch (Exception ex) { }
            return false;
        }

        public static TimeSpan RoundMinutesInTimeSpan(TimeSpan timeSpan, int roundNumber)
        {
            //TimeSpan ts = timeSpan;
            //int surplus = ts.Minutes % roundNumber;
            //if (surplus > 0 && surplus < 3)
            //{
            //    ts = ts - TimeSpan.FromMinutes(surplus);
            //}
            //else if (surplus >= 3)
            //{
            //    ts = ts + TimeSpan.FromMinutes(5 - surplus);
            //}
            //return ts;
            TimeSpan ts = timeSpan;
            int surplus = ts.Minutes % 5;
            if (surplus > 0 && surplus < roundNumber)
            {
                if (roundNumber - surplus >= 3)
                {
                    ts = ts - TimeSpan.FromMinutes(surplus);
                }
                else
                {
                    ts = ts + TimeSpan.FromMinutes(roundNumber - surplus);
                }
            }
            else if (surplus > roundNumber)
            {
                ts = ts + TimeSpan.FromMinutes(surplus - roundNumber);
            }
            return ts;
        }

        public static DateTime GetDateFromString(string dateString, string seperator)
        {
            if (!string.IsNullOrEmpty(dateString) && !string.IsNullOrEmpty(seperator))
            {
                int day = 0;
                int month = 0;
                int year = 0;
                string[] dates = dateString.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
                if (dates != null && dates.Length >= 3)
                {
                    int.TryParse(dates[0], out day);
                    int.TryParse(dates[1], out month);
                    int.TryParse(dates[2], out year);
                }
                DateTime dt = new DateTime(year, month, day);
                return dt;
            }
            return DateTime.Now;
        }

        public static TimeSpan GetTimeSpanFromString(string timeString)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            if (!string.IsNullOrEmpty(timeString))
            {
                string[] times = timeString.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (times != null)
                {
                    if (times.Length == 2)
                    {
                        int.TryParse(times[0], out minutes);
                        int.TryParse(times[1], out seconds);
                    }
                    else if (times.Length >= 3)
                    {
                        int.TryParse(times[0], out hours);
                        int.TryParse(times[1], out minutes);
                        int.TryParse(times[2], out seconds);
                    }
                }
            }
            TimeSpan ts = new TimeSpan(hours, minutes, seconds);
            return ts;
        }

        public static string GetTimeStringFromTimeFrame(int timeFrame)
        {
            int seconds = timeFrame / 25;
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            int hours = time.Hours;
            int minutes = time.Minutes;
            return hours.ToString("00") + ":" + minutes.ToString("00") + ":00";
        }

        public static string GetTimeStringFromTimeSpan(TimeSpan timeSpan)
        {
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        public static string GetTimeAndFrameStringFromTimeSpan(TimeSpan timeSpan)
        {
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00") + ":00";
        }

        //public static string GetFixedTimeStringFromTimeSpan(TimeSpan timeSpan) {
        //    int hours = timeSpan.Hours;
        //    int minutes = timeSpan.Minutes;
        //    int seconds = timeSpan.Seconds;
        //    return hours.ToString("00") + ":-" + minutes.ToString("00") + ":-" + seconds.ToString("00");
        //}

        public static XmlElement CreateNewXmlElement(XmlDocument doc, string nodeName, string attributeName, string attributeValue)
        {
            if (doc != null)
            {
                XmlElement xmlElement = doc.CreateElement(nodeName);
                xmlElement.SetAttribute(attributeName, attributeValue);
                return xmlElement;
            }
            return null;
        }

        public static XmlElement CreateNewXmlElement(XmlDocument doc, string nodeName, string[] attributeNameArray, string[] attributeValueArray)
        {
            if (doc != null)
            {
                XmlElement xmlElement = doc.CreateElement(nodeName);
                for (int i = 0; i < attributeNameArray.Length; i++)
                {
                    try
                    {
                        xmlElement.SetAttribute(attributeNameArray[i], attributeValueArray[i]);
                    }
                    catch (System.Exception ex)
                    {
                        xmlElement.SetAttribute(attributeNameArray[i], "");
                    }
                }
                return xmlElement;
            }
            return null;
        }

        public static DataSet GetDataSetFromExcelFile(string filePath, string sheetName)
        {
            DataSet ds = new DataSet();
            //Provider String Extended properties:
            //"Excel 12.0": Use Excel (xls or xlsx) as source
            //"Header Yes": Header is included in the Excel sheet
            //"IMEX=1": When reading from the excel sheet ignore datatypes and read all data in the sheet.
            //Without setting IMEX=0, the excel reader looks for the datatype in the excel sheet.
            //The code below in comments was used for reading xls file
            //OleDbConnection con = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"");
            // The code below is the one i am using for reading an xlsx file
            OleDbConnection conn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;MAXSCANROWS=0\"");
            conn.Open();
            try
            {
                //Create Dataset and fill with imformation from the Excel Spreadsheet for easier reference
                OleDbDataAdapter myCommand = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "$]", conn);
                myCommand.Fill(ds);
            }
            catch (Exception ex)
            {
                string exce = ex.Message;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
    }
}
