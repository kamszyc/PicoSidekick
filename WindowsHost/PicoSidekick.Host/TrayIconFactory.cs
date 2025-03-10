using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsLifetime;

namespace PicoSidekick.Host
{   
    public class TrayIconFactory
    {
        private IGuiContext _guiContext;

        public TrayIconFactory(
            IGuiContext guiContext)
        {
            _guiContext = guiContext;
        }

        public Task CreateTrayIcon()
        {
            _guiContext.Invoke(() =>
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
            });
            return Task.CompletedTask;
        }
    }
}
