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
            NewCommand = new RelayCommand(ExecuteNewCommand);
            DuplicateCommand = new RelayCommand(ExecuteDuplicateCommand);
            InfoCommand = new RelayCommand(ExecuteInfoCommand);

            _mods = new ObservableCollection<Mod>(FileHelper.GetMods());

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
        public ICommand NewCommand { get; private set; }
        public ICommand DuplicateCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }

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
            UpdateActiveMods();
        }

        private void ExecuteNewCommand()
        {
            InputViewModel viewModel = new InputViewModel("New Mod", "Name:");
            if(ServiceManager.GetService<IViewService>().OpenDialog(viewModel) == true)
            {
                CreateMod(viewModel.Input);
            }
        }

        private void ExecuteDuplicateCommand()
        {
            if (Selected == null) return;
            InputViewModel viewModel = new InputViewModel("Duplicate Mod", "Name:", Selected.Name);
            if (ServiceManager.GetService<IViewService>().OpenDialog(viewModel) == true)
            {
                CreateMod(viewModel.Input, Selected);
            }
        }

        private void ExecuteInfoCommand()
        {
            if (Selected == null) return;

            StringBuilder infoBuilder = new StringBuilder();
            infoBuilder.AppendLine("Name: " + Selected.Name);
            infoBuilder.AppendLine("Gamemode: " + Selected.Gamemode);

            MessageBox.Show(infoBuilder.ToString(), "Mod Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateActiveMods()
        {
            FileHelper.SetActiveMods(Mods.Where(mod => mod.IsActive).ToArray());
        }

        private void CreateMod(string name, Mod from = null)
        {
            if (!FileHelper.IsValidPath(name))
            {
                MessageBox.Show("Invalid name!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string modDir = Path.Combine(FileHelper.ModsDir, name);

            if (Directory.Exists(modDir))
            {
                MessageBox.Show(name + " already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(from == null)
            {
                Directory.CreateDirectory(modDir);
            }
            else
            {
                FileHelper.CopyDirectory(from.Directory, modDir);
            }

            Mod newMod = new Mod(modDir, true);
            Mods.Add(newMod);

            Process.Start(modDir);

            Selected = newMod;
            MessengerInstance.Send(new FocusSelectedItemMessage());
            UpdateActiveMods();
        }
    }
}
