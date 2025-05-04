using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using RATAPP.Forms;
using RATAPP.Interfaces;
using RATAPP.Helpers;

namespace RATAPP.Panels
{
    /// <summary>
    /// Base class for all responsive panels in the application.
    /// Provides common functionality for handling responsive layouts and navigation.
    /// </summary>
    public class ResponsivePanel : Panel, INavigable
    {
        protected readonly RATAppBaseForm _baseForm;
        protected bool _initialized = false;
        protected float _scaleFactor = 1.0f;
        protected Size _originalSize;
        protected bool _isResizing = false;

        /// <summary>
        /// Initializes a new instance of the ResponsivePanel class.
        /// </summary>
        /// <param name="baseForm">The parent base form that contains this panel</param>
        public ResponsivePanel(RATAppBaseForm baseForm)
        {
            _baseForm = baseForm;
            _originalSize = this.Size;

            // Set default properties
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;

            // Register for resize events
            this.SizeChanged += ResponsivePanel_SizeChanged;
            this.Resize += ResponsivePanel_Resize;

            // Register for form-level events
            if (_baseForm != null)
            {
                _baseForm.FormResized += BaseForm_FormResized;
                _baseForm.ThemeChanged += BaseForm_ThemeChanged;
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of the panel.
        /// </summary>
        private void ResponsivePanel_SizeChanged(object sender, EventArgs e)
        {
            if (!_initialized) return;

            // Calculate new scale factor based on original size
            if (_originalSize.Width > 0 && _originalSize.Height > 0)
            {
                float widthRatio = (float)this.Width / _originalSize.Width;
                float heightRatio = (float)this.Height / _originalSize.Height;

                // Use the smaller ratio to ensure everything fits
                _scaleFactor = Math.Min(widthRatio, heightRatio);
            }

            if (!_isResizing)
            {
                _isResizing = true;
                ResizeUI();
                _isResizing = false;
            }
        }

        /// <summary>
        /// Handles the Resize event of the panel.
        /// </summary>
        private void ResponsivePanel_Resize(object sender, EventArgs e)
        {
            if (!_initialized) return;

            if (!_isResizing)
            {
                _isResizing = true;
                ResizeUI();
                _isResizing = false;
            }
        }

        /// <summary>
        /// Handles the FormResized event of the base form.
        /// </summary>
        private void BaseForm_FormResized(object sender, EventArgs e)
        {
            if (!_initialized) return;

            if (!_isResizing)
            {
                _isResizing = true;
                ResizeUI();
                _isResizing = false;
            }
        }

        /// <summary>
        /// Handles the ThemeChanged event of the base form.
        /// </summary>
        private void BaseForm_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        /// <summary>
        /// Applies the current theme to the panel and its controls.
        /// </summary>
        public virtual void ApplyTheme()
        {
            // Get theme settings from the base form
            if (_baseForm != null)
            {
                var theme = _baseForm.CurrentTheme;

                // Apply theme colors
                this.BackColor = theme.BackgroundColor;
                this.ForeColor = theme.TextColor;

                // Apply theme to all child controls
                ApplyThemeToControls(this.Controls, theme);
            }
        }

        /// <summary>
        /// Recursively applies theme settings to all controls.
        /// </summary>
        protected void ApplyThemeToControls(Control.ControlCollection controls, ThemeSettings theme)
        {
            foreach (Control control in controls)
            {
                // Apply theme based on control type
                if (control is Button btn)
                {
                    btn.BackColor = theme.ButtonColor;
                    btn.ForeColor = theme.ButtonTextColor;
                    if (btn.FlatStyle == FlatStyle.Flat)
                    {
                        btn.FlatAppearance.BorderColor = theme.BorderColor;
                    }
                }
                else if (control is Label lbl)
                {
                    lbl.ForeColor = theme.TextColor;
                }
                else if (control is TextBox txt)
                {
                    txt.BackColor = theme.TextBoxColor;
                    txt.ForeColor = theme.TextColor;
                    txt.BorderStyle = theme.BorderStyle;
                }
                else if (control is ComboBox cmb)
                {
                    cmb.BackColor = theme.TextBoxColor;
                    cmb.ForeColor = theme.TextColor;
                }
                else if (control is DataGridView dgv)
                {
                    dgv.BackgroundColor = theme.BackgroundColor;
                    dgv.ForeColor = theme.TextColor;
                    dgv.GridColor = theme.BorderColor;

                    // Apply to headers
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = theme.HeaderColor;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = theme.HeaderTextColor;

                    // Apply to cells
                    dgv.DefaultCellStyle.BackColor = theme.CellColor;
                    dgv.DefaultCellStyle.ForeColor = theme.TextColor;
                    dgv.DefaultCellStyle.SelectionBackColor = theme.SelectionColor;
                    dgv.DefaultCellStyle.SelectionForeColor = theme.SelectionTextColor;
                }
                else if (control is TabControl tabControl)
                {
                    tabControl.BackColor = theme.BackgroundColor;
                    tabControl.ForeColor = theme.TextColor;

                    // Apply to tab pages
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        tabPage.BackColor = theme.BackgroundColor;
                        tabPage.ForeColor = theme.TextColor;

                        // Recursively apply to tab page controls
                        ApplyThemeToControls(tabPage.Controls, theme);
                    }
                }
                else if (control is Panel panel)
                {
                    // Don't apply to ResponsivePanel instances (they handle their own theming)
                    if (!(panel is ResponsivePanel))
                    {
                        panel.BackColor = theme.BackgroundColor;
                        panel.ForeColor = theme.TextColor;
                    }
                }

                // Recursively apply to child controls
                if (control.Controls.Count > 0)
                {
                    ApplyThemeToControls(control.Controls, theme);
                }
            }
        }

        /// <summary>
        /// Resizes and repositions UI elements based on the current panel size.
        /// Override this method in derived classes to implement specific resizing logic.
        /// </summary>
        public virtual void ResizeUI()
        {
            // Base implementation does nothing
            // Derived classes should override this method to implement specific resizing logic
        }

        /// <summary>
        /// Scales a control's size and location based on the current scale factor.
        /// </summary>
        /// <param name="control">The control to scale</param>
        /// <param name="originalSize">The original size of the control</param>
        /// <param name="originalLocation">The original location of the control</param>
        protected void ScaleControl(Control control, Size originalSize, Point originalLocation)
        {
            if (_scaleFactor <= 0) return;

            // Scale size
            int newWidth = (int)(originalSize.Width * _scaleFactor);
            int newHeight = (int)(originalSize.Height * _scaleFactor);
            control.Size = new Size(newWidth, newHeight);

            // Scale location
            int newX = (int)(originalLocation.X * _scaleFactor);
            int newY = (int)(originalLocation.Y * _scaleFactor);
            control.Location = new Point(newX, newY);

            // Scale font if needed
            if (control.Font != null)
            {
                float newFontSize = control.Font.Size * _scaleFactor;
                // Ensure minimum font size
                newFontSize = Math.Max(newFontSize, 8.0f);
                control.Font = new Font(control.Font.FontFamily, newFontSize, control.Font.Style);
            }
        }

        /// <summary>
        /// Scales a control's font based on the current scale factor.
        /// </summary>
        /// <param name="control">The control whose font should be scaled</param>
        /// <param name="originalFont">The original font of the control</param>
        protected void ScaleFont(Control control, Font originalFont)
        {
            if (_scaleFactor <= 0 || originalFont == null) return;

            float newFontSize = originalFont.Size * _scaleFactor;
            // Ensure minimum font size
            newFontSize = Math.Max(newFontSize, 8.0f);
            control.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
        }

        /// <summary>
        /// Calculates a proportional size based on the current panel size.
        /// </summary>
        /// <param name="widthPercent">Width as a percentage of panel width (0-100)</param>
        /// <param name="heightPercent">Height as a percentage of panel height (0-100)</param>
        /// <returns>A size object with the calculated dimensions</returns>
        protected Size GetProportionalSize(float widthPercent, float heightPercent)
        {
            int width = (int)(this.Width * (widthPercent / 100f));
            int height = (int)(this.Height * (heightPercent / 100f));
            return new Size(width, height);
        }

        /// <summary>
        /// Calculates a proportional location based on the current panel size.
        /// </summary>
        /// <param name="xPercent">X position as a percentage of panel width (0-100)</param>
        /// <param name="yPercent">Y position as a percentage of panel height (0-100)</param>
        /// <returns>A point object with the calculated location</returns>
        protected Point GetProportionalLocation(float xPercent, float yPercent)
        {
            int x = (int)(this.Width * (xPercent / 100f));
            int y = (int)(this.Height * (yPercent / 100f));
            return new Point(x, y);
        }

        /// <summary>
        /// Refreshes the data displayed in the panel.
        /// Override this method in derived classes to implement specific data refresh logic.
        /// </summary>
        public virtual async Task RefreshDataAsync()
        {
            // Base implementation does nothing
            // Derived classes should override this method to implement specific data refresh logic
            await Task.CompletedTask;
        }

        /// <summary>
        /// Saves the current state of the panel.
        /// Override this method in derived classes to implement specific state saving logic.
        /// </summary>
        public virtual void SaveState()
        {
            // Base implementation does nothing
            // Derived classes should override this method to implement specific state saving logic
        }

        /// <summary>
        /// Restores the previously saved state of the panel.
        /// Override this method in derived classes to implement specific state restoration logic.
        /// </summary>
        public virtual void RestoreState()
        {
            // Base implementation does nothing
            // Derived classes should override this method to implement specific state restoration logic
        }

        /// <summary>
        /// Cleans up resources when the panel is being disposed.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unregister events
                this.SizeChanged -= ResponsivePanel_SizeChanged;
                this.Resize -= ResponsivePanel_Resize;

                if (_baseForm != null)
                {
                    _baseForm.FormResized -= BaseForm_FormResized;
                    _baseForm.ThemeChanged -= BaseForm_ThemeChanged;
                }
            }

            base.Dispose(disposing);
        }
    }
}
