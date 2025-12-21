using System.Windows;
using System.Windows.Controls;
using erp.DTOs;
using erp.ViewModels.CategoryView;

namespace erp.Views.Category;

public partial class CategoryEditPage : Page
{
    private readonly CategoryListViewModel? _listVm;

    // Create
    public CategoryEditPage(CategoryListViewModel? listVm = null)
    {
        InitializeComponent();
        _listVm = listVm;

        Loaded += (_, __) =>
        {
            if (DataContext is CategoryEditViewModel vm)
                vm.LoadForCreate();
        };
    }

    // Edit
    public CategoryEditPage(CategoryDto dto, CategoryListViewModel? listVm = null)
    {
        InitializeComponent();
        _listVm = listVm;

        Loaded += (_, __) =>
        {
            if (DataContext is CategoryEditViewModel vm)
                vm.LoadForEdit(dto);
        };
    }

    private async void Back_Click(object sender, RoutedEventArgs e)
    {
        // رجوع لليست + refresh (لو متاحة)
        if (_listVm != null)
            await _listVm.RefreshAsync();

        NavigationService?.GoBack();
    }
}
