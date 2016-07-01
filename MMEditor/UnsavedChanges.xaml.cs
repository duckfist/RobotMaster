using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Threading;

namespace MMEditor
{
    /// <summary>
    /// Interaction logic for UnsavedChanges.xaml
    /// </summary>
    public partial class UnsavedChanges : Window
    {
        public SaveResult Result = SaveResult.Cancel;
        public bool closeRequest = false;
        private UIElement _parent;

        public UnsavedChanges(UIElement parent)
        {
            InitializeComponent();
            Result = SaveResult.Cancel;
            Visibility = Visibility.Hidden;
            _parent = parent;
        }


        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = SaveResult.Yes;
            closeRequest = true;
            Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = SaveResult.No;
            closeRequest = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = SaveResult.Cancel;
            closeRequest = true;
            Close();
        }

        public SaveResult ShowCustomDialog()
        {
            Visibility = Visibility.Visible;
            _parent.IsEnabled = false;

            while (!closeRequest)
            {
                // HACK: Stop the thread if the application is about to close
                if (this.Dispatcher.HasShutdownStarted ||
                    this.Dispatcher.HasShutdownFinished)
                {
                    break;
                }

                // HACK: Simulate "DoEvents"
                this.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
                Thread.Sleep(20);
            }

            _parent.IsEnabled = true;
            return Result;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closeRequest = true;
        }
    }

    public enum SaveResult
    {
        Yes,
        No,
        Cancel
    }
}
