using Microsoft.Practices.Unity;
using Prism.Unity;
using VTVTrafficDataManager.Views;
using System.Windows;

namespace VTVTrafficDataManager
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
