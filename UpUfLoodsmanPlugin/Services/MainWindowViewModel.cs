using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using UpUfLoodsmanPlugin.Entities;

namespace UpUfLoodsmanPlugin.Services
{
    public class MainWindowViewModel
    {
        string testDocumentPath = @"C:\Users\SPankratov\source\repos\UpUfLoodsmanPlugin\UpUfLoodsmanPlugin\TestFiles\4ГК.320.415 СБ.cdw";
        KompasService _kompasService = new KompasService();

        public ICommand MainButtonCommand { get; }
        public AsyncRelayCommand AsyncMainButtonCommand { get; }


        //Этот список будет передаваться из лоцмана (полльзователь до запуска приложения выделяет список необходимых объектов)
        List<string> selectedLoodsmanObjects = new List<string> { "4ГК.320.415", "4ГК.320.415-001", "4ГК.320.415-002" };

        public ObservableCollection<Operation> _operationList;


        public ObservableCollection<Operation> operationList
        {
            get => _operationList;
            set
            {
                _operationList = value;
                OnPropertyChanged(nameof(operationList));
            }
        }
        public MainWindowViewModel()
        {
            operationList = new ObservableCollection<Operation> { new Operation("Запуск компаса", OperationStatus.Waiting), new Operation("Открытие модели", OperationStatus.Waiting) };
            MainButtonCommand = new RelayCommand(MainButtonClick, CanBeClicked);
            AsyncMainButtonCommand = new AsyncRelayCommand(AsyncMainButtonClick);

        }

        public void MainMethod()
        {

            var testObject = _kompasService.ConnectToKompasApp();

        }
        public void SetOperationStatus(Operation operation, OperationStatus operationStatus)
        {
            Operation oper = operationList.FirstOrDefault(_ => _ == operation);
            oper.OperationStatus = operationStatus;
        }
        private void MainButtonClick(object parameter)
        {
            //MethodeOne();
            //MethodeTwo();


        }

        private async Task AsyncMainButtonClick()
        {

            bool startKompasCheck = await _kompasService.ConnectOrStartKompas();
            if (startKompasCheck)
            {
                operationList[0].OperationStatus = OperationStatus.Success;
            }
            

            bool openDocumentCheck = await _kompasService.OpenDocument(testDocumentPath);            
            
            if (openDocumentCheck)
            {
                operationList[1].OperationStatus |= OperationStatus.Success;
            }
            Thread.Sleep(2000);
        }

        private bool CanBeClicked(object parameter)
        {
            return true; // Здесь можно задать условие доступности кнопки
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
