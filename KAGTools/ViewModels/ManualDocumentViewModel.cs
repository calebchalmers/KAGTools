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
        private string _typeFilter = "";

        private IWindowService WindowService { get; }
        private ManualDocument Document { get; }

        public ManualDocumentViewModel(ManualDocument document, IWindowService windowService, IManualService manualService)
        {
            Document = document;
            WindowService = windowService;

            Name = Document.Name;
            HasTypes = Document.HasTypes;

            OpenSourceFileCommand = new RelayCommand(ExecuteOpenSourceFileCommand);

            Items = new ObservableCollection<ManualItem>(manualService.EnumerateManualDocument(document));

            if (HasTypes)
            {
                Types = new ObservableCollection<string>(Items.Select(i => i.Type).Distinct());
            }
        }

        protected override bool FilterItem(ManualItem item)
        {
            if (!string.IsNullOrEmpty(TypeFilter) &&
                item.Type.IndexOf(TypeFilter, StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            return FilterValueToSearchInput(item.Value);
        }

        public string Name { get; }
        public bool HasTypes { get; }
        public ObservableCollection<string> Types { get; }

        public ICommand OpenSourceFileCommand { get; private set; }

        public string TypeFilter
        {
            get => _typeFilter;
            set
            {
                Set(ref _typeFilter, value);
                RefreshFilters();
            }
        }

        private void ExecuteOpenSourceFileCommand()
        {
            WindowService.OpenInExplorer(Document.Path);
        }
    }
}
