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
            bool startKompas = Task.Run(() => _kompasService.ConnectOrStartKompas()).Result;


            if (startKompas)
            {
                operationList[0].OperationStatus = OperationStatus.Success;
            }
            _kompasService.TestMethode();


        }
        private async Task AsyncMainButtonClick()
        {
            await Task.Run(() => { _kompasService.ConnectOrStartKompas(); });


            operationList[0].OperationStatus = OperationStatus.Success;
                        


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
