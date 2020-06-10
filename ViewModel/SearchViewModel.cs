using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GMDCGiphyPlugin.ViewModel
{
    public class SearchViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private ICommand searchCompletionCommand;

        private string searchQuery;

        public SearchViewModel()
        {
            SearchCompletionCommand = new RelayCommand<Window>(onSearchQueryCompletion);
        }

        public ICommand SearchCompletionCommand
        {
            get => this.searchCompletionCommand;
            private set => this.searchCompletionCommand = value;
        }

        public string SearchQuery
        {
            get => searchQuery;
            set => this.Set(() => this.SearchQuery, ref this.searchQuery, value);
        }

        public void onSearchQueryCompletion(Window window)
        {
            window.Close();
        }
    }
}
