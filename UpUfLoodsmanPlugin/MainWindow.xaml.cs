using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using UpUfLoodsmanPlugin.Entities;
using UpUfLoodsmanPlugin.Services;

namespace UpUfLoodsmanPlugin
{
    public partial class MainWindow : Window
    {
        public static MainWindowViewModel _MainWindowViewModel = new MainWindowViewModel();
        //public ObservableCollection<Operation> operationsList = _MainWindowViewModel.operationList;


        public MainWindow()
        {
            InitializeComponent();

            ListBox1.Items.Add("Объект1");
            ListBox1.Items.Add("Объект2");
            ListBox1.Items.Add("Объект3");
            ListBox1.Items.Add("Объект4");
            ListBox1.Items.Add("Объект5");

           

        }



        private async void mainButton_Click(object sender, RoutedEventArgs e)
        {
            mainButton.IsEnabled = false;

            

            mainButton.IsEnabled = true;
        }


    }
}
