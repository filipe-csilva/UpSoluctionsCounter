namespace UpSoluctionsCounter.Services.Interface
{
    public interface IQrCodeService
    {
        Task<string> ScanBarcodeAsync(); // Novo método específico
        Task<string> ScanQrCodeAsync(); // Mantido para compatibilidade
        bool HasCameraPermission();
        Task<bool> RequestCameraPermissionAsync();
    }
}