using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMonitoringSystem.DataAccess;
using DataMonitoringSystem.Model;

namespace DataMonitoringSystem.ViewModel
{
    public class DetailViewModel
    {
        public CourseModel CourseModel { get; set; } = new CourseModel();
        public SensorModel SensorModel { get; set; } = new SensorModel();
        public DetailViewModel()
        {
            
        }
        public void GetSensor(string courseID)
        {
            SensorModel = LocalDataAccess.GetInstance().GetSensor(courseID);
            CourseModel = LocalDataAccess.GetInstance().GetCourse(courseID);
        }
    }
}
