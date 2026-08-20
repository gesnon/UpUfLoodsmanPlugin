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
using System.Security.Policy;
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
        static string testDocumentPath = @"C:\Users\SPankratov\source\repos\UpUfLoodsmanPlugin\UpUfLoodsmanPlugin\TestFiles\4ГК.320.415 СБ.cdw";

        string testDocumentName = testDocumentPath.Split('\\').Last();

        KompasService _kompasService = new KompasService();

        public ICommand PreviousButtonCommand { get; }
        public ICommand NextButtonCommand { get; }
        public AsyncRelayCommand AsyncMainButtonCommand { get; }
        public ObservableCollection<Operation> _operationList;
        public ObservableCollection<string> _selectedLoodsmanObjects;
        public ObservableCollection<PrintGroup> _pdfObjectsList;
        public ObservableCollection<Uri> _outputFiles;
        public int currentDpfViewIndex;
        
        public Uri _currentWebViewFile;
        

        public ObservableCollection<Operation> operationList
        {
            get => _operationList;
            set
            {
                _operationList = value;
                OnPropertyChanged(nameof(operationList));
            }
        }
        public ObservableCollection<string> selectedLoodsmanObjects
        {
            get => _selectedLoodsmanObjects;
            set
            {
                _selectedLoodsmanObjects = value;
                OnPropertyChanged(nameof(selectedLoodsmanObjects));
            }
        }
        public ObservableCollection<PrintGroup> pdfObjectsList
        {
            get => _pdfObjectsList;
            set
            {
                _pdfObjectsList = value;
                OnPropertyChanged(nameof(pdfObjectsList));
            }
        }

        public ObservableCollection<Uri> outputFiles
        {
            get => _outputFiles;
            set
            {
                _outputFiles = value;
                OnPropertyChanged(nameof(outputFiles));
            }
        }
                
        public Uri currentWebViewFile
        {
            get => _currentWebViewFile;
            set
            {
                _currentWebViewFile = value;
                OnPropertyChanged(nameof(currentWebViewFile));
            }
        }
        public MainWindowViewModel()
        {
            operationList = new ObservableCollection<Operation> {
                new Operation("Поиск модели", OperationStatus.Waiting),
                new Operation("Выгрузка модели", OperationStatus.Waiting),
                new Operation("Запуск компаса", OperationStatus.Waiting),
                new Operation("Открытие модели", OperationStatus.Waiting),
                new Operation("Очистка мусора", OperationStatus.Waiting),
                new Operation("Копирование изображений", OperationStatus.Waiting)

            };

            selectedLoodsmanObjects = new ObservableCollection<string> { "4ГК.320.415", "4ГК.320.415-001", "4ГК.320.415-002" };
            pdfObjectsList = new ObservableCollection<PrintGroup>();
            outputFiles = new ObservableCollection<Uri>();
            PreviousButtonCommand = new RelayCommand(PreviousButtonClick, CanBeClicked);
            NextButtonCommand = new RelayCommand(NextButtonClick, CanBeClicked);
            AsyncMainButtonCommand = new AsyncRelayCommand(AsyncMainButtonClick);

        }

        public void MainMethod()
        {

            var testObject = _kompasService.ConnectToKompasApp();

        }

        private void PreviousButtonClick(object parameter)
        {
            if (currentDpfViewIndex > 0)
            {
                currentDpfViewIndex --;
                currentWebViewFile = outputFiles[currentDpfViewIndex];
                ((MainWindow)Application.Current.MainWindow).MyWebView.Source = currentWebViewFile;
                ((MainWindow)Application.Current.MainWindow).PictureNameTextBlock.Text = _pdfObjectsList[currentDpfViewIndex].Name;
            }



        }
        private void NextButtonClick(object parameter)
        {
            if (currentDpfViewIndex < outputFiles.Count-1)
            {
                currentDpfViewIndex++;
                currentWebViewFile = outputFiles[currentDpfViewIndex];
                ((MainWindow)Application.Current.MainWindow).MyWebView.Source = currentWebViewFile;
                ((MainWindow)Application.Current.MainWindow).PictureNameTextBlock.Text = _pdfObjectsList[currentDpfViewIndex].Name;
            }


        }

        private async Task AsyncMainButtonClick()
        {

            if (await Task.Run(() => _kompasService.ConnectOrStartKompas().Result))
            {
                operationList[2].OperationStatus = OperationStatus.Success;
            }

            if (await Task.Run(() => _kompasService.OpenOrSelectDocument(testDocumentPath).Result))
            {
                operationList[3].OperationStatus = OperationStatus.Success;
            }
            if (await Task.Run(() => _kompasService.DeleteTrashFromDocument().Result))
            {
                operationList[4].OperationStatus = OperationStatus.Success;
            }
            if (await Task.Run(() => _kompasService.CreatetemporaryGroup().Result))
            {
                operationList[5].OperationStatus = OperationStatus.Success;
            }

            List<PrintGroup> printGroups = await Task.Run(() => _kompasService.SplitAndCreate());
            foreach (PrintGroup printGroup in printGroups)
            {

                await Task.Run(() =>
                {

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        pdfObjectsList.Add(printGroup);
                    });
                });
            }

            for (int i = 0; i < pdfObjectsList.Count; i++)
            {
                if (await Task.Run(() => _kompasService.CreatePdf(pdfObjectsList[i], i + 1).Result))
                {
                    pdfObjectsList[i].OperationStatus = OperationStatus.Success;
                    outputFiles.Add(new Uri(pdfObjectsList[i].OutputPath));
                    currentWebViewFile = outputFiles[i];
                    ((MainWindow)Application.Current.MainWindow).MyWebView.Source = currentWebViewFile;
                    ((MainWindow)Application.Current.MainWindow).PictureNameTextBlock.Text = _pdfObjectsList[i].Name;
                    currentDpfViewIndex = i;
                }
            }
            _kompasService.CloseTemporaryDocument();

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
