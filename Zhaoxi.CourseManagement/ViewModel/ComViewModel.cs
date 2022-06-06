using LiveCharts;
using LiveCharts.Configurations;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataMonitoringSystem.Common;
using DataMonitoringSystem.DataAccess;
using DataMonitoringSystem.Model;
using MQTTnet.Client.Options;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Configuration;

namespace DataMonitoringSystem.ViewModel
{
    public class ComViewModel : NotifyBase
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

        public CourseModel CourseModel { get; set; } = new CourseModel();
        public SensorModel SensorModel { get; set; } = new SensorModel();


        public CommandBase SaveCommand { get; set; }
        public CommandBase BeginCommand { get; set; }
        public CommandBase StopCommand { get; set; }
        public ObservableCollection<CategoryItemModel> CategoryTeacher { get; set; }
        List<double> Voltage;
        List<double> Electricity;
        List<double> Speed;
        List<double> AccSpeed;
        public ComViewModel()
        {
            this.SaveCommand = new CommandBase();
            this.SaveCommand.DoExecute = new Action<object>(DoSave);
            this.SaveCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });

            this.BeginCommand = new CommandBase();
            this.BeginCommand.DoExecute = new Action<object>(DoBegin);
            this.BeginCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });

            this.StopCommand = new CommandBase();
            this.StopCommand.DoExecute = new Action<object>(DoStop);
            this.StopCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });

            this.InitCategory();

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
        }
        private void DoBegin(object o)
        {
            Voltage = new List<double>();
            Electricity = new List<double>();
            Speed = new List<double>();
            AccSpeed = new List<double>();

            InitSerialPort();
        }
        private void DoStop(object o)
        {
            var avgVoltage = Voltage.Average();
            var varVoltage = Voltage.Sum(x => Math.Pow(x - avgVoltage, 2)) / Voltage.Count();
            var avgElectricity = Electricity.Average();
            var varElectricity = Electricity.Sum(x => Math.Pow(x - avgElectricity, 2)) / Electricity.Count();
            var avgSpeed = Speed.Average();
            var varSpeed = Speed.Sum(x => Math.Pow(x - avgSpeed, 2)) / Speed.Count();
            var avgAccSpeed = AccSpeed.Average();
            var varAccSpeed = AccSpeed.Sum(x => Math.Pow(x - avgAccSpeed, 2)) / AccSpeed.Count();

            SensorModel.VoltageAvg = avgVoltage.ToString("0.0000");
            SensorModel.VoltageVar = varVoltage.ToString("0.0000");
            SensorModel.ElectricityAvg = avgElectricity.ToString("0.0000");
            SensorModel.ElectricityVar = varElectricity.ToString("0.0000");
            SensorModel.SpeedAvg = avgSpeed.ToString("0.0000");
            SensorModel.SpeedVar = varSpeed.ToString("0.0000");
            SensorModel.AccSpeedAvg = avgAccSpeed.ToString("0.0000");
            SensorModel.AccSpeedVar = varAccSpeed.ToString("0.0000");
        }
        private void DoSave(object o)
        {
            CategoryItemModel categoryItemModel = CategoryTeacher.Where(x => x.IsSelected == true).FirstOrDefault();
            string teacherID = categoryItemModel.UserID;
            string coursesID = Guid.NewGuid().ToString().Split('-')[4].ToString();
            SensorModel.CoursesID = coursesID;
            CourseModel.CourseID = coursesID;
            if (string.IsNullOrEmpty(CourseModel.CourseName) || string.IsNullOrEmpty(CourseModel.Description))
            {
                MessageBox.Show("请填写完整信息！");
                return;
            }
            LocalDataAccess.GetInstance().AddCourse(CourseModel);
            LocalDataAccess.GetInstance().AddSensor(SensorModel);
            LocalDataAccess.GetInstance().AddCourseTearcherRealation(coursesID, teacherID);
            LocalDataAccess.GetInstance().AddPlayRecord(coursesID);
        }
        private void InitCategory()
        {
            this.CategoryTeacher = new ObservableCollection<CategoryItemModel>();
            foreach (var item in LocalDataAccess.GetInstance().GetTeachers())
                this.CategoryTeacher.Add(item);
            this.CategoryTeacher[0].IsSelected = true;
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
                AddSensorData(ChartValue1, ChartValue2, ChartValue3, ChartValue4);

            }
            catch (Exception ex)
            {

            }
        }
        public void AddSensorData(float v1, float v2, float v3, float v4)
        {
            Voltage.Add(v1);
            Electricity.Add(v2);
            Speed.Add(v3);
            AccSpeed.Add(v4);
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
                if (null != serialPortUtil)
                {
                    serialPortUtil.ClosePort();
                }
            }
            catch { }
        }
        #endregion

    }
}
