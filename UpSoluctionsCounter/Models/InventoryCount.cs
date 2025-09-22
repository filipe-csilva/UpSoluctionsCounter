using SQLite;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace UpSoluctionsCounter.Models
{
    public class InventoryCount
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public bool IsCompleted { get; set; }
        public string ProductsJson { get; set; }

        // Propriedade para ordenação
        public DateTime SortDate => ModifiedDate ?? CreatedDate;

        public ObservableCollection<ProductItem> GetProducts()
        {
            if (string.IsNullOrEmpty(ProductsJson))
                return new ObservableCollection<ProductItem>();

            try
            {
                var products = JsonSerializer.Deserialize<List<ProductItem>>(ProductsJson);
                return new ObservableCollection<ProductItem>(products);
            }
            catch
            {
                return new ObservableCollection<ProductItem>();
            }
        }

        public void SetProducts(ObservableCollection<ProductItem> products)
        {
            var productList = products.ToList();
            ProductsJson = JsonSerializer.Serialize(productList);
        }
    }
}