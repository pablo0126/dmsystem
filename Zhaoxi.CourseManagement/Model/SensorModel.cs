using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMonitoringSystem.Common;

namespace DataMonitoringSystem.Model
{
    public class SensorModel : NotifyBase
    {
        private int _sensorID;
        public int SensorID
        {
            get { return _sensorID; }
            set
            {
                _sensorID = value;
                this.DoNotify();
            }
        }
        private string _coursesID;
        public string CoursesID
        {
            get { return _coursesID; }
            set
            {
                _coursesID = value;
                this.DoNotify();
            }
        }
        private string _voltageAvg;
        public string VoltageAvg
        {
            get { return _voltageAvg; }
            set
            {
                _voltageAvg = value;
                this.DoNotify();
            }
        }
        private string _voltageVar;
        public string VoltageVar
        {
            get { return _voltageVar; }
            set
            {
                _voltageVar = value;
                this.DoNotify();
            }
        }
        private string _electricityAvg;
        public string ElectricityAvg
        {
            get { return _electricityAvg; }
            set
            {
                _electricityAvg = value;
                this.DoNotify();
            }
        }
        private string _electricityVar;
        public string ElectricityVar
        {
            get { return _electricityVar; }
            set
            {
                _electricityVar = value;
                this.DoNotify();
            }
        }
        private string _speedAvg;
        public string SpeedAvg
        {
            get { return _speedAvg; }
            set
            {
                _speedAvg = value;
                this.DoNotify();
            }
        }
        private string _speedVar;
        public string SpeedVar
        {
            get { return _speedVar; }
            set
            {
                _speedVar = value;
                this.DoNotify();
            }
        }
        private string _accSpeedAvg;
        public string AccSpeedAvg
        {
            get { return _accSpeedAvg; }
            set
            {
                _accSpeedAvg = value;
                this.DoNotify();
            }
        }
        private string _accSpeedVar;
        public string AccSpeedVar
        {
            get { return _accSpeedVar; }
            set
            {
                _accSpeedVar = value;
                this.DoNotify();
            }
        }

    }
}
