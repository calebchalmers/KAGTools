using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
using KAGTools.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace KAGTools.ViewModels.Manual
{
    public abstract class ManualGenericViewModel : FilterListViewModelBase<ManualItem>
    {
        private string _searchFilter = "";
        private string _typeFilter = "";
        private ObservableCollection<string> _types = null;
        private string _fileName = "";
        private bool _hasTypes = false;

        private Regex searchRegex;

        public ManualGenericViewModel(string fileName, bool hasTypes) :
            base(FileHelper.GetManualFunctions(fileName, hasTypes))
        {
            OpenFileCommand = new RelayCommand(ExecuteOpenFileCommand);

            _fileName = fileName;
            _hasTypes = hasTypes;

            if (hasTypes)
            {
                _types = new ObservableCollection<string>(Items.Select(i => i.Type).Distinct().OrderBy(i => i));
            }
        }

        protected override bool FilterItem(ManualItem item)
        {
            if (!string.IsNullOrEmpty(TypeFilter) &&
                item.Type.IndexOf(TypeFilter, StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            if (!string.IsNullOrEmpty(SearchFilter) && 
                !searchRegex.IsMatch(item.Value))
                return false;

            return true;
        }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                string escapedFilter = Regex.Replace(value, @"[.*+?^${}()|[\]\\]", @"\$&");
                searchRegex = new Regex(string.Format(@"^{0}.*$", Regex.Replace(escapedFilter, @"([^ ]+) *", "(?=.*$1)")), RegexOptions.IgnoreCase);

                this.SetProperty(ref _searchFilter, value, RefreshFilters);
            }
        }

        public string TypeFilter
        {
            get => _typeFilter;
            set => this.SetProperty(ref _typeFilter, value, RefreshFilters);
        }

        public string FileName
        {
            get => _fileName;
        }

        public bool HasTypes
        {
            get => _hasTypes;
        }

        public ObservableCollection<string> Types
        {
            get => _types;
        }

        public ICommand OpenFileCommand { get; private set; }

        public void ExecuteOpenFileCommand()
        {
            Process.Start(FileName);
        }
    }
}
