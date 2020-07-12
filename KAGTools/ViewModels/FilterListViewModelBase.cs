using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace KAGTools.ViewModels
{
    public abstract class FilterListViewModelBase<T> : ViewModelBase where T : class
    {
        private ObservableCollection<T> _items;
        private ObservableCollection<T> _filteredItems;
        private T _selected;
        private string _searchInput;

        private string SearchRegexPattern { get; set; }

        public FilterListViewModelBase(ObservableCollection<T> items)
        {
            Items = items;
        }

        public FilterListViewModelBase() :
            this(new ObservableCollection<T>())
        {
        }

        public FilterListViewModelBase(IEnumerable<T> items) :
            this(new ObservableCollection<T>(items))
        {
        }

        protected abstract bool FilterItem(T item);

        protected bool FilterValueToSearchInput(string value)
        {
            return string.IsNullOrEmpty(SearchInput) || Regex.IsMatch(value, SearchRegexPattern, RegexOptions.IgnoreCase);
        }

        protected void RefreshFilters()
        {
            FilteredItems = new ObservableCollection<T>(Items.Where(FilterItem));
        }

        public ObservableCollection<T> Items
        {
            get => _items;
            set
            {
                Set(ref _items, value);
                RefreshFilters();
            }
        }

        public ObservableCollection<T> FilteredItems
        {
            get => _filteredItems;
            private set => Set(ref _filteredItems, value);
        }

        public T Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public string SearchInput
        {
            get => _searchInput;
            set
            {
                Set(ref _searchInput, value);

                string escapedInput = Regex.Replace(value, @"[.*+?^${}()|[\]\\]", @"\$&");
                string lookaheads = Regex.Replace(escapedInput, @"([^ ]+) *", "(?=.*$1)");
                SearchRegexPattern = $@"^{lookaheads}.*$";

                RefreshFilters();
            }
        }
    }
}
