[Previous content up to InitializeCustomComponents remains the same...]

        private void InitializeCustomComponents()
        {
            // Main panel setup
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Create main container panel
            Panel mainContainer = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Create info panel container with padding
            Panel infoPanelContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(20, 0, 20, 0)
            };

            // Add info panel
            ancestryInfoPanel = CreateInfoPanel(
                "Ancestry Viewer",
                "• View detailed family trees for any animal in your colony\n" +
                "• Track lineage across multiple generations\n" +
                "• Analyze breeding history and relationships\n" +
                "• Export pedigree charts for documentation"
            );
            ancestryInfoPanel.Dock = DockStyle.Fill;
            infoPanelContainer.Controls.Add(ancestryInfoPanel);
            mainContainer.Controls.Add(infoPanelContainer);

            // Create content container with padding
            Panel contentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create filter container with padding
            Panel filterContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10)
            };

            // Create filter section
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Generations selection
            generationsLabel = new Label
            {
                Text = "Generations:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 10)
            };
            filterPanel.Controls.Add(generationsLabel);

            generationsComboBox = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 8)
            };
            generationsComboBox.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            generationsComboBox.SelectedIndex = 2;
            generationsComboBox.SelectedIndexChanged += GenerationsComboBox_SelectedIndexChanged;
            filterPanel.Controls.Add(generationsComboBox);
            filterContainer.Controls.Add(filterPanel);
            contentContainer.Controls.Add(filterContainer);

            // Create split container for tree and info
            TableLayoutPanel splitContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 10, 0, 0)
            };
            splitContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            splitContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // TreeView panel (left side)
            Panel treeContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 10, 0)
            };

            ancestryTree = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                ShowNodeToolTips = true,
                HideSelection = false
            };
            ancestryTree.AfterSelect += AncestryTree_AfterSelect;
            treeContainer.Controls.Add(ancestryTree);
            splitContainer.Controls.Add(treeContainer, 0, 0);

            // Info panel (right side)
            infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };
            splitContainer.Controls.Add(infoPanel, 1, 0);
            contentContainer.Controls.Add(splitContainer);

            // Create button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 20, 0, 0)
            };

            printButton = CreateButton("Print Tree", 20);
            generatePedigree = CreateButton("Generate Pedigree", 130);

            buttonPanel.Controls.Add(printButton);
            buttonPanel.Controls.Add(generatePedigree);
    
            printButton.Click += PrintButton_Click;
            generatePedigree.Click += GeneratePedigree_Click;
            contentContainer.Controls.Add(buttonPanel);

            mainContainer.Controls.Add(contentContainer);
            this.Controls.Add(mainContainer);
            this.Controls.Add(headerPanel);

            InitializeDataGridView();
        }

[Rest of the file content remains unchanged...]
