using PicoSidekick.Host.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsLifetime;

namespace PicoSidekick.Host
{
    public class SidekickApplicationContext : ApplicationContext
    {
        public SidekickApplicationContext(SettingsService settingsService, IFormProvider formProvider)
        {
            var trayIcon = new NotifyIcon()
            {
                Icon = new Icon("Pi.ico"),
                Text = "PicoSidekick Host",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            ToolStripMenuItem settingsItem = new()
            {
                Text = "Settings"
            };
            settingsItem.Click += (sender, e) =>
            {
                using var form = formProvider.GetForm<SettingsForm>();
                if (form.ShowDialog() == DialogResult.OK)
                    settingsService.SetFromSettingsForm(form.Settings);
            };
            trayIcon.ContextMenuStrip.Items.Add(settingsItem);

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
