using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPP.Utilities
{
    public static class FormatManager
    {
        public static void ApplyCustomTheme(Form form, string themeName)
        {
            if (themeName == "Dark")
            {
                form.BackColor = Color.Black;
                form.ForeColor = Color.White;
                // Other style changes...
            }
            else
            {
                form.BackColor = Color.White;
                form.ForeColor = Color.Black;
                // Other style changes...
            }
        }

        // Additional helper methods for managing form-specific themes TODO 
    }
}
