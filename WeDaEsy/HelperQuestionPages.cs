using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage;

namespace WeDaESy
{
    ///<summary>
    ///
    ///</summary>
    public static class DataTableExtensions
    {
        #region Public Methods

        ///<summary>
        ///
        ///</summary>
        public static string ToCsv(this DataTable dataTable)
        {
            StringBuilder sbData = new StringBuilder();

            // Only return Null if there is no structure.
            if(dataTable.Columns.Count == 0)
                return null;

            foreach(var col in dataTable.Columns)
            {
                if(col == null)
                    sbData.Append(",");
                else
                    sbData.Append("\"" + col.ToString().Replace("\"", "\"\"") + "\",");
            }

            sbData.Replace(",", Environment.NewLine, sbData.Length - 1, 1);

            foreach(DataRow dr in dataTable.Rows)
            {
                foreach(var column in dr.ItemArray)
                {
                    if(column == null)
                        sbData.Append(",");
                    else
                        sbData.Append("\"" + column.ToString().Replace("\"", "\"\"") + "\",");
                }
                sbData.Replace(",", Environment.NewLine, sbData.Length - 1, 1);
            }

            return sbData.ToString();
        }

        #endregion Public Methods
    }

    internal class HelperQuestionPages
    {


        #region Public Methods

        /// <summary>
        /// Logs the given results to the corresponding logfile.
        /// If the needed File exists, Data is Appended. Otherwise file is created.
        /// Use for questionnaires whose items load onto the same construct.
        /// </summary>
        /// <param name="results">Array of Item-responses, including the average as first item</param>
        /// <param name="name">Name of questionnaire conducted</param>
        public static async void LogResults(double[] results, string name)
        {
            string LogfilenameQuest = "log_" + name + ".txt";
            string event_header_current = "vp_code; timecode; event_typeQ1; scoreQ1; item1Q1; item2Q1; item3Q1; item4Q1; item5Q1; item6Q1; info1; info2; info3\n";/* event_typeQ2; scoreQ2; item1Q2; item2Q2; item3Q2; item4Q2; item5Q2; item6Q2; item10Q2*/
            string[] BasicInfo = new string[3] { MainPage.VPCode, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture), name };
            string[] printData = new string[BasicInfo.Length + results.Length];
            BasicInfo.CopyTo(printData, 0);
            string[] StringResults = results.Select(x => x.ToString()).ToArray();
            StringResults.CopyTo(printData, BasicInfo.Length);
            string printLine = string.Join(";", printData) + Environment.NewLine;
            Debug.WriteLine(printLine);
            if(!File.Exists(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuest))
            {
                if(event_header_current != "") printLine = event_header_current + printLine; // Wenn neue Datei, dann Header schreiben
            }
            StorageFile QuestionFile = await MainPage.Current.LocalFolder.CreateFileAsync("\\log\\" + LogfilenameQuest, CreationCollisionOption.OpenIfExists);
            try
            {
                await FileIO.AppendTextAsync(QuestionFile, printLine);
            }
            catch(Exception)
            {
            }
        }

        /// <summary>
        /// Logs the given results to the corresponding logfile.
        /// If the needed File exists, Data is Appended. Otherwise file is created.
        /// Use for questionnaires whose items load onto the same construct.
        /// </summary>
        /// <param name="results">Array of Item-responses, including the average as first item</param>
        /// <param name="name">Name of questionnaire conducted</param>
        /// <param name="info">Array to contain up to 6 additional info strings</param>
        public static async void LogResults(double[] results, string name, string[] info)
        {
            string LogfilenameQuest = "log_" + name + ".txt";
            string event_header_current = "vp_code; timecode; event_typeQ1; scoreQ1; item1Q1; item2Q1; item3Q1; item4Q1; item5Q1; item6Q1; info1; info2; info3; info4; info5; info6\n";/* event_typeQ2; scoreQ2; item1Q2; item2Q2; item3Q2; item4Q2; item5Q2; item6Q2; item10Q2*/
            string[] BasicInfo = new string[3] { MainPage.VPCode, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture), name };
            string[] printData = new string[BasicInfo.Length + results.Length + info.Length];
            BasicInfo.CopyTo(printData, 0);
            string[] StringResults = results.Select(x => x.ToString()).ToArray();
            StringResults.CopyTo(printData, BasicInfo.Length);
            info.CopyTo(printData, BasicInfo.Length + StringResults.Length);
            string printLine = string.Join(";", printData) + Environment.NewLine;
            Debug.WriteLine(printLine);
            if (!File.Exists(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuest))
            {
                if (event_header_current != "") printLine = event_header_current + printLine; // Wenn neue Datei, dann Header schreiben
            }
            StorageFile QuestionFile = await MainPage.Current.LocalFolder.CreateFileAsync("\\log\\" + LogfilenameQuest, CreationCollisionOption.OpenIfExists);
            try
            {
                await FileIO.AppendTextAsync(QuestionFile, printLine);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Logs the given results to the corresponding logfile in JSON Format.
        /// If the needed File exists, Data is Appended to the existing JSON Object. Otherwise file is created.
        /// Use for Questionnaires whose items score on different psychological constructs.
        /// </summary>
        /// <param name="results">JSON-Object containing the answers to the items to be logged</param>
        /// <param name="name">Name of the Questionnaire conducted</param>
        public static async void LogResults(JObject results, string name)
        {
            string LogfilenameQuest = "log_" + name + ".json";
            string LogfilenameQuestCSV = "log_" + name + ".csv";
            JArray fileResultsArray;

            retry:
            try
            {
                string fileContent = File.ReadAllText(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuest);
                fileResultsArray = JArray.Parse(fileContent);
                fileResultsArray.Add(results);
                File.WriteAllText(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuest, fileResultsArray.ToString());

                //Additionally write to csv
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(fileResultsArray.ToString(), (typeof(DataTable)));
                string csvString = dt.ToCsv();
                File.WriteAllText(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuestCSV, csvString);
            }
            catch(FileNotFoundException)
            {
                StorageFile QuestionFile = await MainPage.Current.LocalFolder.CreateFileAsync("\\log\\" + LogfilenameQuest, CreationCollisionOption.OpenIfExists);
                File.WriteAllText(QuestionFile.Path, "[]");
                goto retry;
            }
            catch(JsonReaderException)
            {
                await Dialogs.DisplayInfoBoxAsync("Fehler beim Auslesen der Logdatei.\n" +
                    "Möglicherweise enthält sie kein gültiges JSON-Array (mehr).\n" +
                    "Die Antworten konnten nicht gespeichert werden.");
            }
            catch(Exception ex)
            {
                await Dialogs.DisplayInfoBoxAsync(ex.Message);
            }
        }
        /// <summary>
        /// Logs the given results to the corresponding logfile in JSON Format.
        /// If the needed File exists, Data is Appended to the existing JSON Object. Otherwise file is created.
        /// Use for Questionnaires whose items score on different psychological constructs.
        /// </summary>
        /// <param name="results"><see cref="SurveyResultsObject"/> containing the answers to the items to be logged</param>
        /// <param name="name">Name of the Questionnaire conducted</param>
        public static async void LogResults(SurveyResultsObject results, string name)
        {
            JObject Results = (JObject)JToken.FromObject(results);
            string LogfilenameQuest = "log_" + name + ".json";
            string LogfilenameQuestCSV = "log_" + name + ".csv";
            JArray fileResultsArray; 

            retry:
            try
            {
                //TODO check if file contains a JSON Object with current vp number and update it
                string fileContent = File.ReadAllText(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuest);
                fileResultsArray = JArray.Parse(fileContent);
                fileResultsArray.Add(Results);
                File.WriteAllText(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuest, fileResultsArray.ToString());

                //Additionally write to csv
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(fileResultsArray.ToString(), (typeof(DataTable)));
                string csvString = dt.ToCsv();
                File.WriteAllText(MainPage.Current.LocalFolder.Path + "\\log\\" + LogfilenameQuestCSV, csvString);
            }
            catch(FileNotFoundException)
            {
                StorageFile QuestionFile = await MainPage.Current.LocalFolder.CreateFileAsync("\\log\\" + LogfilenameQuest, CreationCollisionOption.OpenIfExists);
                File.WriteAllText(QuestionFile.Path, "[]");
                goto retry;
            }
            catch(JsonReaderException)
            {
                await Dialogs.DisplayInfoBoxAsync("Fehler beim Auslesen der Logdatei.\n" +
                    "Möglicherweise enthält sie kein gültiges JSON-Array (mehr).\n" +
                    "Die Antworten konnten nicht gespeichert werden.");
            }
            catch(Exception ex)
            {
                await Dialogs.DisplayInfoBoxAsync(ex.Message);
            }
        }

        #endregion Public Methods
    }
}