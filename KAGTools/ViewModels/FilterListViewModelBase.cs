using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace KAGTools.ViewModels
{
    public abstract class FilterListViewModelBase<T> : ViewModelBase where T : class
    {
        private ObservableCollection<T> _items;
        private ObservableCollection<T> _filteredItems;
        private T _selected;

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

        private void FilteredItems_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = FilterItem((T)e.Item);
        }

        protected virtual bool FilterItem(T item)
        {
            return true;
        }

        protected void RefreshFilters()
        {
            FilteredItems = new ObservableCollection<T>(Items.Where(FilterItem));
        }

        public ObservableCollection<T> Items
        {
            get => _items;
            set => this.SetProperty(ref _items, value, RefreshFilters);
        }

        public ObservableCollection<T> FilteredItems
        {
            get => _filteredItems;
            private set => this.SetProperty(ref _filteredItems, value);
        }

        public T Selected
        {
            get => _selected;
            set => this.SetProperty(ref _selected, value);
        }
    }
}
