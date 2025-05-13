using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JR.Utils.GUI.Forms
{
    /* FlexibleMessageBox – A flexible replacement for the .NET MessageBox
     *
     * Original Author: Jörg Reichert (public@jreichert.de)
     * Contributors:    Thanks to: David Hall, Roink
     * Enhanced Version Author: Chris Pringle
     * Version:         2.0
     * Published at:    http://www.codeproject.com/Articles/601900/FlexibleMessageBox
     *
     ************************************************************************************************************
     * THE SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS", WITHOUT WARRANTY
     * OF ANY KIND, EXPRESS OR IMPLIED. IN NO EVENT SHALL THE AUTHOR BE
     * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OF THIS
     * SOFTWARE.
     ************************************************************************************************************
     * See "Enhancing FlexibleMessageBox" document for a list of new features and improvements in version 2.0.
     */

    #region Public Enums and Classes for Configuration

    /// <summary>
    /// Defines the style for the FlexibleMessageBox.
    /// </summary>
    public class MessageBoxStyle
    {
        public Font Font { get; set; } = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
        public Color BackColor { get; set; } = SystemColors.Control;
        public Color TextColor { get; set; } = SystemColors.ControlText;
        public Color ButtonBackColor { get; set; } = SystemColors.Control;
        public Color ButtonForeColor { get; set; } = SystemColors.ControlText;
        public Size? MinimumButtonSize { get; set; } = new Size(75, 26); // Default from original
        public Size? MaximumButtonSize { get; set; } = null;

        // Style for the RichTextBox message area
        public Color MessageBackColor { get; set; } = Color.White;
        public Color MessageForeColor { get; set; } = SystemColors.WindowText;

        // Style for the Input TextBox
        public Color InputBackColor { get; set; } = SystemColors.Window;
        public Color InputForeColor { get; set; } = SystemColors.WindowText;

        // Style for the "Don't Show Again" CheckBox
        public Color CheckBoxBackColor { get; set; } = SystemColors.Control; // Or Color.Transparent
        public Color CheckBoxForeColor { get; set; } = SystemColors.ControlText;

        // Style for the ProgressBar
        public Color ProgressBarBackColor { get; set; } = SystemColors.Control;
        public Color ProgressBarForeColor { get; set; } = Color.Green; // Typical progress color

        public MessageBoxStyle() { }

        public static MessageBoxStyle Default => new MessageBoxStyle();
    }

    /// <summary>
    /// Represents the result of a FlexibleMessageBox dialog.
    /// </summary>
    public class FlexibleDialogResult
    {
        public DialogResult DialogResult { get; internal set; }
        public string? InputText { get; internal set; }
        public bool DontShowAgainChecked { get; internal set; }
        public string? ClickedButtonTag { get; internal set; } // To identify which custom button was clicked

        public FlexibleDialogResult(DialogResult result, string? inputText = null, bool dontShowAgain = false, string? buttonTag = null)
        {
            DialogResult = result;
            InputText = inputText;
            DontShowAgainChecked = dontShowAgain;
            ClickedButtonTag = buttonTag;
        }
    }

    #endregion

    public static class FlexibleMessageBox
    {
        #region P/Invoke for System Sounds
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint uType);

        private const uint MB_ICONHAND = 0x00000010;
        private const uint MB_ICONQUESTION = 0x00000020;
        private const uint MB_ICONEXCLAMATION = 0x00000030;
        private const uint MB_ICONASTERISK = 0x00000040;
        private const uint MB_OK = 0x00000000; // Default sound
        #endregion

        #region Public statics (Configurable Globals)

        private static double _maxWidthFactor = 0.7;
        public static double MAX_WIDTH_FACTOR
        {
            get => _maxWidthFactor;
            set => _maxWidthFactor = Math.Max(0.2, Math.Min(1.0, value));
        }

        private static double _maxHeightFactor = 0.9;
        public static double MAX_HEIGHT_FACTOR
        {
            get => _maxHeightFactor;
            set => _maxHeightFactor = Math.Max(0.2, Math.Min(1.0, value));
        }

        public static Font FONT { get; set; } = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;

        // Made public static to allow users to add/modify translations
        public static readonly Dictionary<LanguageID, string[]> ButtonTexts = new Dictionary<LanguageID, string[]>
        {
            { LanguageID.en, new[] { "OK", "Cancel", "&Yes", "&No", "&Abort", "&Retry", "&Ignore" } },
            { LanguageID.de, new[] { "OK", "Abbrechen", "&Ja", "&Nein", "&Abbrechen", "&Wiederholen", "&Ignorieren" } },
            { LanguageID.es, new[] { "Aceptar", "Cancelar", "&Sí", "&No", "&Abortar", "&Reintentar", "&Ignorar" } },
            { LanguageID.it, new[] { "OK", "Annulla", "&Sì", "&No", "&Interrompi", "&Riprova", "&Ignora" } }
        };

        #endregion

        #region Builder Creation
        /// <summary>
        /// Creates a new builder instance to configure a FlexibleMessageBox.
        /// </summary>
        /// <returns>A new FlexibleMessageBoxBuilder.</returns>
        public static FlexibleMessageBoxBuilder Create() => new FlexibleMessageBoxBuilder();

        #endregion

        // Simplified Show methods for basic usage (delegating to builder)
        #region Public Show methods (Plain Text) - Simplified
        public static FlexibleDialogResult Show(string text) =>
            Create().SetText(text).Show();
        public static FlexibleDialogResult Show(IWin32Window owner, string text) =>
            Create().SetOwner(owner).SetText(text).Show();
        public static FlexibleDialogResult Show(string text, string caption) =>
            Create().SetText(text).SetCaption(caption).Show();
        public static FlexibleDialogResult Show(IWin32Window owner, string text, string caption) =>
            Create().SetOwner(owner).SetText(text).SetCaption(caption).Show();
        public static FlexibleDialogResult Show(string text, string caption, MessageBoxButtons buttons) =>
            Create().SetText(text).SetCaption(caption).WithButtons(buttons).Show();
        public static FlexibleDialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons) =>
            Create().SetOwner(owner).SetText(text).SetCaption(caption).WithButtons(buttons).Show();
        public static FlexibleDialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) =>
            Create().SetText(text).SetCaption(caption).WithButtons(buttons).WithIcon(icon).Show();
        public static FlexibleDialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) =>
            Create().SetOwner(owner).SetText(text).SetCaption(caption).WithButtons(buttons).WithIcon(icon).Show();
        public static FlexibleDialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton) =>
            Create().SetText(text).SetCaption(caption).WithButtons(buttons).WithIcon(icon).WithDefaultButton(defaultButton).Show();
        public static FlexibleDialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton) =>
            Create().SetOwner(owner).SetText(text).SetCaption(caption).WithButtons(buttons).WithIcon(icon).WithDefaultButton(defaultButton).Show();
        #endregion

        #region Public ShowRtf methods (RTF Text) - Simplified
        public static FlexibleDialogResult ShowRtf(string rtfText) =>
             Create().SetRtfText(rtfText).Show();
        public static FlexibleDialogResult ShowRtf(IWin32Window owner, string rtfText) =>
            Create().SetOwner(owner).SetRtfText(rtfText).Show();
        public static FlexibleDialogResult ShowRtf(string rtfText, string caption) =>
            Create().SetRtfText(rtfText).SetCaption(caption).Show();
        public static FlexibleDialogResult ShowRtf(IWin32Window owner, string rtfText, string caption) =>
            Create().SetOwner(owner).SetRtfText(rtfText).SetCaption(caption).Show();
        #endregion

        #region Public Enums (moved here for accessibility)
        /// <summary>
        /// Defines the recognized buttons for the FlexibleMessageBox.
        /// Used for custom button text and callbacks.
        /// </summary>
        public enum ButtonID { OK = 0, CANCEL, YES, NO, ABORT, RETRY, IGNORE }
        /// <summary>
        /// Defines the supported languages for default button texts.
        /// </summary>
        public enum LanguageID { en, de, es, it }
        #endregion

        #region Internal helper to play sound
        internal static void PlaySound(MessageBoxIcon icon)
        {
            try
            {
                uint soundType = icon switch
                {
                    MessageBoxIcon.Error /*Hand*/ => MB_ICONHAND,
                    MessageBoxIcon.Question => MB_ICONQUESTION,
                    MessageBoxIcon.Warning /*Exclamation*/ => MB_ICONEXCLAMATION,
                    MessageBoxIcon.Information /*Asterisk*/ => MB_ICONASTERISK,
                    _ => MB_OK // Default sound for None or other
                };
                MessageBeep(soundType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FlexibleMessageBox: Could not play sound - {ex.Message}");
            }
        }
        #endregion
    }

    #region FlexibleMessageBoxBuilder
    public class FlexibleMessageBoxBuilder
    {
        internal IWin32Window? Owner { get; private set; }
        internal string Text { get; private set; } = string.Empty;
        internal string Caption { get; private set; } = string.Empty;
        internal MessageBoxButtons Buttons { get; private set; } = MessageBoxButtons.OK;
        internal MessageBoxIcon Icon { get; private set; } = MessageBoxIcon.None;
        internal Image? CustomIcon { get; private set; }
        internal MessageBoxDefaultButton DefaultButton { get; private set; } = MessageBoxDefaultButton.Button1;
        internal bool IsRtf { get; private set; } = false;
        internal MessageBoxStyle Style { get; private set; } = MessageBoxStyle.Default;
        internal bool PlaySystemSound { get; private set; } = true; // Play sound by default

        // New features
        internal bool ShowTextBox { get; private set; } = false;
        internal string? InputLabelText { get; private set; }
        internal string? DefaultInputText { get; private set; }
        internal bool IsPassword { get; private set; } = false;
        internal int MaxInputLength { get; private set; } = 32767; // TextBox default

        internal bool ShowDontShowAgainCheckBox { get; private set; } = false;
        internal string? DontShowAgainText { get; private set; } = "Don't show this again";
        internal bool DontShowAgainCheckedDefault { get; private set; } = false;

        internal int TimeoutMilliseconds { get; private set; } = 0; // 0 means no timeout
        internal DialogResult TimeoutResult { get; private set; } = DialogResult.Cancel;

        internal bool ShowProgressBar { get; private set; } = false;
        internal int ProgressBarMinimum { get; private set; } = 0;
        internal int ProgressBarMaximum { get; private set; } = 100;
        internal int ProgressBarValue { get; private set; } = 0;
        internal ProgressBarStyle ProgressBarStyle { get; private set; } = System.Windows.Forms.ProgressBarStyle.Blocks; 

        internal string? Button1Text { get; private set; }
        internal string? Button2Text { get; private set; }
        internal string? Button3Text { get; private set; }

        internal Action<FlexibleMessageBoxForm, FlexibleDialogResult>? Button1Callback { get; private set; }
        internal Action<FlexibleMessageBoxForm, FlexibleDialogResult>? Button2Callback { get; private set; }
        internal Action<FlexibleMessageBoxForm, FlexibleDialogResult>? Button3Callback { get; private set; }

        internal event LinkClickedEventHandler? HyperlinkClickedEvent;


        public FlexibleMessageBoxBuilder SetOwner(IWin32Window owner) { Owner = owner; return this; }
        public FlexibleMessageBoxBuilder SetText(string text) { Text = text; IsRtf = false; return this; }
        public FlexibleMessageBoxBuilder SetRtfText(string rtfText) { Text = rtfText; IsRtf = true; return this; }
        public FlexibleMessageBoxBuilder SetCaption(string caption) { Caption = caption; return this; }
        public FlexibleMessageBoxBuilder WithButtons(MessageBoxButtons buttons) { Buttons = buttons; return this; }
        public FlexibleMessageBoxBuilder WithIcon(MessageBoxIcon icon) { Icon = icon; CustomIcon = null; return this; }
        public FlexibleMessageBoxBuilder WithCustomIcon(Image customIcon) { CustomIcon = customIcon; Icon = MessageBoxIcon.None; return this; } 
        public FlexibleMessageBoxBuilder WithDefaultButton(MessageBoxDefaultButton defaultButton) { DefaultButton = defaultButton; return this; }
        public FlexibleMessageBoxBuilder WithStyle(MessageBoxStyle style) { Style = style; return this; }
        public FlexibleMessageBoxBuilder SetPlaySystemSound(bool play) { PlaySystemSound = play; return this; }

        public FlexibleMessageBoxBuilder AddTextBox(string? label = null, string? defaultValue = null, bool isPassword = false, int maxLength = 32767)
        {
            ShowTextBox = true;
            InputLabelText = label;
            DefaultInputText = defaultValue;
            IsPassword = isPassword;
            MaxInputLength = maxLength;
            return this;
        }

        public FlexibleMessageBoxBuilder AddDontShowAgainCheckBox(string? text = null, bool defaultChecked = false)
        {
            ShowDontShowAgainCheckBox = true;
            if (!string.IsNullOrWhiteSpace(text)) DontShowAgainText = text;
            DontShowAgainCheckedDefault = defaultChecked;
            return this;
        }

        public FlexibleMessageBoxBuilder SetTimeout(int milliseconds, DialogResult onTimeoutResult = DialogResult.Cancel)
        {
            TimeoutMilliseconds = milliseconds;
            TimeoutResult = onTimeoutResult;
            return this;
        }

        public FlexibleMessageBoxBuilder AddProgressBar(int min = 0, int max = 100, int value = 0, ProgressBarStyle style = System.Windows.Forms.ProgressBarStyle.Blocks) 
        {
            ShowProgressBar = true;
            ProgressBarMinimum = min;
            ProgressBarMaximum = max;
            ProgressBarValue = value;
            ProgressBarStyle = style;
            return this;
        }

        public FlexibleMessageBoxBuilder SetButtonText(FlexibleMessageBox.ButtonID button, string text)
        {
            switch (button)
            {
                case FlexibleMessageBox.ButtonID.ABORT: case FlexibleMessageBox.ButtonID.YES: Button1Text = text; break;
                case FlexibleMessageBox.ButtonID.RETRY: case FlexibleMessageBox.ButtonID.NO: Button2Text = text; break;
                case FlexibleMessageBox.ButtonID.IGNORE: case FlexibleMessageBox.ButtonID.OK: case FlexibleMessageBox.ButtonID.CANCEL: Button3Text = text; break;
            }
            return this;
        }

        public FlexibleMessageBoxBuilder SetButtonTexts(string? button1Text = null, string? button2Text = null, string? button3Text = null)
        {
            Button1Text = button1Text;
            Button2Text = button2Text;
            Button3Text = button3Text;
            return this;
        }


        public FlexibleMessageBoxBuilder SetButtonCallback(FlexibleMessageBox.ButtonID button, Action<FlexibleMessageBoxForm, FlexibleDialogResult> callback)
        {
            switch (button)
            {
                case FlexibleMessageBox.ButtonID.ABORT: case FlexibleMessageBox.ButtonID.YES: Button1Callback = callback; break;
                case FlexibleMessageBox.ButtonID.RETRY: case FlexibleMessageBox.ButtonID.NO: Button2Callback = callback; break;
                case FlexibleMessageBox.ButtonID.IGNORE: case FlexibleMessageBox.ButtonID.OK: case FlexibleMessageBox.ButtonID.CANCEL: Button3Callback = callback; break;
            }
            return this;
        }
        
        public FlexibleMessageBoxBuilder SetButtonCallbacks(Action<FlexibleMessageBoxForm, FlexibleDialogResult>? button1Callback = null, Action<FlexibleMessageBoxForm, FlexibleDialogResult>? button2Callback = null, Action<FlexibleMessageBoxForm, FlexibleDialogResult>? button3Callback = null)
        {
            Button1Callback = button1Callback;
            Button2Callback = button2Callback;
            Button3Callback = button3Callback;
            return this;
        }


        public FlexibleMessageBoxBuilder AddHyperlinkClickHandler(LinkClickedEventHandler handler)
        {
            HyperlinkClickedEvent += handler;
            return this;
        }

        /// <summary>
        /// Internally used by FlexibleMessageBoxForm to raise the HyperlinkClickedEvent.
        /// Returns true if any handlers were invoked.
        /// </summary>
        internal bool RaiseHyperlinkClickedEvent(object sender, LinkClickedEventArgs e)
        {
            if (HyperlinkClickedEvent != null)
            {
                HyperlinkClickedEvent(sender, e);
                return true; // Assume handled if any subscribers
            }
            return false;
        }


        public FlexibleDialogResult Show()
        {
            using (var form = new FlexibleMessageBoxForm(this))
            {
                form.ShowDialog(Owner);
                return form.FlexibleResult;
            }
        }

        public async Task<FlexibleDialogResult> ShowAsync()
        {
            using (var form = new FlexibleMessageBoxForm(this))
            {
                if (Owner is Control ownerControl && ownerControl.InvokeRequired)
                {
                    await Task.Factory.StartNew(() => ownerControl.Invoke((Action)(() => form.ShowDialog(Owner))), TaskCreationOptions.AttachedToParent);
                }
                else
                {
                    form.ShowDialog(Owner); 
                }
                return form.FlexibleResult;
            }
        }
    }

    #endregion

    #region Form class
    /// <summary>
    /// The form to show the customized message box.
    /// This class is public to resolve inconsistent accessibility issues with Action delegates.
    /// </summary>
    public class FlexibleMessageBoxForm : Form 
    {
        #region Controls
        private RichTextBox richTextBoxMessage = null!;
        private Panel panelButtons = null!;
        private PictureBox pictureBoxIcon = null!;
        private Button button1 = null!; 
        private Button button2 = null!; 
        private Button button3 = null!; 
        private Panel panelTop = null!;

        // New Controls
        private Label? labelInput;
        private TextBox? textBoxInput;
        private CheckBox? checkBoxDontShowAgain;
        private System.Windows.Forms.Timer? autoCloseTimer;
        private ProgressBar? progressBar;
        #endregion

        #region Private members
        private readonly FlexibleMessageBoxBuilder _config;
        private int _visibleButtonsCount;
        private readonly FlexibleMessageBox.LanguageID _languageID = FlexibleMessageBox.LanguageID.en; 
        public FlexibleDialogResult FlexibleResult { get; private set; } = null!; 
        #endregion

        #region Constructor
        internal FlexibleMessageBoxForm(FlexibleMessageBoxBuilder config) 
        {
            _config = config;
            InitializeFormComponents(); 

            var cultureName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (Enum.TryParse<FlexibleMessageBox.LanguageID>(cultureName, out var langId) && FlexibleMessageBox.ButtonTexts.ContainsKey(langId))
            {
                _languageID = langId;
            }

            KeyPreview = true;
            KeyUp += FlexibleMessageBoxForm_KeyUp;
            FormClosing += FlexibleMessageBoxForm_FormClosing;

            this.Font = _config.Style.Font;
            this.BackColor = _config.Style.BackColor;
            this.ForeColor = _config.Style.TextColor;

            panelTop.BackColor = _config.Style.MessageBackColor; 
            richTextBoxMessage.BackColor = _config.Style.MessageBackColor;
            richTextBoxMessage.ForeColor = _config.Style.MessageForeColor;
            richTextBoxMessage.Font = this.Font; 

            panelButtons.BackColor = _config.Style.BackColor; 

            if (textBoxInput != null)
            {
                textBoxInput.BackColor = _config.Style.InputBackColor;
                textBoxInput.ForeColor = _config.Style.InputForeColor;
                textBoxInput.Font = this.Font;
            }
            if (labelInput != null)
            {
                 labelInput.BackColor = Color.Transparent; 
                 labelInput.ForeColor = _config.Style.MessageForeColor; 
                 labelInput.Font = this.Font;
            }
            if (checkBoxDontShowAgain != null)
            {
                checkBoxDontShowAgain.BackColor = _config.Style.CheckBoxBackColor;
                checkBoxDontShowAgain.ForeColor = _config.Style.CheckBoxForeColor;
                checkBoxDontShowAgain.Font = this.Font;
            }
            if (progressBar != null)
            {
                progressBar.BackColor = _config.Style.ProgressBarBackColor;
                progressBar.ForeColor = _config.Style.ProgressBarForeColor; 
            }

            this.Text = _config.Caption;
            if (_config.IsRtf)
            {
                try { richTextBoxMessage.Rtf = _config.Text; }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"Invalid RTF: {ex.Message}. Displaying as plain text.");
                    richTextBoxMessage.Text = _config.Text; 
                }
            }
            else { richTextBoxMessage.Text = _config.Text; }

            SetDialogIcon();
            SetDialogButtons(); 

            SetupInputField();
            SetupDontShowAgainCheckBox();
            SetupProgressBar();
            SetupTimeout();
            
            SetDialogSizes();
            SetDialogStartPosition(_config.Owner);

            if (_config.PlaySystemSound && _config.Icon != MessageBoxIcon.None)
            {
                FlexibleMessageBox.PlaySound(_config.Icon);
            }
            else if (_config.PlaySystemSound && _config.CustomIcon != null) 
            {
                 FlexibleMessageBox.PlaySound(MessageBoxIcon.None); 
            }

            this.AccessibleName = _config.Caption;
            richTextBoxMessage.AccessibleName = "Message content"; 
        }
        #endregion

        #region Component Initialization (Manual)
        private void InitializeFormComponents()
        {
            this.components = new System.ComponentModel.Container(); 

            panelTop = new Panel
            {
                Dock = DockStyle.Fill, 
                BackColor = Color.White, 
                Name = "panelTop"
            };

            pictureBoxIcon = new PictureBox
            {
                BackColor = Color.Transparent,
                Location = new Point(15, 15), 
                Name = "pictureBoxIcon",
                Size = new Size(32, 32), 
                SizeMode = PictureBoxSizeMode.StretchImage,
                TabStop = false,
                Visible = false, 
                AccessibleRole = AccessibleRole.Graphic
            };
            panelTop.Controls.Add(pictureBoxIcon);

            richTextBoxMessage = new RichTextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White, 
                BorderStyle = BorderStyle.None,
                Location = new Point(53, 15), 
                Margin = new Padding(0),
                Name = "richTextBoxMessage",
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Size = new Size(219, 46), 
                TabIndex = 0, 
                TabStop = false, 
                DetectUrls = true, 
                AccessibleRole = AccessibleRole.Text
            };
            richTextBoxMessage.LinkClicked += RichTextBoxMessage_LinkClicked;
            panelTop.Controls.Add(richTextBoxMessage);


            panelButtons = new Panel
            {
                BackColor = SystemColors.Control, 
                Dock = DockStyle.Bottom,
                Name = "panelButtons",
                Padding = new Padding(10),
                Size = new Size(284, 50), 
                TabIndex = 1
            };

            button1 = CreateButton("button1", "B1", 2);
            button2 = CreateButton("button2", "B2", 1);
            button3 = CreateButton("button3", "B3", 0); 

            panelButtons.Controls.Add(button1);
            panelButtons.Controls.Add(button2);
            panelButtons.Controls.Add(button3);

            this.Controls.Add(panelTop);
            this.Controls.Add(panelButtons);

            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(284, 150); 
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(220, 160); 
            this.Name = "FlexibleMessageBoxForm";
            this.ShowIcon = false; 
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Show; 
            this.StartPosition = FormStartPosition.CenterParent; 
            this.Text = "<Caption>"; 
            this.Shown += FlexibleMessageBoxForm_Shown;

            labelInput = new Label
            {
                AutoSize = true,
                Visible = false,
                Name = "labelInput",
                TextAlign = ContentAlignment.MiddleLeft,
                AccessibleRole = AccessibleRole.StaticText
            };
            panelTop.Controls.Add(labelInput);


            textBoxInput = new TextBox
            {
                Visible = false,
                Name = "textBoxInput",
                TabIndex = 1, 
                AccessibleRole = AccessibleRole.Text
            };
            panelTop.Controls.Add(textBoxInput);

            checkBoxDontShowAgain = new CheckBox
            {
                AutoSize = true,
                Visible = false,
                Name = "checkBoxDontShowAgain",
                TabIndex = 4, 
                AccessibleRole = AccessibleRole.CheckButton
            };
            panelButtons.Controls.Add(checkBoxDontShowAgain); 

            progressBar = new ProgressBar
            {
                Visible = false,
                Name = "progressBar",
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = System.Windows.Forms.ProgressBarStyle.Blocks, 
                AccessibleRole = AccessibleRole.ProgressBar
            };
            panelTop.Controls.Add(progressBar); 
        }

        private Button CreateButton(string name, string placeholderText, int tabIndex)
        {
            var btn = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right, 
                AutoSize = true, 
                DialogResult = DialogResult.None, 
                Location = new Point(0, 0), 
                Margin = new Padding(3),
                Name = name,
                Size = new Size(75, 26), 
                TabIndex = tabIndex,
                Text = placeholderText,
                UseVisualStyleBackColor = true,
                Visible = false, 
                AccessibleRole = AccessibleRole.PushButton
            };
            btn.Click += Button_Click;
            return btn;
        }

        #endregion

        #region Setup New Features
        private void SetupInputField()
        {
            if (_config.ShowTextBox && textBoxInput != null && labelInput != null)
            {
                textBoxInput.Visible = true;
                textBoxInput.Text = _config.DefaultInputText ?? string.Empty;
                textBoxInput.UseSystemPasswordChar = _config.IsPassword;
                textBoxInput.MaxLength = _config.MaxInputLength;

                if (!string.IsNullOrWhiteSpace(_config.InputLabelText))
                {
                    labelInput.Text = _config.InputLabelText;
                    labelInput.Visible = true;
                }
                else
                {
                    labelInput.Visible = false;
                }
            }
        }

        private void SetupDontShowAgainCheckBox()
        {
            if (_config.ShowDontShowAgainCheckBox && checkBoxDontShowAgain != null)
            {
                checkBoxDontShowAgain.Visible = true;
                checkBoxDontShowAgain.Text = _config.DontShowAgainText;
                checkBoxDontShowAgain.Checked = _config.DontShowAgainCheckedDefault;
                checkBoxDontShowAgain.AccessibleName = _config.DontShowAgainText;
            }
        }

        private void SetupProgressBar()
        {
            if (_config.ShowProgressBar && progressBar != null)
            {
                progressBar.Visible = true;
                progressBar.Minimum = _config.ProgressBarMinimum;
                progressBar.Maximum = _config.ProgressBarMaximum;
                progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(_config.ProgressBarValue, progressBar.Maximum)); 
                progressBar.Style = _config.ProgressBarStyle;
            }
        }

        private void SetupTimeout()
        {
            if (_config.TimeoutMilliseconds > 0)
            {
                autoCloseTimer = new System.Windows.Forms.Timer(this.components ?? (this.components = new System.ComponentModel.Container()));
                autoCloseTimer.Interval = _config.TimeoutMilliseconds;
                autoCloseTimer.Tick += AutoCloseTimer_Tick;
                autoCloseTimer.Start();
            }
        }
        #endregion

        #region Private helper functions (Sizing, Icon, Buttons, Position)

        private string GetButtonText(FlexibleMessageBox.ButtonID buttonID)
        {
            int index = (int)buttonID;
            if (FlexibleMessageBox.ButtonTexts.TryGetValue(_languageID, out var texts) && index >= 0 && index < texts.Length)
            {
                return texts[index];
            }
            return FlexibleMessageBox.ButtonTexts[FlexibleMessageBox.LanguageID.en][index]; 
        }

        private static double GetCorrectedWorkingAreaFactor(double factor) => Math.Max(0.2, Math.Min(1.0, factor));

        private void SetDialogStartPosition(IWin32Window? owner)
        {
            this.StartPosition = FormStartPosition.Manual;
            Screen screen;

            if (owner != null)
            {
                var ownerForm = owner as Form ?? Control.FromHandle(owner.Handle)?.FindForm();
                if (ownerForm != null)
                {
                    screen = Screen.FromControl(ownerForm);
                    this.Left = ownerForm.Left + (ownerForm.Width - this.Width) / 2;
                    this.Top = ownerForm.Top + (ownerForm.Height - this.Height) / 2;
                }
                else
                {
                    screen = Screen.FromPoint(Cursor.Position); 
                    CenterOnScreen(screen);
                }
            }
            else
            {
                screen = Screen.FromPoint(Cursor.Position);
                CenterOnScreen(screen);
            }

            this.Left = Math.Max(screen.WorkingArea.Left, Math.Min(this.Left, screen.WorkingArea.Right - this.Width));
            this.Top = Math.Max(screen.WorkingArea.Top, Math.Min(this.Top, screen.WorkingArea.Bottom - this.Height));
        }
        private void CenterOnScreen(Screen screen)
        {
            this.Left = screen.WorkingArea.Left + (screen.WorkingArea.Width - this.Width) / 2;
            this.Top = screen.WorkingArea.Top + (screen.WorkingArea.Height - this.Height) / 2;
        }


        private void SetDialogSizes()
        {
            const int marginX = 15;
            const int marginY = 15; 
            const int interControlSpacingY = 8; 

            var screen = Screen.FromPoint(this.Location); 
            if (this.IsHandleCreated && this.Owner != null) {
                screen = Screen.FromControl(this.Owner);
            } else if (this.IsHandleCreated) {
                screen = Screen.FromControl(this);
            }


            var maxFactorWidth = GetCorrectedWorkingAreaFactor(FlexibleMessageBox.MAX_WIDTH_FACTOR);
            var maxFactorHeight = GetCorrectedWorkingAreaFactor(FlexibleMessageBox.MAX_HEIGHT_FACTOR);
            int maxFormWidth = (int)(screen.WorkingArea.Width * maxFactorWidth);
            int maxFormHeight = (int)(screen.WorkingArea.Height * maxFactorHeight);

            int panelTopContentHeight = 0;
            int panelTopContentWidth = 0; 

            int iconDisplayWidth = 0;
            if (pictureBoxIcon.Visible)
            {
                iconDisplayWidth = pictureBoxIcon.Width + ICON_TEXT_SPACING;
                panelTopContentHeight = Math.Max(panelTopContentHeight, pictureBoxIcon.Height);
            }

            int availableTextContentWidth = maxFormWidth - (marginX * 2) - iconDisplayWidth - SystemInformation.VerticalScrollBarWidth; 
            availableTextContentWidth = Math.Max(50, availableTextContentWidth); 

            richTextBoxMessage.Width = availableTextContentWidth; 
            Size preferredRichTextSize = richTextBoxMessage.GetPreferredSize(new Size(richTextBoxMessage.Width, int.MaxValue)); 
            panelTopContentHeight += preferredRichTextSize.Height;
            panelTopContentWidth = Math.Max(panelTopContentWidth, preferredRichTextSize.Width + iconDisplayWidth);


            if (labelInput != null && labelInput.Visible)
            {
                panelTopContentHeight += interControlSpacingY;
                Size labelSize = TextRenderer.MeasureText(labelInput.Text, labelInput.Font, new Size(availableTextContentWidth, int.MaxValue), TextFormatFlags.WordBreak);
                panelTopContentHeight += labelSize.Height;
                panelTopContentWidth = Math.Max(panelTopContentWidth, labelSize.Width + iconDisplayWidth); 
            }
            if (textBoxInput != null && textBoxInput.Visible)
            {
                if (labelInput == null || !labelInput.Visible) panelTopContentHeight += interControlSpacingY; 
                panelTopContentHeight += textBoxInput.Height; 
                panelTopContentWidth = Math.Max(panelTopContentWidth, 100 + iconDisplayWidth); 
            }

            if (progressBar != null && progressBar.Visible)
            {
                panelTopContentHeight += interControlSpacingY;
                panelTopContentHeight += progressBar.Height; 
                panelTopContentWidth = Math.Max(panelTopContentWidth, 150 + iconDisplayWidth); 
            }

            panelTopContentHeight += (marginY * 2); 

            int panelButtonsHeight = 0;
            int panelButtonsContentWidth = 0;
            if (_visibleButtonsCount > 0)
            {
                panelButtonsHeight = panelButtons.Padding.Top + panelButtons.Padding.Bottom;
                var visibleButtons = new[] { button1, button2, button3 }.Where(b => b.Visible).ToList();
                if (visibleButtons.Any())
                {
                    var firstButton = visibleButtons.First();
                    ApplyButtonStyle(firstButton); 
                    if (!firstButton.AutoSize) firstButton.Size = firstButton.MinimumSize; 
                    else TextRenderer.MeasureText(firstButton.Text, firstButton.Font); 

                    panelButtonsHeight += firstButton.Height; 
                    
                    int totalButtonWidth = 0;
                    foreach(var btn in visibleButtons) {
                        ApplyButtonStyle(btn); 
                        if (!btn.AutoSize) btn.Size = btn.MinimumSize;
                        else TextRenderer.MeasureText(btn.Text, btn.Font); 
                        totalButtonWidth += btn.Width;
                    }
                    panelButtonsContentWidth = totalButtonWidth + (Math.Max(0, visibleButtons.Count -1) * button1.Margin.Horizontal) + panelButtons.Padding.Left + panelButtons.Padding.Right;
                }
            }

            if (checkBoxDontShowAgain != null && checkBoxDontShowAgain.Visible)
            {
                if (panelButtonsHeight > 0) panelButtonsHeight += interControlSpacingY; 
                else panelButtonsHeight += panelButtons.Padding.Top; 

                panelButtonsHeight += checkBoxDontShowAgain.Height + panelButtons.Padding.Bottom;
                panelButtonsContentWidth = Math.Max(panelButtonsContentWidth, checkBoxDontShowAgain.Width + panelButtons.Padding.Left + panelButtons.Padding.Right); 
            }
             if (panelButtonsHeight == 0 && (panelButtons.Padding.Top + panelButtons.Padding.Bottom > 0)) { 
                panelButtonsHeight = panelButtons.Padding.Top + panelButtons.Padding.Bottom;
            } else if (panelButtonsHeight == 0) { 
                 panelButtons.Visible = false; 
            } else {
                panelButtons.Visible = true;
            }

            int requiredCaptionWidth = TextRenderer.MeasureText(this.Text, SystemFonts.CaptionFont).Width + SystemInformation.CaptionButtonSize.Width * 2 + 45; 
            int formWidth = Math.Max(panelTopContentWidth, panelButtonsContentWidth);
            formWidth = Math.Max(formWidth, requiredCaptionWidth);
            formWidth += this.Padding.Left + this.Padding.Right + (SystemInformation.FrameBorderSize.Width * 2);
            
            richTextBoxMessage.Width = Math.Max(50, formWidth - (marginX*2) - iconDisplayWidth - (SystemInformation.FrameBorderSize.Width * 2) - this.Padding.Left - this.Padding.Right);
            bool needsScrollbar = richTextBoxMessage.GetPreferredSize(new Size(richTextBoxMessage.Width, 0)).Height > (maxFormHeight - panelButtonsHeight - SystemInformation.CaptionHeight - (marginY*2));
            if (needsScrollbar) {
                 formWidth += SystemInformation.VerticalScrollBarWidth;
            }

            int formHeight = panelTopContentHeight + (panelButtons.Visible ? panelButtonsHeight : 0) + SystemInformation.CaptionHeight + (SystemInformation.FrameBorderSize.Height * 2) + this.Padding.Top + this.Padding.Bottom;

            formWidth = Math.Max(this.MinimumSize.Width, Math.Min(formWidth, maxFormWidth));
            formHeight = Math.Max(this.MinimumSize.Height, Math.Min(formHeight, maxFormHeight));

            this.Size = new Size(formWidth, formHeight);

            panelTop.Height = this.ClientSize.Height - (panelButtons.Visible ? panelButtons.Height : 0);


            int currentY = marginY;
            int contentStartX = marginX;

            if (pictureBoxIcon.Visible)
            {
                pictureBoxIcon.Location = new Point(marginX, currentY); 
                contentStartX = pictureBoxIcon.Right + ICON_TEXT_SPACING;
            }

            int availableWidthForAnchoredControls = panelTop.ClientSize.Width - contentStartX - marginX;
            availableWidthForAnchoredControls = Math.Max(10, availableWidthForAnchoredControls); 

            richTextBoxMessage.Location = new Point(contentStartX, currentY);
            richTextBoxMessage.Width = availableWidthForAnchoredControls;
            int rtbMaxHeight = panelTop.ClientSize.Height - currentY - marginY; 
            if (labelInput !=null && labelInput.Visible) rtbMaxHeight -= (labelInput.Height + interControlSpacingY);
            if (textBoxInput !=null && textBoxInput.Visible) rtbMaxHeight -= (textBoxInput.Height + (labelInput !=null && labelInput.Visible ? 0 : interControlSpacingY));
            if (progressBar !=null && progressBar.Visible) rtbMaxHeight -= (progressBar.Height + interControlSpacingY);
            rtbMaxHeight = Math.Max(20, rtbMaxHeight); 

            richTextBoxMessage.Height = Math.Min(preferredRichTextSize.Height, rtbMaxHeight); 
            currentY += richTextBoxMessage.Height + interControlSpacingY;


            if (labelInput != null && labelInput.Visible)
            {
                labelInput.Location = new Point(contentStartX, currentY);
                labelInput.Width = availableWidthForAnchoredControls; 
                currentY += labelInput.Height + (textBoxInput !=null && textBoxInput.Visible ? 2 : interControlSpacingY); 
            }

            if (textBoxInput != null && textBoxInput.Visible)
            {
                textBoxInput.Location = new Point(contentStartX, currentY);
                textBoxInput.Width = availableWidthForAnchoredControls;
                currentY += textBoxInput.Height + interControlSpacingY;
            }

            if (progressBar != null && progressBar.Visible)
            {
                progressBar.Location = new Point(contentStartX, currentY);
                progressBar.Width = availableWidthForAnchoredControls;
            }
            
            if (panelButtons.Visible) {
                LayoutButtonsAndCheckbox();
            }
        }


        private void SetDialogIcon()
        {
            if (_config.CustomIcon != null)
            {
                pictureBoxIcon.Image = _config.CustomIcon;
            }
            else
            {
                pictureBoxIcon.Image = _config.Icon switch
                {
                    MessageBoxIcon.Information => SystemIcons.Information.ToBitmap(),
                    MessageBoxIcon.Warning => SystemIcons.Warning.ToBitmap(),
                    MessageBoxIcon.Error => SystemIcons.Error.ToBitmap(),
                    MessageBoxIcon.Question => SystemIcons.Question.ToBitmap(),
                    _ => null
                };
            }
            pictureBoxIcon.Visible = (pictureBoxIcon.Image != null);
        }

        private void SetDialogButtons()
        {
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            this.CancelButton = null; 

            ApplyButtonStyle(button1);
            ApplyButtonStyle(button2);
            ApplyButtonStyle(button3);


            switch (_config.Buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    _visibleButtonsCount = 3;
                    SetupButton(button1, FlexibleMessageBox.ButtonID.ABORT, DialogResult.Abort, _config.Button1Text);
                    SetupButton(button2, FlexibleMessageBox.ButtonID.RETRY, DialogResult.Retry, _config.Button2Text);
                    SetupButton(button3, FlexibleMessageBox.ButtonID.IGNORE, DialogResult.Ignore, _config.Button3Text);
                    ControlBox = false;
                    break;
                case MessageBoxButtons.OKCancel:
                    _visibleButtonsCount = 2;
                    SetupButton(button2, FlexibleMessageBox.ButtonID.OK, DialogResult.OK, _config.Button2Text); 
                    SetupButton(button3, FlexibleMessageBox.ButtonID.CANCEL, DialogResult.Cancel, _config.Button3Text);
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.RetryCancel:
                    _visibleButtonsCount = 2;
                    SetupButton(button2, FlexibleMessageBox.ButtonID.RETRY, DialogResult.Retry, _config.Button2Text);
                    SetupButton(button3, FlexibleMessageBox.ButtonID.CANCEL, DialogResult.Cancel, _config.Button3Text);
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.YesNo:
                    _visibleButtonsCount = 2;
                    SetupButton(button2, FlexibleMessageBox.ButtonID.YES, DialogResult.Yes, _config.Button2Text);
                    SetupButton(button3, FlexibleMessageBox.ButtonID.NO, DialogResult.No, _config.Button3Text);
                    ControlBox = false;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    _visibleButtonsCount = 3;
                    SetupButton(button1, FlexibleMessageBox.ButtonID.YES, DialogResult.Yes, _config.Button1Text);
                    SetupButton(button2, FlexibleMessageBox.ButtonID.NO, DialogResult.No, _config.Button2Text);
                    SetupButton(button3, FlexibleMessageBox.ButtonID.CANCEL, DialogResult.Cancel, _config.Button3Text);
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.OK:
                default:
                    _visibleButtonsCount = 1;
                    SetupButton(button3, FlexibleMessageBox.ButtonID.OK, DialogResult.OK, _config.Button3Text);
                    this.CancelButton = button3;
                    break;
            }
            if (panelButtons.Visible) LayoutButtonsAndCheckbox(); 
        }

        private void ApplyButtonStyle(Button btn)
        {
            btn.Font = _config.Style.Font;
            btn.BackColor = _config.Style.ButtonBackColor;
            btn.ForeColor = _config.Style.ButtonForeColor;

            if (_config.Style.MinimumButtonSize.HasValue)
            {
                btn.AutoSize = false; 
                btn.MinimumSize = _config.Style.MinimumButtonSize.Value;
                btn.Size = _config.Style.MinimumButtonSize.Value;
            } else {
                 btn.AutoSize = true; 
            }

            if (_config.Style.MaximumButtonSize.HasValue)
            {
                btn.AutoSize = false; 
                btn.MaximumSize = _config.Style.MaximumButtonSize.Value;
            }
            
            if (_config.Style.MinimumButtonSize.HasValue && _config.Style.MaximumButtonSize.HasValue)
            {
                if (_config.Style.MinimumButtonSize.Value.Width > _config.Style.MaximumButtonSize.Value.Width)
                {
                     btn.MinimumSize = new Size(_config.Style.MaximumButtonSize.Value.Width, btn.MinimumSize.Height);
                }
                if (_config.Style.MinimumButtonSize.Value.Height > _config.Style.MaximumButtonSize.Value.Height)
                {
                     btn.MinimumSize = new Size(btn.MinimumSize.Width, _config.Style.MaximumButtonSize.Value.Height);
                }
                 btn.AutoSize = false; 
            }
        }


        private void LayoutButtonsAndCheckbox()
        {
            if (!panelButtons.Visible) return;

            var visibleButtons = new List<Button>();
            if (button1.Visible) visibleButtons.Add(button1);
            if (button2.Visible) visibleButtons.Add(button2);
            if (button3.Visible) visibleButtons.Add(button3);

            int currentX = panelButtons.ClientSize.Width - panelButtons.Padding.Right;
            int buttonY = panelButtons.Padding.Top;

            foreach (var btn in visibleButtons.AsEnumerable().Reverse()) 
            {
                ApplyButtonStyle(btn);

                if (btn.AutoSize)
                {
                    btn.Size = TextRenderer.MeasureText(btn.Text, btn.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    btn.Width += 10; 
                    btn.Height += 6;
                }
                else 
                {
                    var textSize = TextRenderer.MeasureText(btn.Text, btn.Font);
                    int newWidth = Math.Max(btn.MinimumSize.Width, textSize.Width + 10);
                    int newHeight = Math.Max(btn.MinimumSize.Height, textSize.Height + 6);

                    if (btn.MaximumSize.Width > 0) newWidth = Math.Min(newWidth, btn.MaximumSize.Width);
                    if (btn.MaximumSize.Height > 0) newHeight = Math.Min(newHeight, btn.MaximumSize.Height);
                    btn.Size = new Size(newWidth, newHeight);
                }

                currentX -= btn.Width;
                btn.Location = new Point(currentX, buttonY);
                currentX -= btn.Margin.Right; 
            }

            if (checkBoxDontShowAgain != null && checkBoxDontShowAgain.Visible)
            {
                checkBoxDontShowAgain.Text = _config.DontShowAgainText; 

                if (visibleButtons.Any())
                {
                    int checkBoxY = buttonY + (visibleButtons.First().Height - checkBoxDontShowAgain.Height) / 2; 
                    checkBoxDontShowAgain.Location = new Point(panelButtons.Padding.Left, checkBoxY);
                }
                else
                {
                    int checkBoxX = (panelButtons.ClientSize.Width - checkBoxDontShowAgain.Width) / 2;
                    int checkBoxY = (panelButtons.ClientSize.Height - checkBoxDontShowAgain.Height) / 2;
                    checkBoxDontShowAgain.Location = new Point(Math.Max(panelButtons.Padding.Left, checkBoxX), checkBoxY);
                }
            }
        }


        private void SetupButton(Button button, FlexibleMessageBox.ButtonID buttonID, DialogResult dialogResult, string? customText)
        {
            button.Text = !string.IsNullOrWhiteSpace(customText) ? customText : GetButtonText(buttonID);
            button.DialogResult = dialogResult; 
            button.Tag = buttonID; 
            button.Visible = true;
        }

        #endregion

        #region Private event handlers
        private void FlexibleMessageBoxForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (FlexibleResult == null)
            {
                DialogResult dr = this.DialogResult; 
                if (dr == DialogResult.None && _config.Buttons == MessageBoxButtons.OK) dr = DialogResult.OK; 
                else if (dr == DialogResult.None && (this.CancelButton as Button) != null) dr = (this.CancelButton as Button)!.DialogResult; 
                else if (dr == DialogResult.None) dr = DialogResult.Cancel; 

                FlexibleResult = new FlexibleDialogResult(
                    dr,
                    textBoxInput?.Text,
                    checkBoxDontShowAgain?.Checked ?? false
                );
            }

            autoCloseTimer?.Stop();
            autoCloseTimer?.Dispose();
        }


        private void FlexibleMessageBoxForm_Shown(object? sender, EventArgs e)
        {
            Button? buttonToFocus = null;
            if (_config.ShowTextBox && textBoxInput != null && string.IsNullOrEmpty(textBoxInput.Text))
            {
                textBoxInput.Focus();
                return; 
            }

            switch (_config.DefaultButton)
            {
                case MessageBoxDefaultButton.Button1: buttonToFocus = button1.Visible ? button1 : (button2.Visible ? button2 : button3); break;
                case MessageBoxDefaultButton.Button2: buttonToFocus = button2.Visible ? button2 : (button3.Visible ? button3 : button1); break;
                case MessageBoxDefaultButton.Button3: buttonToFocus = button3.Visible ? button3 : (button1.Visible ? button1 : button2); break;
            }
            buttonToFocus ??= button3.Visible ? button3 : (button2.Visible ? button2 : (button1.Visible ? button1 : null));
            
            if (buttonToFocus != null)
            {
                try { this.BeginInvoke(new Action(() => buttonToFocus.Focus())); }
                catch (Exception ex) { Debug.WriteLine($"Focus error: {ex.Message}"); }
            }
            else if (textBoxInput != null && textBoxInput.Visible) 
            {
                 try { this.BeginInvoke(new Action(() => textBoxInput.Focus())); }
                 catch (Exception ex) { Debug.WriteLine($"Focus error (textbox): {ex.Message}"); }
            }
        }

        private void RichTextBoxMessage_LinkClicked(object? sender, LinkClickedEventArgs e)
        {
            // Invoke the handlers collected by the builder by calling the builder's public method
            bool handledByCustom = _config.RaiseHyperlinkClickedEvent(this, e);

            // Default behavior if no custom handler was attached or if the custom handler didn't "handle" it
            // (RaiseHyperlinkClickedEvent returns true if any handler was invoked, assuming it's handled)
            if (!handledByCustom && e.LinkText != null) 
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
                }
                catch (Exception ex) { Debug.WriteLine($"Error opening link: {ex.Message}"); }
                finally { Cursor.Current = Cursors.Default; }
            }
        }

        private void FlexibleMessageBoxForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.Insert))
            {
                var buttonsTexts = string.Join("   ", 
                    new[] { button1, button2, button3 }
                    .Where(btn => btn.Visible)
                    .Select(btn => btn.Text.Replace("&", ""))); 

                var textForClipboard =
                    "---------------------------\n" +
                    this.Text + Environment.NewLine + 
                    "---------------------------\n" +
                    (_config.IsRtf ? richTextBoxMessage.Text : richTextBoxMessage.Rtf) + Environment.NewLine + 
                    "---------------------------\n" +
                    buttonsTexts + Environment.NewLine +
                    "---------------------------";
                try { Clipboard.SetText(textForClipboard); }
                catch (Exception ex) { Debug.WriteLine($"Clipboard error: {ex.Message}"); }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                var cancelButton = this.CancelButton as Button;
                if (cancelButton != null && cancelButton.Visible)
                {
                    PerformButtonClick(cancelButton); 
                }
                else if (ControlBox && _visibleButtonsCount <= 1 && button3.Visible && button3.DialogResult == DialogResult.OK) 
                {
                     PerformButtonClick(button3); 
                }
            }
        }

        private void AutoCloseTimer_Tick(object? sender, EventArgs e)
        {
            autoCloseTimer?.Stop();
            this.DialogResult = _config.TimeoutResult; 
            FlexibleResult = new FlexibleDialogResult(
                _config.TimeoutResult,
                textBoxInput?.Text,
                checkBoxDontShowAgain?.Checked ?? false,
                "Timeout"
            );
            this.Close();
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                PerformButtonClick(clickedButton);
            }
        }

        private void PerformButtonClick(Button clickedButton)
        {
            this.DialogResult = clickedButton.DialogResult; 

            FlexibleResult = new FlexibleDialogResult(
                clickedButton.DialogResult,
                textBoxInput?.Text,
                checkBoxDontShowAgain?.Checked ?? false,
                (clickedButton.Tag as FlexibleMessageBox.ButtonID?)?.ToString() ?? clickedButton.Name
            );

            Action<FlexibleMessageBoxForm, FlexibleDialogResult>? callback = null;
            if (clickedButton == button1) callback = _config.Button1Callback;
            else if (clickedButton == button2) callback = _config.Button2Callback;
            else if (clickedButton == button3) callback = _config.Button3Callback;

            callback?.Invoke(this, FlexibleResult);

            if (clickedButton.DialogResult != DialogResult.None || callback == null) 
            {
                 this.Close();
            }
        }

        #endregion

        #region Constants for Layout
        private const int ICON_TEXT_SPACING = 10;
        #endregion

        private System.ComponentModel.IContainer? components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    #endregion
}
