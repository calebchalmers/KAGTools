using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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
    public class ModsViewModel : ViewModelBase
    {
        private Mod _selected;
        private ObservableCollection<Mod> _mods;
        private CollectionViewSource _filteredMods;
        private string _searchFilter = "";

        public ModsViewModel()
        {
            OpenCommand = new RelayCommand(ExecuteOpenCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            UpdateActiveModsCommand = new RelayCommand(ExecuteUpdateActiveModsCommand);

            Mods = new ObservableCollection<Mod>(FileHelper.GetMods());

            _filteredMods = new CollectionViewSource();
            _filteredMods.Source = Mods;
            _filteredMods.Filter += FilteredMods_Filter;
        }

        private void FilteredMods_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Mod)e.Item).Name.ToLower().Contains(_searchFilter.ToLower());
        }

        public Mod Selected
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

        public ObservableCollection<Mod> Mods
        {
            get { return _mods; }
            set
            {
                if (_mods != value)
                {
                    _mods = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICollectionView FilteredMods
        {
            get { return _filteredMods.View; }
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
                    FilteredMods.Refresh();
                }
            }
        }

        public ICommand OpenCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand UpdateActiveModsCommand { get; private set; }

        private void ExecuteOpenCommand()
        {
            if (Selected == null) return;
            Process.Start(Selected.Directory);
        }

        private void ExecuteDeleteCommand()
        {
            if (Selected == null) return;
            if(MessageBox.Show("Are you sure you want to delete '" + Selected.Name + "'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Directory.Delete(Selected.Directory, true);
                Mods.Remove(Selected);
                UpdateActiveMods();
            }
        }

        private void ExecuteUpdateActiveModsCommand()
        {
            FileHelper.SetActiveMods(Mods.Where(mod => mod.IsActive).ToArray());
        }
    }
}
