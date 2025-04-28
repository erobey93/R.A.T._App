using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RATAPPLibrary.Data.Models.Research;
using RATAPPLibrary.Services;

namespace RATAPP.Panels
{
    public class ResearchPanel : UserControl, INavigable
    {
        private readonly ResearchService _researchService;
        
        // UI Components
        private TabControl mainTabControl;
        private TabPage studiesTab;
        private TabPage dataEntryTab;
        private TabPage analysisTab;
        
        private DataGridView studiesGrid;
        private Button addStudyButton;
        private Button editStudyButton;
        private Button deleteStudyButton;
        
        private Panel studyDetailsPanel;
        private ComboBox studyTypeComboBox;
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private TextBox studyNameTextBox;
        private TextBox descriptionTextBox;
        
        private Panel animalSelectionPanel;
        private ListBox availableAnimalsList;
        private ListBox selectedAnimalsList;
        private ComboBox groupTypeComboBox;
        private Label animalCountLabel;
        
        private Panel dataPointsPanel;
        private DataGridView dataPointsGrid;
        private Button addDataPointButton;
        
        private Panel dataEntryPanel;
        private ComboBox studySelectionComboBox;
        private ComboBox animalSelectionComboBox;
        private FlowLayoutPanel dataEntryFieldsPanel;
        private Button saveObservationButton;
        
        private Panel analysisPanel;
        private ComboBox analysisStudyComboBox;
        private ComboBox dataPointSelectionComboBox;
        private DataGridView resultsGrid;
        private Panel chartPanel;

        private readonly RATAppBaseForm _baseForm;

        public ResearchPanel(RATAppBaseForm baseForm, ResearchService researchService)
        {
            _researchService = researchService;
            InitializeComponent();
            InitializeEventHandlers();
        }

        public async Task RefreshDataAsync()
        {
            await LoadStudies();
        }

        private void InitializeComponent()
        {
            // Main layout
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Initialize tabs
            studiesTab = new TabPage("Studies");
            dataEntryTab = new TabPage("Data Entry");
            analysisTab = new TabPage("Analysis");
            
            mainTabControl.TabPages.Add(studiesTab);
            mainTabControl.TabPages.Add(dataEntryTab);
            mainTabControl.TabPages.Add(analysisTab);

            // Studies Tab Components
            InitializeStudiesTab();
            
            // Data Entry Tab Components
            InitializeDataEntryTab();
            
            // Analysis Tab Components
            InitializeAnalysisTab();

            Controls.Add(mainTabControl);
        }

        private void InitializeStudiesTab()
        {
            // Studies Grid
            studiesGrid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 200,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            
            // Study Management Buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };
            
            addStudyButton = new Button { Text = "Add Study" };
            editStudyButton = new Button { Text = "Edit Study" };
            deleteStudyButton = new Button { Text = "Delete Study" };
            
            buttonPanel.Controls.AddRange(new Control[] { addStudyButton, editStudyButton, deleteStudyButton });
            
            // Study Details Panel
            studyDetailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            // Add components to study details panel
            studyNameTextBox = new TextBox { Width = 200 };
            descriptionTextBox = new TextBox { Width = 200, Multiline = true, Height = 60 };
            studyTypeComboBox = new ComboBox { Width = 200 };
            startDatePicker = new DateTimePicker { Width = 200 };
            endDatePicker = new DateTimePicker { Width = 200 };
            
            var detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(5)
            };
            
            detailsLayout.Controls.Add(new Label { Text = "Study Name:" }, 0, 0);
            detailsLayout.Controls.Add(studyNameTextBox, 1, 0);
            detailsLayout.Controls.Add(new Label { Text = "Description:" }, 0, 1);
            detailsLayout.Controls.Add(descriptionTextBox, 1, 1);
            detailsLayout.Controls.Add(new Label { Text = "Type:" }, 0, 2);
            detailsLayout.Controls.Add(studyTypeComboBox, 1, 2);
            detailsLayout.Controls.Add(new Label { Text = "Start Date:" }, 0, 3);
            detailsLayout.Controls.Add(startDatePicker, 1, 3);
            detailsLayout.Controls.Add(new Label { Text = "End Date:" }, 0, 4);
            detailsLayout.Controls.Add(endDatePicker, 1, 4);
            
            studyDetailsPanel.Controls.Add(detailsLayout);
            
            // Add all components to studies tab
            studiesTab.Controls.Add(studyDetailsPanel);
            studiesTab.Controls.Add(buttonPanel);
            studiesTab.Controls.Add(studiesGrid);
        }

        private void InitializeDataEntryTab()
        {
            // Study and Animal Selection
            var selectionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };
            
            studySelectionComboBox = new ComboBox { Width = 200 };
            animalSelectionComboBox = new ComboBox { Width = 200 };
            
            selectionPanel.Controls.Add(new Label { Text = "Study:", Location = new Point(10, 10) });
            selectionPanel.Controls.Add(studySelectionComboBox);
            selectionPanel.Controls.Add(new Label { Text = "Animal:", Location = new Point(220, 10) });
            selectionPanel.Controls.Add(animalSelectionComboBox);
            
            // Data Entry Fields
            dataEntryFieldsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            
            saveObservationButton = new Button
            {
                Text = "Save Observation",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            
            dataEntryTab.Controls.Add(saveObservationButton);
            dataEntryTab.Controls.Add(dataEntryFieldsPanel);
            dataEntryTab.Controls.Add(selectionPanel);
        }

        private void InitializeAnalysisTab()
        {
            // Analysis Controls
            var controlsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };
            
            analysisStudyComboBox = new ComboBox { Width = 200 };
            dataPointSelectionComboBox = new ComboBox { Width = 200 };
            
            controlsPanel.Controls.Add(new Label { Text = "Study:", Location = new Point(10, 10) });
            controlsPanel.Controls.Add(analysisStudyComboBox);
            controlsPanel.Controls.Add(new Label { Text = "Data Point:", Location = new Point(220, 10) });
            controlsPanel.Controls.Add(dataPointSelectionComboBox);
            
            // Results Grid
            resultsGrid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 200,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            // Chart Panel
            chartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            
            analysisTab.Controls.Add(chartPanel);
            analysisTab.Controls.Add(resultsGrid);
            analysisTab.Controls.Add(controlsPanel);
        }

        private void InitializeEventHandlers()
        {
            // Study Management
            addStudyButton.Click += AddStudyButton_Click;
            editStudyButton.Click += EditStudyButton_Click;
            deleteStudyButton.Click += DeleteStudyButton_Click;
            studiesGrid.SelectionChanged += StudiesGrid_SelectionChanged;
            
            // Data Entry
            studySelectionComboBox.SelectedIndexChanged += StudySelectionComboBox_SelectedIndexChanged;
            animalSelectionComboBox.SelectedIndexChanged += AnimalSelectionComboBox_SelectedIndexChanged;
            saveObservationButton.Click += SaveObservationButton_Click;
            
            // Analysis
            analysisStudyComboBox.SelectedIndexChanged += AnalysisStudyComboBox_SelectedIndexChanged;
            dataPointSelectionComboBox.SelectedIndexChanged += DataPointSelectionComboBox_SelectedIndexChanged;
            
            // Load initial data
            LoadStudies();
        }

        private async void LoadStudies()
        {
            var studies = await _researchService.GetStudiesAsync();
            studiesGrid.DataSource = studies;
            studySelectionComboBox.DataSource = new List<Study>(studies);
            analysisStudyComboBox.DataSource = new List<Study>(studies);
        }

        // Event Handlers
        private async void AddStudyButton_Click(object sender, EventArgs e)
        {
            var study = new Study
            {
                Name = studyNameTextBox.Text,
                Description = descriptionTextBox.Text,
                Type = (StudyType)studyTypeComboBox.SelectedItem,
                StartDate = startDatePicker.Value,
                EstimatedEndDate = endDatePicker.Value,
                Status = StudyStatus.Planned
            };

            await _researchService.CreateStudyAsync(study);
            LoadStudies();
        }

        private async void EditStudyButton_Click(object sender, EventArgs e)
        {
            if (studiesGrid.SelectedRows.Count > 0)
            {
                var study = (Study)studiesGrid.SelectedRows[0].DataBoundItem;
                study.Name = studyNameTextBox.Text;
                study.Description = descriptionTextBox.Text;
                study.Type = (StudyType)studyTypeComboBox.SelectedItem;
                study.StartDate = startDatePicker.Value;
                study.EstimatedEndDate = endDatePicker.Value;

                await _researchService.UpdateStudyAsync(study);
                LoadStudies();
            }
        }

        private async void DeleteStudyButton_Click(object sender, EventArgs e)
        {
            if (studiesGrid.SelectedRows.Count > 0)
            {
                var study = (Study)studiesGrid.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show("Are you sure you want to delete this study?", "Confirm Delete",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    await _researchService.DeleteStudyAsync(study.Id);
                    LoadStudies();
                }
            }
        }

        private void StudiesGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (studiesGrid.SelectedRows.Count > 0)
            {
                var study = (Study)studiesGrid.SelectedRows[0].DataBoundItem;
                studyNameTextBox.Text = study.Name;
                descriptionTextBox.Text = study.Description;
                studyTypeComboBox.SelectedItem = study.Type;
                startDatePicker.Value = study.StartDate;
                endDatePicker.Value = study.EstimatedEndDate;
            }
        }

        private async void StudySelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (studySelectionComboBox.SelectedItem is Study selectedStudy)
            {
                var dataPoints = await _researchService.GetStudyDataPointsAsync(selectedStudy.Id);
                RefreshDataEntryFields(dataPoints);
            }
        }

        private void AnimalSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update data entry fields based on selected animal
        }

        private async void SaveObservationButton_Click(object sender, EventArgs e)
        {
            // Collect and save observation data
        }

        private async void AnalysisStudyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (analysisStudyComboBox.SelectedItem is Study selectedStudy)
            {
                var dataPoints = await _researchService.GetStudyDataPointsAsync(selectedStudy.Id);
                dataPointSelectionComboBox.DataSource = dataPoints;
            }
        }

        private async void DataPointSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (analysisStudyComboBox.SelectedItem is Study selectedStudy &&
                dataPointSelectionComboBox.SelectedItem is DataPoint selectedDataPoint)
            {
                var stats = await _researchService.GetStudyStatsAsync(selectedStudy.Id, selectedDataPoint.Id);
                UpdateAnalysisView(stats);
            }
        }

        private void RefreshDataEntryFields(List<DataPoint> dataPoints)
        {
            dataEntryFieldsPanel.Controls.Clear();
            foreach (var dataPoint in dataPoints)
            {
                var label = new Label { Text = dataPoint.Name, Width = 150 };
                Control inputControl;

                switch (dataPoint.Type)
                {
                    case DataType.Text:
                        inputControl = new TextBox { Width = 200 };
                        break;
                    case DataType.Number:
                        inputControl = new NumericUpDown { Width = 200 };
                        break;
                    case DataType.Boolean:
                        inputControl = new CheckBox { Width = 200 };
                        break;
                    case DataType.Date:
                        inputControl = new DateTimePicker { Width = 200 };
                        break;
                    case DataType.Selection:
                        inputControl = new ComboBox { Width = 200 };
                        if (!string.IsNullOrEmpty(dataPoint.Options))
                        {
                            ((ComboBox)inputControl).Items.AddRange(
                                System.Text.Json.JsonSerializer.Deserialize<string[]>(dataPoint.Options));
                        }
                        break;
                    default:
                        inputControl = new TextBox { Width = 200 };
                        break;
                }

                var fieldPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Width = 400,
                    Height = 30,
                    Margin = new Padding(5)
                };

                fieldPanel.Controls.Add(label);
                fieldPanel.Controls.Add(inputControl);
                dataEntryFieldsPanel.Controls.Add(fieldPanel);
            }
        }

        private void UpdateAnalysisView(Dictionary<string, StudyStats> stats)
        {
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Metric");
            dataTable.Columns.Add("Value");

            if (stats.TryGetValue("overall", out var overallStats))
            {
                if (overallStats.Average.HasValue)
                    dataTable.Rows.Add("Average", overallStats.Average.Value);
                if (overallStats.Minimum.HasValue)
                    dataTable.Rows.Add("Minimum", overallStats.Minimum.Value);
                if (overallStats.Maximum.HasValue)
                    dataTable.Rows.Add("Maximum", overallStats.Maximum.Value);
                dataTable.Rows.Add("Count", overallStats.Count);
            }

            resultsGrid.DataSource = dataTable;
        }
    }
}
