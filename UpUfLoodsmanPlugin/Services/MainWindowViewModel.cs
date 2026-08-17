using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpUfLoodsmanPlugin.Entities;

namespace UpUfLoodsmanPlugin.Services
{
    public class MainWindowViewModel
    {
        KompasService _kompasService = new KompasService();
        //Этот список будет передаваться из лоцмана (полльзователь до запуска приложения выделяет список необходимых объектов)
        List<string> selectedLoodsmanObjects = new List<string> { "4ГК.320.415", "4ГК.320.415-001", "4ГК.320.415-002" };

        public ObservableCollection<Operation> operationList = new ObservableCollection<Operation> { new Operation("Операция1", OperationStatus.Waiting), new Operation("Операция2", OperationStatus.Waiting) };

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
    }
}
