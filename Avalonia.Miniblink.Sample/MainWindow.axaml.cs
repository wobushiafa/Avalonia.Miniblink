using Avalonia.Controls;

namespace Avalonia.Miniblink.Sample;

public partial class MainWindow : Window
{
    private readonly MiniblinkBrowser _miniblinkBrowser;
    
    public MainWindow()
    {
        InitializeComponent();
        _miniblinkBrowser = this.FindControl<MiniblinkBrowser>("MiniblinkBrowser")!;
    }
}