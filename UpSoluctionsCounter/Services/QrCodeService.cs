using UpSoluctionsCounter.Pages;
using UpSoluctionsCounter.Services.Interface;
using ZXing.Net.Maui;

namespace UpSoluctionsCounter.Services
{
    public class QrCodeService : IQrCodeService
    {
        public async Task<string> ScanBarcodeAsync()
        {
            try
            {
                // Verificar permissão da câmera
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permissão", "A câmera é necessária para escanear códigos", "OK");
                        return null;
                    }
                }

                // Criar a página de scanner
                var scannerPage = new BarcodeScanPage();

                // Navegar como modal em tela cheia
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(scannerPage, true);
                }

                // Aguardar o resultado
                var result = await scannerPage.GetResultAsync();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SCAN] Erro: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao abrir a câmera", "OK");
                return null;
            }
        }

        public async Task<string> ScanQrCodeAsync()
        {
            return await ScanBarcodeAsync();
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
                System.Diagnostics.Debug.WriteLine($"Erro permissão: {ex.Message}");
                return false;
            }
        }
    }

    //// Página de scanner customizada
    //public class BarcodeScanPage : ContentPage
    //{
    //    private TaskCompletionSource<string> _tcs;
    //    private ZXing.Net.Maui.Controls.CameraBarcodeReaderView _cameraView;

    //    public BarcodeScanPage()
    //    {
    //        _tcs = new TaskCompletionSource<string>();
    //        SetupUI();
    //    }

    //    private void SetupUI()
    //    {
    //        // Configurar a camera view
    //        _cameraView = new ZXing.Net.Maui.Controls.CameraBarcodeReaderView
    //        {
    //            Options = new BarcodeReaderOptions
    //            {
    //                Formats = BarcodeFormats.All,
    //                AutoRotate = true,
    //                Multiple = false
    //            },
    //            IsDetecting = true,
    //            HeightRequest = 400
    //        };

    //        _cameraView.BarcodesDetected += OnBarcodesDetected;

    //        var cancelButton = new Button
    //        {
    //            Text = "Cancelar",
    //            BackgroundColor = Colors.Red,
    //            TextColor = Colors.White,
    //            Margin = new Thickness(20),
    //            HeightRequest = 50
    //        };
    //        cancelButton.Clicked += OnCancelClicked;

    //        Content = new VerticalStackLayout
    //        {
    //            Children = {
    //                new Label {
    //                    Text = "Aponte para o código de barras",
    //                    FontSize = 18,
    //                    TextColor = Colors.White,
    //                    HorizontalOptions = LayoutOptions.Center,
    //                    Margin = new Thickness(0, 20, 0, 10)
    //                },
    //                _cameraView,
    //                cancelButton
    //            },
    //            BackgroundColor = Colors.Black
    //        };
    //    }

    //    private void OnBarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    //    {
    //        if (e.Results != null && e.Results.Length > 0)
    //        {
    //            var barcode = e.Results[0].Value;
    //            Device.BeginInvokeOnMainThread(async () =>
    //            {
    //                _tcs.TrySetResult(barcode);
    //                await Navigation.PopModalAsync();
    //            });
    //        }
    //    }

    //    private async void OnCancelClicked(object sender, EventArgs e)
    //    {
    //        _tcs.TrySetResult(null);
    //        await Navigation.PopModalAsync();
    //    }

    //    public Task<string> GetResultAsync()
    //    {
    //        return _tcs.Task;
    //    }

    //    protected override void OnDisappearing()
    //    {
    //        base.OnDisappearing();
    //        _cameraView.BarcodesDetected -= OnBarcodesDetected;
    //        _cameraView.IsDetecting = false;
    //    }
    //}
}