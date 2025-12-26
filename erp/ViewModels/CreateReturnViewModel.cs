using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace erp.ViewModels.Returns
{
    public class CreateReturnViewModel : BaseReturnsViewModel
    {
        private readonly ReturnsService _returnsService;

        public CreateReturnViewModel(ReturnsService returnsService)
        {
            _returnsService = returnsService;
            Items = new ObservableCollection<OrderItemForReturnDto>();
        }

        public ObservableCollection<OrderItemForReturnDto> Items { get; }

        public string OrderId { get; set; }
        public string CustomerId { get; set; }

        public async Task<bool> SubmitReturnAsync()
        {
            if (IsBusy) return false;
            IsBusy = true;

            try
            {
                var selectedItems = Items
                    .Where(i => i.ReturnQuantity > 0)
                    .Select(i => new CreateReturnItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.ReturnQuantity,
                        Reason = i.Reason
                    })
                    .ToList();

                if (!selectedItems.Any())
                    return false;

                var request = new CreateReturnRequestDto
                {
                    OrderId = OrderId,
                    CustomerId = CustomerId,
                    Items = selectedItems
                };

                return await _returnsService.CreateReturnAsync(request);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
