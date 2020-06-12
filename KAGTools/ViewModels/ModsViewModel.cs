﻿using GalaSoft.MvvmLight;
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

        public ModsViewModel() : base(FileHelper.GetMods())
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
            infoBuilder.AppendLine("Gamemode: " + Selected.Gamemode);

            MessageBox.Show(infoBuilder.ToString(), "Mod Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteUpdateActiveModsCommand()
        {
            UpdateActiveMods();
        }

        private void UpdateActiveMods()
        {
            FileHelper.SetActiveMods(Items.Where(mod => mod.IsActive).ToArray());
        }
    }
}
