using UpSoluctionsCounter.Services.Interface;
using ZXing.Net.Maui;
using Microsoft.Maui.Controls;

namespace UpSoluctionsCounter.Services
{
    public class QrCodeService : IQrCodeService
    {
        public async Task<string> ScanQrCodeAsync()
        {
            try
            {
                // Verificar e solicitar permissão da câmera
                if (!HasCameraPermission())
                {
                    var granted = await RequestCameraPermissionAsync();
                    if (!granted)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Permissão",
                            "É necessária permissão da câmera para escanear QR Codes",
                            "OK");
                        return null;
                    }
                }

                // Configurar opções do leitor
                var options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.All,
                    AutoRotate = true,
                    Multiple = false
                };

                // Criar a página do leitor
                var barcodeReader = new ZXing.Net.Maui.Controls.CameraBarcodeReaderView
                {
                    Options = options,
                    IsDetecting = true
                };

                // Criar página customizada para o scanner
                var scanPage = new ContentPage
                {
                    Content = barcodeReader,
                    Title = "Escanear QR Code"
                };

                // Aguardar o resultado
                string result = null;
                barcodeReader.BarcodesDetected += (sender, e) =>
                {
                    var first = e.Results.FirstOrDefault();
                    if (first != null)
                    {
                        result = first.Value;
                        Application.Current.MainPage.Navigation.PopAsync();
                    }
                };

                // Navegar para a página do scanner
                await Application.Current.MainPage.Navigation.PushAsync(scanPage);

                // Aguardar até ter resultado ou página fechar
                while (Application.Current.MainPage.Navigation.NavigationStack.Contains(scanPage))
                {
                    await Task.Delay(100);
                    if (result != null) break;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao escanear QR Code: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao acessar a câmera", "OK");
                return null;
            }
        }

        public bool HasCameraPermission()
        {
            return Permissions.CheckStatusAsync<Permissions.Camera>().Result == PermissionStatus.Granted;
        }

        public async Task<bool> RequestCameraPermissionAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao solicitar permissão: {ex.Message}");
                return false;
            }
        }
    }
}