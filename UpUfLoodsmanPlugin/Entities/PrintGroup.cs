using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UpUfLoodsmanPlugin.Entities
{
    public class PrintGroup : INotifyPropertyChanged
    {
        public string _name { get; set; }
        private OperationStatus _operationStatus;
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
        public int Reference { get; set; }
        public string OutputPath { get; set; }



        public PrintGroup(string name, OperationStatus operationStatus, double xmin, double ymin, double xmax,double ymax, int reference,string outputPath)
        {
            Name = name;
            OperationStatus = operationStatus;
            Xmin = xmin;
            Ymin = ymin;
            Xmax = xmax;
            Ymax = ymax;
            Reference = reference;
            OutputPath = outputPath;
        }


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
