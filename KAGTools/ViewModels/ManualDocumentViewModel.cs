using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
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

        public ManualDocumentViewModel(ManualDocument document) : base(document.Items ?? Enumerable.Empty<ManualItem>())
        {
            Name = document.Name;
            HasTypes = document.HasTypes;

            OpenSourceFileCommand = new RelayCommand(document.OpenSourceFile);

            if (HasTypes)
            {
                Types = new ObservableCollection<string>(Items.Select(i => i.Type).Distinct().OrderBy(i => i));
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

        public Regex GenerateSearchRegex(string input)
        {
            string escapedFilter = Regex.Replace(input, @"[.*+?^${}()|[\]\\]", @"\$&");
            string lookaheads = Regex.Replace(escapedFilter, @"([^ ]+) *", "(?=.*$1)");
            string pattern = $@"^{lookaheads}.*$";
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
    }
}
