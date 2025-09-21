namespace UpSoluctionsCounter.Services
{
    public class FileExportService
    {
        public static async Task ExportToTxtAsync(string filename, IEnumerable<string> lines)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, filename);
            await File.WriteAllLinesAsync(path, lines);
            await Application.Current.MainPage.DisplayAlert("Exportado", $"Arquivo salvo em:\n{path}", "OK");
        }
    }
}
