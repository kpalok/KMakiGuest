using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace KMakiGuest.Droid
{
    [Service]
    public class NotificationService : Service
    {
        private NotificationCompat.Builder notificationBuilder;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // From shared code or in your PCL
            notificationBuilder = new NotificationCompat.Builder(this)
                .SetSmallIcon(Resource.Drawable.notification_icon_background)
                .SetContentTitle("Kmaki hotline")
                .SetChannelId("Kmaki");

            _ = StartPolling(intent.GetIntExtra("ActiveAlertID", 0), DateTime.Parse(intent.GetStringExtra("TimeOfPost")));

            return StartCommandResult.NotSticky;
        }

        private async System.Threading.Tasks.Task StartPolling(int activeAlertID, DateTime timeOfPost)
        {
            RestService restService = new RestService();
            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            NotificationChannel channel = new NotificationChannel("Kmaki", "Kmaki", NotificationImportance.Default);
            Intent notificationIntent = new Intent(this, typeof(MainActivity));
            PendingIntent contentIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 0);

            notificationManager.CreateNotificationChannel(channel);

            if (activeAlertID > 0)
            {
                restService.CreateActiveAlertWithID(activeAlertID);
            }

            while (restService.ActiveAlert != null && DateTime.Now.Subtract(timeOfPost).TotalMinutes < Constants.PollMinutes)
            {
                await restService.RefreshActiveAlert();

                if (restService.ActiveAlert != null && (restService.ActiveAlert.Approved || restService.ActiveAlert.Declined))
                {
                    string messageText = restService.ActiveAlert.Approved
                                       ? "Tervetuloa!"
                                       : "Pyyntönne on hylätty";

                    notificationBuilder = notificationBuilder.SetContentText(messageText)
                                            .SetVisibility(NotificationCompat.VisibilityPublic)
                                            .SetContentIntent(contentIntent);

                    notificationManager.Notify(restService.ActiveAlert.ID, notificationBuilder.Build());
                    break;
                }

                Thread.Sleep(1000);
            }

            if (restService.ActiveAlert != null && !restService.ActiveAlert.Approved && !restService.ActiveAlert.Declined)
            {
                notificationBuilder = notificationBuilder.SetContentText("Ei vastausta...")
                        .SetVisibility(NotificationCompat.VisibilityPublic)
                        .SetContentIntent(contentIntent);

                notificationManager.Notify(restService.ActiveAlert.ID, notificationBuilder.Build());
            }
        }
    }
}