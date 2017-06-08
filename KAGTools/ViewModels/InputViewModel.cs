using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using KAGTools.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace KAGTools.ViewModels
{
    public class InputViewModel : ViewModelBase
    {
        private string _title;
        private string _inputLabelText;
        private string _input;

        public InputViewModel(string title, string inputLabelText, string defaultText = "")
        {
            OKCommand = new RelayCommand(ExecuteOKCommand);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);

            _title = title;
            _inputLabelText = inputLabelText;
            _input = defaultText;
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string InputLabelText
        {
            get { return _inputLabelText; }
            set
            {
                if (_inputLabelText != value)
                {
                    _inputLabelText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Input
        {
            get { return _input; }
            set
            {
                if (_input != value)
                {
                    _input = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand OKCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void ExecuteOKCommand()
        {
            MessengerInstance.Send(new RequestCloseMessage(this, true), this);
        }

        private void ExecuteCancelCommand()
        {
            MessengerInstance.Send(new RequestCloseMessage(this, false), this);
        }
    }
}
