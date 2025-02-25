using PdfSharp.Charting;
using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;

namespace RATAPP.Panels
{
    public class ReportsPanel : Panel, INavigable
    {
        private RatAppDbContext _context;
        private ReportsService _reportService;
        private ComboBox reportTypeComboBox;
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private Button generateReportButton;
        private TabControl reportTabControl;
        private Chart reportChart;
        private DataGridView reportDataGridView;

        public ReportsPanel(RatAppDbContext context)
        {
            _context = context;
            _reportService = new ReportsService(_context);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Panel properties
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Initialize controls
            InitializeControls();

            // Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RowStyles = {
                    new RowStyle(SizeType.Absolute, 100),
                    new RowStyle(SizeType.Percent, 100)
                }
            };

            Panel controlsPanel = new Panel { Dock = DockStyle.Fill };
            controlsPanel.Controls.AddRange(new Control[] { reportTypeComboBox, startDatePicker, endDatePicker, generateReportButton });

            mainLayout.Controls.Add(controlsPanel, 0, 0);
            //mainLayout.Controls.Add(reportTabControl, 0, 1);

            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private void InitializeControls()
        {
            // Report Type ComboBox
            reportTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Location = new System.Drawing.Point(20, 20),
                Size = new Size(200, 30)
            };
            reportTypeComboBox.Items.AddRange(new object[] { "Breeding Performance", "Litter Statistics", "Animal Health" });
            reportTypeComboBox.SelectedIndex = 0;

            // Date Pickers
            startDatePicker = new DateTimePicker
            {
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Location = new System.Drawing.Point(240, 20),
                Size = new Size(150, 30)
            };

            endDatePicker = new DateTimePicker
            {
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Location = new System.Drawing.Point(410, 20),
                Size = new Size(150, 30)
            };

            // Generate Report Button
            generateReportButton = new Button
            {
                Text = "Generate Report",
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Location = new System.Drawing.Point(580, 20),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generateReportButton.FlatAppearance.BorderSize = 0;
            generateReportButton.Click += GenerateReportButton_Click;

            //// Tab Control for Chart and Data Grid
            //reportTabControl = new TabControl
            //{
            //    Dock = DockStyle.Fill,
            //    Font = new System.Drawing.Font("Segoe UI", 10F)
            //};

            //// Chart Tab
            //TabPage chartTab = new TabPage("Chart");
            //reportChart = new Chart
            //{
            //    //Dock = DockStyle.Fill,
            //    //BackColor = Color.White
            //};
            //chartTab.Controls.Add(reportChart);

            //// Data Grid Tab
            //TabPage dataGridTab = new TabPage("Data");
            //reportDataGridView = new DataGridView
            //{
            //    Dock = DockStyle.Fill,
            //    BackgroundColor = Color.White,
            //    Font = new Font("Segoe UI", 9F),
            //    AllowUserToAddRows = false,
            //    AllowUserToDeleteRows = false,
            //    ReadOnly = true
            //};
            //dataGridTab.Controls.Add(reportDataGridView);

            //reportTabControl.TabPages.Add(chartTab);
            //reportTabControl.TabPages.Add(dataGridTab);
        }

        private async void GenerateReportButton_Click(object sender, EventArgs e)
        {
            //string reportType = reportTypeComboBox.SelectedItem.ToString();
            //DateTime startDate = startDatePicker.Value;
            //DateTime endDate = endDatePicker.Value;

            //try
            //{
            //    var reportData = await _reportService.GenerateReportAsync(reportType, startDate, endDate);
            //    UpdateChart(reportData);
            //    UpdateDataGrid(reportData);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void UpdateChart(object reportData)
        {
            //// Clear existing series
            //reportChart.Series.Clear();

            //// Add new series based on report data
            //// This is a placeholder implementation. You'll need to adjust this based on your actual report data structure.
            //Series series = new Series("Report Data");
            //series.ChartType = SeriesChartType.Column;

            //// Assuming reportData is a Dictionary<string, int> for this example
            //foreach (var item in (Dictionary<string, int>)reportData)
            //{
            //    series.Points.AddXY(item.Key, item.Value);
            //}

            //reportChart.Series.Add(series);
            //reportChart.Titles.Clear();
            //reportChart.Titles.Add(reportTypeComboBox.SelectedItem.ToString());
        }

        private void UpdateDataGrid(object reportData)
        {
            // Clear existing data
            reportDataGridView.DataSource = null;
            reportDataGridView.Columns.Clear();

            // Assuming reportData is a List<T> where T is a custom class representing your report data
            // You'll need to adjust this based on your actual report data structure
            reportDataGridView.DataSource = reportData;

            // Auto-size columns
            reportDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        public async Task RefreshDataAsync()
        {
            // Implement if needed, or leave as is if no refresh is required
            await Task.CompletedTask;
        }
    }
}