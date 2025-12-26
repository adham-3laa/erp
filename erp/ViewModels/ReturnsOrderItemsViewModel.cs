using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace erp.ViewModels.Returns
{
    public class ReturnsOrderItemsViewModel : BaseReturnsViewModel
    {
        private readonly ReturnsService _returnsService;

        public ReturnsOrderItemsViewModel(ReturnsService returnsService)
        {
            _returnsService = returnsService;
            OrderItems = new ObservableCollection<OrderItemForReturnDto>();
        }

        public ObservableCollection<OrderItemForReturnDto> OrderItems { get; }

        public async Task LoadOrderItemsAsync(string orderId)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                OrderItems.Clear();

                var items = await _returnsService.GetOrderItemsByOrderIdAsync(orderId);

                System.Diagnostics.Debug.WriteLine($"Items count: {items.Count}");

                foreach (var item in items)
                {
                    item.ReturnQuantity = 0;
                    item.Reason = string.Empty;

                    System.Diagnostics.Debug.WriteLine(
                        $"ProductId={item.ProductId}, Qty={item.Quantity}, Price={item.UnitPrice}");

                    OrderItems.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
