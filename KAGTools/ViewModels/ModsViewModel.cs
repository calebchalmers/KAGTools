using GalaSoft.MvvmLight.CommandWpf;
using KAGTools.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace KAGTools.ViewModels
{
    public class ModsViewModel : FilterListViewModelBase<Mod>
    {
        private string _searchFilter = "";

        private IWindowService WindowService { get; set; }
        private IConfigService ConfigService { get; set; }
        private IModsService ModsService { get; set; }

        public ModsViewModel(IWindowService windowService, IConfigService configService, IModsService modsService)
        {
            WindowService = windowService;
            ConfigService = configService;
            ModsService = modsService;

            OpenCommand = new RelayCommand(ExecuteOpenCommand);
            InfoCommand = new RelayCommand(ExecuteInfoCommand);

            IEnumerable<Mod> allMods = ModsService.EnumerateAllMods();
            Items = new ObservableCollection<Mod>(allMods);
        }

        protected override bool FilterItem(Mod item)
        {
            return item.Name.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                Set(ref _searchFilter, value);
                RefreshFilters();
            }
        }

        public ICommand OpenCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }

        private void ExecuteOpenCommand()
        {
            if (Selected != null)
            {
                WindowService.OpenInExplorer(Selected.Directory);
            }
        }

        private void ExecuteInfoCommand()
        {
            if (Selected == null) return;

            string gamemode = ConfigService.FindGamemodeOfMod(Selected);

            StringBuilder infoBuilder = new StringBuilder();
            infoBuilder.AppendLine("Name: " + Selected.Name);
            infoBuilder.AppendLine("Gamemode: " + (gamemode ?? "N/A"));

            WindowService.Alert(infoBuilder.ToString(), "Mod Info");
        }
    }
}
