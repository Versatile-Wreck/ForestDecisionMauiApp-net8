// Views/SiteManagementPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;
using ForestDecisionMauiApp.Models; // ȷ�������� MonitoringSite
using Syncfusion.Maui.DataGrid; // <-- ���� using ������Ϊ�˷���ʹ�� SfDataGrid ����
using System.ComponentModel; // <-- ȷ������� using


namespace ForestDecisionMauiApp.Views;

public partial class SiteManagementPage : ContentPage
{
    //  private bool _isInitialLoad = true; // ��־�Ƿ��ǳ��μ���

    private readonly SiteManagementViewModel _viewModel;

    public SiteManagementPage(SiteManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {

        // ���������߼����ֲ���
        if (_viewModel.LoadSitesCommand.CanExecute(null))
        {
            _viewModel.LoadSitesCommand.Execute(null);
        }
    }


    // --- �������¼������� ---
    private void OnGridSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
    {
        if (BindingContext is not SiteManagementViewModel viewModel)
        {
            return;
        }

        // 1. e.AddedRows ��������ʵ�ʵ����ݶ�������Ϊ object��
        var selectedItem = e.AddedRows.FirstOrDefault();

        // 2. ֱ�ӽ�ѡ�е� object ת��Ϊ MonitoringSite ����
        if (selectedItem is MonitoringSite selectedSite)
        {
            // 3. ����ȷת����Ķ���ֵ�� ViewModel
            viewModel.SelectedSite = selectedSite;
        }
        else
        {
            // 4. ���û��ѡ��������Ͳ�ƥ�䣬�����ѡ��
            viewModel.SelectedSite = null;
        }
    }

    // **����������˫���¼��ķ���**
    private async void OnGridCellDoubleTapped(object sender, DataGridCellDoubleTappedEventArgs e)
    {
        // e.RowData ������˫����Ԫ�����ڵ��е����ݶ���
        if (e.RowData is MonitoringSite siteToView)
        {
            // ����������ҳ������ SiteID ��Ϊ�������ݹ�ȥ
            await Shell.Current.GoToAsync($"{nameof(SiteDetailPage)}?siteId={siteToView.SiteID}");
        }
    }

}