using System.Diagnostics;

namespace UpSoluctionsCounter.Services
{
    public class FileExportService
    {
        public static async Task ExportToTxtAsync(string filename, IEnumerable<string> lines)
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, filename);
                await File.WriteAllLinesAsync(path, lines);

                // Usar DisplayAlert na thread UI
                await Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("✅ Exportado", $"Arquivo salvo em:\n{path}", "OK");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Export] Erro: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("❌ Erro", "Falha ao exportar arquivo", "OK");
            }
        }
    }
}