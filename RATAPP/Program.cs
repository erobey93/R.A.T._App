using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPP
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup DbContext
            var optionsBuilder = new DbContextOptionsBuilder<RatAppDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection")); // Ensure your connection string is in appsettings.json
            var context = new RatAppDbContext(optionsBuilder.Options);

            // Initialize PasswordHashing
            var passwordHashing = new PasswordHashing();

            // Start the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm(context, configuration, passwordHashing));
        }
    }
}