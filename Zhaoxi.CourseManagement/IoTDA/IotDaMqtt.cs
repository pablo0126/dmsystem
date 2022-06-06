using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using DataMonitoringSystem.Common;
using System.Windows;

namespace DataMonitoringSystem.IoTDA
{
    public class IotDaMqtt
    {
        private static IManagedMqttClient client = null;

        private static IManagedMqttClientOptions options = null;

        private static readonly ushort DEFAULT_KEEPLIVE = 120;

        private static readonly ushort RECONNECT_TIME = 60000;

        private static readonly ushort DEFAULT_CONNECT_TIMEOUT = 1;

        private long minBackoff = 1000;

        private long maxBackoff = 30 * 1000;

        private long defaultBackoff = 1000;

        private static int retryTimes = 0;

        Random random = new Random();


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

    }
}
