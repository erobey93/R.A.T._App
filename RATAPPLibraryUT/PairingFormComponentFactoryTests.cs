using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPP.Helpers;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class PairingFormComponentFactoryTests
    {
        [TestMethod]
        public void CreateLabel_WithoutLocation_ShouldSetCorrectProperties()
        {
            // Arrange
            var text = "Test Label";

            // Act
            var label = PairingFormComponentFactory.CreateLabel(text);

            // Assert
            Assert.AreEqual(text, label.Text);
            Assert.AreEqual("Segoe UI", label.Font.Name);
            Assert.AreEqual(10, label.Font.Size);
            Assert.IsTrue(label.AutoSize);
            Assert.AreEqual(AnchorStyles.Left | AnchorStyles.Right, label.Anchor);
        }

        [TestMethod]
        public void CreateLabel_WithLocation_ShouldSetCorrectProperties()
        {
            // Arrange
            var text = "Test Label";
            var location = new Point(10, 20);

            // Act
            var label = PairingFormComponentFactory.CreateLabel(text, location);

            // Assert
            Assert.AreEqual(text, label.Text);
            Assert.AreEqual(location, label.Location);
            Assert.AreEqual("Segoe UI", label.Font.Name);
            Assert.AreEqual(10, label.Font.Size);
            Assert.IsTrue(label.AutoSize);
            Assert.AreEqual(AnchorStyles.Left | AnchorStyles.Right, label.Anchor);
        }

        [TestMethod]
        public void CreateFormField_ShouldCreatePanelWithLabelAndInput()
        {
            // Arrange
            var labelText = "Test Field";
            var input = new TextBox();

            // Act
            var panel = PairingFormComponentFactory.CreateFormField(labelText, input);

            // Assert
            Assert.AreEqual(40, panel.Height);
            Assert.AreEqual(DockStyle.Top, panel.Dock);
            Assert.AreEqual(new Padding(10), panel.Padding);
            Assert.AreEqual(2, panel.Controls.Count);

            var label = panel.Controls[0] as Label;
            Assert.IsNotNull(label);
            Assert.AreEqual(labelText, label.Text);
            Assert.AreEqual(120, label.Width);
            Assert.AreEqual(new Point(0, 10), label.Location);

            var inputControl = panel.Controls[1];
            Assert.AreEqual(input, inputControl);
            Assert.AreEqual(new Point(130, 5), inputControl.Location);
            Assert.AreEqual(AnchorStyles.Left | AnchorStyles.Right, inputControl.Anchor);
        }

        [TestMethod]
        public void CreatePairingTabPage_WithoutContent_ShouldSetCorrectProperties()
        {
            // Arrange
            var title = "Test Tab";

            // Act
            var tabPage = PairingFormComponentFactory.CreatePairingTabPage(title);

            // Assert
            Assert.AreEqual(title, tabPage.Text);
            Assert.AreEqual(new Padding(20), tabPage.Padding);
            Assert.AreEqual(Color.White, tabPage.BackColor);
            Assert.AreEqual(0, tabPage.Controls.Count);
        }

        [TestMethod]
        public void CreatePairingTabPage_WithContent_ShouldSetCorrectProperties()
        {
            // Arrange
            var title = "Test Tab";
            var content = new Panel();

            // Act
            var tabPage = PairingFormComponentFactory.CreatePairingTabPage(title, content);

            // Assert
            Assert.AreEqual(title, tabPage.Text);
            Assert.AreEqual(new Padding(20), tabPage.Padding);
            Assert.AreEqual(Color.White, tabPage.BackColor);
            Assert.AreEqual(1, tabPage.Controls.Count);
            Assert.AreEqual(content, tabPage.Controls[0]);
            Assert.AreEqual(DockStyle.Fill, content.Dock);
        }

        [TestMethod]
        public void CreateButtonPanel_ShouldCreatePanelWithStyledButtons()
        {
            // Arrange
            var primaryButton = new Button { Text = "Primary" };
            var secondaryButton = new Button { Text = "Secondary" };

            // Act
            var panel = PairingFormComponentFactory.CreateButtonPanel(primaryButton, secondaryButton);

            // Assert
            Assert.AreEqual(60, panel.Height);
            Assert.AreEqual(DockStyle.Bottom, panel.Dock);
            Assert.AreEqual(new Padding(0, 10, 0, 0), panel.Padding);
            Assert.AreEqual(2, panel.Controls.Count);

            // Primary button should be styled as primary
            Assert.AreEqual(Color.FromArgb(0, 120, 212), primaryButton.BackColor);
            Assert.AreEqual(Color.White, primaryButton.ForeColor);
            Assert.AreEqual(0, primaryButton.FlatAppearance.BorderSize);

            // Secondary button should be styled as secondary
            Assert.AreEqual(Color.FromArgb(240, 240, 240), secondaryButton.BackColor);
            Assert.AreEqual(Color.Black, secondaryButton.ForeColor);
            Assert.AreEqual(1, secondaryButton.FlatAppearance.BorderSize);
        }

        [TestMethod]
        public void CreatePairingsGrid_ShouldCreateGridWithCorrectColumns()
        {
            // Act
            var grid = PairingFormComponentFactory.CreatePairingsGrid();

            // Assert
            Assert.AreEqual(6, grid.Columns.Count);

            // Verify column names and types
            Assert.IsInstanceOfType(grid.Columns["PairingID"], typeof(DataGridViewTextBoxColumn));
            Assert.IsInstanceOfType(grid.Columns["Project"], typeof(DataGridViewTextBoxColumn));
            Assert.IsInstanceOfType(grid.Columns["Dam"], typeof(DataGridViewTextBoxColumn));
            Assert.IsInstanceOfType(grid.Columns["Sire"], typeof(DataGridViewTextBoxColumn));
            Assert.IsInstanceOfType(grid.Columns["PairingDate"], typeof(DataGridViewTextBoxColumn));
            Assert.IsInstanceOfType(grid.Columns["Remove"], typeof(DataGridViewButtonColumn));

            // Verify Remove button column properties
            var removeColumn = grid.Columns["Remove"] as DataGridViewButtonColumn;
            Assert.IsTrue(removeColumn.UseColumnTextForButtonValue);
            Assert.AreEqual("Remove", removeColumn.Text);
        }

        [TestMethod]
        public void CreateInfoPanel_ShouldCreateGroupBoxWithInfoLabel()
        {
            // Arrange
            var title = "Test Info";
            var infoText = "This is test information";

            // Act
            var infoPanel = PairingFormComponentFactory.CreateInfoPanel(title, infoText);

            // Assert
            Assert.AreEqual(title, infoPanel.Text);
            Assert.AreEqual(DockStyle.Bottom, infoPanel.Dock);
            Assert.AreEqual(150, infoPanel.Height);
            Assert.AreEqual(1, infoPanel.Controls.Count);

            var label = infoPanel.Controls[0] as Label;
            Assert.IsNotNull(label);
            Assert.AreEqual(infoText, label.Text);
            Assert.AreEqual(new Point(20, 30), label.Location);
            Assert.AreEqual(new Size(700, 100), label.Size);
            Assert.AreEqual("Segoe UI", label.Font.Name);
            Assert.AreEqual(9.5f, label.Font.Size);
            Assert.IsFalse(label.AutoSize);
        }
    }
}
