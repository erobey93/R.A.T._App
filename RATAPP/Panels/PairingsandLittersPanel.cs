using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RATAPP.Panels
{
    public partial class PairingsAndLittersPanel : Panel
    {
        private Button addPairingButton;
        private Button updatePairingButton;
        private Button deletePairingButton;
        private Button addLitterButton;
        private Button updateLitterButton;
        private Button deleteLitterButton;
        private DataGridView pairingsGridView;
        private DataGridView littersGridView;
        private Panel pairingsButtonsPanel;
        private Panel littersButtonsPanel;
        private Panel pairingsGridPanel;
        private Panel littersGridPanel;

        public PairingsAndLittersPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Set the Dock style for the custom panel
            this.Dock = DockStyle.Fill;
            BackColor = Color.LightBlue;

            // Initialize Pairings and Litters panels
            InitializePairingsPanel();
            InitializeLittersPanel();
        }

        private void InitializePairingsPanel()
        {
            // Create the panel for Pairings section
            var pairingsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200
            };

            // Initialize Pairing Buttons Panel
            pairingsButtonsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };
            InitializePairingButtons();
            pairingsPanel.Controls.Add(pairingsButtonsPanel);

            // Initialize Pairings Grid Panel
            pairingsGridPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            InitializePairingsGrid();
            pairingsPanel.Controls.Add(pairingsGridPanel);

            // Add pairings panel to the custom panel
            this.Controls.Add(pairingsPanel);
        }

        private void InitializeLittersPanel()
        {
            // Create the panel for Litters section
            var littersPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200
            };

            // Initialize Litter Buttons Panel
            littersButtonsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };
            InitializeLitterButtons();
            littersPanel.Controls.Add(littersButtonsPanel);

            // Initialize Litters Grid Panel
            littersGridPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            InitializeLittersGrid();
            littersPanel.Controls.Add(littersGridPanel);

            // Add litters panel to the custom panel
            this.Controls.Add(littersPanel);
        }

        private void InitializePairingButtons()
        {
            // Add Pairing Button
            addPairingButton = new Button
            {
                Text = "Add Pairing",
                Font = new Font("Arial", 12F),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                Width = 200,
                Height = 40,
                Location = new Point(20, 20)
            };
            addPairingButton.Click += AddPairingButton_Click;

            // Update Pairing Button
            updatePairingButton = new Button
            {
                Text = "Update Pairing",
                Font = new Font("Arial", 12F),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                Width = 200,
                Height = 40,
                Location = new Point(240, 20)
            };
            updatePairingButton.Click += UpdatePairingButton_Click;

            // Delete Pairing Button
            deletePairingButton = new Button
            {
                Text = "Delete Pairing",
                Font = new Font("Arial", 12F),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                Width = 200,
                Height = 40,
                Location = new Point(460, 20)
            };
            deletePairingButton.Click += DeletePairingButton_Click;

            // Add buttons to the pairingsButtonsPanel
            pairingsButtonsPanel.Controls.Add(addPairingButton);
            pairingsButtonsPanel.Controls.Add(updatePairingButton);
            pairingsButtonsPanel.Controls.Add(deletePairingButton);
        }

        private void InitializeLitterButtons()
        {
            // Add Litter Button
            addLitterButton = new Button
            {
                Text = "Add Litter",
                Font = new Font("Arial", 12F),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                Width = 200,
                Height = 40,
                Location = new Point(20, 20)
            };
            addLitterButton.Click += AddLitterButton_Click;

            // Update Litter Button
            updateLitterButton = new Button
            {
                Text = "Update Litter",
                Font = new Font("Arial", 12F),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                Width = 200,
                Height = 40,
                Location = new Point(240, 20)
            };
            updateLitterButton.Click += UpdateLitterButton_Click;

            // Delete Litter Button
            deleteLitterButton = new Button
            {
                Text = "Delete Litter",
                Font = new Font("Arial", 12F),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                Width = 200,
                Height = 40,
                Location = new Point(460, 20)
            };
            deleteLitterButton.Click += DeleteLitterButton_Click;

            // Add buttons to the littersButtonsPanel
            littersButtonsPanel.Controls.Add(addLitterButton);
            littersButtonsPanel.Controls.Add(updateLitterButton);
            littersButtonsPanel.Controls.Add(deleteLitterButton);
        }

        private void InitializePairingsGrid()
        {
            // Pairings DataGridView
            pairingsGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(pairingsGridPanel.Width - 40, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Columns =
                {
                    new DataGridViewTextBoxColumn { HeaderText = "Pairing ID", Name = "PairingID" },
                    new DataGridViewTextBoxColumn { HeaderText = "Male Animal", Name = "MaleAnimal" },
                    new DataGridViewTextBoxColumn { HeaderText = "Female Animal", Name = "FemaleAnimal" },
                    new DataGridViewTextBoxColumn { HeaderText = "Breeding Date", Name = "BreedingDate" }
                }
            };

            // Add the DataGrid to the pairingsGridPanel
            pairingsGridPanel.Controls.Add(pairingsGridView);
        }

        private void InitializeLittersGrid()
        {
            // Litters DataGridView
            littersGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(littersGridPanel.Width - 40, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Columns =
                {
                    new DataGridViewTextBoxColumn { HeaderText = "Litter ID", Name = "LitterID" },
                    new DataGridViewTextBoxColumn { HeaderText = "Dam", Name = "Dam" },
                    new DataGridViewTextBoxColumn { HeaderText = "Sire", Name = "Sire" },
                    new DataGridViewTextBoxColumn { HeaderText = "Litter Date", Name = "LitterDate" }
                }
            };

            // Add the DataGrid to the littersGridPanel
            littersGridPanel.Controls.Add(littersGridView);
        }

        // Button Click Event Handlers (Just placeholders for now)
        private void AddPairingButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add Pairing functionality will be implemented.");
        }

        private void UpdatePairingButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Update Pairing functionality will be implemented.");
        }

        private void DeletePairingButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Delete Pairing functionality will be implemented.");
        }

        private void AddLitterButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add Litter functionality will be implemented.");
        }

        private void UpdateLitterButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Update Litter functionality will be implemented.");
        }

        private void DeleteLitterButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Delete Litter functionality will be implemented.");
        }
    }
}

