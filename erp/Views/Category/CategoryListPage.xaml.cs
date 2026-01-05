using System.Windows;
using System.Windows.Controls;
using erp.ViewModels.CategoryView;
using erp.Views.Shared;

namespace erp.Views.Category;

public partial class CategoryListPage : Page
{
    public CategoryListPage()
    {
        InitializeComponent();

        Loaded += async (_, __) =>
        {
            if (DataContext is CategoryListViewModel vm)
                await vm.RefreshAsync();
        };
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CategoryListViewModel listVm) return;
        NavigationService?.Navigate(new CategoryEditPage(listVm));
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CategoryListViewModel listVm) return;

        if (listVm.Selected == null)
        {
            var owner = Window.GetWindow(this);
            ThemedDialog.ShowWarning(owner, "تنبيه", "اختار صنف الأول");
            return;
        }

        NavigationService?.Navigate(new CategoryEditPage(listVm.Selected, listVm));
    }
}
