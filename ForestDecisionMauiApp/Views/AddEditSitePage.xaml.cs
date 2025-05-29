// Views/AddEditSitePage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class AddEditSitePage : ContentPage
{
    private readonly AddEditSiteViewModel _viewModel;
    public AddEditSitePage(AddEditSiteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // ��ҳ�����ʱ������ ViewModel �ĳ�ʼ������
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(); // �������ݻ���������ģʽ
    }
}