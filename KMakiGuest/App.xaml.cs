using System;
using Xamarin.Forms;

namespace KMakiGuest
{
    public partial class App : Application
    {
        public event EventHandler<OnSleepEventArgs> OnSleepEvent;
        public event EventHandler OnResumeEvent;

        public App()
        {
            InitializeComponent();
            
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            OnSleepEvent?.Invoke(this, new OnSleepEventArgs(((MainPage)MainPage).RestService.ActiveAlert?.ID ?? 0, ((MainPage)MainPage).RestService.TimeOfPost));
        }

        protected override void OnResume()
        {
            OnResumeEvent?.Invoke(this, new EventArgs());
            ((MainPage)MainPage).StartPolling();
        }
    }
}
