[Previous content up to InitializeCustomComponents remains the same...]

        private async void InitializeDataGridView()
        {
            // Clear existing controls
            infoPanel.Controls.Clear();

            // Create and configure DataGridView
            animalCollectionDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false
            };

            await GetAnimalData(); 

            // Create columns with proper data binding
            animalCollectionDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "name",
                HeaderText = "Name",
                Name = "colName"
            });

            animalCollectionDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "regNum",
                HeaderText = "Registration #",
                Name = "colRegNum"
            });

            animalCollectionDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "species",
                HeaderText = "Species",
                Name = "colSpecies"
            });

            // Create binding source
            var bindingSource = new BindingSource
            {
                DataSource = _animals
            };

            // Assign binding source to DataGridView
            animalCollectionDataGridView.DataSource = bindingSource;

            // Set up proper selection handling
            animalCollectionDataGridView.SelectionChanged += AnimalDataGridView_SelectionChanged;

            // Add to panel
            infoPanel.Controls.Add(animalCollectionDataGridView);
        }

        private Button CreateButton(string text, int x)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, 0),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Panel CreateInfoPanel(string title, string content)
        {
            Panel panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            Label contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(10, 35),
                MaximumSize = new Size(800, 0)
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(contentLabel);
            return panel;
        }

        private async Task GetAnimalData()
        {
            if (!isSingleAnimal)
            {
                _animals = await _animalService.GetAllAnimalsAsync();
            }
            else
            {
                _animals = new AnimalDto[] { _currentAnimal };
            }
        }

        private void AnimalDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (animalCollectionDataGridView.SelectedRows.Count == 0) return;

            var selectedAnimal = (AnimalDto)animalCollectionDataGridView.SelectedRows[0].DataBoundItem;
            _currentAnimal = selectedAnimal;
            LoadAnimalAncestry(_currentAnimal);
        }

        public void LoadAnimalAncestry(AnimalDto animal)
        {
            if (animal == null) return;

            _currentAnimal = animal;

            // Clear existing tree
            ancestryTree.Nodes.Clear();

            // Create root node for the selected rodent
            TreeNode rootNode = CreateRodentNode(animal);
            ancestryTree.Nodes.Add(rootNode);

            // Load parents based on the selected number of generations
            int generations = int.Parse(generationsComboBox.SelectedItem.ToString());
            LoadParents(rootNode, animal, 1, generations);

            // Expand the tree
            ancestryTree.ExpandAll();

            // Select the root node
            ancestryTree.SelectedNode = rootNode;
        }

        private async void LoadParents(TreeNode parentNode, AnimalDto animal, int currentGeneration, int maxGenerations)
        {
            if (currentGeneration >= maxGenerations || animal == null) return;

            Animal dam = await _lineageService.GetDamByAnimalId(animal.Id);
            Animal sire = await _lineageService.GetSireByAnimalId(animal.Id);

            AnimalDto damDto = await _animalService.MapSingleAnimaltoDto(dam);
            AnimalDto sireDto = await _animalService.MapSingleAnimaltoDto(sire);

            if (animal.sireId != null)
            {
                TreeNode fatherNode = CreateRodentNode(sireDto);
                parentNode.Nodes.Add(fatherNode);
                LoadParents(fatherNode, sireDto, currentGeneration + 1, maxGenerations);
            }
            else
            {
                TreeNode unknownNode = new TreeNode("Unknown Father");
                unknownNode.ForeColor = Color.Gray;
                parentNode.Nodes.Add(unknownNode);
            }

            if (dam != null)
            {
                TreeNode motherNode = CreateRodentNode(damDto);
                parentNode.Nodes.Add(motherNode);
                LoadParents(motherNode, damDto, currentGeneration + 1, maxGenerations);
            }
            else
            {
                TreeNode unknownNode = new TreeNode("Unknown Mother");
                unknownNode.ForeColor = Color.Gray;
                parentNode.Nodes.Add(unknownNode);
            }
        }

        private TreeNode CreateRodentNode(AnimalDto animal)
        {
            TreeNode node = new TreeNode($"{animal.name} (ID: {animal.Id})");

            if (animal.sex == "Male")
            {
                node.ForeColor = Color.Blue;
            }
            else if (animal.sex == "Female")
            {
                node.ForeColor = Color.DeepPink;
            }

            node.ToolTipText = $"Name: {animal.name}\n" +
                              $"ID: {animal.Id}\n" +
                              $"Gender: {animal.sex}\n" +
                              $"DOB: {animal.DateOfBirth.ToShortDateString()}\n" +
                              $"Coat: {animal.color}";

            node.Tag = animal;
            return node;
        }

        private void AncestryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is AnimalDto selectedRodent)
            {
                DisplayAnimalInfo(selectedRodent);
            }
            else
            {
                infoPanel.Controls.Clear();
                Label unknownLabel = new Label
                {
                    Text = "No information available",
                    Location = new Point(10, 10),
                    Size = new Size(260, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                infoPanel.Controls.Add(unknownLabel);
            }
        }

        private void DisplayAnimalInfo(AnimalDto animal)
        {
            infoPanel.Controls.Clear();

            if (animal == null) return;

            int yPos = 10;

            if (animal.imageUrl != null && File.Exists(animal.imageUrl))
            {
                try
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Location = new Point(90, yPos),
                        Size = new Size(100, 100),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = Image.FromFile(animal.imageUrl),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    infoPanel.Controls.Add(pictureBox);
                    yPos += 110;
                }
                catch
                {
                    yPos += 10;
                }
            }
            else
            {
                Panel noImagePanel = new Panel
                {
                    Location = new Point(90, yPos),
                    Size = new Size(100, 100),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightGray
                };

                Label noImageLabel = new Label
                {
                    Text = "No Image",
                    Location = new Point(0, 40),
                    Size = new Size(100, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                noImagePanel.Controls.Add(noImageLabel);
                infoPanel.Controls.Add(noImagePanel);
                yPos += 110;
            }

            AddInfoLabel("Name:", animal.name, yPos); yPos += 25;
            AddInfoLabel("ID:", animal.Id.ToString(), yPos); yPos += 25;
            AddInfoLabel("Gender:", animal.sex, yPos); yPos += 25;
            AddInfoLabel("Date of Birth:", animal.DateOfBirth.ToShortDateString(), yPos); yPos += 25;
            AddInfoLabel("Age:", CalculateAge(animal.DateOfBirth), yPos); yPos += 25;
            AddInfoLabel("Coat Color:", animal.color, yPos); yPos += 25;

            Panel separator = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(260, 1),
                BackColor = Color.LightGray
            };
            infoPanel.Controls.Add(separator);
            yPos += 10;

            Button viewDetailsButton = new Button
            {
                Text = "View Animal's Page",
                Location = new Point(70, yPos),
                Size = new Size(140, 30),
                BackColor = SystemColors.Control
            };
            viewDetailsButton.Click += (sender, e) => ViewFullDetails(animal);
            infoPanel.Controls.Add(viewDetailsButton);

            Button clearSelectionButton = new Button
            {
                Text = "Clear Selection",
                Location = new Point(70, yPos + 40),
                Size = new Size(140, 30),
                BackColor = SystemColors.Control
            };
            clearSelectionButton.Click += (sender, e) => ClearSelection();
            infoPanel.Controls.Add(clearSelectionButton);
        }

        private void AddInfoLabel(string labelText, string value, int yPosition)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(10, yPosition),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label valueLabel = new Label
            {
                Text = value,
                Location = new Point(110, yPosition),
                Size = new Size(160, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(label);
            infoPanel.Controls.Add(valueLabel);
        }

        private string CalculateAge(DateTime dateOfBirth)
        {
            DateTime now = DateTime.Now;
            int months = (now.Year - dateOfBirth.Year) * 12 + now.Month - dateOfBirth.Month;

            if (months < 1)
            {
                int days = (int)(now - dateOfBirth).TotalDays;
                return $"{days} days";
            }
            else if (months < 24)
            {
                return $"{months} months";
            }
            else
            {
                int years = months / 12;
                int remainingMonths = months % 12;
                return remainingMonths > 0 ? $"{years} years, {remainingMonths} months" : $"{years} years";
            }
        }

        private void ViewFullDetails(AnimalDto animal)
        {
            MessageBox.Show("Will navigate to individual animal page");
        }

        private async void ClearSelection()
        {
            isSingleAnimal = false;

            animalCollectionDataGridView.ClearSelection();
            this.Controls.Clear();
            InitializeComponent();
            InitializeHeaderPanel();
            InitializeCustomComponents();
        }

        private void GenerationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentAnimal != null)
            {
                LoadAnimalAncestry(_currentAnimal);
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Printing functionality would be implemented here.", "Print Tree", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GeneratePedigree_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export functionality would be implemented here.", "Export Tree", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public async Task RefreshDataAsync()
        {
            await GetAnimalData();
            this.Refresh();
        }
    }
}
