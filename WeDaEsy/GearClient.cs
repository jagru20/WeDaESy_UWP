using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace WeDaESy
{
    internal abstract class GearClient
    {
        #region Protected Fields

        protected JObject _dataObj;

        #endregion Protected Fields

        #region Private Fields

        private static bool _lIsConnected;
        private static bool _mIsConnected;
        private static bool _rIsConnected;
        private static bool _uIsConnected;
        private string _vpCode;

        #endregion Private Fields

        #region Public Properties

        public static bool LIsConnected
        {
            set => _lIsConnected = value;
            get => _lIsConnected;
        }

        public static bool MIsConnected
        {
            set => _mIsConnected = value;
            get => _mIsConnected;
        }

        public static bool RIsConnected
        {
            set => _rIsConnected = value;
            get => _rIsConnected;
        }

        public static bool UIsConnected
        {
            set => _uIsConnected = value;
            get => _uIsConnected;
        }

        public string ConnType { get; protected set; } = "bt";

        public JObject DataObj
        {
            get => _dataObj;
            set
            {
                if(_dataObj != value)
                {
                    _dataObj = value;
                }
            }
        }

        public StorageFile LogFile { get; set; } = null;

        public string VpCode
        {
            get => _vpCode;

            set => _vpCode = value;
        }

        public string WriteString { get; set; } = "";

        #endregion Public Properties

        #region Protected Properties

        protected TextBox _dataBox { get; set; }
        protected TextBox _infoBox { get; set; }
        protected CancellationToken Ct { get; set; }
        protected string DataString { get; set; }
        protected Timer DispTimer { get; set; } = null;
        protected MainPage MainPage { get; set; }
        protected bool ReadComplete { get; set; } = true;
        protected int TlxAnswerIndex { get; set; } = 0;
        protected int Watch { get; set; }

        #endregion Protected Properties

        #region Public Constructors

        public GearClient(TextBox infoBox, string vpCodeFromMain, string connectionType)
        {
            //_dataBox = dataBox;
            MainPage = MainPage.Current;
            _infoBox = infoBox;
            LIsConnected = false;
            VpCode = vpCodeFromMain;
            ConnType = connectionType;
            Initialize();
        }

        public GearClient(TextBox infoBox, string vpCodeFromMain, string connectionType, int selectedWatch)
        {
            MainPage = MainPage.Current;
            _infoBox = infoBox;
            LIsConnected = false;
            VpCode = vpCodeFromMain;
            ConnType = connectionType;
            Watch = selectedWatch;
            Initialize();
        }

        public GearClient()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public abstract void Detach();

        public abstract void ReadData();

        public abstract void ReadDataAsync();

        /// <summary>
        /// Publish an arbitrary message as an answer for the current TLX Question.
        /// </summary>
        /// <param name="payload">integer between 0 and 100</param>
        /// <returns></returns>
        public abstract Task SendCustomTLXAsync(int payload);

        public abstract Task SendCustomTLXAsync(double payload);

        public abstract void SendMessageAsync(string message_sendString);

        public abstract void SendMessageAsync(string message_sendString, string alarmMode);
        /// <summary>
        /// Sends settings to the Smartwatch.
        /// </summary>
        /// <param name="setting">Settings Object</param>
        /// <returns></returns>
        public abstract Task SendSettingAsync(JObject setting);
        /// <summary>
        /// Sends Question in form of the Update, i.e. plain information.
        /// </summary>
        /// <param name="status_sendString">information to be sent</param>
        /// <returns>void</returns>
        public abstract void SendUpdateAsync(string status_sendString);
        /// <summary>
        /// Sends Question in form of the Update, i.e. plain information.
        /// </summary>
        /// <param name="status_sendString">information to be sent</param>
        /// <param name="alarmMode">specifies whether the message should occur with tone, vibration, both or none</param>
        /// <returns>void</returns>
        public abstract void SendUpdateAsync(string status_sendString, string alarmMode);
        /// <summary>
        /// Sends Question in form of the Dialog, i.e. with possible answers Yes or No.
        /// </summary>
        /// <param name="request_sendString">question to be asked</param>
        /// <returns>void</returns>
        public abstract Task<bool> StartDialog(string request_sendString);
        /// <summary>
        /// Sends Question in form of the Dialog, i.e. with possible answers Yes or No.
        /// </summary>
        /// <param name="request_sendString">question to be asked</param>
        /// <param name="alarmMode">specifies whether the message should occur with tone, vibration, both or none</param>
        /// <returns>answer as bool (true = yes)</returns>
        public abstract Task<bool> StartDialog(string request_sendString, string alarmMode);
        /// <summary>
        /// Starts a survey of the given type, waits for completion and returns its answers.
        /// </summary>
        /// <param name="answerType">answer to the asked question</param>
        /// <param name="question">question string</param>
        /// <returns></returns>
        public abstract Task<string> StartSurvey(int answerType, string question);
        /// <summary>
        /// Starts a survey of the given type, waits for completion and returns the answers.
        /// </summary>
        /// <param name="answerType">answers to several questions</param>
        /// <returns></returns>
        public abstract Task<string[]> StartSurvey(int answerType);

        #endregion Public Methods

        #region Protected Methods

        protected string DataToString()
        {
            /*
             * Als timestamp wird in allen Logdateien der Zeitpunkt genutzt,
             * an dem das JSON Objekt in eine Datenzeile konvertiert wurde.
             */
            MainPage.Timestamp = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture) + ";";

            WriteString = "";
            if(DataObj != null && DataObj.Count > 0)
            {
                //MainPage.LastKnownWatchTime[DataObj.GetValue("WatchID").Value<int>()] = new System.DateTime();
                MainPage.LastKnownWatchTime[DataObj.GetValue("WatchID").Value<int>() - 1] = System.DateTime.ParseExact(DataObj.GetValue("timestamp").Value<string>(), "dd.MM.yyyy HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
                var TimeOffset = System.DateTime.Now - MainPage.LastKnownWatchTime[DataObj.GetValue("WatchID").Value<int>() - 1];
                WriteString += VpCode + ";";
                WriteString += DataObj.GetValue("WatchID").Value<string>() + ";";
                WriteString += DataObj.GetValue("Number").Value<string>() + ";";
                WriteString += (System.DateTime.ParseExact(DataObj.GetValue("timestamp").Value<string>(), "dd.MM.yyyy HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture) + TimeOffset).ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture) + ";"; //DataObj.GetValue("timestamp").Value<string>()+";";
                WriteString += DataObj.GetValue("heartRate").Value<string>() + ";";
                //WriteString += DataObj.GetValue("rRInterval").Value<string>() + ";";
                //WriteString += DataObj.GetValue("hrLightType").Value<string>() + ";";
                WriteString += DataObj.GetValue("hr_Intensity").Value<string>() + ";";
                WriteString += DataObj.GetValue("stepStatus").Value<string>() + ";";
                WriteString += DataObj.GetValue("curSpeed").Value<string>() + ";";
                WriteString += DataObj.GetValue("stepFreq").Value<string>() + ";";
                WriteString += DataObj.GetValue("dist").Value<string>() + ";";
                WriteString += DataObj.GetValue("cals").Value<string>() + ";";
                WriteString += DataObj.GetValue("stepDiff").Value<string>() + ";";
                WriteString += DataObj.GetValue("stepDiffW").Value<string>() + ";";
                WriteString += DataObj.GetValue("stepDiffR").Value<string>() + ";";
                WriteString += DataObj.GetValue("light").Value<string>() + ";";
                WriteString += DataObj.GetValue("pressure").Value<string>() + ";";
                WriteString += DataObj.GetValue("gravity_x").Value<string>() + ";";
                WriteString += DataObj.GetValue("gravity_y").Value<string>() + ";";
                WriteString += DataObj.GetValue("gravity_z").Value<string>() + ";";
                WriteString += DataObj.GetValue("gyro_x").Value<string>() + ";";
                WriteString += DataObj.GetValue("gyro_y").Value<string>() + ";";
                WriteString += DataObj.GetValue("gyro_z").Value<string>() + ";";
                //WriteString += DataObj.GetValue("noise").Value<string>() + ";";
            }
            else
            {
                //writeString = "Warte auf Daten...";
            }
            //Debug.WriteLine(writeString);
            return WriteString;
        }

        /// <summary>
        /// liest Daten aus dem Streamtream und speichert sie als JSON.
        /// Danach wird das JSON Object in eine log-file Zeile konvertiert (mit DataToString()) und in die logfile geschrieben.
        /// </summary>
        protected abstract void GetData();

        protected abstract void GetData(string input);

        protected abstract Task<bool> GetReqAnswer();

        protected abstract Task<string> GetSurveyAnswerAsync();

        protected abstract Task<string[]> GetSurveyAnswerAsync(int checkInterval);

        protected abstract void Initialize();

        protected abstract void SendRequestAsync(string request_sendString);

        protected abstract void SendRequestAsync(string status_sendString, string alarmMode);

        /// <summary>
        /// Sendet zunächst den gewünschten Antworttyp (1-6) und anschließend die Frage.
        /// Wenn nur der answerType angegeben wird, muss dieser 6 sein. daraufhin wird der TLX gestartet.
        /// </summary>
        /// <param name="answerType">Antworttyp 1-6</param>
        protected abstract void SendSurveyAsync(int answerType);
        /// <summary>
        /// Sendet zunächst den gewünschten Antworttyp (1-6) und anschließend die Frage.
        /// Wenn nur der answerType angegeben wird, muss dieser 6 sein. daraufhin wird der TLX gestartet.
        /// </summary>
        /// <param name="answerType">Antworttyp 1-6</param>
        /// <param name="question">Frage aus dem Feld txt_survey_question</param>
        protected abstract void SendSurveyAsync(int answerType, string question);

        #endregion Protected Methods
    }
}