using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI;
using WindowsFormsLifetime;

namespace PicoSidekick.Host.Settings
{
    public partial class SettingsForm : Form
    {
        private readonly SettingsService _settingsService;
        private readonly IGuiContext _guiContext;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SettingsModel Settings { get; set; }

        public SettingsForm(SettingsService settingsService, IGuiContext guiContext)
        {
            InitializeComponent();
            Icon = new Icon("Pi.ico");
            _settingsService = settingsService;
            _guiContext = guiContext;

            Settings = settingsService.Settings;
            devModeEnabledCheckbox.Checked = Settings.DevModeEnabled;
            EnableDisableControls(!settingsService.SettingsLocked);

            settingsService.Locked += SettingsService_Locked;
        }

        private void SettingsService_Locked(object sender, EventArgs e)
        {
            EnableDisableControls(false);
        }

        private void EnableDisableControls(bool enable)
        {
            _guiContext.Invoke(() =>
            {
                devModeEnabledCheckbox.Enabled = enable;
                okButton.Enabled = enable;
            });
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Settings = Settings with { DevModeEnabled = devModeEnabledCheckbox.Checked };
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _settingsService.Locked -= SettingsService_Locked;
        }
    }
}
