using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace WeDaESy
{
    /// <summary>
    /// App's Mainpage
    /// </summary>
    public sealed partial class MainPage : Page, IDisposable
    {
        #region Public Fields

        ///<summary>
        ///
        ///</summary>
        public static MainPage Current;

        ///<summary>
        ///
        ///</summary>
        public bool config_loaded = false;

        ///<summary>
        ///
        ///</summary>
        public string[] event_data = new string[13];

        ///<summary>
        ///
        ///</summary>
        public StorageFile EventFile;

        ///<summary>
        ///
        ///</summary>
        public int notif_message_pos = 0;

        ///<summary>
        ///
        ///</summary>
        public int notif_request_pos = 0;

        ///<summary>
        ///
        ///</summary>
        public int notif_update_pos = 0;

        ///<summary>
        ///
        ///</summary>
        public string[][] notifications_message;

        ///<summary>
        ///
        ///</summary>
        public string[][] notifications_request;

        ///<summary>
        ///
        ///</summary>
        public string[][] notifications_update;

        #endregion Public Fields

        #region Private Fields

        private static string _VPCode = "";
        private bool _affectiveFlag;
        private string _alarmMode;
        private double _bIT_Current;
        private string _connectionType;
        private bool _usedPwbForCurrentTask;
        private bool _usePwbForNextTask;
        private bool answer;
        private XmlDocument config = new XmlDocument();
        private StorageFile ConfigFile;
        private string eventlog_header = "vp_code; timecode; meas_id; event_type; source1; source2; source3; par1; par2; par3; par4; par5; par6\n";
        private HttpClient httpClient_db = new HttpClient();
        private HttpClient HttpClientDBEvent = new HttpClient();
        private Stopwatch Stopwatch = new Stopwatch();
        private string[] tlxAnswerScales = new string[] { "Mental", "Physical", "Temporal", "Success", "Effort", "Frustration" };

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        ///
        /// </summary>
        public static string VPCode
        {
            set
            {
                if (value != "")
                {
                    _VPCode = value;
                    //write VPCode to txt_local_log and enable logging.
                    Current.txt_local_filename.Text = value + ".txt";
                    Current.toggle_local_logfile.IsOn = true;
                    if (Current.GearClient != null)
                    {
                        Current.GearClient.VpCode = value;
                    }
                }
                else
                {
                    _VPCode = value;
                }
            }
            get => _VPCode;
        }

        /// <summary>
        ///
        /// </summary>
        public bool AffectiveFlag { get => _affectiveFlag; private set => _affectiveFlag = value; }

        /// <summary>
        ///
        /// </summary>
        public string AlarmMode
        {
            get => _alarmMode;
            set
            {
                _alarmMode = value;
                switch (value)
                {
                    case "1":
                        VibrationBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        VibrationBorder.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        ToneBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        ToneBorder.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        break;

                    case "2":
                        VibrationBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                        VibrationBorder.Background = new SolidColorBrush(Color.FromArgb(255, 90, 224, 255));
                        ToneBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        ToneBorder.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        break;

                    case "3":
                        VibrationBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                        VibrationBorder.Background = new SolidColorBrush(Color.FromArgb(255, 90, 224, 255));
                        ToneBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                        ToneBorder.Background = new SolidColorBrush(Color.FromArgb(255, 90, 224, 255));
                        break;

                    case "4":
                        VibrationBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        VibrationBorder.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                        ToneBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                        ToneBorder.Background = new SolidColorBrush(Color.FromArgb(255, 90, 224, 255));
                        break;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public JObject Bfi_Current { get; set; } = null;

        /// <summary>
        ///
        /// </summary>
        public double BIT_Current { get => _bIT_Current; set => _bIT_Current = value; }

        /// <summary>
        ///
        /// </summary>
        public bool ChangeWebDBUploadOK { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public bool ConfigLoaded { get; private set; } = false;

        /// <summary>
        ///
        /// </summary>
        public string ConnectionType
        {
            get => _connectionType;
            set => _connectionType = value;
        }

        /// <summary>
        ///
        /// </summary>
        public string ConnectionType1 { get => _connectionType; set => _connectionType = value; }

        /// <summary>
        ///
        /// </summary>
        public bool DataWriteLog { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int Db_records_saved { get; set; } = 0;

        /// <summary>
        ///
        /// </summary>
        public bool DBConnectionFailed { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public bool DBLogEnabled { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public string EventAusgabe { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public string GenericSep1 { get; set; } = ";";

        /// <summary>
        ///
        /// </summary>
        public DateTime[] LastKnownWatchTime { get; set; } = new DateTime[10];

        /// <summary>
        ///
        /// </summary>
        public string LastSurveyAnswer { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public StorageFolder LocalFolder { get; set; } = ApplicationData.Current.LocalFolder;

        /// <summary>
        ///
        /// </summary>
        public StorageFile LogFile { get; set; } = null;

        /// <summary>
        ///
        /// </summary>
        public StorageFolder LogFolder { get; set; } = null;

        /// <summary>
        ///
        /// </summary>
        public string Message_sendString { get; set; } = "";

        /// <summary>
        ///
        /// </summary>
        public string Name_Logfile { get; set; } = "test.test";

        /// <summary>
        ///
        /// </summary>
        public int RecordsSaved { get; set; } = 0;

        /// <summary>
        ///
        /// </summary>
        public Regex RegDigitsOnly { get; private set; } = new Regex(@"[^\d]");

        /// <summary>
        ///
        /// </summary>
        public string ResultWebDB { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public int SelectedWatch { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public string SurveyType { get; private set; } = "none";

        /// <summary>
        ///
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool TLXIsHandleShown { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public bool TLXUsesFakeStt { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public bool UpdateVisible { get; private set; } = true;

        public bool UsedPwbForCurrentTask { get => _usedPwbForCurrentTask; private set => _usedPwbForCurrentTask = value; }

        public bool UsePwbForNextTask
        {
            get => _usePwbForNextTask;
            private set
            {
                _usePwbForNextTask = value;
                switch (value)
                {
                    case true:
                        Border_Pwb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                        Border_Pwb.Background = new SolidColorBrush(Color.FromArgb(255, 90, 224, 255));
                        break;

                    case false:
                        Border_Pwb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        Border_Pwb.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        Flyout_Subitem_UsePWB.IsChecked = false;
                        break;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int WebDBUploadOK { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private Random Bohrtemp { get; } = new Random();
        private Timer DataDisplayTimer { get; set; } = null;
        private int DBEventsRecordsSaved { get; set; }
        private bool EventFileCreated { get; set; } = false;
        private int EventMeasID { get; set; }
        private bool EventWriteLog { get; set; }
        private int FileEventsRecordsSaved { get; set; }
        private GearClient GearClient { get; set; } = null;

        private string Headline { get; } = "vp_code;WatchId;Nr;timestamp;heartRate;hr_Intensity;" +
                                    "stepStatus;curSpeedKmh;StepsPerSecond;traveledMeters;" +
                                    "burnedCals;TotalStepDiff;WalkedStepDiff;RunStepDiff;" +
                                    "light;pressure;gravity_x;gravity_y;gravity_z;gyro_x;gyro_y;gyro_z;";

        private string Name_EventLogfile { get; set; }
        private string Request_sendString { get; set; } = "";
        private bool RequestEventFlag { get; set; } = false;
        private string Status_body { get; set; } = "";
        private string Status_sendString { get; set; } = "";
        private string Status_title { get; set; } = "";
        private int Status_update { get; set; } = 0;
        private string TaskCode { get; set; } = "";
        private Timer Ticker1 { get; set; }
        private string TlxAnswerMode { get; set; } = "touch";
        private string WlCompValFilePath { get; set; }

        #endregion Private Properties

        #region Protected Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is BitNavArgs bitNavArgs)
            {
                if (VPCode == "")
                {
                    VPCode = bitNavArgs.ObtainedVpCode;
                }

                LastSurveyAnswers.Score_Bit = bitNavArgs.BitScore;
            }
            else if (e.Parameter is Bfi10NavArgs bfi10NavArgs)
            {
                if (VPCode == "")
                {
                    VPCode = bfi10NavArgs.ObtainedVpCode;
                }

                LastSurveyAnswers.Score_Conscientiousness = bfi10NavArgs.ScoreConscientiousness;
                LastSurveyAnswers.Score_Extraversion = bfi10NavArgs.ScoreExtraversion;
                LastSurveyAnswers.Score_Neuroticism = bfi10NavArgs.ScoreNeuroticism;
            }
            else if (e.Parameter is DemographicNavParams demographicNavParams)
            {
                if (VPCode == "")
                {
                    VPCode = demographicNavParams.ObtainedVpCode;
                }
                LastSurveyAnswers.Skill = demographicNavParams.SkillLevel;
            }
            else if (e.Parameter is string passedVpCode)
            {
                if (VPCode == "")
                {
                    VPCode = passedVpCode;
                }
            }
            base.OnNavigatedTo(e);
        }

        #endregion Protected Methods

        #region Public Constructors

        /// <summary>
        ///
        /// </summary>
        public MainPage()
        {
            event_data[0] = Name_Logfile.Split('.')[0] + GenericSep1;
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            Current = this;
            ConnectionType = "mqtt";
            WlCompValFilePath = Current.LocalFolder.Path + @"\Infos\WLScores.xml";
            // check if file with Workload comparison values exists, if yes, read it
            if (File.Exists(WlCompValFilePath))
            {
                GlobalSettings.GlobalWorkloadCompValArray = DeserializeWorkloadCompObject();
            }
            else
            {
                // if not, display error
                _ = Dialogs.DisplayInfoBoxAsync("Please place xml formatted list of workload values for comparison in\n " + WlCompValFilePath);
            }
            GlobalSettings.CurrentTaskId = 1;
            UsePwbForNextTask = false;
            //scr_gesamt.Width = page.ActualWidth;
            //LoadConfig();
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Current.Dispose();
        }

        /// <summary>
        /// Save sensordata to sql db via weblink
        /// </summary>
        public async void Save_stream_db_web()
        {
            // Prepare string for DB: , to ., trim spaces, \N for null values and \n and ; away at the end.
            string db_data_web = GearClient.WriteString.Replace(',', '.').Replace("; ", ";").Replace(";;", ";\\N;").TrimEnd(';', '\n');
            var uri = new Uri(@"https://link-to-db/ins_stream.php?str[]=" + db_data_web);//TODO: read link from file

            // Always catch network exceptions for async methods
            try
            {
                ResultWebDB = await httpClient_db.GetStringAsync(uri);
                Db_records_saved++;
                // Status-flag permanent und Änderung des Status, um nicht jedesmal Farbe im UI-Task zu aktualisieren. Grundsätzlich soll Logging dauernd probiert werden bei Web-Zugriff
                if (WebDBUploadOK == 0)
                {
                    ChangeWebDBUploadOK = true;
                    WebDBUploadOK = 1;
                }
            }
            catch (Exception ex)
            {
                ResultWebDB = ex.Message + " DB-Weblink reachable?";
                if (WebDBUploadOK == 1)
                {
                    ChangeWebDBUploadOK = true;
                    WebDBUploadOK = 0;
                }
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txt_db_records_saved.Text = Db_records_saved.ToString("###,###,##0");
                txt_db_status.Text = ResultWebDB;
                //Einmalig Farbe umsetzen, nur bei Änderung Zustand.
                if (WebDBUploadOK == 1 && ChangeWebDBUploadOK == true)
                {
                    txt_db_status.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    txt_db_records_saved.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    ChangeWebDBUploadOK = false;
                }
                else if (WebDBUploadOK == 0 && ChangeWebDBUploadOK == true)
                {
                    txt_db_status.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                    txt_db_records_saved.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                    ChangeWebDBUploadOK = false;
                }
            });
        }

        /// <summary>
        /// Serialize a WorkloadCompValList
        /// </summary>
        /// <param name="obj">Object to be serialized. must be WorkloadCompValList</param>
        public void SerializeWorkloadCompObject(object obj)
        {
            FileStream stream;
            File.SetAttributes(WlCompValFilePath, System.IO.FileAttributes.Normal);
            stream = new FileStream(WlCompValFilePath, FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(WorkloadCompValArray));
            serializer.Serialize(stream, obj);
            stream.Close();
        }

        /// <summary>
        /// Deserialize WorkloadCompvalList from xml document
        /// </summary>
        /// <returns> <see cref="WorkloadCompValArray"/> containing the "Ground Truth" for every Workload Score obtained in your experiment.
        /// </returns>
        private WorkloadCompValArray DeserializeWorkloadCompObject()
        {
            FileStream stream;
            File.SetAttributes(WlCompValFilePath, System.IO.FileAttributes.Normal);
            stream = new FileStream(WlCompValFilePath, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(WorkloadCompValArray));
            return (WorkloadCompValArray)serializer.Deserialize(stream);
        }

        #endregion Public Methods

        #region Private Methods

        private void AllRadiosFalse()
        {
            radio_survey_percent.IsChecked = false;
            radio_survey_fivePoint.IsChecked = false;
            radio_survey_sevenPoint.IsChecked = false;
            radio_survey_fourPoint.IsChecked = false;
            radio_survey_dichotom.IsChecked = false;
            radio_survey_affective.IsChecked = false;
            radio_survey_sleep.IsChecked = false;
            radio_survey_sleepam.IsChecked = false;
            radio_survey_stress.IsChecked = false;
            radio_survey_satis.IsChecked = false;
            radio_survey_moti.IsChecked = false;
            radio_survey_satis100.IsChecked = false;
        }

        private async void Cont_status_update(object state)
        {
            if (Status_update <= 100)
            {
                Status_update++;
            }
            else Status_update = 0;
            int bohrtemp_zufall = Bohrtemp.Next(60, 100);

            // Weil nicht im UI-Task für Ausgabe
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txt_status_body.Text = string.Format("Bohrungen: {0}%{2}Bohrer: 4mm{2}Bohrertemp: {1}°C", Status_update, bohrtemp_zufall, "\n");
                txt_status_title.Text = "M3 Status Bohren";
                Status_title = txt_status_title.Text;
                Status_body = txt_status_body.Text;
                event_data[3] = "page_update" + GenericSep1;
                event_data[4] = "app" + GenericSep1;
                //event_daten[5] = "status_page" + sep;
                event_data[7] = txt_status_title.Text + GenericSep1;
                event_data[8] = txt_status_body.Text.Replace("\r\n", " ") + GenericSep1;
                Event_display();
            });
            Status_sendString = Status_title + "\n" + Status_body.Replace("\n", "<br>");
            if (GearClient != null && GearClient.UIsConnected == true)
            {
                GearClient.SendUpdateAsync(Status_sendString);
            }
        }

        /// <summary>
        /// Displays the information given in the given JObject
        /// </summary>
        /// <param name="state">Contains the collectd</param>
        private async void DataDisplayTimerCB(JObject state)
        {
            if (state != null)
            {
                if (state.GetValue("heartRate").ToString() != "-3" && state.GetValue("heartRate").ToString() != "0")
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_contact_data.Text = "Kontakt"; });
                }
                else await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_contact_data.Text = "kein Kontakt"; });
                //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_rr_data.Text = state.GetValue("rRInterval").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_hr_data.Text = state.GetValue("heartRate").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_pedometer_data.Text = state.GetValue("stepStatus").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_speed_data.Text = state.GetValue("curSpeed").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_stepFreq_data.Text = state.GetValue("stepFreq").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_distance_data.Text = state.GetValue("dist").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_calories_data.Text = state.GetValue("cals").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_stepDiff_data.Text = state.GetValue("stepDiff").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_stepDiffW_data.Text = state.GetValue("stepDiffW").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_stepDiffR_data.Text = state.GetValue("stepDiffR").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_ambient_light_data.Text = state.GetValue("light").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_barometer_data.Text = state.GetValue("pressure").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_accX_data.Text = "X: " + state.GetValue("gravity_x").Value<string>() + "\n"; });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_accY_data.Text = "Y: " + state.GetValue("gravity_y").Value<string>() + "\n"; });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_accZ_data.Text = "Z: " + state.GetValue("gravity_z").Value<string>(); });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_gyroX_data.Text = "X: " + state.GetValue("gyro_x").Value<string>() + "\n"; });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_gyroY_data.Text = "Y: " + state.GetValue("gyro_y").Value<string>() + "\n"; });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_gyroZ_data.Text = "Z: " + state.GetValue("gyro_z").Value<string>() + "\n"; });
                //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_noise_data.Text = state.GetValue("noise").Value<string>() + "\n"; });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txt_records_saved.Text = RecordsSaved.ToString(); });
            }
        }

        private void DisplaySurveyAnswer()
        {
            txt_survey_answer.Text = LastSurveyAnswer;
            //source2 = Antwortmöglichkeit (%,7,etc.), par1 [7] =  Leer, par2 = Frage, , par3 = Antwort
            event_data[3] = "survey" + GenericSep1;
            event_data[4] = "app" + GenericSep1;
            event_data[5] = SurveyType + GenericSep1;
            event_data[8] = txt_survey_question.Text + GenericSep1;
            event_data[9] = txt_survey_answer.Text + GenericSep1;
            event_data[10] = GlobalSettings.CurrentTaskId.ToString();
            Event_display();
        }

        /// <summary>
        /// Displays the available Survey answers to the Event Log section and triggers their storage in the event_logfile
        /// </summary>
        /// <param name="answerIndex"></param>
        private void DisplaySurveyAnswer(int answerIndex)
        {
            //build the required event_data array
            //source2 = Antwortmöglichkeit, source3 = benutzt STT?, par1 [7] = TaskCode , par2 = entsprechende TLX-Skala, , par3 = Antwort
            event_data[3] = "survey" + GenericSep1;
            event_data[4] = "app" + GenericSep1;
            event_data[5] = SurveyType + GenericSep1;
            if (TLXUsesFakeStt)
            {
                event_data[6] = "fake_stt_used" + GenericSep1;
            }
            if (TLXIsHandleShown)
            {
                event_data[10] = "anchor_shown";
            }
            if (AffectiveFlag)
            {
                event_data[7] = "" + GenericSep1;
                switch (answerIndex)
                {
                    case 0:
                        event_data[8] = "arousal" + GenericSep1;
                        break;

                    case 1:
                        event_data[8] = "pleasure" + GenericSep1;
                        break;
                }
            }
            else
            {
                event_data[7] = TaskCode + GenericSep1;
                event_data[8] = tlxAnswerScales[answerIndex] + GenericSep1;
            }
            event_data[9] = LastSurveyAnswer + GenericSep1;
            event_data[10] = GlobalSettings.CurrentTaskId.ToString();
            Event_display();
        }

        /// <summary>
        /// Displays Event-data within the textbox "txt_eventlog_data"
        /// Calls Event_save() to write the shown data to the event-logfile.
        /// </summary>
        private async void Event_display()
        {
            string ausgabe_block = "";
            string[] teilstring;
            int anz;
            int anfang;

            if (EventWriteLog == true)
            {
                EventMeasID++;
                event_data[1] = Timestamp;
                event_data[2] = EventMeasID.ToString() + GenericSep1;

                for (int i = 1; i != event_data.Length; i++)
                {
                    if (event_data[i] == null) { event_data[i] = GenericSep1; }
                }

                EventAusgabe = string.Concat(event_data) + "\n";
                Debug.WriteLine("event_ausgabe:" + EventAusgabe);
                // Do the following in the UI-Task
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //show latest Line on top

                    teilstring = txt_event_log_data.Text.Split('\n');
                    anz = teilstring.Length;

                    anfang = anz >= 10 ? anz - 10 : 0;

                    ausgabe_block = string.Join(separator: "\n", value: teilstring, startIndex: 0, count: teilstring.Length - anfang);

                    txt_event_log_data.Text = EventAusgabe + ausgabe_block;
                });

                if (EventWriteLog == true) { Event_save(); }
                //if (db_log_enabled == true && event_write_log == true && db_method == 1) { save_event_db(); }
                if (DBLogEnabled == true && EventWriteLog == true /*&& db_method == 2*/) { Save_event_db_web(); }
                Array.Clear(event_data, 1, 12); // array zurücksetzen bis auf vp_code
            }
        }

        /// <summary>
        /// Speichert in Logdatei. Wenn nicht vorhanden, wird sie kommentarlos angelegt mit Header.
        /// Wenn vorhanden, werden Daten immer angehängt
        /// </summary>
        private async void Event_save()
        {
            string event_header_current = "";

            Name_EventLogfile = Name_Logfile.Replace(".", "_events.");

            // Headerdefinition, wenn Datei nicht existiert
            if (!File.Exists(LogFolder.Path + "\\" + Name_EventLogfile)) event_header_current = eventlog_header;

            if (!EventFileCreated)
            {
                EventFile = await LogFolder.CreateFileAsync(Name_EventLogfile, CreationCollisionOption.OpenIfExists);
                if (event_header_current != "") EventAusgabe = event_header_current + EventAusgabe; // Wenn neue Datei, dann Header schreiben
                EventFileCreated = true;
            }

            try
            {
                await FileIO.AppendTextAsync(EventFile, EventAusgabe);
            }
            catch (Exception)
            {
            }
            FileEventsRecordsSaved++;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txt_events_saved_file.Text = FileEventsRecordsSaved.ToString("###,###,##0");
            });
        }

        private async void LoadConfig()
        {
            try
            {
                ConfigFile = await LocalFolder.GetFileAsync("default_config.xml");
                string content = await FileIO.ReadTextAsync(ConfigFile);
                config.LoadXml(content);
                LoadConfigParameter('A');
            }
            catch (Exception ex)
            {
                MessageDialog msgdlg = new MessageDialog("default_config.xml required. Please copy to App-Folder " + LocalFolder.Path + "\n Error: " + ex.Message, "Standard config file not found");
                var res = msgdlg.ShowAsync();
            }
        }

        private void LoadConfigParameter(char order)
        {// Notifications Updates
            XmlNodeList notif_update_node = config.SelectNodes("/root/NotifUpdate/NotifUpdate" + order + "/*");
            notifications_update = new string[notif_update_node.Count][];
            int i = 0;
            foreach (XmlElement notif_update in notif_update_node)
            {
                notifications_update[i] = new string[2];
                notifications_update[i][0] = notif_update.SelectSingleNode("title").InnerText;
                notifications_update[i][1] = notif_update.SelectSingleNode("body").InnerText;
                i++;
            }
            txt_status_title.Text = notifications_update[0][0];
            txt_status_body.Text = notifications_update[0][1];

            // Notifications Messages
            XmlNodeList notif_message_node = config.SelectNodes("/root/notifications_message/*");
            notifications_message = new string[notif_message_node.Count][];
            i = 0;
            foreach (XmlElement notif_message in notif_message_node)
            {
                notifications_message[i] = new string[3];
                notifications_message[i][0] = notif_message.SelectSingleNode("title").InnerText;
                notifications_message[i][1] = notif_message.SelectSingleNode("body").InnerText;
                notifications_message[i][2] = notif_message.SelectSingleNode("alarm").InnerText;
                i++;
            }
            txt_message_title.Text = notifications_message[0][0];
            txt_message_body.Text = notifications_message[0][1];

            // Notifications Requests
            XmlNodeList notif_request_node = config.SelectNodes("/root/notifications_request/*");
            notifications_request = new string[notif_request_node.Count][];
            i = 0;
            foreach (XmlElement notif_request in notif_request_node)
            {
                notifications_request[i] = new string[4];
                notifications_request[i][0] = notif_request.SelectSingleNode("title").InnerText;
                notifications_request[i][1] = notif_request.SelectSingleNode("body").InnerText;
                notifications_request[i][2] = notif_request.SelectSingleNode("cmd_1").InnerText;
                notifications_request[i][3] = notif_request.SelectSingleNode("cmd_2").InnerText;
                i++;
            }
            txt_info_title.Text = notifications_request[0][0];
            txt_info_body.Text = notifications_request[0][1];
            txt_request_button1.Text = notifications_request[0][2];
            txt_request_button2.Text = notifications_request[0][3];

            config_loaded = true;
        }

        private async void Save_event_db_web()
        {
            // Speichert Events in Datenbank über Weblink

            // String für DB vorbereiten: Leerzeichen raus, \N für Null-Werte und \n und ; weg am Ende
            string db_data_web = EventAusgabe.Replace("; ", ";").Replace(";;", ";\\N;").Replace(";;", ";\\N;").TrimEnd('\n', ';');
            var uri = new Uri(@"https://exampleURLToPHPInsertScript/ins_event.php?str[]=" + db_data_web);

            // Always catch network exceptions for async methods
            try
            {
                ResultWebDB = await HttpClientDBEvent.GetStringAsync(uri);
                DBEventsRecordsSaved++;
                // Status-flag permanent und Änderung des Status, um nicht jedesmal Farbe im UI-Task zu aktualisieren. Grundsätzlich soll Logging dauernd probiert werden bei Web-Zugriff
                if (WebDBUploadOK == 0)
                {
                    ChangeWebDBUploadOK = true;
                    WebDBUploadOK = 1;
                }
                Debug.WriteLine("Fehler Event DB: " + ResultWebDB);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler Event DB: " + ResultWebDB);
                ResultWebDB = ex.Message + " DB-Weblink reachable?";

                if (WebDBUploadOK == 1)
                {
                    ChangeWebDBUploadOK = true;
                    WebDBUploadOK = 0;
                }
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txt_db_status.Text = ResultWebDB;
                txt_events_saved_db.Text = DBEventsRecordsSaved.ToString("###,###,##0");
                //Einmalig Farbe umsetzen, nur bei Änderung Zustand.
                if (WebDBUploadOK == 1 && ChangeWebDBUploadOK == true)
                {
                    txt_db_status.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    txt_db_records_saved.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    txt_events_saved_db.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    ChangeWebDBUploadOK = false;
                }
                else if (WebDBUploadOK == 0 && ChangeWebDBUploadOK == true)
                {
                    txt_db_status.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                    txt_db_records_saved.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                    txt_events_saved_db.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                    ChangeWebDBUploadOK = false;
                }
            });
        }

        #region UI Eventhandler

        private void Btn_set_custom_TLX_val_Click(object sender, RoutedEventArgs e)
        {
            if (GearClient != null)
            {
                GearClient.SendCustomTLXAsync(payload: Slider_custom_TLX_val.Value);
            }
        }

        /// <summary>
        /// Starts the NASA-TLX sequence on the Watch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_start_TLX_Click(object sender, RoutedEventArgs e)
        {
            AffectiveFlag = false;
            SurveyType = "TLX";
            string[] tlxAnswerArray = new string[6];
            if (GearClient != null)
            {
                // GearClient.StarSurvey waits for the TLX to be completed, then returns the given answers
                tlxAnswerArray = await GearClient.StartSurvey(6);
                TaskCode = txt_taskCode.Text;
                _ = Task.Factory.StartNew(arr =>
                 {
                     Task saveSurveyEvent = Task.Factory.StartNew(async arr1 =>
                     {
                         for (int i = 0; i < 6; i++)
                         {
                             string[] p = (string[])arr;
                             LastSurveyAnswer = p[i];
                             DisplaySurveyAnswer(i);
                             await Task.Delay(TimeSpan.FromMilliseconds(100));
                         }
                     }, arr);
                     saveSurveyEvent.Wait();
                 }, tlxAnswerArray);
                AllRadiosFalse();
                //Convert string array to int array for calculation
                LastSurveyAnswers.LastTlxAnswers = Array.ConvertAll(tlxAnswerArray, double.Parse);
                // calculate WL Score of current task
                LastSurveyAnswers.Score_Tlx = LastSurveyAnswers.LastTlxAnswers.Average();
                double[] tlxLogArray = new double[7];
                tlxLogArray[0] = LastSurveyAnswers.Score_Tlx;
                for (int i = 1; i < tlxLogArray.Length; i++)
                {
                    tlxLogArray[i] = LastSurveyAnswers.LastTlxAnswers[i - 1];
                }
                string[] logInfos = new string[5];
                logInfos[0] = UsedPwbForCurrentTask ? "PwbConsidered" : "PwbNotConsidered";
                logInfos[1] = TaskFinder.IsHigh(LastSurveyAnswers.Score_Bit, ScaleNames.Scale5) ? "PwbInfluence" : "PwbNoInfluence";
                logInfos[2] = TaskCode;
                logInfos[3] = GlobalSettings.CurrentTaskId.ToString();
                HelperQuestionPages.LogResults(tlxLogArray, "NASATLX", logInfos);
            }
        }

        /// <summary>
        /// Depending on the selected response mode, initiates the sending
        /// of a message via the MQTT topic "Survey",
        /// which triggers the corresponding display on the Smartwatch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_survey_send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (SurveyType)
                {
                    case "none":
                        txt_survey_answer.Text = "Wählen sie einen Antwortmodus";
                        break;

                    case "TLX":
                        txt_survey_answer.Text = "Klicken Sie bitte noch ein Mal auf den gewünschten Antwortmodus";

                        break;

                    case "percent":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(1, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_percent.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "five-point":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(2, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_fivePoint.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "seven-point":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(3, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_sevenPoint.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "four-point":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(4, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_fourPoint.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "dichotom":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(5, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_dichotom.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "affective":
                        if (GearClient != null)
                        {
                            string[] affectiveAnswerArray = new string[2];
                            affectiveAnswerArray = await GearClient.StartSurvey(7);
                            TaskCode = txt_taskCode.Text;
                            AffectiveFlag = true;
                            await Task.Factory.StartNew(arr =>
                             {
                                 Task saveSurveyEvent = Task.Factory.StartNew(async arr1 =>
                                 {
                                     for (int i = 0; i < 2; i++)
                                     {
                                         string[] p = (string[])arr;
                                         LastSurveyAnswer = p[i];
                                         DisplaySurveyAnswer(i);
                                         await Task.Delay(TimeSpan.FromMilliseconds(100));
                                     }
                                     SurveyType = "none";
                                 }, arr);
                                 saveSurveyEvent.Wait();
                             }, affectiveAnswerArray);

                            radio_survey_affective.IsChecked = false;
                        }
                        break;

                    case "stress":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(8, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_stress.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "sleep":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(9, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_sleep.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "sleepam":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(11, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_sleepam.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;

                    case "satis":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(10, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_satis.IsChecked = false;
                            //logging vorbereiten
                            string[] logInfos = new string[5];
                            logInfos[0] = UsedPwbForCurrentTask ? "PwbConsidered" : "PwbNotConsidered";
                            logInfos[1] = TaskFinder.IsHigh(LastSurveyAnswers.Score_Bit, ScaleNames.Scale5) ? "PwbInfluence" : "PwbNoInfluence";
                            logInfos[2] = TaskCode;
                            logInfos[3] = GlobalSettings.CurrentTaskId.ToString();
                            HelperQuestionPages.LogResults(new double[1] { LastSurveyAnswers.Score_Satisfaction }, "Satisfaction", logInfos);
                            SurveyType = "none";
                            if (GlobalSettings.GlobalIsFindNextTask && LastSurveyAnswers.Score_Tlx != 0 && GlobalSettings.GlobalIsExperimentalGroup)
                            {
                                int newId = TaskFinder.GetNextTaskID(lastTaskId: GlobalSettings.CurrentTaskId,
                                    TlxScore: LastSurveyAnswers.Score_Tlx,
                                    satisfactionScore: LastSurveyAnswers.Score_Satisfaction,
                                    conscientiousnessScore: LastSurveyAnswers.Score_Conscientiousness,
                                    neuroticismScore: LastSurveyAnswers.Score_Neuroticism,
                                    extraversionScore: LastSurveyAnswers.Score_Extraversion);
                                GlobalSettings.CurrentTaskId = newId;
                                _ = Dialogs.DisplayInfoBoxAsync("Die neue Aufgabennumer lautet '" + newId + "'.");
                                GearClient.SendUpdateAsync("Neue Aufgabe\nBitte bearbeiten Sie nun Aufgabe Nummer " + newId);
                            }
                            else if (GlobalSettings.GlobalIsFindNextTask && LastSurveyAnswers.Score_Tlx != 0 && !GlobalSettings.GlobalIsExperimentalGroup)
                            {
                                int newId = TaskFinder.GetNextTaskID(lastTaskId: GlobalSettings.CurrentTaskId, skillLevel: LastSurveyAnswers.Skill);
                                GlobalSettings.CurrentTaskId = newId;
                                _ = Dialogs.DisplayInfoBoxAsync("Die neue Aufgabennumer lautet '" + newId + "'.");
                                GearClient.SendUpdateAsync("Neue Aufgabe\nBitte bearbeiten Sie nun Aufgabe Nummer " + newId);
                            }

                            if (UsePwbForNextTask) { UsePwbForNextTask = false; UsedPwbForCurrentTask = true; } else { UsedPwbForCurrentTask = false; }
                        }
                        break;

                    case "satis100":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(12, txt_survey_question.Text);
                            LastSurveyAnswers.Score_Satisfaction = int.Parse(LastSurveyAnswer);
                            DisplaySurveyAnswer();
                            radio_survey_satis100.IsChecked = false;
                            //bereite logging vor
                            string[] logInfos = new string[5];
                            logInfos[0] = UsedPwbForCurrentTask ? "PwbConsidered" : "PwbNotConsidered";
                            logInfos[1] = TaskFinder.IsHigh(LastSurveyAnswers.Score_Bit, ScaleNames.Scale5) ? "PwbInfluence" : "PwbNoInfluence";
                            logInfos[2] = TaskCode;
                            logInfos[3] = GlobalSettings.CurrentTaskId.ToString();
                            HelperQuestionPages.LogResults(new double[1] { LastSurveyAnswers.Score_Satisfaction }, "Satisfaction", logInfos);
                            SurveyType = "none";
                            if (GlobalSettings.GlobalIsFindNextTask && LastSurveyAnswers.Score_Tlx != 0 && GlobalSettings.GlobalIsExperimentalGroup)
                            {
                                int newId = TaskFinder.GetNextTaskID(lastTaskId: GlobalSettings.CurrentTaskId,
                                    TlxScore: LastSurveyAnswers.Score_Tlx,
                                    satisfactionScore: LastSurveyAnswers.Score_Satisfaction,
                                    conscientiousnessScore: LastSurveyAnswers.Score_Conscientiousness,
                                    neuroticismScore: LastSurveyAnswers.Score_Neuroticism,
                                    extraversionScore: LastSurveyAnswers.Score_Extraversion);
                                GlobalSettings.CurrentTaskId = newId;
                                _ = Dialogs.DisplayInfoBoxAsync("Die neue Aufgabennumer lautet '" + newId + "'.");
                                GearClient.SendUpdateAsync("Neue Aufgabe\nBitte bearbeiten Sie nun Aufgabe Nummer  " + newId);
                            }
                            else if (GlobalSettings.GlobalIsFindNextTask && LastSurveyAnswers.Score_Tlx != 0 && !GlobalSettings.GlobalIsExperimentalGroup)
                            {
                                int newId = TaskFinder.GetNextTaskID(lastTaskId: GlobalSettings.CurrentTaskId, skillLevel: LastSurveyAnswers.Skill);
                                GlobalSettings.CurrentTaskId = newId;
                                _ = Dialogs.DisplayInfoBoxAsync("Die neue Aufgabennumer lautet '" + newId + "'.");
                                GearClient.SendUpdateAsync("Neue Aufgabe\nBitte bearbeiten Sie nun Aufgabe Nummer " + newId);
                            }
                            if (UsePwbForNextTask) { UsePwbForNextTask = false; UsedPwbForCurrentTask = true; } else { UsedPwbForCurrentTask = false; }
                        }
                        break;

                    case "moti":
                        if (GearClient != null)
                        {
                            LastSurveyAnswer = await GearClient.StartSurvey(13, txt_survey_question.Text);
                            DisplaySurveyAnswer();
                            radio_survey_satis100.IsChecked = false;
                            SurveyType = "none";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147020579)
                {
                    txt_survey_answer.Text = "Fehler bei der Antwortübertragung, Sie sollten diese Frage wiederholen oder abwarten, ob sie doch noch ankommt.";
                }
                else _ = Dialogs.DisplayInfoBoxAsync(ex.Message);
            }
            AllRadiosFalse();
        }

        private void Chk_cont_update_Checked(object sender, RoutedEventArgs e)
        {
            Ticker1 = new Timer(Cont_status_update, null, 0, 1000);
        }

        private void Chk_cont_update_Unchecked(object sender, RoutedEventArgs e)
        {
            Ticker1.Dispose();
        }

        private void Chk_visible_update_Checked(object sender, RoutedEventArgs e)
        {
            UpdateVisible = true;
        }

        private void Chk_visible_update_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateVisible = false;
        }

        /// <summary>
        /// Opens a Launcher Window pointing to the logfiles.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Cmd_folder_open_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
            StorageFolder localLogFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\log");
            await Windows.System.Launcher.LaunchFolderAsync(localLogFolder);
        }

        private void Cmd_notif_message_left_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cmd_notif_message_right_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cmd_notif_request_left_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cmd_notif_request_right_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cmd_notif_update_left_Click(object sender, RoutedEventArgs e)
        {
            if (notif_update_pos > 0)
            {
                notif_update_pos--;
                txt_status_title.Text = notifications_update[notif_update_pos][0];
                txt_status_body.Text = notifications_update[notif_update_pos][1];
            }
        }

        private void Cmd_notif_update_right_Click(object sender, RoutedEventArgs e)
        {
            if (notif_update_pos < notifications_update.Length - 1)
            {
                notif_update_pos++;
                txt_status_title.Text = notifications_update[notif_update_pos][0];
                txt_status_body.Text = notifications_update[notif_update_pos][1];
            }
        }

        private async void Cmd_send_dialog_Click(object sender, RoutedEventArgs e)
        {
            txt_request_button1.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            txt_request_button2.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            string info_body = txt_info_body.Text.Replace("\r", "<br>");
            Request_sendString = txt_info_title.Text + "\n" + info_body.Replace("\r", "<br>");
            if (GearClient != null && GearClient.RIsConnected == true)
            {
                // loggen, dass request gesendet wurde
                event_data[3] = "dialog_sent" + GenericSep1;
                event_data[4] = "app" + GenericSep1;
                event_data[7] = txt_info_title.Text + GenericSep1;
                event_data[8] = txt_info_body.Text.Replace("\r\n", " ") + GenericSep1;
                event_data[9] = txt_request_button1.Text + GenericSep1;
                event_data[10] = txt_request_button2.Text + GenericSep1;
                Event_display();
                answer = await GearClient.StartDialog(Request_sendString, AlarmMode);
                if (answer == true)
                {
                    txt_request_button1.Background = new SolidColorBrush(Color.FromArgb(100, 0, 200, 0));
                }
                else { txt_request_button2.Background = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0)); }

                // antwort loggen
                event_data[3] = "dialog_answer" + GenericSep1;
                event_data[4] = "gear" + GenericSep1;
                event_data[7] = txt_info_title.Text + GenericSep1;
                event_data[8] = txt_info_body.Text.Replace("\r\n", " ") + GenericSep1;
                string answerStr;
                if (answer == true) { answerStr = txt_request_button1.Text; }
                else
                {
                    answerStr = txt_request_button2.Text;
                }
                event_data[9] = answerStr + GenericSep1;
                Event_display();
                answer = false;
            }
        }

        private void Cmd_send_message_Click(object sender, RoutedEventArgs e)
        {
            Message_sendString = txt_message_title.Text + "\n" + txt_message_body.Text.Replace("\r", "<br>");
            if (GearClient != null && GearClient.MIsConnected == true)
            {
                GearClient.SendMessageAsync(Message_sendString, AlarmMode);
                //Message-Event Loggen
                event_data[3] = "message" + GenericSep1;
                event_data[4] = "app" + GenericSep1;
                event_data[7] = txt_message_title.Text + GenericSep1;
                event_data[8] = txt_message_body.Text.Replace("\r\n", " ") + GenericSep1;
                Event_display();
            }
        }

        private void Cmd_update_status_Click(object sender, RoutedEventArgs e)
        {
            string temp = txt_status_body.Text;
            Status_sendString = txt_status_title.Text + "\n" + temp.Replace("\r", "<br>");
            if (GearClient != null && GearClient.UIsConnected == true)
            {
                if (UpdateVisible)
                {
                    GearClient.SendUpdateAsync(Status_sendString, AlarmMode);
                    event_data[3] = "page_update" + GenericSep1;
                    event_data[4] = "app" + GenericSep1;
                    //event_daten[5] = "status_page" + sep;
                    event_data[7] = txt_status_title.Text + GenericSep1;
                    event_data[8] = txt_status_body.Text.Replace("\r", " ") + GenericSep1;
                    Event_display();
                }
                else
                {
                    event_data[3] = "page_update" + GenericSep1;
                    event_data[4] = "app" + GenericSep1;
                    //event_daten[5] = "status_page" + sep;
                    event_data[7] = txt_status_title.Text + GenericSep1;
                    event_data[8] = txt_status_body.Text.Replace("\r", " ") + GenericSep1;
                    event_data[9] = "invisible" + GenericSep1;
                    Event_display();
                }
            }
        }

        private void Flyout_Calc_Next_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (Flyout_Calc_Next.IsChecked)
            {
                GlobalSettings.GlobalIsFindNextTask = true;
            }
            else
            {
                GlobalSettings.GlobalIsFindNextTask = false;
            }
        }

        private void Flyout_CG_EG_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (Flyout_CG_EG.IsChecked)
            {
                Border_CG_EG.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                Border_CG_EG.Background = new SolidColorBrush(Color.FromArgb(255, 90, 224, 255));
                GlobalSettings.GlobalIsExperimentalGroup = true;
            }
            else
            {
                Border_CG_EG.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                Border_CG_EG.Background = new SolidColorBrush(Color.FromArgb(255, 255, 225, 255));
                GlobalSettings.GlobalIsExperimentalGroup = false;
            }
        }

        private void Flyout_Subitem_UsePWB_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (UsePwbForNextTask)
            {
                UsePwbForNextTask = false;
            }
            else { UsePwbForNextTask = true; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlyoutAlarmNone_Click(object sender, RoutedEventArgs e)
        {
            AlarmMode = "1";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlyoutAlarmTone_Click(object sender, RoutedEventArgs e)
        {
            AlarmMode = "4";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlyoutAlarmToneAndVib_Click(object sender, RoutedEventArgs e)
        {
            AlarmMode = "3";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlyoutAlarmVib_Click(object sender, RoutedEventArgs e)
        {
            AlarmMode = "2";
        }

        private void Gear1ChoiceChosen(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SelectedWatch = 0;
        }

        private void Gear2ChoiceChosen(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SelectedWatch = 1;
        }

        private void Lbl_timer_ms_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lbl_TLX_Value_SelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Navigates to the BFI10 questions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_BFI10_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(BFI10));
        }

        /// <summary>
        /// Navigates to the BIT questions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_BIT_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(BIT));
        }

        /// <summary>
        /// Navigates to the Demographic questions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Demograph_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Demograph));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainPage NewSender = (MainPage)sender;
            Debug.WriteLine(NewSender.ActualWidth);
            //scr_gesamt.Width = page.ActualWidth;
        }

        private void Radio_survey_affective_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "affective";
        }

        private void Radio_survey_dichotom_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "dichotom";
        }

        private void Radio_survey_fivePoint_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "five-point";
        }

        private void Radio_survey_fourPoint_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "four-point";
        }

        private void Radio_survey_moti_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "moti";
            txt_survey_question.Text = "Motivation";
        }

        private void Radio_survey_percent_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "percent";
        }

        private void Radio_survey_satis_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "satis";
            txt_survey_question.Text = "Wie zufrieden sind Sie jetzt gerade?";
        }

        private void Radio_survey_satis100_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "satis100";
            txt_survey_question.Text = "Wie zufrieden sind Sie jetzt gerade?";
        }

        private void Radio_survey_sevenPoint_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "seven-point";
        }

        private void Radio_survey_sleep_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "sleep";
            txt_survey_question.Text = "Wie bewerten Sie die Qualität ihres Schlafes in der letzten Nacht?";
        }

        private void Radio_survey_sleepam_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "sleepam";
            txt_survey_question.Text = "Wie viele Stunden haben Sie letzte Nacht geschlafen?";
        }

        private void Radio_survey_stress_Checked(object sender, RoutedEventArgs e)
        {
            SurveyType = "stress";
            txt_survey_question.Text = "Unter Stress versteht man eine Situation, in der sich eine Person " +
                "angespannt, nervös oder ängstlich fühlt oder nachts nicht schlafen kann, weil die Gedanken kreisen. " +
                "Fühlen Sie diese Art von Stress zurzeit?";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_custom_TLX_val_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Txt_TLX_Value.Text = Slider_custom_TLX_val.Value.ToString();
        }

        private void Toggle_connectiontype_Checked(object sender, RoutedEventArgs e)
        {
            inputIPMQTT.Visibility = Visibility.Collapsed;
            toggle_connectiontype.Content = "Wifi";
            ConnectionType = "wifi";
        }

        private void Toggle_connectiontype_Unchecked(object sender, RoutedEventArgs e)
        {
            inputIPMQTT.Visibility = Visibility.Visible;
            toggle_connectiontype.Content = "MQTT";
            ConnectionType = "mqtt";
        }

        private async void Toggle_db_Toggled(object sender, RoutedEventArgs e)
        {
            if (toggle_db.IsOn == true)
            {
                // Testeintrag in event_Tabelle der DB schreiben um connection zu prüfen
                var time_db_connect = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var uri = new Uri(@"https://example/url/to/phpScript/ins_event.php?str[]=" + event_data[0] + time_db_connect + ";\\N;\\N;\\N;\\N;\\N;\\N;\\N;\\N;\\N;\\N;\\N");

                // Always catch network exceptions for async methods
                try
                {
                    var result = await httpClient_db.GetStringAsync(uri);
                    txt_db_status.Text = result;
                    //txt_db_status.Text = "Web DB-connection successful";
                    txt_db_status.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    txt_db_records_saved.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    //txt_events_saved_db.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
                    DBLogEnabled = true;
                    DBConnectionFailed = false;
                    if (ConfigLoaded == true) { config.SelectSingleNode("/root/database/@status").InnerText = "1"; };
                    // Testeintrag wieder löschen in event_Tabelle
                    var uri_del = new Uri(@"https://example/url/to/phpScript/del_event.php?vp_code=" + event_data[0].TrimEnd(';', ' ') + "&timecode=" + time_db_connect);
                    var result1 = await httpClient_db.GetStringAsync(uri_del);
                }
                catch (Exception ex)
                {
                    DBConnectionFailed = true;
                    toggle_db.IsOn = false;
                    txt_db_status.Text = ex.Message + " DB-Weblink reachable?";
                }
            }
            else
            {
                DBLogEnabled = false;
            }
        }

        /// <summary>
        /// Connects to MQTT Server with IP provided in "inputIPMQTT"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle_gear_connection_Toggled(object sender, RoutedEventArgs e)
        {
            info_Box.Text = "";
            if (ConnectionType == "mqtt" && toggle_gear_connection.IsOn && GearClient == null)
            {
                Debug.WriteLine("Start this mqtt stuff");
                Debug.WriteLine("IP:" + inputIPMQTT.Text);
                GearClient = new GearClientMQTT(info_Box, VPCode, "mqtt", inputIPMQTT.Text);
                if (LogFile != null)
                {
                    GearClient.LogFile = LogFile;
                }
            }
            else if (toggle_gear_connection.IsOn == false && GearClient != null)
            {
                //TODO Does not work properly, repair for convenience
                if (DataDisplayTimer != null) { DataDisplayTimer.Dispose(); }
                GearClient.Detach();
                GearClient = null;
                if (toggle_local_logfile.IsOn)
                {
                    toggle_local_logfile.IsOn = false;
                }
                if (toggle_log_write.IsOn)
                {
                    toggle_log_write.IsOn = false;
                }
            }
        }

        private async void Toggle_local_logfile_Toggled(object sender, RoutedEventArgs e)
        {
            if (toggle_local_logfile.IsOn)
            {
                EventWriteLog = true;
                txt_pfad.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                LogFolder = await LocalFolder.CreateFolderAsync("log", CreationCollisionOption.OpenIfExists);
                try
                {
                    LogFile = await LogFolder.CreateFileAsync(txt_local_filename.Text, CreationCollisionOption.GenerateUniqueName);
                    Name_Logfile = txt_local_filename.Text;
                    await FileIO.AppendTextAsync(LogFile, Headline + "\n");
                    txt_pfad.Text = LogFile.Path;
                    if (GearClient != null)
                    {
                        GearClient.LogFile = LogFile;
                    }
                    if (VPCode != txt_local_filename.Text.TrimEnd('.', 't', 'x', 't'))
                    {
                        VPCode = txt_local_filename.Text.TrimEnd('.', 't', 'x', 't');
                    }
                    event_data[0] = VPCode + GenericSep1;
                    toggle_log_write.IsEnabled = true;
                }
                catch (Exception exeption)
                {
                    if ((uint)exeption.HResult == 0x800700B7)
                    {
                        txt_pfad.Background = new SolidColorBrush(Color.FromArgb(50, 200, 0, 0));
                        txt_pfad.Text = "Datei ist bereits vorhanden. Bitte anderen Dateinamen eingeben.";
                        toggle_local_logfile.IsOn = false;
                    }
                    else
                        txt_pfad.Text = exeption.Message;
                }
            }
            else
            {
                if (GearClient != null)
                {
                    GearClient.LogFile = null;
                    LogFile = null;
                }
                DataWriteLog = false;
                txt_local_filename.Text = "VP_Code.txt";
            }
        }

        private void Toggle_log_write_Toggled(object sender, RoutedEventArgs e)
        {
            if (toggle_log_write.IsOn && GearClient != null)
            {
                DataDisplayTimer = new Timer(TimerCallback => DataDisplayTimerCB(GearClient.DataObj), null, 0, 100);
                if (LogFile != null)
                {
                    DataWriteLog = true;
                }
            }
            else if (toggle_log_write.IsOn == true && GearClient == null)
            {
                txt_status_events.Text = "Toggle connection first";
                toggle_log_write.IsOn = false;
            }
            else if (toggle_log_write.IsOn == false && GearClient != null)
            {
                DataWriteLog = false;
            }
        }

        private void Toggle_TLX_Anchor_Toggled(object sender, RoutedEventArgs e)
        {
            if (Toggle_TLX_Anchor.IsOn && GearClient != null)
            {
                JObject setting = JObject.FromObject(new { variable = "showHandleOnTLX", value = true });
                GearClient.SendSettingAsync(setting);
                txt_taskCode.Text = "anchor_visible";
                TLXIsHandleShown = true;
            }
            else if (!Toggle_TLX_Anchor.IsOn && GearClient != null)
            {
                JObject setting = JObject.FromObject(new { variable = "showHandleOnTLX", value = false });
                GearClient.SendSettingAsync(setting);
                txt_taskCode.Text = "anchor_invisible";
                TLXIsHandleShown = false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle_TLX_Fake_STT_Toggled(object sender, RoutedEventArgs e)
        {
            if (Toggle_TLX_Fake_STT.IsOn && GearClient != null)
            {
                JObject setting = JObject.FromObject(new { variable = "enableFakeSpeechOnTLX", value = true });
                GearClient.SendSettingAsync(setting);
                TLXUsesFakeStt = true;
            }
            else if (!Toggle_TLX_Fake_STT.IsOn && GearClient != null)
            {
                JObject setting = JObject.FromObject(new { variable = "enableFakeSpeechOnTLX", value = false });
                GearClient.SendSettingAsync(setting);
                TLXUsesFakeStt = false;
            }
        }

        private void Txt_local_filename_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Txt_pfad_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_taskCode_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            string CorrectedInput = Regex.Replace(txt_taskCode.Text, @";+", string.Empty);
            txt_taskCode.Text = CorrectedInput;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_taskCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string CorrectedInput = Regex.Replace(txt_taskCode.Text, @";+", string.Empty);
            txt_taskCode.Text = CorrectedInput;
        }

        /// <summary>
        ///
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_TLX_Value_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            string CorrectedInput = RegDigitsOnly.Replace(Txt_TLX_Value.Text, string.Empty);
            Txt_TLX_Value.Text = CorrectedInput;
            if (e.Key == Windows.System.VirtualKey.Enter && int.TryParse(s: CorrectedInput, style: NumberStyles.Integer, provider: CultureInfo.CurrentCulture, result: out int TLXVal))
            {
                Slider_custom_TLX_val.Value = TLXVal;
                if (GearClient != null)
                {
                    GearClient.SendCustomTLXAsync(TLXVal);
                }
            }
        }

        #endregion UI Eventhandler

        #endregion Private Methods
    }
}