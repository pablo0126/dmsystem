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
using DataMonitoringSystem.Common;
using DataMonitoringSystem.ViewModel;

namespace DataMonitoringSystem.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        MainViewModel model = new MainViewModel();
        public MainView()
        {
            InitializeComponent();

            this.DataContext = model;

            model.UserInfo.Avatar = GlobalValues.UserInfo.Avatar;
            model.UserInfo.UserName = GlobalValues.UserInfo.RealName;
            model.UserInfo.Gender = GlobalValues.UserInfo.Gender;

            this.MaxHeight = SystemParameters.PrimaryScreenHeight;

            //LoadPage1();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ?
                WindowState.Normal : WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //IPageChange currentControl;
        private void MainWindow_OnPageChange(int obj)
        {
            switch (obj)
            {
                case 2: LoadPage1(); break; // si on vient de la page 2 on va vers la page 1
            }
        }

        public void LoadPage1()
        {
            model.DoNavChanged("AddPageView");

            //currentControl.PageChange += MainWindow_OnPageChange;
        }

    }
}
