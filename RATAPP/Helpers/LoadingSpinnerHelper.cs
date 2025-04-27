using System;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Manages loading spinner functionality across the application.
    /// Implement this in any form that needs loading indicators.
    /// </summary>
    public class LoadingSpinnerHelper
    {
        private readonly PictureBox _spinner;
        private readonly Form _parentForm;

        /// <summary>
        /// Initializes a new instance of the LoadingSpinnerHelper
        /// </summary>
        /// <param name="parentForm">The form that will contain the spinner</param>
        /// <param name="spinnerImagePath">Path to the spinner image file</param>
        public LoadingSpinnerHelper(Form parentForm, string spinnerImagePath)
        {
            _parentForm = parentForm;
            _spinner = InitializeSpinner(spinnerImagePath);
            _parentForm.Controls.Add(_spinner);
            _parentForm.Resize += (s, e) => CenterSpinner();
        }

        /// <summary>
        /// Creates and configures the spinner PictureBox
        /// </summary>
        private PictureBox InitializeSpinner(string imagePath)
        {
            return new PictureBox
            {
                Size = new Size(50, 50),
                Image = Image.FromFile(imagePath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };
        }

        /// <summary>
        /// Centers the spinner in the parent form
        /// </summary>
        private void CenterSpinner()
        {
            if (_spinner != null && _parentForm != null)
            {
                _spinner.Location = new Point(
                    (_parentForm.ClientSize.Width - _spinner.Width) / 2,
                    (_parentForm.ClientSize.Height - _spinner.Height) / 2
                );
            }
        }

        /// <summary>
        /// Shows the loading spinner
        /// </summary>
        public void Show()
        {
            if (_spinner != null)
            {
                _spinner.BringToFront();
                _spinner.Visible = true;
                CenterSpinner();
                _parentForm.Refresh();
            }
        }

        /// <summary>
        /// Hides the loading spinner
        /// </summary>
        public void Hide()
        {
            if (_spinner != null)
            {
                _spinner.Visible = false;
                _parentForm.Refresh();
            }
        }

        /// <summary>
        /// Executes an action while showing the loading spinner
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ExecuteWithSpinner(Func<Task> action)
        {
            try
            {
                Show();
                await action();
            }
            finally
            {
                Hide();
            }
        }

        /// <summary>
        /// Executes an action while showing the loading spinner and returns a result
        /// </summary>
        /// <typeparam name="T">The type of the result</typeparam>
        /// <param name="action">The action to execute</param>
        /// <returns>The result of the action</returns>
        public async Task<T> ExecuteWithSpinner<T>(Func<Task<T>> action)
        {
            try
            {
                Show();
                return await action();
            }
            finally
            {
                Hide();
            }
        }
    }
}
