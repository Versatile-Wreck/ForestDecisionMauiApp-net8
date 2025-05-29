// App.xaml.cs
namespace ForestDecisionMauiApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell(); // AppShell handles the initial page display
    }
}