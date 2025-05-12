using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    public class ResponsiveLayoutHelper
    {
        // Base resolution that the application was designed for
        private static readonly Size BaseResolution = new Size(1200, 800);

        // Minimum sizes for various control types
        private static readonly Size MinButtonSize = new Size(80, 25);
        private static readonly Size MinTextBoxSize = new Size(100, 20);
        private static readonly Size MinComboBoxSize = new Size(100, 20);
        private static readonly int MinPanelWidth = 150;
        private static readonly int MinPanelHeight = 100;

        // Scaling factors
        private float _widthScaleFactor = 1.0f;
        private float _heightScaleFactor = 1.0f;
        private float _fontScaleFactor = 1.0f;

        // Reference to the form being managed
        private Form _form;

        // Store original control positions and sizes
        private class ControlProperties
        {
            public Point Location { get; set; }
            public Size Size { get; set; }
            public Font Font { get; set; }
            public Padding Padding { get; set; }
            public Padding Margin { get; set; }
        }

        // Dictionary to store original control properties
        private System.Collections.Generic.Dictionary<Control, ControlProperties> _originalProperties =
            new System.Collections.Generic.Dictionary<Control, ControlProperties>();

        public ResponsiveLayoutHelper(Form form)
        {
            _form = form;

            // Store original form size
            _originalProperties[form] = new ControlProperties
            {
                Size = form.ClientSize
            };

            // Register for form resize events
            form.Resize += Form_Resize;

            // Store original properties of all controls
            StoreOriginalProperties(form.Controls);
        }

        private void StoreOriginalProperties(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (!_originalProperties.ContainsKey(control))
                {
                    _originalProperties[control] = new ControlProperties
                    {
                        Location = control.Location,
                        Size = control.Size,
                        Font = control.Font,
                        Padding = control is Padding ? ((dynamic)control).Padding : new Padding(0),
                        Margin = control is Margins ? ((dynamic)control).Margin : new Padding(0)
                    };
                }

                // Recursively store properties for child controls
                if (control.Controls.Count > 0)
                {
                    StoreOriginalProperties(control.Controls);
                }
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            UpdateScalingFactors();
            ResizeControls(_form.Controls);
        }

        private void UpdateScalingFactors()
        {
            var originalSize = _originalProperties[_form].Size;
            _widthScaleFactor = (float)_form.ClientSize.Width / originalSize.Width;
            _heightScaleFactor = (float)_form.ClientSize.Height / originalSize.Height;

            // Use the smaller scaling factor for fonts to prevent them from becoming too large
            _fontScaleFactor = Math.Min(_widthScaleFactor, _heightScaleFactor);

            // Limit scaling factors to reasonable ranges
            _widthScaleFactor = Math.Max(0.5f, Math.Min(2.0f, _widthScaleFactor));
            _heightScaleFactor = Math.Max(0.5f, Math.Min(2.0f, _heightScaleFactor));
            _fontScaleFactor = Math.Max(0.7f, Math.Min(1.5f, _fontScaleFactor));
        }

        private void ResizeControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (_originalProperties.ContainsKey(control))
                {
                    var original = _originalProperties[control];

                    // Skip controls with Dock = Fill, DockStyle.Top, DockStyle.Bottom, DockStyle.Left, or DockStyle.Right
                    if (control.Dock != DockStyle.None && control.Dock != DockStyle.Bottom)
                    {
                        // For docked controls, we might still want to adjust some properties
                        // like font size or internal padding
                        if (control.Font != null)
                        {
                            float newSize = original.Font.Size * _fontScaleFactor;
                            control.Font = new Font(original.Font.FontFamily, newSize, original.Font.Style);
                        }
                    }
                    else
                    {
                        // Scale location and size for non-docked controls
                        int newX = (int)(original.Location.X * _widthScaleFactor);
                        int newY = (int)(original.Location.Y * _heightScaleFactor);
                        control.Location = new Point(newX, newY);

                        int newWidth = (int)(original.Size.Width * _widthScaleFactor);
                        int newHeight = (int)(original.Size.Height * _heightScaleFactor);

                        // Ensure minimum sizes based on control type
                        if (control is Button)
                        {
                            newWidth = Math.Max(MinButtonSize.Width, newWidth);
                            newHeight = Math.Max(MinButtonSize.Height, newHeight);
                        }
                        else if (control is TextBox)
                        {
                            newWidth = Math.Max(MinTextBoxSize.Width, newWidth);
                            newHeight = Math.Max(MinTextBoxSize.Height, newHeight);
                        }
                        else if (control is ComboBox)
                        {
                            newWidth = Math.Max(MinComboBoxSize.Width, newWidth);
                            newHeight = Math.Max(MinComboBoxSize.Height, newHeight);
                        }
                        else if (control is Panel)
                        {
                            newWidth = Math.Max(MinPanelWidth, newWidth);
                            newHeight = Math.Max(MinPanelHeight, newHeight);
                        }

                        control.Size = new Size(newWidth, newHeight);

                        // Scale font
                        if (control.Font != null)
                        {
                            float newSize = original.Font.Size * _fontScaleFactor;
                            control.Font = new Font(original.Font.FontFamily, newSize, original.Font.Style);
                        }

                        // Scale padding and margin for controls that support it
                        try
                        {
                            if (control.GetType().GetProperty("Padding") != null)
                            {
                                var originalPadding = original.Padding;
                                var newPadding = new Padding(
                                    (int)(originalPadding.Left * _widthScaleFactor),
                                    (int)(originalPadding.Top * _heightScaleFactor),
                                    (int)(originalPadding.Right * _widthScaleFactor),
                                    (int)(originalPadding.Bottom * _heightScaleFactor)
                                );
                                control.GetType().GetProperty("Padding").SetValue(control, newPadding);
                            }

                            if (control.GetType().GetProperty("Margin") != null)
                            {
                                var originalMargin = original.Margin;
                                var newMargin = new Padding(
                                    (int)(originalMargin.Left * _widthScaleFactor),
                                    (int)(originalMargin.Top * _heightScaleFactor),
                                    (int)(originalMargin.Right * _widthScaleFactor),
                                    (int)(originalMargin.Bottom * _heightScaleFactor)
                                );
                                control.GetType().GetProperty("Margin").SetValue(control, newMargin);
                            }
                        }
                        catch { /* Ignore errors for controls that don't support these properties */ }
                    }
                }

                // Recursively resize child controls
                if (control.Controls.Count > 0)
                {
                    ResizeControls(control.Controls);
                }
            }
        }

        // Method to manually trigger a resize (useful after adding new controls)
        public void RefreshLayout()
        {
            StoreOriginalProperties(_form.Controls);
            UpdateScalingFactors();
            ResizeControls(_form.Controls);
        }

        // Method to register a new control that was added after initialization
        public void RegisterControl(Control control)
        {
            if (!_originalProperties.ContainsKey(control))
            {
                _originalProperties[control] = new ControlProperties
                {
                    Location = control.Location,
                    Size = control.Size,
                    Font = control.Font,
                    Padding = control is Padding ? ((dynamic)control).Padding : new Padding(0),
                    Margin = control is Margins ? ((dynamic)control).Margin : new Padding(0)
                };

                // Recursively register child controls
                if (control.Controls.Count > 0)
                {
                    StoreOriginalProperties(control.Controls);
                }
            }
        }
    }
}
