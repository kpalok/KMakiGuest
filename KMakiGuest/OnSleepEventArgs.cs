using System;

namespace KMakiGuest
{
    public class OnSleepEventArgs : EventArgs
    {
        public int ActiveAlertID { get; private set; }
        public DateTime TimeOfPost { get; set; }

        public OnSleepEventArgs(int activeAlertID, DateTime timeOfPost)
        {
            ActiveAlertID = activeAlertID;
            TimeOfPost = timeOfPost;
        }
    }
}
