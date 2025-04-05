using Microsoft.Extensions.DependencyInjection;
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
        public SidekickApplicationContext(SettingsService settingsService, IFormProvider formProvider, IServiceScopeFactory serviceScopeFactory)
        {
            var trayIcon = new NotifyIcon()
            {
                Icon = new Icon("Pi.ico"),
                Text = "Pico Sidekick Host",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            ToolStripMenuItem settingsItem = new()
            {
                Text = "Settings"
            };
            IServiceScope scope = serviceScopeFactory.CreateScope();
            settingsItem.Click += (sender, e) =>
            {
                var form = formProvider.GetForm<SettingsForm>(scope);
                if (form.CanFocus)
                {
                    form.WindowState = FormWindowState.Normal;
                    form.Focus();
                }
                else
                {
                    try
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                            settingsService.SetFromSettingsForm(form.Settings);
                    }
                    finally
                    {
                        form.Dispose();
                        scope.Dispose();
                        scope = serviceScopeFactory.CreateScope();
                    }
                }
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
