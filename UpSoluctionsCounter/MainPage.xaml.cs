using UpSoluctionsCounter.ViewModels;

namespace UpSoluctionsCounter
{
    public partial class MainPage : ContentPage
    {
        private Action _focusHandler;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            _focusHandler = () => QuantityEntry.Focus();
            viewModel.OnFocusQuantityRequested += _focusHandler;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is MainViewModel viewModel)
            {
                viewModel.OnFocusQuantityRequested -= _focusHandler;
            }
        }
    }
}