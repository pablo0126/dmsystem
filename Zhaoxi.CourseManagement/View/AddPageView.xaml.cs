using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataMonitoringSystem.ViewModel;

namespace DataMonitoringSystem.View
{
    /// <summary>
    /// AddPageView.xaml 的交互逻辑
    /// </summary>
    public partial class AddPageView : UserControl
    {
        public AddPageView()
        {
            InitializeComponent();
            this.DataContext = new AddPageViewModel();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            AddPageViewModel viewModel = (AddPageViewModel)this.DataContext;
            viewModel.Dispose();
        }
    }
}
