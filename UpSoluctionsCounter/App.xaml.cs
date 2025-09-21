using Microsoft.Extensions.DependencyInjection;

namespace UpSoluctionsCounter
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var mainPage = serviceProvider.GetService<MainPage>();
            MainPage = new NavigationPage(mainPage);
        }
    }
}