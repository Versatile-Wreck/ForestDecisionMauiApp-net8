// Views/DashboardPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // �첽���ã�������UI�߳�
        _viewModel.LoadDashboardDataCommand.Execute(null);
    }
}