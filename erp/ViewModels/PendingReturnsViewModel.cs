using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace erp.ViewModels.Returns
{
    public class PendingReturnsViewModel : BaseReturnsViewModel
    {
        private readonly ReturnsService _returnsService;

        public PendingReturnsViewModel(ReturnsService returnsService)
        {
            _returnsService = returnsService;
            PendingReturns = new ObservableCollection<PendingReturnDto>();
        }

        public ObservableCollection<PendingReturnDto> PendingReturns { get; }

        public async Task LoadPendingReturnsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                PendingReturns.Clear();
                var returns = await _returnsService.GetPendingReturnsAsync();

                foreach (var item in returns)
                    PendingReturns.Add(item);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> ApproveReturnAsync(string returnId)
        {
            if (IsBusy) return false;
            IsBusy = true;

            try
            {
                var success = await _returnsService.ApproveReturnAsync(returnId);
                if (success)
                    await LoadPendingReturnsAsync();

                return success;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
