using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;

namespace erp.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        // ✅ نفس الاسم اللي بتستخدمه في كل الـ ViewModels
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
            => SetProperty(ref field, value, name);
    }
}
