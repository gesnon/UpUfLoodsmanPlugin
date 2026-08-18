using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace UpUfLoodsmanPlugin.Entities
{
    public class Operation: INotifyPropertyChanged
    {        
        public Operation(string name, OperationStatus operationStatus)
        {
            Name = name;
            OperationStatus = operationStatus;
        }
        public string _name { get; set; }
        private OperationStatus _operationStatus;

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;

                OnPropertyChanged(nameof(Name));
            }
        }

        public OperationStatus OperationStatus
        {
            get => _operationStatus;
            set
            {
                if (_operationStatus == value)
                    return;

                _operationStatus = value;

                OnPropertyChanged(nameof(OperationStatus));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
             this,
             new PropertyChangedEventArgs(propertyName));
        }

    }
}
