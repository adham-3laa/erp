using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using erp.DTOs;
using erp.ViewModels.CategoryView;

namespace erp.Views.Category;

public partial class CategoryEditPage : Page
{
    private readonly CategoryListViewModel? _listVm;
    private bool _initialized;

    // ✅ دي اللي بتحدد وضع الصفحة: Edit لو مش null
    public CategoryDto? EditDto { get; set; }

    public CategoryEditPage(CategoryListViewModel? listVm = null)
    {
        InitializeComponent();
        _listVm = listVm;

        // ✅ يمنع مشاكل الـ Designer (XDG0003)
        if (!DesignerProperties.GetIsInDesignMode(this))
            DataContext ??= new CategoryEditViewModel();

        Loaded += CategoryEditPage_Loaded;
    }

    private void CategoryEditPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        _initialized = true;

        if (DataContext is not CategoryEditViewModel vm) return;

        // ✅ 결정 نهائي: Edit ولا Create
        if (EditDto != null)
            vm.LoadForEdit(EditDto);
        else
            vm.LoadForCreate();

        // ✅ subscribe once
        vm.RequestClose -= OnRequestClose;
        vm.RequestClose += OnRequestClose;
    }

    private async void OnRequestClose()
    {
        if (_listVm != null)
            await _listVm.RefreshAsync();

        NavigationService?.GoBack();
    }

    private async void Back_Click(object sender, RoutedEventArgs e)
    {
        if (_listVm != null)
            await _listVm.RefreshAsync();

        NavigationService?.GoBack();
    }
}
