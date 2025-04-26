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
        private readonly SettingsService settingsService;
        private readonly IFormProvider formProvider;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private IServiceScope scope;

        public SidekickApplicationContext(SettingsService settingsService, IFormProvider formProvider, IServiceScopeFactory serviceScopeFactory)
        {
            this.settingsService = settingsService;
            this.formProvider = formProvider;
            this.serviceScopeFactory = serviceScopeFactory;
            this.scope = serviceScopeFactory.CreateScope();
            CreateTrayIcon();
        }

        private void CreateTrayIcon()
        {
            var trayIcon = new NotifyIcon()
            {
                Icon = new Icon("Pi.ico"),
                Text = "Pico Sidekick Host",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };
            trayIcon.DoubleClick += (sender, e) =>
            {
                HandleSettingsClick();
            };

            var settingsItem = new ToolStripMenuItem()
            {
                Text = "Settings"
            };
            settingsItem.Click += (sender, e) =>
            {
                HandleSettingsClick();
            };

            var exitItem = new ToolStripMenuItem()
            {
                Text = "Exit"
            };
            exitItem.Click += (sender, e) =>
            {
                Application.Exit();
            };

            trayIcon.ContextMenuStrip.Items.Add(settingsItem);
            trayIcon.ContextMenuStrip.Items.Add(exitItem);
        }

        private IServiceScope HandleSettingsClick()
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

            return scope;
        }
    }
}
