using UpSoluctionsCounter.Models;

namespace UpSoluctionsCounter.Services.Interface
{
    public interface IDatabaseService
    {
        Task InitializeAsync();
        Task<List<InventoryCount>> GetInventoryCountsAsync();
        Task<InventoryCount> GetInventoryCountAsync(string id);
        Task<bool> SaveInventoryCountAsync(InventoryCount count);
        Task<bool> DeleteInventoryCountAsync(string id);
    }
}
