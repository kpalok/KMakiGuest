using System;

namespace KMakiGuest
{
    public class AlertItem
    {
        public int ID { get; set; }

        public DateTime ServerTime { get; set; }

        public string Message { get; set; }

        public bool Approved { get; set; }

        public bool Declined { get; set; }
    }
}
