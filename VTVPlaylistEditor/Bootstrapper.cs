using Microsoft.Practices.Unity;
using Prism.Unity;
using VTVPlaylistEditor.Views;
using System.Windows;

namespace VTVPlaylistEditor
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}
