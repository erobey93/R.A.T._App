using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPP.Helpers;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class LoadingSpinnerHelperTests
    {
        private Form _testForm;
        private LoadingSpinnerHelper _spinnerHelper;
        private const string TEST_IMAGE_PATH = "RATAPP/Resources/Loading_2.gif";

        [TestInitialize]
        public void Setup()
        {
            _testForm = new Form
            {
                ClientSize = new Size(800, 600)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testForm.Dispose();
        }

        [TestMethod]
        public void Constructor_ShouldInitializeSpinnerCorrectly()
        {
            // Act
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);

            // Assert
            var spinner = GetSpinnerFromForm();
            Assert.IsNotNull(spinner);
            Assert.AreEqual(new Size(50, 50), spinner.Size);
            Assert.AreEqual(PictureBoxSizeMode.StretchImage, spinner.SizeMode);
            Assert.IsFalse(spinner.Visible);
        }

        [TestMethod]
        public void Show_ShouldMakeSpinnerVisible()
        {
            // Arrange
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);

            // Act
            _spinnerHelper.Show();

            // Assert
            var spinner = GetSpinnerFromForm();
            Assert.IsTrue(spinner.Visible);
        }

        [TestMethod]
        public void Hide_ShouldMakeSpinnerInvisible()
        {
            // Arrange
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);
            _spinnerHelper.Show();

            // Act
            _spinnerHelper.Hide();

            // Assert
            var spinner = GetSpinnerFromForm();
            Assert.IsFalse(spinner.Visible);
        }

        [TestMethod]
        public void FormResize_ShouldCenterSpinner()
        {
            // Arrange
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);
            var spinner = GetSpinnerFromForm();
            var initialLocation = spinner.Location;

            // Act
            _testForm.ClientSize = new Size(1000, 800);
            _testForm.OnResize(EventArgs.Empty);

            // Assert
            var expectedX = (_testForm.ClientSize.Width - spinner.Width) / 2;
            var expectedY = (_testForm.ClientSize.Height - spinner.Height) / 2;
            Assert.AreEqual(new Point(expectedX, expectedY), spinner.Location);
            Assert.AreNotEqual(initialLocation, spinner.Location);
        }

        [TestMethod]
        public async Task ExecuteWithSpinner_ShouldShowAndHideSpinner()
        {
            // Arrange
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);
            var spinner = GetSpinnerFromForm();
            bool actionExecuted = false;

            // Act
            await _spinnerHelper.ExecuteWithSpinner(async () =>
            {
                Assert.IsTrue(spinner.Visible, "Spinner should be visible during execution");
                await Task.Delay(100); // Simulate work
                actionExecuted = true;
            });

            // Assert
            Assert.IsTrue(actionExecuted, "Action should have executed");
            Assert.IsFalse(spinner.Visible, "Spinner should be hidden after execution");
        }

        [TestMethod]
        public async Task ExecuteWithSpinner_WithResult_ShouldShowAndHideSpinner()
        {
            // Arrange
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);
            var spinner = GetSpinnerFromForm();
            const string expectedResult = "Test Result";

            // Act
            var result = await _spinnerHelper.ExecuteWithSpinner(async () =>
            {
                Assert.IsTrue(spinner.Visible, "Spinner should be visible during execution");
                await Task.Delay(100); // Simulate work
                return expectedResult;
            });

            // Assert
            Assert.AreEqual(expectedResult, result, "Should return correct result");
            Assert.IsFalse(spinner.Visible, "Spinner should be hidden after execution");
        }

        [TestMethod]
        public async Task ExecuteWithSpinner_WhenExceptionOccurs_ShouldHideSpinnerAndThrowException()
        {
            // Arrange
            _spinnerHelper = new LoadingSpinnerHelper(_testForm, TEST_IMAGE_PATH);
            var spinner = GetSpinnerFromForm();
            var expectedException = new Exception("Test exception");

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _spinnerHelper.ExecuteWithSpinner(async () =>
                {
                    Assert.IsTrue(spinner.Visible, "Spinner should be visible during execution");
                    await Task.Delay(100); // Simulate work
                    throw expectedException;
                });
            });

            Assert.IsFalse(spinner.Visible, "Spinner should be hidden after exception");
        }

        private PictureBox GetSpinnerFromForm()
        {
            return _testForm.Controls[0] as PictureBox;
        }
    }
}
