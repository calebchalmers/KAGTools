﻿using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using KAGTools.Data;
using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public abstract class ManualGenericViewModel : FilterListViewModelBase<ManualItem>
    {
        private string _searchFilter = "";
        private string _typeFilter = "";
        private ObservableCollection<string> _types = null;
        private string _fileName = "";
        private bool _hasTypes = false;

        public ManualGenericViewModel(string fileName, bool hasTypes) :
            base(FileHelper.GetManualFunctions(fileName, hasTypes))
        {
            OpenFileCommand = new RelayCommand(ExecuteOpenFileCommand);

            _fileName = fileName;
            _hasTypes = hasTypes;

            if (hasTypes)
            {
                _types = new ObservableCollection<string>(Items.Select(i => i.Type).Distinct());
            }
        }

        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                if (_searchFilter != value)
                {
                    _searchFilter = value;
                    RaisePropertyChanged();
                    RefreshFilters();
                }
            }
        }

        public string TypeFilter
        {
            get { return _typeFilter; }
            set
            {
                if (_typeFilter != value)
                {
                    _typeFilter = value;
                    RaisePropertyChanged();
                    RefreshFilters();
                }
            }
        }

        protected override bool FilterItem(ManualItem item)
        {
            if (!string.IsNullOrEmpty(SearchFilter) &&
               item.Value.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) == -1)
                return false;
            if (!string.IsNullOrEmpty(TypeFilter) &&
               item.Type.IndexOf(TypeFilter, StringComparison.OrdinalIgnoreCase) == -1)
                return false;
            return true;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public bool HasTypes
        {
            get { return _hasTypes; }
        }

        public ObservableCollection<string> Types
        {
            get { return _types; }
        }

        public ICommand OpenFileCommand { get; private set; }

        public void ExecuteOpenFileCommand()
        {
            Process.Start(FileName);
        }
    }
}
