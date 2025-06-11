using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;
public partial class AddEditUserPage : ContentPage
{
    private readonly AddEditUserViewModel _viewModel;
    public AddEditUserPage(AddEditUserViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeAsync();
    }
}