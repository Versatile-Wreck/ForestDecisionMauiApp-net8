// Platforms/Windows/FileSaveService.cs
using ForestDecisionMauiApp.Interfaces;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ForestDecisionMauiApp.Platforms.Windows
{
    public class FileSaveService : IFileSaveService
    {
        public async Task<string> SaveFileAsync(string suggestedFileName, string fileExtension)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // 设置文件类型过滤器
            savePicker.FileTypeChoices.Add("SQLite Database", new List<string>() { fileExtension });
            savePicker.SuggestedFileName = suggestedFileName;

            // 获取当前窗口的句柄 (HWND)，这是让对话框正确弹出的关键
            var window = App.Current.Windows.FirstOrDefault() ?? throw new InvalidOperationException("No active window found");
            var hwnd = WindowNative.GetWindowHandle(window.Handler.PlatformView);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            StorageFile file = await savePicker.PickSaveFileAsync();

            return file?.Path; // 如果用户选择了路径，则返回完整路径；否则返回 null
        }
    }
}