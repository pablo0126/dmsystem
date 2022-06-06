using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMonitoringSystem.Model
{
    public class CategoryItemModel
    {
        public CategoryItemModel() { }
        public CategoryItemModel(string name, bool state = false, string id = "")
        {
            this.UserID = id;
            this.CategoryName = name;
            this.IsSelected = state;
        }
        public string UserID { get; set; }
        public bool IsSelected { get; set; }
        public string CategoryName { get; set; }
    }
}
