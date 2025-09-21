using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UpSoluctionsCounter.Models;
using UpSoluctionsCounter.Services;

namespace UpSoluctionsCounter.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private string _code;
        private string _quantity;

        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }

        public string Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductItem> Products { get; } = new();

        public ICommand AddCommand => new Command(AddProduct);
        public ICommand ExportCommand => new Command(Export);

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(Code) || !int.TryParse(Quantity, out var qty)) return;

            var existing = Products.FirstOrDefault(p => p.Code == Code);
            if (existing != null)
            {
                existing.Quantity += qty;
                OnPropertyChanged(nameof(Products)); // Forçar atualização
            }
            else
            {
                Products.Add(new ProductItem { Code = Code, Quantity = qty });
            }

            Code = string.Empty;
            Quantity = string.Empty;
        }

        private async void Export()
        {
            var lines = Products.Select(p => $"{p.Code};{p.Quantity}");
            await FileExportService.ExportToTxtAsync("balanco.txt", lines);
        }
    }
}
