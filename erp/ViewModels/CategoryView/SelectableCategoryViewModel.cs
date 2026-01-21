using System.ComponentModel;
using System.Runtime.CompilerServices;
using erp.DTOs;

namespace erp.ViewModels.CategoryView;

public class SelectableCategoryViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public CategoryDto Category { get; }

    public SelectableCategoryViewModel(CategoryDto category)
    {
        Category = category;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
