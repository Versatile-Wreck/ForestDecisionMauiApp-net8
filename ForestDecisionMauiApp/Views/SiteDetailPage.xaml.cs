// Views/SiteDetailPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class SiteDetailPage : ContentPage // <-- ȷ������Ҳ�� TabbedPage
{
    // ���캯�����ֲ���
    public SiteDetailPage(SiteDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}