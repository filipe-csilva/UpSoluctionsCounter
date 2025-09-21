using SQLite;
using System.Diagnostics;
using System.Text.Json;
using UpSoluctionsCounter.Models;
using UpSoluctionsCounter.Services.Interface;

namespace UpSoluctionsCounter.Services
{
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _initialized = false;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public DatabaseService()
        {
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            await _semaphore.WaitAsync();
            try
            {
                if (_initialized) return;

                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "inventory.db3");
                Debug.WriteLine($"[DB] Caminho do banco: {databasePath}");

                _database = new SQLiteAsyncConnection(databasePath);

                // Criar tabela
                var result = await _database.CreateTableAsync<InventoryCount>();
                Debug.WriteLine($"[DB] Tabela criada: {result}");

                _initialized = true;
                Debug.WriteLine("[DB] Banco de dados inicializado com sucesso");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Erro ao inicializar banco: {ex.Message}");
                Debug.WriteLine($"[DB] StackTrace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<InventoryCount>> GetInventoryCountsAsync()
        {
            await InitializeAsync();

            try
            {
                Debug.WriteLine("[DB] Buscando todas as contagens...");
                var counts = await _database.Table<InventoryCount>().ToListAsync();
                Debug.WriteLine($"[DB] Encontradas {counts.Count} contagens");

                // Ordenar manualmente
                return counts.OrderByDescending(x => x.ModifiedDate ?? x.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Erro ao buscar contagens: {ex.Message}");
                return new List<InventoryCount>();
            }
        }

        public async Task<InventoryCount> GetInventoryCountAsync(string id)
        {
            await InitializeAsync();

            try
            {
                Debug.WriteLine($"[DB] Buscando contagem: {id}");
                var count = await _database.Table<InventoryCount>()
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();

                if (count != null)
                {
                    Debug.WriteLine($"[DB] Contagem encontrada: {count.Name}");
                }
                else
                {
                    Debug.WriteLine($"[DB] Contagem não encontrada: {id}");
                }

                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Erro ao buscar contagem: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SaveInventoryCountAsync(InventoryCount count)
        {
            await InitializeAsync();

            try
            {
                count.ModifiedDate = DateTime.Now;

                Debug.WriteLine($"[DB] Salvando contagem: {count.Name}");
                Debug.WriteLine($"[DB] ID: {count.Id}");

                int result;
                if (string.IsNullOrEmpty(count.Id) || count.Id == Guid.Empty.ToString())
                {
                    count.Id = Guid.NewGuid().ToString();
                    result = await _database.InsertAsync(count);
                    Debug.WriteLine($"[DB] INSERT result: {result}");
                }
                else
                {
                    result = await _database.UpdateAsync(count);
                    Debug.WriteLine($"[DB] UPDATE result: {result}");
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] ERRO ao salvar contagem: {ex.Message}");
                Debug.WriteLine($"[DB] StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[DB] InnerException: {ex.InnerException.Message}");
                }

                // Mostrar erro mais específico para o usuário
                var errorMessage = ex.Message;
                if (errorMessage.Contains("no such table"))
                    errorMessage = "Erro no banco de dados. Reinicie o aplicativo.";
                else if (errorMessage.Contains("constraint"))
                    errorMessage = "Erro de dados. Verifique os valores informados.";

                throw new Exception(errorMessage, ex);
            }
        }

        public async Task<bool> DeleteInventoryCountAsync(string id)
        {
            await InitializeAsync();

            try
            {
                Debug.WriteLine($"[DB] Excluindo contagem: {id}");
                var result = await _database.DeleteAsync<InventoryCount>(id);
                Debug.WriteLine($"[DB] DELETE result: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Erro ao excluir contagem: {ex.Message}");
                throw;
            }
        }

        // Método para debug da estrutura da tabela (opcional)
        public async Task DebugTableStructure()
        {
            try
            {
                Debug.WriteLine("[DB] Estrutura da tabela InventoryCount:");

                // Método alternativo para verificar a estrutura
                var sampleData = await _database.Table<InventoryCount>().FirstOrDefaultAsync();
                if (sampleData != null)
                {
                    Debug.WriteLine($"[DB] Exemplo de dados: {JsonSerializer.Serialize(sampleData)}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Erro ao debuggar estrutura: {ex.Message}");
            }
        }
    }
}
