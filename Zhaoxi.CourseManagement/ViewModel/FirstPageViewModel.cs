using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataMonitoringSystem.Common;
using DataMonitoringSystem.DataAccess;
using DataMonitoringSystem.Model;

using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace DataMonitoringSystem.ViewModel
{
    public class FirstPageViewModel : NotifyBase
    {
        private int _instrumentValue = 0;

        public int InstrumentValue
        {
            get { return _instrumentValue; }
            set { _instrumentValue = value; this.DoNotify(); }
        }

        private int _itemCount;

        public int ItemCount
        {
            get { return _itemCount; }
            set { _itemCount = value; this.DoNotify(); }
        }

        public ObservableCollection<CourseSeriesModel> CourseSeriesList { get; set; } = new ObservableCollection<CourseSeriesModel>();


        Random random = new Random();
        bool taskSwitch = true;
        List<Task> taskList = new List<Task>();
        public FirstPageViewModel()
        {
            this.RefreshInstrumentValue();

            this.InitCourseSeries();

            InitSerialPort();

            // 设置图表的XY和数值对应
            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.Index)
                .Y(model => model.Value);
            Charting.For<MeasureModel>(mapper);
            ChartValues1 = new ChartValues<MeasureModel>();
            ChartValues2 = new ChartValues<MeasureModel>();
            ChartValues3 = new ChartValues<MeasureModel>();
            ChartValues4 = new ChartValues<MeasureModel>();

            ChartValue1 = 0;
            ChartValue2 = 0;
            ChartValue3 = 0;
            ChartValue4 = 0;
            Index = index++;

            //Task.Factory.StartNew(RecordData);


            SetDefaultProperty();//设置界面的默认值
            //Task.Run(async () => { await ConnectMqttServerAsync(); });
            ConnectMqttServerAsync();
            Thread.Sleep(3000);
            Subscribe();
            

        }

        #region 串口
        SerialPortUtil serialPortUtil;
        private void InitSerialPort()
        {
            if (null != serialPortUtil)
            {
                if (serialPortUtil.IsOpen)
                {
                    serialPortUtil.ClosePort();
                }
            }
            string portName = ConfigurationManager.AppSettings["port"];
            serialPortUtil = new SerialPortUtil(portName, "9600", "0", "8", "1");
            serialPortUtil.DataReceived += SerialPortUtil_DataReceived;
            serialPortUtil.OpenPort();
        }
        private void SerialPortUtil_DataReceived(string data)
        {
            string temp = data.Replace("\r\n", "");
            RecordData(temp);
            Console.WriteLine(data);
        }
        public void RecordData(string data)
        {
            try
            {
                var dataArry = data.Split(' ');
                // 更新图表数据
                ChartValue1 = float.Parse(dataArry[3].ToString());
                ChartValue2 = float.Parse(dataArry[4].ToString());
                ChartValue3 = float.Parse(dataArry[5].ToString());
                ChartValue4 = float.Parse(dataArry[6].ToString());
                Index = index++;

                //向云平台发送数据
                string strJson = "{\"services\":[{\"properties\":{\"voltage\":" + ChartValue1 + ",\"acceleratex\":" + ChartValue2 + ",\"acceleratey\":" + ChartValue3 + ",\"acceleratez\":" + ChartValue4 + "},\"service_id\":\"smokeDetector\",\"event_time\":null}]}";
                Publish(strJson);

            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region 图表

        private int index = 0;
        private void RecordData()
        {
            // 持续生成随机数，模拟数据处理过程
            while (true)
            {
                Thread.Sleep(500);
                var r = new Random();
                float value1 = r.Next(-100, 100);
                float value2 = r.Next(1, 10);
                float value3 = r.Next(50, 100);
                float value4 = r.Next(-50, 10);
                // 更新图表数据
                ChartValue1 = value1;
                ChartValue2 = value2;
                ChartValue3 = value3;
                ChartValue4 = value4;
                Index = index++;
            }
        }
        private int _index;
        public ChartValues<MeasureModel> ChartValues1 { get; set; }
        public ChartValues<MeasureModel> ChartValues2 { get; set; }
        public ChartValues<MeasureModel> ChartValues3 { get; set; }
        public ChartValues<MeasureModel> ChartValues4 { get; set; }
        public float ChartValue1 { get; set; }
        public float ChartValue2 { get; set; }
        public float ChartValue3 { get; set; }
        public float ChartValue4 { get; set; }

        // 当数值被更改时，触发更新
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                Read();
            }
        }



        // 更新图表
        private void Read()
        {
            ChartValues1.Add(new MeasureModel
            {
                Index = this.Index,
                Value = this.ChartValue1
            });
            ChartValues2.Add(new MeasureModel
            {
                Index = this.Index,
                Value = this.ChartValue2
            });
            ChartValues3.Add(new MeasureModel
            {
                Index = this.Index,
                Value = this.ChartValue3
            });
            ChartValues4.Add(new MeasureModel
            {
                Index = this.Index,
                Value = this.ChartValue4
            });

            // 限定图表最多只有十五个元素
            if (ChartValues1.Count > 15)
            {
                ChartValues1.RemoveAt(0);
                ChartValues2.RemoveAt(0);
                ChartValues3.RemoveAt(0);
                ChartValues4.RemoveAt(0);
            }
        }

        private void InitCourseSeries()
        {
            var cList = LocalDataAccess.GetInstance().GetCoursePlayRecord();
            this.ItemCount = cList.Max(c => c.SeriesList.Count);
            foreach (var item in cList)
                this.CourseSeriesList.Add(item);
        }
        private void RefreshInstrumentValue()
        {
            var task = Task.Factory.StartNew(new Action(async () =>
            {
                while (taskSwitch)
                {
                    InstrumentValue = random.Next(Math.Max(this.InstrumentValue - 5, -10), Math.Min(this.InstrumentValue + 5, 90));

                    await Task.Delay(1000);
                }
            }));
            taskList.Add(task);
        }

        public void Dispose()
        {
            try
            {
                taskSwitch = false;
                Task.WaitAll(this.taskList.ToArray());
                serialPortUtil.ClosePort();
                Disconnect();
            }
            catch { }
        }
        #endregion

        #region IotDa物联网

        private static IManagedMqttClient client = null;

        private static IManagedMqttClientOptions options = null;

        private static readonly ushort DEFAULT_KEEPLIVE = 120;

        private static readonly ushort RECONNECT_TIME = 60000;

        private static readonly ushort DEFAULT_CONNECT_TIMEOUT = 1;

        private long minBackoff = 1000;

        private long maxBackoff = 30 * 1000;

        private long defaultBackoff = 1000;

        private static int retryTimes = 0;

        //Random random = new Random();


        bool cbSSLConnect = false;
        int cbOosSelect = 0;

        string txtServerUri;
        string txtDeviceId;
        string txtDeviceSecret;
        string txtSubTopic;
        string txtPubTopic;
        private void SetDefaultProperty()
        {
            txtServerUri = ConfigurationManager.AppSettings["serverUri"];
            txtDeviceId = ConfigurationManager.AppSettings["deviceId"];
            txtDeviceSecret = ConfigurationManager.AppSettings["deviceSecret"];

            txtSubTopic = string.Format("$oc/devices/{0}/sys/commands/#", txtDeviceId);

            txtPubTopic = string.Format("$oc/devices/{0}/sys/properties/report", txtDeviceId);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <returns></returns>
        private async Task ConnectMqttServerAsync()
        {
            try
            {
                int portIsSsl = int.Parse(ConfigurationManager.AppSettings["portIsSsl"]);
                int portNotSsl = int.Parse(ConfigurationManager.AppSettings["portNotSsl"]);

                if (client == null)
                {
                    client = new MqttFactory().CreateManagedMqttClient();
                }

                string timestamp = DateTime.Now.ToString("yyyyMMddHH");
                string clientID = txtDeviceId + "_0_0_" + timestamp;

                // 对密码进行HmacSHA256加密
                string secret = string.Empty;
                if (!string.IsNullOrEmpty(txtDeviceSecret))
                {
                    secret = EncryptUtil.HmacSHA256(txtDeviceSecret, timestamp);
                }

                // 判断是否为安全连接
                if (!cbSSLConnect)
                {
                    options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(RECONNECT_TIME))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithTcpServer(txtServerUri, portNotSsl)
                        .WithCommunicationTimeout(TimeSpan.FromSeconds(DEFAULT_CONNECT_TIMEOUT))
                        .WithCredentials(txtDeviceId, secret)
                        .WithClientId(clientID)
                        .WithKeepAlivePeriod(TimeSpan.FromSeconds(DEFAULT_KEEPLIVE))
                        .WithCleanSession(false)
                        .WithProtocolVersion(MqttProtocolVersion.V311)
                        .Build())
                    .Build();
                }
                else
                {
                    string caCertPath = Environment.CurrentDirectory + @"\certificate\rootcert.pem";
                    X509Certificate2 crt = new X509Certificate2(caCertPath);

                    options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(RECONNECT_TIME))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithTcpServer(txtServerUri, portIsSsl)
                        .WithCommunicationTimeout(TimeSpan.FromSeconds(DEFAULT_CONNECT_TIMEOUT))
                        .WithCredentials(txtDeviceId, secret)
                        .WithClientId(clientID)
                        .WithKeepAlivePeriod(TimeSpan.FromSeconds(DEFAULT_KEEPLIVE))
                        .WithCleanSession(false)
                        .WithTls(new MqttClientOptionsBuilderTlsParameters()
                        {
                            AllowUntrustedCertificates = true,
                            UseTls = true,
                            Certificates = new List<X509Certificate> { crt },
                            CertificateValidationHandler = delegate { return true; },
                            IgnoreCertificateChainErrors = false,
                            IgnoreCertificateRevocationErrors = false
                        })
                        .WithProtocolVersion(MqttProtocolVersion.V311)
                        .Build())
                    .Build();
                }

                ShowLogs($"{"try to connect to server " + txtServerUri}{Environment.NewLine}");

                if (client.IsStarted)
                {
                    await client.StopAsync();
                }

                // 注册事件
                client.ApplicationMessageProcessedHandler = new ApplicationMessageProcessedHandlerDelegate(new Action<ApplicationMessageProcessedEventArgs>(ApplicationMessageProcessedHandlerMethod)); // 消息发布回调

                client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(MqttApplicationMessageReceived)); // 命令下发回调

                client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(new Action<MqttClientConnectedEventArgs>(OnMqttClientConnected)); // 连接成功回调

                client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(new Action<MqttClientDisconnectedEventArgs>(OnMqttClientDisconnected)); // 连接断开回调

                // 连接平台设备
                await client.StartAsync(options);

            }
            catch (Exception ex)
            {
                ShowLogs($"connect to mqtt server fail" + Environment.NewLine);
            }
        }

        private void Disconnect()
        {
            minBackoff = 1000;

            maxBackoff = 30 * 1000;

            defaultBackoff = 1000;

            retryTimes = 0;

            Task.Run(async () => { await DisconnectMqttServerAsync(); });
        }

        private async Task DisconnectMqttServerAsync()
        {
            //断开连接
            await client.StopAsync();
        }

        /// <summary>
        /// 消息发布回调
        /// </summary>
        /// <param name="e"></param>
        private void ApplicationMessageProcessedHandlerMethod(ApplicationMessageProcessedEventArgs e)
        {
            try
            {
                if (e.HasFailed)
                {
                    ShowLogs("publish messageId is " + e.ApplicationMessage.Id + ", topic: " + e.ApplicationMessage.ApplicationMessage.Topic + ", payload: " + Encoding.UTF8.GetString(e.ApplicationMessage.ApplicationMessage.Payload) + " is published fail");
                }
                else if (e.HasSucceeded)
                {
                    ShowLogs("publish messageId " + e.ApplicationMessage.Id + ", topic: " + e.ApplicationMessage.ApplicationMessage.Topic + ", payload: " + Encoding.UTF8.GetString(e.ApplicationMessage.ApplicationMessage.Payload) + " is published success");
                }
            }
            catch (Exception ex)
            {
                ShowLogs("mqtt demo message publish error: " + ex.Message + Environment.NewLine);
            }
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MqttApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            ShowLogs($"received message is {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}{Environment.NewLine}");

            string msg = "{\"result_code\": 0,\"response_name\": \"COMMAND_RESPONSE\",\"paras\": {\"result\": \"success\"}}";

            string topic = "$oc/devices/" + txtDeviceId + "/sys/commands/response/request_id=" + e.ApplicationMessage.Topic.Split('=')[1];

            ShowLogs($"{"response message msg = " + msg}{Environment.NewLine}");

            var appMsg = new MqttApplicationMessage();
            appMsg.Payload = Encoding.UTF8.GetBytes(msg);
            appMsg.Topic = topic;
            appMsg.QualityOfServiceLevel = cbOosSelect == 0 ? MqttQualityOfServiceLevel.AtMostOnce : MqttQualityOfServiceLevel.AtLeastOnce;
            appMsg.Retain = false;

            // 上行响应
            client.PublishAsync(appMsg).Wait();
        }

        /// <summary>
        /// 服务器连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMqttClientConnected(MqttClientConnectedEventArgs e)
        {
            ShowLogs("connect to mqtt server success, deviceId is " + txtDeviceId + Environment.NewLine);
        }

        /// <summary>
        /// 断开服务器连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMqttClientDisconnected(MqttClientDisconnectedEventArgs e)
        {
            try
            {
                ShowLogs("mqtt server is disconnected" + Environment.NewLine);

                //if (cbReconnect.Checked)
                //{
                //    ShowLogs("reconnect is starting" + Environment.NewLine);

                //    //退避重连
                //    int lowBound = (int)(defaultBackoff * 0.8);
                //    int highBound = (int)(defaultBackoff * 1.2);
                //    long randomBackOff = random.Next(highBound - lowBound);
                //    long backOffWithJitter = (int)(Math.Pow(2.0, retryTimes)) * (randomBackOff + lowBound);
                //    long waitTImeUtilNextRetry = (int)(minBackoff + backOffWithJitter) > maxBackoff ? maxBackoff : (minBackoff + backOffWithJitter);

                //    ShowLogs("next retry time: " + waitTImeUtilNextRetry + Environment.NewLine);

                //    Thread.Sleep((int)waitTImeUtilNextRetry);

                //    retryTimes++;

                //    Task.Run(async () => { await ConnectMqttServerAsync(); });
                //}
            }
            catch (Exception ex)
            {
                ShowLogs("mqtt demo error: " + ex.Message + Environment.NewLine);
            }
        }

        private void ShowLogs(string msg)
        {
            Console.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), msg));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="msg"></param>
        private void Publish(string msg)
        {
            string topic = txtPubTopic;

            ShowLogs("publish message topic = " + topic + Environment.NewLine);

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("发布主题不能为空");
                return;
            }

            string inputString = msg;

            var appMsg = new MqttApplicationMessage();
            appMsg.Payload = Encoding.UTF8.GetBytes(inputString);
            appMsg.Topic = topic;
            appMsg.QualityOfServiceLevel = cbOosSelect == 0 ? MqttQualityOfServiceLevel.AtMostOnce : MqttQualityOfServiceLevel.AtLeastOnce;
            appMsg.Retain = false;

            // 上行响应
            client.PublishAsync(appMsg).Wait();
        }

        private void Subscribe()
        {
            string topic = txtSubTopic;

            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("订阅主题不能为空！");
                return;
            }

            if (!client.IsConnected)
            {
                MessageBox.Show("MQTT客户端尚未连接！");
                return;
            }

            List<MqttTopicFilter> listTopic = new List<MqttTopicFilter>();

            var topicFilterBulderPreTopic = new MqttTopicFilterBuilder().WithTopic(topic).Build();
            listTopic.Add(topicFilterBulderPreTopic);

            // 订阅Topic
            client.SubscribeAsync(listTopic.ToArray()).Wait();

            ShowLogs($"topic : [{topic}] is subscribe success" + Environment.NewLine);

        }

        #endregion

    }
    public class MeasureModel
    {
        public int Index { get; set; }
        public float Value { get; set; }
    }

}
