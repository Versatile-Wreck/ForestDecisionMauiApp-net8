// Views/RegisterPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel) // ����ע�� viewModel
    {
        InitializeComponent(); // ��ʼ������������Զ����ɵķ��������캯���б������ȵ��ã�
        BindingContext = viewModel; // �������ݰ�������Ϊ����� ViewModel ʵ��
    }
}