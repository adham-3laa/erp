using System.Windows;
using System.Windows.Media;

namespace erp.Views.Shared;

public partial class ThemedDialog : Window
{
    public string TitleText { get; }
    public string MessageText { get; }
    public string IconText { get; }
    public Brush AccentBrush { get; }
    public Brush AccentBackground { get; }

    private ThemedDialog(
        Window? owner,
        string title,
        string message,
        string icon,
        Brush accentBrush,
        Brush accentBackground)
    {
        InitializeComponent();

        Owner = owner;
        TitleText = title;
        MessageText = message;
        IconText = icon;
        AccentBrush = accentBrush;
        AccentBackground = accentBackground;

        DataContext = this;
    }

    public static void ShowInfo(Window? owner, string title, string message)
    {
        // Indigo / Primary
        var accent = (Brush)Application.Current.FindResource("PrimaryBrush");
        var bg = new SolidColorBrush(Color.FromArgb(0x18, 0x31, 0x2E, 0x81)); // light indigo tint
        new ThemedDialog(owner, title, message, "ℹ️", accent, bg).ShowDialog();
    }

    public static void ShowWarning(Window? owner, string title, string message)
    {
        // soft amber-ish
        var accent = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
        var bg = new SolidColorBrush(Color.FromArgb(0x1A, 0xF5, 0x9E, 0x0B));
        new ThemedDialog(owner, title, message, "⚠️", accent, bg).ShowDialog();
    }

    public static void ShowError(Window? owner, string title, string message)
    {
        // use your ErrorRedBrush if موجود
        Brush accent;
        try { accent = (Brush)Application.Current.FindResource("ErrorRedBrush"); }
        catch { accent = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); }

        var bg = new SolidColorBrush(Color.FromArgb(0x16, 0xEF, 0x44, 0x44));
        new ThemedDialog(owner, title, message, "⛔", accent, bg).ShowDialog();
    }

    private void Ok_Click(object sender, RoutedEventArgs e) => Close();
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
