using System.ComponentModel;

namespace UpSoluctionsCounter.ViewModels
{
    public class InventoryCountViewModel : INotifyPropertyChanged
    {
        private string _name;
        private DateTime _createdDate;
        private int _itemsCount;

        public string Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
            }
        }

        public int ItemsCount
        {
            get => _itemsCount;
            set
            {
                _itemsCount = value;
                OnPropertyChanged(nameof(ItemsCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}