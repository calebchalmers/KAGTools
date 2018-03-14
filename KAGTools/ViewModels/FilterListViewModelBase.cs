using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KAGTools.ViewModels
{
    public abstract class FilterListViewModelBase<T> : ViewModelBase where T : class
    {
        private ObservableCollection<T> _items;
        private CollectionViewSource _filteredItems;
        private T _selected;

        public FilterListViewModelBase(ObservableCollection<T> items)
        {
            _filteredItems = new CollectionViewSource();
            _filteredItems.Filter += FilteredItems_Filter;
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
            FilteredItems.Refresh();
        }

        public ObservableCollection<T> Items
        {
            get { return _items; }
            set
            {
                if(_items != value)
                {
                    _items = value;
                    _filteredItems.Source = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICollectionView FilteredItems
        {
            get { return _filteredItems.View; }
        }

        public T Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
