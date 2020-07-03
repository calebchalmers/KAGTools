using GalaSoft.MvvmLight.CommandWpf;
using KAGTools.Data;
using KAGTools.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace KAGTools.ViewModels
{
    public class ModsViewModel : FilterListViewModelBase<Mod>
    {
        private string _searchFilter = "";

        public ModsViewModel() : base(FileHelper.GetMods() ?? Enumerable.Empty<Mod>())
        {
            OpenCommand = new RelayCommand(ExecuteOpenCommand);
            InfoCommand = new RelayCommand(ExecuteInfoCommand);
            UpdateActiveModsCommand = new RelayCommand(ExecuteUpdateActiveModsCommand);
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
        public ICommand InfoCommand { get; private set; }
        public ICommand UpdateActiveModsCommand { get; private set; }

        private void ExecuteOpenCommand()
        {
            if (Selected == null) return;
            Process.Start(Selected.Directory);
        }

        private void ExecuteInfoCommand()
        {
            if (Selected == null) return;

            StringBuilder infoBuilder = new StringBuilder();
            infoBuilder.AppendLine("Name: " + Selected.Name);
            infoBuilder.AppendLine("Gamemode: " + (FileHelper.FindGamemodeOfMod(Selected.Directory) ?? "N/A"));

            MessageBox.Show(infoBuilder.ToString(), "Mod Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteUpdateActiveModsCommand()
        {
            UpdateActiveMods();
        }

        private void UpdateActiveMods()
        {
            FileHelper.SetActiveMods(Items.Where(mod => mod.IsActive == true).ToArray());
        }
    }
}
