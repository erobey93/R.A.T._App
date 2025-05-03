//using System.Drawing;
//using System.Windows.Forms;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using RATAPP.Helpers;

//TODO
//namespace RATAPPLibraryUT
//{
//    [TestClass]
//    public class FormStyleHelperTests
//    {
//        private readonly Color PRIMARY_COLOR = Color.FromArgb(0, 120, 212);
//        private readonly Color SECONDARY_COLOR = Color.FromArgb(240, 240, 240);
//        private readonly Color TEXT_COLOR = Color.FromArgb(60, 60, 60);

//        [TestMethod]
//        public void ApplyHeaderStyle_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var panel = new Panel();
//            var title = "Test Header";

//            // Act
//            FormStyleHelper.ApplyHeaderStyle(panel, title);

//            // Assert
//            Assert.AreEqual(DockStyle.Top, panel.Dock);
//            Assert.AreEqual(60, panel.Height);
//            Assert.AreEqual(PRIMARY_COLOR, panel.BackColor);

//            var label = panel.Controls[0] as Label;
//            Assert.IsNotNull(label);
//            Assert.AreEqual(title, label.Text);
//            Assert.AreEqual(Color.White, label.ForeColor);
//            Assert.AreEqual(new Point(20, 12), label.Location);
//            Assert.AreEqual("Segoe UI", label.Font.Name);
//            Assert.AreEqual(20, label.Font.Size);
//            Assert.IsTrue(label.Font.Bold);
//        }

//        [TestMethod]
//        public void ApplyButtonStyle_PrimaryButton_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var button = new Button();
//            const int width = 150;
//            const int height = 40;

//            // Act
//            FormStyleHelper.ApplyButtonStyle(button, true, width, height);

//            // Assert
//            Assert.AreEqual(width, button.Width);
//            Assert.AreEqual(height, button.Height);
//            Assert.AreEqual(PRIMARY_COLOR, button.BackColor);
//            Assert.AreEqual(Color.White, button.ForeColor);
//            Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
//            Assert.AreEqual(0, button.FlatAppearance.BorderSize);
//            Assert.AreEqual(Cursors.Hand, button.Cursor);
//            Assert.AreEqual("Segoe UI", button.Font.Name);
//            Assert.AreEqual(10, button.Font.Size);
//        }

//        [TestMethod]
//        public void ApplyButtonStyle_SecondaryButton_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var button = new Button();
//            const int width = 150;
//            const int height = 40;

//            // Act
//            FormStyleHelper.ApplyButtonStyle(button, false, width, height);

//            // Assert
//            Assert.AreEqual(width, button.Width);
//            Assert.AreEqual(height, button.Height);
//            Assert.AreEqual(SECONDARY_COLOR, button.BackColor);
//            Assert.AreEqual(Color.Black, button.ForeColor);
//            Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
//            Assert.AreEqual(1, button.FlatAppearance.BorderSize);
//            Assert.AreEqual(Cursors.Hand, button.Cursor);
//            Assert.AreEqual("Segoe UI", button.Font.Name);
//            Assert.AreEqual(10, button.Font.Size);
//        }

//        [TestMethod]
//        public void ApplyComboBoxStyle_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var comboBox = new ComboBox();
//            const int width = 300;

//            // Act
//            FormStyleHelper.ApplyComboBoxStyle(comboBox, width);

//            // Assert
//            Assert.AreEqual(width, comboBox.Width);
//            Assert.AreEqual(ComboBoxStyle.DropDownList, comboBox.DropDownStyle);
//            Assert.AreEqual(FlatStyle.Flat, comboBox.FlatStyle);
//            Assert.AreEqual(Color.White, comboBox.BackColor);
//            Assert.AreEqual("Segoe UI", comboBox.Font.Name);
//            Assert.AreEqual(10, comboBox.Font.Size);
//        }

//        [TestMethod]
//        public void ApplyGroupBoxStyle_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var groupBox = new GroupBox();
//            var title = "Test Group";
//            const int height = 300;

//            // Act
//            FormStyleHelper.ApplyGroupBoxStyle(groupBox, title, height);

//            // Assert
//            Assert.AreEqual(title, groupBox.Text);
//            Assert.AreEqual(height, groupBox.Height);
//            Assert.AreEqual(TEXT_COLOR, groupBox.ForeColor);
//            Assert.AreEqual(new Padding(15), groupBox.Padding);
//            Assert.AreEqual("Segoe UI", groupBox.Font.Name);
//            Assert.AreEqual(10, groupBox.Font.Size);
//            Assert.IsTrue(groupBox.Font.Bold);
//        }

//        [TestMethod]
//        public void ApplyDataGridViewStyle_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var grid = new DataGridView();

//            // Act
//            FormStyleHelper.ApplyDataGridViewStyle(grid);

//            // Assert
//            Assert.AreEqual(DataGridViewAutoSizeColumnsMode.Fill, grid.AutoSizeColumnsMode);
//            Assert.AreEqual(DataGridViewHeaderBorderStyle.Single, grid.RowHeadersBorderStyle);
//            Assert.AreEqual(DataGridViewSelectionMode.FullRowSelect, grid.SelectionMode);
//            Assert.IsFalse(grid.AllowUserToAddRows);
//            Assert.AreEqual(Color.White, grid.BackColor);
//            Assert.AreEqual(BorderStyle.None, grid.BorderStyle);
//            Assert.IsFalse(grid.RowHeadersVisible);
//            Assert.AreEqual(DataGridViewCellBorderStyle.SingleHorizontal, grid.CellBorderStyle);
//            Assert.AreEqual(Color.FromArgb(230, 230, 230), grid.GridColor);

//            // Check header style
//            Assert.AreEqual(PRIMARY_COLOR, grid.ColumnHeadersDefaultCellStyle.BackColor);
//            Assert.AreEqual(Color.White, grid.ColumnHeadersDefaultCellStyle.ForeColor);
//            Assert.IsTrue(grid.ColumnHeadersDefaultCellStyle.Font.Bold);

//            // Check alternating row style
//            Assert.AreEqual(Color.FromArgb(247, 247, 247), grid.AlternatingRowsDefaultCellStyle.BackColor);
//        }

//        [TestMethod]
//        public void ApplyTextBoxStyle_ShouldSetCorrectProperties()
//        {
//            // Arrange
//            var textBox = new TextBox();
//            const int width = 300;

//            // Act
//            FormStyleHelper.ApplyTextBoxStyle(textBox, width);

//            // Assert
//            Assert.AreEqual(width, textBox.Width);
//            Assert.AreEqual(BorderStyle.FixedSingle, textBox.BorderStyle);
//            Assert.AreEqual(Color.White, textBox.BackColor);
//            Assert.AreEqual("Segoe UI", textBox.Font.Name);
//            Assert.AreEqual(10, textBox.Font.Size);
//        }
//    }
//}
