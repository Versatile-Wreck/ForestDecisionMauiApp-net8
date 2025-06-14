// Interfaces/IFileSaveService.cs
namespace ForestDecisionMauiApp.Interfaces
{
    public interface IFileSaveService
    {
        // 这个方法会打开“另存为”对话框，并返回用户选择的完整文件路径。
        // 如果用户取消，则返回 null。
        Task<string> SaveFileAsync(string suggestedFileName, string fileExtension);
    }
}