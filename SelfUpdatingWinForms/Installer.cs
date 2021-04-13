using System.Threading.Tasks;

namespace SelfUpdatingApp
{
    public static partial class Installer
    {
        public static Task<bool> IsUpdateAvailableAsync(string id, bool showGUI)
        {
            if (showGUI)
            {
                using var f = new frmCheckForUpdates(id);
                f.ShowDialog();
                return Task.FromResult(f.UpdateAvailable);
            }
            else
            {
                return IsUpdateAvailableAsync(id);
            }
        }
    }
}
