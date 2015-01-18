using System;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ICEsTATUS
{
    class Systray : Form
    {
        private NotifyIcon _sysTrayIcon;
        private string _quotaUsed = "Downloading usage...";
        private string _quotaMax = "";

        private const int IconHeight = 47;
        private const int IconWidth = 47;

        public Systray()
        {
            // Initial update
            SysTrayMenuCreate();
            
            // Wait for internet connection
            while (!HttpHelper.IsOnline(HttpHelper.PING_PAGE))
                Thread.Sleep(500);
            
            UpdateSystrayMenu();

            int interval;
            if (!int.TryParse(ConfigurationManager.AppSettings["timerIntervalInHours"], out interval))
                interval = 1;

            var timer = new Timer();
            timer.Tick += Reload;
            timer.Interval = (int)TimeSpan.FromHours(interval).TotalMilliseconds;  // Run every 1 our
            timer.Start();
        }

        private void SysTrayMenuCreate()
        {
            // Create a tray icon. In this example we use a standard system icon for simplicity, but you can of course use your own custom icon too.

            if (_sysTrayIcon == null) {      // First run
                _sysTrayIcon = new NotifyIcon();
                _sysTrayIcon.ContextMenu = CreateContextMenu(_quotaUsed, _quotaMax);
            }
            else
            {
                _sysTrayIcon.ContextMenu.MenuItems.RemoveAt(0);

                var newHeader = new MenuItem(string.Format("{0}% av {1}MB", _quotaUsed, _quotaMax));
                _sysTrayIcon.ContextMenu.MenuItems.Add(0, newHeader);
            }

            _sysTrayIcon.Text = string.Format("ICEsTATUS: {0}%", _quotaUsed);
            _sysTrayIcon.Icon = IconHelper.GetIconFromString(_quotaUsed, IconWidth, IconHeight);

            _sysTrayIcon.Visible = true;
        }
        
            
            
        private void UpdateSystrayMenu()
        {
            var stateDic = HttpHelper.GetCapacityUsage("sigvefast88@yahoo.no", "88jodiProd");

            if (stateDic.Count <= 0)
                return;

            _quotaUsed = stateDic[HttpHelper.UsedQuota];
            _quotaMax = stateDic[HttpHelper.MaxQuota];

            SysTrayMenuCreate();
        }

        // Create a simple tray menu with only one item.
        private ContextMenu CreateContextMenu(string quotaUsed, string quotaMax)
        {
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(string.Format("{0}% av {1}MB", quotaUsed, quotaMax));
            contextMenu.MenuItems.Add("Reload", Reload);
            contextMenu.MenuItems.Add("Exit", OnExit);

            return contextMenu;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;        // Hide form window.
            ShowInTaskbar = false;  // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Reload(object sender, EventArgs e)
        {
            if (HttpHelper.IsOnline(HttpHelper.PING_PAGE))
                UpdateSystrayMenu();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                _sysTrayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
