using erp.ViewModels.Cheques;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace erp.Views.Cheques
{
    public partial class AddChequePage : Page
    {
        private readonly AddChequeViewModel _viewModel;

        public AddChequePage()
        {
            InitializeComponent();
            _viewModel = new AddChequeViewModel();
            DataContext = _viewModel;
            _viewModel.OnSuccess += (s, e) => 
            {
               if (NavigationService.CanGoBack) NavigationService.GoBack();
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void RelatedNameBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!_viewModel.IsSuggestionOpen) return;

            switch (e.Key)
            {
                case System.Windows.Input.Key.Down:
                    if (SuggestionsList.SelectedIndex < SuggestionsList.Items.Count - 1)
                    {
                        SuggestionsList.SelectedIndex++;
                        SuggestionsList.ScrollIntoView(SuggestionsList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case System.Windows.Input.Key.Up:
                    if (SuggestionsList.SelectedIndex > 0)
                    {
                        SuggestionsList.SelectedIndex--;
                        SuggestionsList.ScrollIntoView(SuggestionsList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case System.Windows.Input.Key.Enter:
                    if (SuggestionsList.SelectedItem is string selected)
                    {
                        _viewModel.SelectedSuggestion = selected;
                    }
                    else if (SuggestionsList.Items.Count > 0)
                    {
                        // Select first if none selected but user hits enter? 
                        // Or just let them save what they typed. 
                        // Standard autocomplete usually selects top result on enter if one exists.
                        // But here, Enter might submit the form if not handled.
                        // Let's assume explicit selection is safer. 
                        // But if an item is highlighted (SelectedIndex != -1), select it.
                         if (SuggestionsList.SelectedIndex != -1)
                         {
                            _viewModel.SelectedSuggestion = (string)SuggestionsList.SelectedItem;
                         }
                    }
                    e.Handled = true;
                    break;

                case System.Windows.Input.Key.Escape:
                    _viewModel.IsSuggestionOpen = false;
                    e.Handled = true;
                    break;
            }
        }
    }
}
