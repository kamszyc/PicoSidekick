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
            components = new System.ComponentModel.Container();
            groupBox1 = new GroupBox();
            restartInUf2ModeCheckbox = new CheckBox();
            devModeEnabledCheckbox = new CheckBox();
            okButton = new Button();
            cancelButton = new Button();
            groupBox2 = new GroupBox();
            rotateDisplayCheckbox = new CheckBox();
            label1 = new Label();
            trackBarBrightness = new TrackBar();
            tooltip = new ToolTip(components);
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarBrightness).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Bottom;
            groupBox1.Controls.Add(restartInUf2ModeCheckbox);
            groupBox1.Controls.Add(devModeEnabledCheckbox);
            groupBox1.Location = new Point(12, 170);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(331, 82);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Dev settings";
            // 
            // restartInUf2ModeCheckbox
            // 
            restartInUf2ModeCheckbox.AutoSize = true;
            restartInUf2ModeCheckbox.Location = new Point(16, 47);
            restartInUf2ModeCheckbox.Name = "restartInUf2ModeCheckbox";
            restartInUf2ModeCheckbox.Size = new Size(132, 19);
            restartInUf2ModeCheckbox.TabIndex = 1;
            restartInUf2ModeCheckbox.Text = "Restart in UF2 mode";
            tooltip.SetToolTip(restartInUf2ModeCheckbox, "For updating CircuitPython or dumping UF2 file");
            restartInUf2ModeCheckbox.UseVisualStyleBackColor = true;
            // 
            // devModeEnabledCheckbox
            // 
            devModeEnabledCheckbox.AutoSize = true;
            devModeEnabledCheckbox.Location = new Point(16, 22);
            devModeEnabledCheckbox.Name = "devModeEnabledCheckbox";
            devModeEnabledCheckbox.Size = new Size(118, 19);
            devModeEnabledCheckbox.TabIndex = 0;
            devModeEnabledCheckbox.Text = "Enable Dev Mode";
            tooltip.SetToolTip(devModeEnabledCheckbox, "Enable CIRCUITPY drive and console serial port");
            devModeEnabledCheckbox.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom;
            okButton.Location = new Point(92, 258);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 1;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom;
            cancelButton.Location = new Point(173, 258);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(rotateDisplayCheckbox);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(trackBarBrightness);
            groupBox2.Location = new Point(12, 18);
            groupBox2.Name = "groupBox2";
            groupBox2.RightToLeft = RightToLeft.No;
            groupBox2.Size = new Size(331, 135);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "General";
            // 
            // rotateDisplayCheckbox
            // 
            rotateDisplayCheckbox.AutoSize = true;
            rotateDisplayCheckbox.Location = new Point(16, 96);
            rotateDisplayCheckbox.Name = "rotateDisplayCheckbox";
            rotateDisplayCheckbox.Size = new Size(165, 19);
            rotateDisplayCheckbox.TabIndex = 2;
            rotateDisplayCheckbox.Text = "Rotate display 180 degrees";
            rotateDisplayCheckbox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 27);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 1;
            label1.Text = "Brightness";
            // 
            // trackBarBrightness
            // 
            trackBarBrightness.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            trackBarBrightness.Location = new Point(6, 45);
            trackBarBrightness.Maximum = 100;
            trackBarBrightness.Name = "trackBarBrightness";
            trackBarBrightness.Size = new Size(319, 45);
            trackBarBrightness.TabIndex = 0;
            trackBarBrightness.TickStyle = TickStyle.None;
            // 
            // SettingsForm
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(355, 293);
            Controls.Add(groupBox2);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pico Sidekick";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarBrightness).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private CheckBox devModeEnabledCheckbox;
        private Button okButton;
        private Button cancelButton;
        private CheckBox restartInUf2ModeCheckbox;
        private GroupBox groupBox2;
        private TrackBar trackBarBrightness;
        private Label label1;
        private ToolTip tooltip;
        private CheckBox rotateDisplayCheckbox;
    }
}