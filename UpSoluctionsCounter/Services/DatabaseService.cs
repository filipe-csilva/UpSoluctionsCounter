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

        public DatabaseService()
        {
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            try
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "inventory_v2.db3");
                Debug.WriteLine($"[DB] Caminho do banco: {databasePath}");

                _database = new SQLiteAsyncConnection(databasePath);

                // Criar tabela com configuração explícita
                var result = await _database.CreateTableAsync<InventoryCount>();
                Debug.WriteLine($"[DB] Tabela criada: {result}");

                _initialized = true;
                Debug.WriteLine("[DB] Banco de dados inicializado com sucesso");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] ERRO ao inicializar banco: {ex.Message}");
                throw;
            }
        }

        public async Task<List<InventoryCount>> GetInventoryCountsAsync()
        {
            await InitializeAsync();

            try
            {
                var counts = await _database.Table<InventoryCount>().ToListAsync();
                Debug.WriteLine($"[DB] Encontradas {counts.Count} contagens");
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
                return await _database.Table<InventoryCount>()
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
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

                // Garantir que temos um ID válido
                if (string.IsNullOrEmpty(count.Id) || count.Id == Guid.Empty.ToString())
                {
                    count.Id = Guid.NewGuid().ToString();
                    Debug.WriteLine($"[DB] Inserindo nova contagem: {count.Id}");

                    var result = await _database.InsertAsync(count);
                    Debug.WriteLine($"[DB] Insert result: {result}");

                    return result == 1; // SQLite retorna 1 para inserção bem-sucedida
                }
                else
                {
                    Debug.WriteLine($"[DB] Atualizando contagem existente: {count.Id}");

                    // Primeiro verificar se existe
                    var existing = await _database.Table<InventoryCount>()
                        .Where(x => x.Id == count.Id)
                        .FirstOrDefaultAsync();

                    if (existing != null)
                    {
                        var result = await _database.UpdateAsync(count);
                        Debug.WriteLine($"[DB] Update result: {result}");
                        return result == 1; // SQLite retorna 1 para update bem-sucedido
                    }
                    else
                    {
                        // Se não existe, insere como novo
                        var result = await _database.InsertAsync(count);
                        Debug.WriteLine($"[DB] Insert (fallback) result: {result}");
                        return result == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] ERRO ao salvar: {ex.Message}");
                throw new Exception("Erro ao salvar contagem no banco de dados");
            }
        }

        public async Task<bool> DeleteInventoryCountAsync(string id)
        {
            await InitializeAsync();

            try
            {
                var result = await _database.DeleteAsync<InventoryCount>(id);
                return result == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Erro ao excluir: {ex.Message}");
                throw;
            }
        }
    }
}
