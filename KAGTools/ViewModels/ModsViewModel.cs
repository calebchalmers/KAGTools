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
using KAGTools.Data;
using KAGTools.Helpers;

namespace KAGTools.ViewModels
{
    public class ModsViewModel : FilterListViewModelBase<Mod>
    {
        private string _searchFilter = "";

        public ModsViewModel() :
            base(FileHelper.GetMods())
        {
            OpenCommand = new RelayCommand(ExecuteOpenCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            UpdateActiveModsCommand = new RelayCommand(ExecuteUpdateActiveModsCommand);
            NewCommand = new RelayCommand(ExecuteNewCommand);
            DuplicateCommand = new RelayCommand(ExecuteDuplicateCommand);
            InfoCommand = new RelayCommand(ExecuteInfoCommand);
        }

        protected override bool FilterItem(Mod item)
        {
            return item.Name.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string SearchFilter
        {
            get => _searchFilter;
            set => this.SetProperty(ref _searchFilter, value, RefreshFilters);
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
                Items.Remove(Selected);
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
            bool? dialogResult = WindowHelper.OpenDialog(viewModel);

            if(dialogResult == true)
            {
                CreateMod(viewModel.Input);
            }
        }

        private void ExecuteDuplicateCommand()
        {
            if (Selected == null) return;

            InputViewModel viewModel = new InputViewModel("Duplicate Mod", "Name:", Selected.Name);
            bool? dialogResult = WindowHelper.OpenDialog(viewModel);

            if (dialogResult == true)
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
            FileHelper.SetActiveMods(Items.Where(mod => mod.IsActive).ToArray());
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

            Directory.CreateDirectory(modDir);

            if (from != null)
            {
                FileHelper.CopyDirectory(from.Directory, modDir);
            }

            Mod newMod = new Mod(modDir, true);
            Items.Add(newMod);

            Process.Start(modDir);

            Selected = newMod;
            MessengerInstance.Send(new FocusSelectedItemMessage());
            UpdateActiveMods();
        }
    }
}
