using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EduGate.Models;
using EduGate.Services;
using erp.ViewModels.InventoryModels;

public class AddProductViewModel
{
    public string ProductName { get; set; }
    public int SalePrice { get; set; }
    public int BuyPrice { get; set; }
    public int Quantity { get; set; }
    public string SKU { get; set; }
    public string Description { get; set; }

    public string SupplierId { get; set; }
    public string CategoryId { get; set; }

    public ObservableCollection<Supplier> Suppliers { get; set; }
    public ObservableCollection<Category> Categories { get; set; }

    public ICommand SaveCommand { get; }

    public AddProductViewModel()
    {
        Suppliers = new ObservableCollection<Supplier>();
        Categories = new ObservableCollection<Category>();

        SaveCommand = new RelayCommand(SaveProduct);

        LoadSuppliers();
        LoadCategories();
    }

    private async void SaveProduct()
    {
        var service = new InventoryService();

        await service.AddProductAsync(new
        {
            productname = ProductName,
            saleprice = SalePrice,
            buyprice = BuyPrice,
            quantity = Quantity,
            sku = SKU,
            description = Description,
            categoryid = CategoryId,
            supplierid = SupplierId
        });

        MessageBox.Show("تم إضافة المنتج بنجاح");
    }

    private async void LoadSuppliers()
    {
        var service = new SupplierService();
        var data = await service.GetAllSuppliersAsync();
        foreach (var s in data)
            Suppliers.Add(s);
    }

    private async void LoadCategories()
    {
        var service = new CategoryService();
        var data = await service.GetAllCategoriesAsync();
        foreach (var c in data)
            Categories.Add(c);
    }
}
