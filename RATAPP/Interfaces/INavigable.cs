using System.Threading.Tasks;

namespace RATAPP.Interfaces
{
    /// <summary>
    /// Interface for components that support navigation and data refresh.
    /// </summary>
    public interface INavigable
    {
        /// <summary>
        /// Refreshes the data displayed in the component.
        /// </summary>
        Task RefreshDataAsync();

        /// <summary>
        /// Saves the current state of the component.
        /// </summary>
        void SaveState();

        /// <summary>
        /// Restores the previously saved state of the component.
        /// </summary>
        void RestoreState();
    }
}
