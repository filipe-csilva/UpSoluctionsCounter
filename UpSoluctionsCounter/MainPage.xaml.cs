using UpSoluctionsCounter.ViewModels;

namespace UpSoluctionsCounter
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}