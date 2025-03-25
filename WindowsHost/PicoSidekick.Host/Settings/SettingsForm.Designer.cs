namespace PicoSidekick.Host.Settings
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            restartInUf2ModeCheckbox = new CheckBox();
            devModeEnabledCheckbox = new CheckBox();
            okButton = new Button();
            cancelButton = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(restartInUf2ModeCheckbox);
            groupBox1.Controls.Add(devModeEnabledCheckbox);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(317, 142);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Settings";
            // 
            // restartInUf2ModeCheckbox
            // 
            restartInUf2ModeCheckbox.AutoSize = true;
            restartInUf2ModeCheckbox.Location = new Point(16, 47);
            restartInUf2ModeCheckbox.Name = "restartInUf2ModeCheckbox";
            restartInUf2ModeCheckbox.Size = new Size(132, 19);
            restartInUf2ModeCheckbox.TabIndex = 1;
            restartInUf2ModeCheckbox.Text = "Restart in UF2 mode";
            restartInUf2ModeCheckbox.UseVisualStyleBackColor = true;
            // 
            // devModeEnabledCheckbox
            // 
            devModeEnabledCheckbox.AutoSize = true;
            devModeEnabledCheckbox.Location = new Point(16, 22);
            devModeEnabledCheckbox.Name = "devModeEnabledCheckbox";
            devModeEnabledCheckbox.Size = new Size(259, 19);
            devModeEnabledCheckbox.TabIndex = 0;
            devModeEnabledCheckbox.Text = "Dev Mode enabled (CIRCUITPY drive visible)";
            devModeEnabledCheckbox.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.Location = new Point(92, 160);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 1;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(173, 160);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(341, 192);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pico Sidekick";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private CheckBox devModeEnabledCheckbox;
        private Button okButton;
        private Button cancelButton;
        private CheckBox restartInUf2ModeCheckbox;
    }
}