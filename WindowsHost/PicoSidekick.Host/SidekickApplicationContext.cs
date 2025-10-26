using Microsoft.Extensions.DependencyInjection;
using PicoSidekick.Host.Serial;
using PicoSidekick.Host.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsLifetime;

namespace PicoSidekick.Host
{
    public class SidekickApplicationContext : ApplicationContext
    {
        private readonly SettingsService _settingsService;
        private readonly SerialPortStatusService _serialPortStatusService;
        private readonly IFormProvider _formProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IServiceScope _scope;

        public SidekickApplicationContext(
            SettingsService settingsService,
            SerialPortStatusService serialPortStatusService,
            IFormProvider formProvider,
            IServiceScopeFactory serviceScopeFactory)
        {
            _settingsService = settingsService;
            _serialPortStatusService = serialPortStatusService;
            _formProvider = formProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _scope = serviceScopeFactory.CreateScope();
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
                var status = _serialPortStatusService.GetStatus();
                if (status.IsConnected)
                    HandleSettingsClick();
                else
                    MessageBox.Show("Device is not connected. Connect the device and try again.", "Pico Sidekick Host", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var serialPortStatus = _serialPortStatusService.GetStatus();
            var connectionItem = new ToolStripMenuItem()
            {
                Text = GetConnectionItemText(serialPortStatus),
                Enabled = false,
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

            trayIcon.ContextMenuStrip.Opening += (sender, e) =>
            {
                var status = _serialPortStatusService.GetStatus();
                connectionItem.Text = GetConnectionItemText(status);
                settingsItem.Enabled = status.IsConnected;
            };

            trayIcon.ContextMenuStrip.Items.Add(connectionItem);
            trayIcon.ContextMenuStrip.Items.Add(settingsItem);
            trayIcon.ContextMenuStrip.Items.Add(exitItem);
        }

        private static string GetConnectionItemText(SerialPortStatus serialPortStatus)
        {
            return !serialPortStatus.IsConnected ? "Disconnected" : $"Connected on {serialPortStatus.PortName} port";
        }

        private IServiceScope HandleSettingsClick()
        {
            var form = _formProvider.GetForm<SettingsForm>(_scope);
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
                        _settingsService.SetFromSettingsForm(form.Settings);
                }
                finally
                {
                    form.Dispose();
                    _scope.Dispose();
                    _scope = _serviceScopeFactory.CreateScope();
                }
            }

            return _scope;
        }
    }
}
