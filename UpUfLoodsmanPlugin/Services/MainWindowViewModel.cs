using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UpUfLoodsmanPlugin.Entities;

namespace UpUfLoodsmanPlugin.Services
{
    public class MainWindowViewModel
    {
        KompasService _kompasService = new KompasService();

        public ICommand MyCommand { get; }
        //Этот список будет передаваться из лоцмана (полльзователь до запуска приложения выделяет список необходимых объектов)
        List<string> selectedLoodsmanObjects = new List<string> { "4ГК.320.415", "4ГК.320.415-001", "4ГК.320.415-002" };

        public ObservableCollection<Operation> _operationList;
        //public ObservableCollection<Operation> operationList { get; set; }= new ObservableCollection<Operation> { new Operation("Операция1", OperationStatus.Waiting), new Operation("Операция2", OperationStatus.Waiting) };


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
            operationList= new ObservableCollection<Operation> { new Operation("Операция1", OperationStatus.Waiting), new Operation("Операция2", OperationStatus.Waiting) };
            MyCommand = new RelayCommand(OnClicked, CanBeClicked); 
        }

        public void MainMethod()
        {

            var testObject = _kompasService.ConnectToKompasApp();

        }
        public void SetOperationStatus(Operation operation, OperationStatus operationStatus)
        {
            Thread.Sleep(1000);
            Operation oper = operationList.FirstOrDefault(_ => _==operation);
            oper.OperationStatus = operationStatus;
        }
        private void OnClicked(object parameter)
        {
            
            operationList[0].OperationStatus=OperationStatus.Success;
            operationList[0].Name="Новая операция 1";
            
            operationList[1].OperationStatus=OperationStatus.Unsuccess;

            operationList.Add(new Operation("Test", OperationStatus.Unsuccess));
            
            
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
