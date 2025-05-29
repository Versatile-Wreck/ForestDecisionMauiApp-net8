// Views/MainDataPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class MainDataPage : ContentPage
{
  //  private bool _isInitialLoad = true; // ��־�Ƿ��ǳ��μ���

    public MainDataPage(MainDataViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainDataViewModel vm)
        {
            // ÿ��ҳ�����ʱ�����Լ���/ˢ������
            // ���ߣ����������һ����־λ�����ڴ� AddEditSitePage ����ʱ��ǿ��ˢ��
            if (vm.LoadSitesCommand.CanExecute(null))
            {
                await vm.LoadSitesCommand.ExecuteAsync(null);
            }
        }
    }
}