using System.Globalization;

namespace UpSoluctionsCounter.Utils
{
    public static class AppResources
    {
        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR"));
        }

        public static string FormatNumber(int number)
        {
            return number.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"));
        }

        public static async Task<bool> ConfirmAction(string title, string message)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, "Sim", "Não");
        }
    }
}