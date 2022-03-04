using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace KMakiGuest
{
    public partial class MainPage : ContentPage
    {
        private readonly RestService restService;

        public RestService RestService
        {
            get { return restService; }
        }

        private enum PollingResult
        {
            None = 0,
            Accepted = 1,
            Declined = 2
        }

        public MainPage()
        {
            restService = new RestService();

            InitializeComponent();
        }

        public async void StartPolling()
        {
            PollingResult pollingResult = await RefreshPollingResult();

            while (pollingResult == PollingResult.None && DateTime.Now.Subtract(restService.TimeOfPost).TotalMinutes < Constants.PollMinutes)
            {
                System.Threading.Thread.Sleep(1000);

                pollingResult = await RefreshPollingResult();
            }

            DisplayPollingResult(pollingResult);
        }

        private async Task<PollingResult> RefreshPollingResult()
        {
            PollingResult pollingResult = PollingResult.None;

            await restService.RefreshActiveAlert();

            if (restService.ActiveAlert?.Approved ?? false)
            {
                pollingResult = PollingResult.Accepted;
            }
            else if (restService.ActiveAlert?.Declined ?? false)
            {
                pollingResult = PollingResult.Declined;
            }

            return pollingResult;
        }

        private void ChangeUIView(bool polling, bool showResults)
        {
            if (!eMessage.IsEnabled && !polling && !showResults)
            {
                eMessage.Text = string.Empty;
            }
            eMessage.IsEnabled = !polling && !showResults;
            ButtonGrid.IsVisible = !polling && !showResults;
            ActiveAlertGrid.IsVisible = polling;
            aiPolling.IsRunning = polling;
            PollResultGrid.IsVisible = showResults;
        }

        private void DisplayPollingResult(PollingResult pollingResult)
        {
            switch (pollingResult)
            {
                case PollingResult.None:
                    PollResultGrid.BackgroundColor = Color.Yellow;
                    lblPollResult.Text = "Ei vastausta 😴";
                    break;

                case PollingResult.Accepted:
                    PollResultGrid.BackgroundColor = Color.Green;
                    lblPollResult.Text = "Tervetuloa! 🤝";
                    break;

                case PollingResult.Declined:
                    PollResultGrid.BackgroundColor = Color.Red;
                    lblPollResult.Text = "Ei passaa 🙁";
                    break;
                default:
                    break;
            }

            ChangeUIView(false, true);
        }

        private async void btnBell_Clicked(object sender, EventArgs e)
        {
            await restService.PostAlert(new AlertItem { Message = eMessage.Text });

            if (restService.ActiveAlert != null)
            {
                ChangeUIView(true, false);
                StartPolling();
            }
        }

        private async void btnContinue_Clicked(object sender, EventArgs e)
        {
            ChangeUIView(false, false);
            await restService.DeleteActiveAlert();
        }
    }
}
