using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMonitoringSystem.Common
{
    /// <summary>
    /// 串口开发辅助类
    /// </summary>
    public class SerialPortUtil
    {
        /// <summary>
        /// 接收事件是否有效 false表示有效
        /// </summary>
        public bool ReceiveEventFlag = false;
        /// <summary>
        /// 结束符比特
        /// </summary>
        public byte EndByte = 0x23;//string End = "#";
        /// <summary>
        /// 完整协议的记录处理事件
        /// </summary>
        public Action<string> DataReceived;
        public event SerialErrorReceivedEventHandler ErrorReceived;
        public event ErrorMsgEventHandler ErrorMsg;
        #region 变量属性
        private SerialPort comPort = new SerialPort();
        /// <summary>
        /// 串口号
        /// </summary>
        public string PortName
        {
            get { return comPort.PortName; }
            set { comPort.PortName = value; }
        }
        /// <summary>
        /// 波特率
        /// </summary>
        public SerialPortBaudRates BaudRate
        {
            get { return (SerialPortBaudRates)comPort.BaudRate; }
            set { comPort.BaudRate = (int)value; }
        }
        /// <summary>
        /// 奇偶校验位
        /// </summary>
        public Parity Parity
        {
            get { return comPort.Parity; }
            set { comPort.Parity = value; }
        }
        /// <summary>
        /// 数据位
        /// </summary>
        public SerialPortDatabits DataBits
        {
            get { return (SerialPortDatabits)comPort.DataBits; }
            set { comPort.DataBits = (int)value; }
        }
        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits
        {
            get { return comPort.StopBits; }
            set { comPort.StopBits = value; }
        }
        #endregion
        #region 构造函数
        /// <summary>
        /// 参数构造函数（使用枚举参数构造）
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        /// <param name="name">串口号</param>
        public SerialPortUtil(string name, SerialPortBaudRates baud, Parity par, SerialPortDatabits dBits, StopBits sBits)
        {
            PortName = name;
            BaudRate = baud;
            Parity = par;
            DataBits = dBits;
            StopBits = sBits;
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }
        /// <summary>
        /// 参数构造函数（使用字符串参数构造）
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        /// <param name="name">串口号</param>
        public SerialPortUtil(string name, string baud, string par, string dBits, string sBits)
        {
            PortName = name;
            BaudRate = (SerialPortBaudRates)Enum.Parse(typeof(SerialPortBaudRates), baud);
            Parity = (Parity)Enum.Parse(typeof(Parity), par);
            DataBits = (SerialPortDatabits)Enum.Parse(typeof(SerialPortDatabits), dBits);
            StopBits = (StopBits)Enum.Parse(typeof(StopBits), sBits);
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SerialPortUtil()
        {
            PortName = "COM1";
            BaudRate = SerialPortBaudRates.BaudRate_9600;
            Parity = Parity.None;
            DataBits = SerialPortDatabits.EightBits;
            StopBits = StopBits.One;
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }
        #endregion
        /// <summary>
        /// 端口是否已经打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return comPort.IsOpen;
            }
        }
        /// <summary>
        /// 打开端口
        /// </summary>
        /// <returns></returns>
        public bool OpenPort()
        {
            try
            {
                if (comPort.IsOpen) comPort.Close();
                comPort.Open();
            }
            catch (Exception ex)
            {
                ErrorMsg(ex.Message);
                return false;
            }

            return true;
        }
        /// <summary>
        /// 关闭端口
        /// </summary>
        public void ClosePort()
        {
            if (comPort.IsOpen) comPort.Close();
        }
        /// <summary>
        /// 丢弃来自串行驱动程序的接收和发送缓冲区的数据
        /// </summary>
        private void DiscardBuffer()
        {
            if (!comPort.IsOpen)
                return;
            comPort.DiscardInBuffer();
            comPort.DiscardOutBuffer();
        }
        /// <summary>
        /// 释放端口数据并关闭端口
        /// </summary>
        public void Dispose()
        {
            DiscardBuffer();
            ClosePort();
        }
        /// <summary>
        /// 检查端口名称是否存在
        /// </summary>
        /// <param name="port_name"></param>
        /// <returns></returns>
        private bool Exists()
        {
            foreach (string port in SerialPort.GetPortNames()) if (port == comPort.PortName) return true;
            return false;
        }
        /// <summary>
        /// 数据接收处理
        /// </summary>
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //禁止接收事件时直接退出
            if (ReceiveEventFlag) return;
            #region 根据结束字节来判断是否全部获取完成
            List<byte> _byteData = new List<byte>();
            //bool found = false;//是否检测到结束符号
            while (comPort.BytesToRead > 0)
            {
                byte[] readBuffer = new byte[comPort.ReadBufferSize + 1];
                int count = comPort.Read(readBuffer, 0, comPort.ReadBufferSize);
                for (int i = 0; i < count; i++)
                {
                    _byteData.Add(readBuffer[i]);
                    //if (readBuffer[i] == EndByte)
                    //{
                    //    found = true;
                    //}
                }
            }
            #endregion
            //字符转换
            string readString = System.Text.Encoding.Default.GetString(_byteData.ToArray(), 0, _byteData.Count);
            //触发整条记录的处理
            if (DataReceived != null)
            {
                DataReceived(readString);
            }
        }
        /// <summary>
        /// 错误处理函数
        /// </summary>
        void comPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (ErrorReceived != null)
            {
                ErrorReceived(sender, e);
            }
        }
        #region 数据写入操作
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg"></param>
        public void WriteData(string msg)
        {
            if (!(comPort.IsOpen))
            {
                if (!OpenPort())
                {
                    return;
                }
            }
            comPort.Write(msg);
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">写入端口的字节数组</param>
        public void WriteData(byte[] msg)
        {
            if (!(comPort.IsOpen))
            {
                if (!OpenPort())
                {

                    return;
                }
            }
            comPort.Write(msg, 0, msg.Length);
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">包含要写入端口的字节数组</param>
        /// <param name="offset">参数从0字节开始的字节偏移量</param>
        /// <param name="count">要写入的字节数</param>
        public void WriteData(byte[] msg, int offset, int count)
        {
            if (!(comPort.IsOpen))
            {
                if (!OpenPort())
                {

                    return;
                }
            }
            comPort.Write(msg, offset, count);
        }
        /// <summary>
        /// 发送串口命令
        /// </summary>
        /// <param name="SendData">发送数据</param>
        /// <param name="ReceiveData">接收数据</param>
        /// <param name="Overtime">重复次数</param>
        /// <returns></returns>
        public int SendCommand(byte[] SendData, ref byte[] ReceiveData, int Overtime)
        {
            if (!(comPort.IsOpen))
            {
                if (!OpenPort())
                {
                    return -1;
                }
            }
            ReceiveEventFlag = true;        //关闭接收事件
            comPort.DiscardInBuffer();      //清空接收缓冲区                
            comPort.Write(SendData, 0, SendData.Length);
            int num = 0, ret = 0;
            while (num++ < Overtime)
            {
                if (comPort.BytesToRead >= ReceiveData.Length) break;
                System.Threading.Thread.Sleep(1);
            }
            if (comPort.BytesToRead >= ReceiveData.Length)
            {
                ret = comPort.Read(ReceiveData, 0, ReceiveData.Length);
            }
            ReceiveEventFlag = false;       //打开事件
            return ret;
        }
        #endregion
    }
    public class DataReceivedEventArgs : EventArgs
    {
        public string DataReceived;
        public DataReceivedEventArgs(string m_DataReceived)
        {
            this.DataReceived = m_DataReceived;
        }
    }
    public delegate void DataReceivedEventHandler(DataReceivedEventArgs e);
    public delegate void ErrorMsgEventHandler(object msg);
    /// <summary>
    /// 串口数据位列表（5,6,7,8）
    /// </summary>
    public enum SerialPortDatabits : int
    {
        FiveBits = 5,
        SixBits = 6,
        SeventBits = 7,
        EightBits = 8
    }
    /// <summary>
    /// 串口波特率列表。
    /// 75,110,150,300,600,1200,2400,4800,9600,14400,19200,28800,38400,56000,57600,
    /// 115200,128000,230400,256000
    /// </summary>
    public enum SerialPortBaudRates : int
    {
        BaudRate_75 = 75,
        BaudRate_110 = 110,
        BaudRate_150 = 150,
        BaudRate_300 = 300,
        BaudRate_600 = 600,
        BaudRate_1200 = 1200,
        BaudRate_2400 = 2400,
        BaudRate_4800 = 4800,
        BaudRate_9600 = 9600,
        BaudRate_14400 = 14400,
        BaudRate_19200 = 19200,
        BaudRate_28800 = 28800,
        BaudRate_38400 = 38400,
        BaudRate_56000 = 56000,
        BaudRate_57600 = 57600,
        BaudRate_115200 = 115200,
        BaudRate_128000 = 128000,
        BaudRate_230400 = 230400,
        BaudRate_256000 = 256000
    }
}
