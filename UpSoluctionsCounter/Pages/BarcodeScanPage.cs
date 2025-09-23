using ZXing.Net.Maui;

namespace UpSoluctionsCounter.Pages
{
    // Página de scanner customizada - VERSÃO COM TELA CHEIA
    public class BarcodeScanPage : ContentPage
    {
        private TaskCompletionSource<string> _tcs;
        private ZXing.Net.Maui.Controls.CameraBarcodeReaderView _cameraView;

        public BarcodeScanPage()
        {
            _tcs = new TaskCompletionSource<string>();
            SetupUI();
        }

        private void SetupUI()
        {
            // Configurar a camera view em tela cheia
            _cameraView = new ZXing.Net.Maui.Controls.CameraBarcodeReaderView
            {
                Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.All,
                    AutoRotate = true,
                    Multiple = false
                },
                IsDetecting = true,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            _cameraView.BarcodesDetected += OnBarcodesDetected;

            // Botão de cancelar sobreposto
            var cancelButton = new Button
            {
                Text = "❌ Cancelar",
                BackgroundColor = Color.FromArgb("#E74C3C"),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 25,
                WidthRequest = 120,
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 0, 40)
            };
            cancelButton.Clicked += OnCancelClicked;

            // Label instrucional sobreposto - CORRIGIDO (removido CornerRadius)
            var instructionLabel = new Label
            {
                Text = "Aponte para o código de barras",
                FontSize = 20,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 60, 0, 0),
                BackgroundColor = Color.FromArgb("#80000000"), // Fundo semi-transparente
                Padding = new Thickness(15, 10)
                // CornerRadius removido - Label não tem essa propriedade
            };

            // Usar Grid para sobreposição - CORRIGIDO
            Content = new Grid
            {
                BackgroundColor = Colors.Black,
                Children = {
                _cameraView, // Câmera em tela cheia
                new VerticalStackLayout {
                    Children = { instructionLabel },
                    VerticalOptions = LayoutOptions.Start
                },
                new VerticalStackLayout {
                    Children = { cancelButton },
                    VerticalOptions = LayoutOptions.End
                }
            }
            };
        }

        private void OnBarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            if (e.Results != null && e.Results.Length > 0)
            {
                var barcode = e.Results[0].Value;
                System.Diagnostics.Debug.WriteLine($"[SCAN] Código detectado: {barcode}");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    _tcs.TrySetResult(barcode);
                    await Navigation.PopModalAsync();
                });
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
            await Navigation.PopModalAsync();
        }

        public Task<string> GetResultAsync()
        {
            return _tcs.Task;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_cameraView != null)
            {
                _cameraView.BarcodesDetected -= OnBarcodesDetected;
                _cameraView.IsDetecting = false;
            }
        }
    }
}
