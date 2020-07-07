using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
using KAGTools.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace KAGTools.ViewModels
{
    public class ManualDocumentViewModel : FilterListViewModelBase<ManualItem>
    {
        private Regex searchRegex;

        private string _searchFilter = "";
        private string _typeFilter = "";

        private WindowService WindowService { get; }
        private ManualDocument Document { get; }

        public ManualDocumentViewModel(ManualDocument document, WindowService windowService, ManualService manualService)
        {
            Document = document;
            WindowService = windowService;

            Name = Document.Name;
            HasTypes = Document.HasTypes;

            OpenSourceFileCommand = new RelayCommand(ExecuteOpenSourceFileCommand);

            Items = new ObservableCollection<ManualItem>(manualService.EnumerateManualDocument(document));

            if (HasTypes)
            {
                Types = new ObservableCollection<string>(Items.Select(i => i.Type).Distinct());
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

        public string Name { get; }
        public bool HasTypes { get; }
        public ObservableCollection<string> Types { get; }

        public ICommand OpenSourceFileCommand { get; private set; }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                Set(ref _searchFilter, value);
                searchRegex = GenerateSearchRegex(value);
                RefreshFilters();
            }
        }

        public string TypeFilter
        {
            get => _typeFilter;
            set
            {
                Set(ref _typeFilter, value);
                RefreshFilters();
            }
        }

        private void ExecuteOpenSourceFileCommand()
        {
            WindowService.OpenInExplorer(Document.Path);
        }

        private Regex GenerateSearchRegex(string input)
        {
            string escapedFilter = Regex.Replace(input, @"[.*+?^${}()|[\]\\]", @"\$&");
            string lookaheads = Regex.Replace(escapedFilter, @"([^ ]+) *", "(?=.*$1)");
            string pattern = $@"^{lookaheads}.*$";
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
    }
}
