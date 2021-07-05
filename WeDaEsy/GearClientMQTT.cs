using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client; 
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace WeDaESy
{
    internal sealed class GearClientMQTT : GearClient
    {
        #region Private Fields

        private string _lastSurveyAnswer = null;
        private string _reqAnswer = null;
        private int CorruptCounter = 0;
        private bool DataCorruptFlag;

        #endregion Private Fields

        #region Public Properties

        public string _brokerIP { get; private set; }
        public string[] _currentAffectiveAnswers { get; private set; }
        public bool AsCompleteFlag { get; private set; } = false;
        public bool IsReading { get; private set; }
        public string LastSurveyAnswer
        {
            get
            {
                string temp = _lastSurveyAnswer;
                _lastSurveyAnswer = null;
                return temp;
            }
            set => _lastSurveyAnswer = value;
        }

        public IMqttClient MqttCommClient { get; private set; }
        public MqttFactory MqttFactory { get; private set; }
        public IMqttClient MqttLogClient { get; private set; }
        public string ReqAnswer
        {
            get
            {
                string temp = _reqAnswer;
                _reqAnswer = null;
                return temp;
            }
            private set => _reqAnswer = value;
        }

        public bool TlxCompleteFlag { get; private set; } = false;
        public Timer WaitTimer { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private string[] _currentTLXAnswers { get; set; } = new string[6];
        private IMqttClientOptions CommMQTTOptions { get; set; }
        private IMqttClientOptions LogMQTTOptions { get; set; }
        private string PrevDat { get; set; } = null;

        #endregion Private Properties

        #region Public Constructors

        /// <summary>
        /// infoBox muss hier die TextBox sein, in der die IP-Adresse des MQTT Brokers eingetragen wurde.
        /// </summary>
        public GearClientMQTT(TextBox infoBox, string vpCodeFromMain, string connectionType, string brokerIP)
        {
            Debug.WriteLine("mqtt Konstruktor");
            MainPage = MainPage.Current;
            _infoBox = infoBox;
            LIsConnected = false;
            VpCode = vpCodeFromMain;
            ConnType = connectionType;
            _brokerIP = brokerIP;
            Initialize();
        }

        #endregion Public Constructors

        #region Public Methods

        public override void Detach()
        {
            MqttCommClient.DisconnectAsync();
            MqttLogClient.DisconnectAsync();
        }

        public override void ReadData()
        {
            throw new NotImplementedException();
        }

        public override void ReadDataAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Publish an arbitrary message as an answer for the current TLX Question on the topic "SurveyResp/FakeTLX"
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public override async Task SendCustomTLXAsync(int payload)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("SurveyResp/FakeTLX")
                                .WithPayload(payload.ToString())
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async Task SendCustomTLXAsync(double payload)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("SurveyResp/FakeTLX")
                                .WithPayload(payload.ToString())
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async void SendMessageAsync(string message_sendString)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("Message")
                                .WithPayload(message_sendString)
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async void SendMessageAsync(string message_sendString, string alarmMode)
        {
            var message = new MqttApplicationMessageBuilder()
                                 .WithTopic("Message")
                                 .WithPayload(message_sendString + "\n" + alarmMode)
                                 .WithAtLeastOnceQoS()
                                 .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async Task SendSettingAsync(JObject payload)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("Settings")
                                .WithPayload(payload.ToString())
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async void SendUpdateAsync(string status_sendString)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("Update")
                                .WithPayload(status_sendString)
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async void SendUpdateAsync(string status_sendString, string alarmMode)
        {
            var message = new MqttApplicationMessageBuilder()
                                 .WithTopic("Update")
                                 .WithPayload(status_sendString + "\n" + alarmMode)
                                 .WithAtLeastOnceQoS()
                                 .Build();

            await MqttCommClient.PublishAsync(message);
        }

        public override async Task<bool> StartDialog(string request_sendString, string alarmMode)
        {
            bool res = false;
            SendRequestAsync(request_sendString, alarmMode);
            res = await GetReqAnswer();
            return res;
        }

        public override Task<bool> StartDialog(string request_sendString)
        {
            throw new NotImplementedException();
        }

        public override async Task<string> StartSurvey(int answerType, string question)
        {
            SendSurveyAsync(answerType, question);
            string res = await GetSurveyAnswerAsync();
            return res;
        }

        public override async Task<string[]> StartSurvey(int answerType)
        {
            if(answerType == 6)
            {
                string[] currentTLXAnswers = new string[6];
                SendSurveyAsync(answerType);
                currentTLXAnswers = await GetSurveyAnswerAsync(answerType);
                return currentTLXAnswers;
            }
            else if(answerType == 7)
            {
                _currentAffectiveAnswers = new string[2];
                SendSurveyAsync(answerType);
                _currentAffectiveAnswers = await GetSurveyAnswerAsync(answerType);
                return _currentAffectiveAnswers;
            }
            else
            {
                string[] res = { "Error", "" };
                return res;
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void GetData(string input)
        {
            string curDat = null;
            DataCorruptFlag = false;
            if(MqttLogClient.IsConnected)
            {
                curDat = input;
            }
            if(curDat != null)
            {
                int index = curDat.LastIndexOf("}");
                if(index > 0)
                { curDat = curDat.Substring(0, index + 1); }

                try
                {
                    DataObj = JObject.Parse(curDat);
                }
                catch(Exception e)
                {
                    if(e.HResult == -2146233088)
                    {
                        DataCorruptFlag = true;
                    }
                }
                if(LogFile != null && MainPage.DataWriteLog == true && !DataCorruptFlag && curDat != PrevDat)
                {
                    _ = FileIO.AppendTextAsync(LogFile, DataToString() + "\n");
                    MainPage.RecordsSaved++;
                    PrevDat = curDat;
                }
                else if(LogFile != null && MainPage.DataWriteLog == true)
                {
                    CorruptCounter++;
                    DataCorruptFlag = false;
                }
                if(MainPage.DBLogEnabled == true)
                {
                    MainPage.Save_stream_db_web();
                }
            }
        }

        protected override void GetData()
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> GetReqAnswer()
        {
            Again3:
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            string curReqAnswer = ReqAnswer;
            if(curReqAnswer == "true")
            {
                return true;
            }
            else if(curReqAnswer == "false")
            {
                return false;
            }
            else
            {
                goto Again3;
            }
        }

        protected override async Task<string[]> GetSurveyAnswerAsync(int answerType)
        {
            if(answerType == 6)
            {
                Again1:
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if(TlxCompleteFlag == true)
                {
                    TlxCompleteFlag = false;
                    return _currentTLXAnswers;
                }
                else { goto Again1; }
            }
            else if(answerType == 7)
            {
                Again2:
                await Task.Delay(TimeSpan.FromMilliseconds(400));
                if(AsCompleteFlag == true)
                {
                    AsCompleteFlag = false;
                    return _currentAffectiveAnswers;
                }
                else { goto Again2; }
            }
            else throw new Exception("Falscher answerType");
        }

        protected override async Task<string> GetSurveyAnswerAsync()
        {
            Again3:
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            string curSurveyAnswer = LastSurveyAnswer;
            if(curSurveyAnswer != null)
            {
                return curSurveyAnswer;
            }
            else
            {
                goto Again3;
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected override async void Initialize()
        {
            try
            {
                Debug.WriteLine("Create MQTT Client");
                // Create a new MQTT client.
                MqttFactory = new MqttFactory();
                MqttLogClient = MqttFactory.CreateMqttClient();
                MqttCommClient = MqttFactory.CreateMqttClient();

                LogMQTTOptions = new MqttClientOptionsBuilder()
                                    .WithClientId("LaptopLog")
                                    .WithTcpServer(_brokerIP, 1883)
                                    .WithCredentials("LaptopLog", "LaptopExp32")
                                    .Build();

                CommMQTTOptions = new MqttClientOptionsBuilder()
                                    .WithClientId("Laptop")
                                    .WithTcpServer(_brokerIP, 1884)
                                    .WithCredentials("Laptop", "LaptopExp32")
                                    .Build();

                MqttLogClient.Connected += async (s, e) =>
                 {
                     Debug.WriteLine("### CONNECTED WITH SERVER ###");
                     await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { _infoBox.Text += "Log connected \n"; });
                     // Subscribe to a topic
                     await MqttLogClient.SubscribeAsync("SensorString", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                     LIsConnected = true;

                     Debug.WriteLine("### SUBSCRIBED ###");
                 };
                MqttCommClient.Connected += async (s, e) =>
                {
                    Debug.WriteLine("### CONNECTED WITH SERVER ###");
                    await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { _infoBox.Text += "Comm connected \n"; });

                    // Subscribe to a topic
                    await MqttCommClient.SubscribeAsync("RequestResp", MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                    await MqttCommClient.SubscribeAsync("SurveyResp", MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                    await MqttCommClient.SubscribeAsync("SurveyResp/TLX", MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                    await MqttCommClient.SubscribeAsync("SurveyResp/AS", MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                    UIsConnected = true;
                    RIsConnected = true;
                    MIsConnected = true;
                    Debug.WriteLine("### SUBSCRIBED ###");
                };

                await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate { _infoBox.Text += "Try to connect to " + _brokerIP + ":1883 \n"; try { await MqttCommClient.ConnectAsync(CommMQTTOptions); _infoBox.Text += "Communication MQTT connect \n"; } catch (Exception ex) { _ = Dialogs.DisplayInfoBoxAsync(ex.Message); } });
                await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate { _infoBox.Text += "Try to connect to " + _brokerIP + ":1884 \n"; try { await MqttLogClient.ConnectAsync(LogMQTTOptions); _infoBox.Text += "Logging MQTT connect \n"; } catch (Exception ex) { _ = Dialogs.DisplayInfoBoxAsync(ex.Message); } });
            }
            catch
            {
            }

            
            // Reconnect LogClient if Connection lost
            MqttLogClient.Disconnected += async (s, e) =>
            {
                await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { _infoBox.Text += "Log disconnected \n"; });
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await MqttLogClient.ConnectAsync(LogMQTTOptions);
                }
                catch
                {
                    await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { _infoBox.Text += "### RECONNECTING FAILED ### \n"; });
                }
            };

            // Reconnect CommClient if Connection lost
            MqttCommClient.Disconnected += async (s, e) =>
            {
                await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { _infoBox.Text += "Comm disconnected \n"; });
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await MqttCommClient.ConnectAsync(CommMQTTOptions);
                }
                catch
                {
                    await _infoBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { _infoBox.Text += "### RECONNECTING FAILED ### \n"; });
                }
            };
            MqttLogClient.ApplicationMessageReceived += (s, e) =>
            {
                //Debug.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                //Debug.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                //Debug.WriteLine($"+ Payload = {System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                //Debug.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                //Debug.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                string receivedString = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if(e.ApplicationMessage.Topic == "SensorString")
                { GetData(receivedString); }
            };
            
            // How to behave if message received?
            MqttCommClient.ApplicationMessageReceived += (s, e) =>
        {
            Debug.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Debug.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Debug.WriteLine($"+ Payload = {System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Debug.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Debug.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            string receivedString = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            switch(e.ApplicationMessage.Topic)
            {
                case "RequestResp":
                    ReqAnswer = receivedString;
                    break;

                case "SurveyResp/TLX":
                    TlxCompleteFlag = false;
                    CommClientHandleTLX(receivedString);
                    break;

                case "SurveyResp/AS":
                    string[] recSplit = receivedString.Split(':');
                    if(recSplit[1] == "save")
                    {
                        AsCompleteFlag = true;
                    }
                    else { CommClientHandleAS(receivedString); }
                    break;

                case "SurveyResp":
                    LastSurveyAnswer = receivedString;
                    break;
            }
        };
        }

        protected override async void SendRequestAsync(string request_sendString)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("Request")
                                .WithPayload(request_sendString)
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        protected override async void SendRequestAsync(string request_sendString, string alarmMode)
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("Request")
                                .WithPayload(request_sendString + "\n" + alarmMode)
                                .WithAtLeastOnceQoS()
                                .Build();

            await MqttCommClient.PublishAsync(message);
        }

        protected override async void SendSurveyAsync(int answerType)
        {
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                             .WithTopic("Survey")
                                             .WithPayload(answerType.ToString())
                                             .WithAtLeastOnceQoS()
                                             .Build();

            MqttApplicationMessage messageAct = new MqttApplicationMessageBuilder().WithPayload("Start").WithTopic("Survey").Build();
            await MqttCommClient.PublishAsync(message);
            await MqttCommClient.PublishAsync(messageAct);
        }

        protected override async void SendSurveyAsync(int answerType, string question)
        {
            MqttApplicationMessage surveySelector = new MqttApplicationMessageBuilder()
                                                 .WithTopic("Survey")
                                                 .WithPayload(answerType.ToString())
                                                 .WithAtLeastOnceQoS()
                                                 .Build();

            await MqttCommClient.PublishAsync(surveySelector);
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                            .WithTopic("Survey")
                                            .WithPayload(question)
                                            .WithAtLeastOnceQoS()
                                            .Build();
            await MqttCommClient.PublishAsync(message);
        }

        #endregion Protected Methods

        #region Private Methods

        private void CommClientHandleAS(string payload)
        {
            string[] recSplit = payload.Split(':');
            if(int.Parse(recSplit[0]) < 2)
            {
                _currentAffectiveAnswers[int.Parse(recSplit[0])] = recSplit[1];
            }
        }

        private void CommClientHandleTLX(string payload)
        {
            string[] recSplit = payload.Split(':');
            if(int.Parse(recSplit[0]) < 7)
            {
                _currentTLXAnswers[int.Parse(recSplit[0]) - 1] = recSplit[1];
            }
            if(recSplit[1] == "save")
            {
                TlxCompleteFlag = true;
            }
        }

        #endregion Private Methods
    }
}