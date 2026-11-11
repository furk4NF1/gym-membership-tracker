using System;
using System.Windows.Forms;

namespace SporSalonuTakip.BLL
{
    public static class SessionManager
    {
        private static readonly Timer timer = new Timer();
        private static readonly ActivityFilter filter = new ActivityFilter();
        private static DateTime lastActivityUtc;
        private static bool warningShown;
        private static bool running;
        public static event EventHandler Warning;
        public static event EventHandler Expired;

        static SessionManager()
        {
            timer.Interval = 15000;
            timer.Tick += CheckSession;
            filter.Activity += (s, e) => { lastActivityUtc = DateTime.UtcNow; warningShown = false; };
        }

        public static void Start()
        {
            Stop();
            lastActivityUtc = DateTime.UtcNow; warningShown = false; running = true;
            Application.AddMessageFilter(filter); timer.Start();
        }

        public static void Stop()
        {
            timer.Stop();
            if (running) Application.RemoveMessageFilter(filter);
            running = false; warningShown = false;
        }

        private static void CheckSession(object sender, EventArgs e)
        {
            if (!running) return;
            TimeSpan idle = DateTime.UtcNow - lastActivityUtc;
            if (idle >= TimeSpan.FromMinutes(15))
            {
                Stop();
                EventHandler expired = Expired; if (expired != null) expired(null, EventArgs.Empty);
            }
            else if (idle >= TimeSpan.FromMinutes(14) && !warningShown)
            {
                warningShown = true;
                EventHandler warning = Warning; if (warning != null) warning(null, EventArgs.Empty);
            }
        }

        private sealed class ActivityFilter : IMessageFilter
        {
            public event EventHandler Activity;
            public bool PreFilterMessage(ref Message message)
            {
                int id = message.Msg;
                if ((id >= 0x0100 && id <= 0x0109) || (id >= 0x0200 && id <= 0x020E) || (id >= 0x00A0 && id <= 0x00AD))
                {
                    EventHandler activity = Activity; if (activity != null) activity(this, EventArgs.Empty);
                }
                return false;
            }
        }
    }
}
