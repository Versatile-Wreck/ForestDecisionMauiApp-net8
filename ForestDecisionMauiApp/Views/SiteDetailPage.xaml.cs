// Views/SiteDetailPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class SiteDetailPage : ContentPage // <-- 确保这里也是 TabbedPage
{
    // 构造函数保持不变
    public SiteDetailPage(SiteDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}