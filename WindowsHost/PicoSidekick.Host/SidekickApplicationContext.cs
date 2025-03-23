using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host
{
    public class SidekickApplicationContext : ApplicationContext
    {
        public SidekickApplicationContext()
        {
            var trayIcon = new NotifyIcon()
            {
                Icon = new Icon("Pi.ico"),
                Text = "PicoSidekick Host",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
            ToolStripMenuItem exitItem = new()
            {
                Text = "Exit"
            };
            exitItem.Click += (sender, e) =>
            {
                Application.Exit();
            };
            trayIcon.ContextMenuStrip.Items.Add(exitItem);
        }
    }
}
