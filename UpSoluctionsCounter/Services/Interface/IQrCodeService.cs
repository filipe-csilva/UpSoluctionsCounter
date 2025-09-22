namespace UpSoluctionsCounter.Services.Interface
{
    public interface IQrCodeService
    {
        Task<string> ScanQrCodeAsync();
        bool HasCameraPermission();
        Task<bool> RequestCameraPermissionAsync();
    }
}
