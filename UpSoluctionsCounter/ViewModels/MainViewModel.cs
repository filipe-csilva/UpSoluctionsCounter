using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;
using UpSoluctionsCounter.Models;
using UpSoluctionsCounter.Services;
using UpSoluctionsCounter.Services.Interface;

namespace UpSoluctionsCounter.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private readonly IDatabaseService _databaseService;
        private readonly IQrCodeService _qrCodeService;
        private string _code;
        private string _quantity;
        private InventoryCount _currentCount;
        private bool _isCounting;

        public ObservableCollection<InventoryCountViewModel> InventoryCounts { get; } = new();
        public ObservableCollection<ProductItem> Products { get; } = new();

        public string Code { get => _code; set { _code = value; OnPropertyChanged(); } }
        public string Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }
        public bool IsCounting { get => _isCounting; set { _isCounting = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotCounting)); } }
        public bool IsNotCounting => !IsCounting;

        public ICommand AddCommand => new Command(AddProduct);
        public ICommand ExportCommand => new Command(async () => await Export());
        public ICommand StartNewCountCommand => new Command(async () => await StartNewCount());
        public ICommand SaveCountCommand => new Command(async () => await SaveCount());
        public ICommand CancelCountCommand => new Command(CancelCount);
        public ICommand LoadCountCommand => new Command<InventoryCountViewModel>(async (count) => await LoadCount(count));
        public ICommand DeleteCountCommand => new Command<InventoryCountViewModel>(async (count) => await DeleteCount(count));
        public ICommand ScanQrCodeCommand => new Command(async () => await ScanQrCode());

        public MainViewModel(IDatabaseService databaseService, IQrCodeService qrCodeService)
        {
            _databaseService = databaseService;
            _qrCodeService = qrCodeService;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await LoadInventoryCounts();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao inicializar: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao carregar contagens. Reinicie o aplicativo.", "OK");
            }
        }

        private async Task ScanQrCode()
        {
            try
            {
                var scanResult = await _qrCodeService.ScanQrCodeAsync();

                if (!string.IsNullOrEmpty(scanResult))
                {
                    Code = scanResult;
                    // Foca automaticamente no campo de quantidade
                    await Task.Delay(500);
                    OnFocusQuantityRequested?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QR] Erro ao escanear: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao escanear QR Code", "OK");
            }
        }

        // Adicione esta propriedade para o foco
        public event Action OnFocusQuantityRequested;

        public void RequestFocusOnQuantity()
        {
            OnFocusQuantityRequested?.Invoke();
        }

        private async Task LoadInventoryCounts()
        {
            try
            {
                var counts = await _databaseService.GetInventoryCountsAsync();

                await Application.Current.MainPage.Dispatcher.DispatchAsync(() =>
                {
                    InventoryCounts.Clear();

                    foreach (var count in counts)
                    {
                        var products = count.GetProducts();
                        InventoryCounts.Add(new InventoryCountViewModel
                        {
                            Id = count.Id,
                            Name = count.Name,
                            CreatedDate = count.CreatedDate,
                            ItemsCount = products.Count
                        });
                    }

                    Debug.WriteLine($"[VM] Carregadas {InventoryCounts.Count} contagens na UI");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao carregar contagens: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao carregar contagens.", "OK");
            }
        }

        private void AddProduct()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Code) || !int.TryParse(Quantity, out var qty) || qty <= 0)
                {
                    Application.Current.MainPage.DisplayAlert("Aviso", "Código inválido ou quantidade deve ser maior que zero", "OK");
                    return;
                }

                var existing = Products.FirstOrDefault(p => p.Code == Code);
                if (existing != null)
                {
                    existing.Quantity += qty;
                    Debug.WriteLine($"[VM] Produto atualizado: {Code}, Quantidade: {existing.Quantity}");
                }
                else
                {
                    Products.Add(new ProductItem { Code = Code, Quantity = qty });
                    Debug.WriteLine($"[VM] Novo produto adicionado: {Code}, Quantidade: {qty}");
                }

                Code = string.Empty;
                Quantity = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao adicionar produto: {ex.Message}");
                Application.Current.MainPage.DisplayAlert("Erro", "Falha ao adicionar produto", "OK");
            }
        }

        private async Task Export()
        {
            if (Products.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Nenhum produto para exportar", "OK");
                return;
            }

            try
            {
                var lines = new List<string> { "Código;Quantidade" };
                lines.AddRange(Products.Select(p => $"{p.Code};{p.Quantity}"));

                await FileExportService.ExportToTxtAsync($"balanco_{DateTime.Now:yyyyMMdd_HHmmss}.txt", lines);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao exportar: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao exportar", "OK");
            }
        }

        private async Task StartNewCount()
        {
            try
            {
                var result = await Application.Current.MainPage.DisplayPromptAsync(
                    "Nova Contagem",
                    "Nome da contagem:",
                    "Iniciar",
                    "Cancelar",
                    "Ex: Contagem Loja A",
                    maxLength: 50);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    _currentCount = new InventoryCount { Name = result };
                    Products.Clear();
                    IsCounting = true;
                    Debug.WriteLine($"[VM] Nova contagem iniciada: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao iniciar contagem: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao iniciar contagem", "OK");
            }
        }

        private async Task SaveCount()
        {
            if (Products.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Nenhum produto adicionado à contagem.", "OK");
                return;
            }

            try
            {
                Debug.WriteLine("=== TENTATIVA DE SALVAMENTO ===");

                _currentCount.SetProducts(new ObservableCollection<ProductItem>(Products));

                // Primeira tentativa
                var success = await _databaseService.SaveInventoryCountAsync(_currentCount);

                if (!success)
                {
                    Debug.WriteLine("[VM] Primeira tentativa falhou, tentando abordagem alternativa...");

                    // Segunda tentativa: criar uma nova contagem
                    var newCount = new InventoryCount
                    {
                        Name = _currentCount.Name,
                        CreatedDate = DateTime.Now
                    };
                    newCount.SetProducts(new ObservableCollection<ProductItem>(Products));

                    success = await _databaseService.SaveInventoryCountAsync(newCount);
                }

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("✅ Sucesso", "Contagem salva com sucesso!", "OK");
                    CancelCount();
                    await LoadInventoryCounts();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("❌ Erro", "Não foi possível salvar a contagem", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] ERRO: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("❌ Erro", ex.Message, "OK");
            }
        }

        private void CancelCount()
        {
            try
            {
                Products.Clear();
                _currentCount = null;
                IsCounting = false;
                Debug.WriteLine("[VM] Contagem cancelada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao cancelar contagem: {ex.Message}");
            }
        }

        private async Task LoadCount(InventoryCountViewModel countVm)
        {
            try
            {
                Debug.WriteLine($"[VM] Carregando contagem: {countVm.Name}");
                var count = await _databaseService.GetInventoryCountAsync(countVm.Id);
                if (count != null)
                {
                    Products.Clear();
                    var products = count.GetProducts();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    _currentCount = count;
                    IsCounting = true;
                    Debug.WriteLine($"[VM] Contagem carregada: {products.Count} produtos");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao carregar contagem: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao carregar contagem", "OK");
            }
        }

        private async Task DeleteCount(InventoryCountViewModel countVm)
        {
            try
            {
                var confirm = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar",
                    $"Deseja excluir a contagem '{countVm.Name}'?",
                    "Sim",
                    "Não");

                if (confirm)
                {
                    await _databaseService.DeleteInventoryCountAsync(countVm.Id);
                    await LoadInventoryCounts();
                    Debug.WriteLine($"[VM] Contagem excluída: {countVm.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Erro ao excluir contagem: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao excluir contagem", "OK");
            }
        }

        // Adicione este método no MainViewModel
        private async Task DebugSaveProcess()
        {
            try
            {
                Debug.WriteLine("=== DEBUG DO PROCESSO DE SALVAMENTO ===");
                Debug.WriteLine($"Nome da contagem: {_currentCount.Name}");
                Debug.WriteLine($"Quantidade de produtos: {Products.Count}");

                // Verificar cada produto
                foreach (var product in Products)
                {
                    Debug.WriteLine($"Produto: {product.Code}, Quantidade: {product.Quantity}");
                }

                // Testar a serialização
                var testProducts = new ObservableCollection<ProductItem>(Products);
                var testJson = JsonSerializer.Serialize(testProducts);
                Debug.WriteLine($"JSON de teste: {testJson}");
                Debug.WriteLine($"Tamanho do JSON: {testJson.Length} caracteres");

                // Testar o banco com dados simples
                var testCount = new InventoryCount
                {
                    Name = "TESTE_DEBUG",
                    ProductsJson = "[{\"Code\":\"123\",\"Quantity\":10}]"
                };

                var testResult = await _databaseService.SaveInventoryCountAsync(testCount);
                Debug.WriteLine($"Teste de salvamento: {testResult}");

                Debug.WriteLine("=== FIM DO DEBUG ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Erro no debug: {ex.Message}");
            }
        }
    }
}