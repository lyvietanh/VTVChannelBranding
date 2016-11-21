using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace VTVPlaylistEditor.ViewModels
{
    public class NotifyIconViewModel : BindableBase
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Activate();
                    Thread.Sleep(100);
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                }, () => Application.Current.MainWindow != null);
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                    Thread.Sleep(100);
                    Application.Current.MainWindow.Hide();
                }, () => Application.Current.MainWindow != null);
            }
        }


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (Application.Current.MainWindow.IsVisible == false)
                    {
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.Activate();
                        Thread.Sleep(100);
                        Application.Current.MainWindow.WindowState = WindowState.Maximized;
                        Thread.Sleep(250);
                    }
                    MainWindowViewModel vmMainWindow = Application.Current.MainWindow.DataContext as MainWindowViewModel;
                    if (vmMainWindow != null && vmMainWindow.ExitApplicationCommand.CanExecute(null))
                    {
                        vmMainWindow.ExitApplicationCommand.Execute(null);
                    }
                });
            }
        }

        public ICommand OpenSettingWindowCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (Application.Current.MainWindow.IsVisible == false)
                    {
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.Activate();
                        Thread.Sleep(100);
                        Application.Current.MainWindow.WindowState = WindowState.Maximized;
                        Thread.Sleep(250);
                    }
                    MainWindowViewModel vmMainWindow = Application.Current.MainWindow.DataContext as MainWindowViewModel;
                    if (vmMainWindow != null && vmMainWindow.OpenSettingWindowCommand.CanExecute(Application.Current.MainWindow))
                    {
                        vmMainWindow.OpenSettingWindowCommand.Execute(Application.Current.MainWindow);
                    }
                });
            }
        }
    }
}
