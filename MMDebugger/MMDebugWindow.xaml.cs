using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace MMDebugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MMDebugWindow : Window
    {
        public MMDebugEventHandler HandleDebugEvent;

        public String MMPositionText
        {
            set 
            {
                this.Dispatcher.Invoke((Action)(() => { lblMMPosition.Content = value; }));
            }
        }

        public String MMPositionOppositeText
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblMMPositionOpposite.Content = value; }));
            }
        }

        public String MMVelocity
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblMMVelocity.Content = value; }));
            }
        }

        public String CameraPositionText
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblCamPosition.Content = value; }));
            }
        }

        public String RoomNumber
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblRoomNum.Content = value; }));
            }
        }

        public String ScreenStackText
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { tbkScreenStack.Text = value; }));
            }
        }

        public bool IsJumping
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblIsJumping.Content = value; }));
            }
        }
        public bool IsAbleToJump
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblIsAbleToJump.Content = value; }));
            }
        }
        public bool IsFalling
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblIsFalling.Content = value; }));
            }
        }
        public bool IsClimbing
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() => { lblIsClimbing.Content = value; }));
            }
        }
        public bool IsRunningSlowly
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() =>
                { 
                    lblIsRunningSlowly.Content = value;
                    if (value)
                    {
                        lblIsRunningSlowly.Background = Brushes.Red;
                    }
                    else
                    {
                        lblIsRunningSlowly.Background = Brushes.White;
                    }
                }));

            }
        }
        public TimeSpan TotalGameTime
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    lblTotalGameTimeFull.Content = value.ToString();
                    //lblTotalGameTime.Content = String.Format("{0:0.0}", value.TotalMilliseconds);
                }));
            }
        }
        public TimeSpan ElapsedGameTime
        {
            set
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    lblElapsedGameTimeFull.Content = value.ToString();
                    lblFPS.Content = String.Format("{0:0.0}", 1000.0 / value.TotalMilliseconds);
                    //lblElapsedGameTime.Content = String.Format("{0:0.0}", value.TotalMilliseconds);
                }));
            }
        }

        public bool IsGameplayScreenEnabled
        {
            set
            {
                grdGameplayScreen.IsEnabled = value;
            }
            get
            {
                return grdGameplayScreen.IsEnabled;
            }
        }

        public bool IsBaseGameEnabled
        {
            set
            {
                grdBaseGame.IsEnabled = value;
            }
            get
            {
                return grdBaseGame.IsEnabled;
            }
        }

        public MMDebugWindow()
        {
            InitializeComponent();
        }

        private void btnRestartLevel_Click(object sender, RoutedEventArgs e)
        {
            MMDebugEventArgs args = new MMDebugEventArgs();
            args.RestartLevel = true;
            HandleDebugEvent(sender, args);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            MMDebugEventArgs args = new MMDebugEventArgs();
            if (btnPause.IsChecked == true)
            {
                btnPause.Content = "Resume";
                args.PauseLevel = true;
            }
            else
            {
                btnPause.Content = "Pause";
                args.PauseLevel = false;
            }
            HandleDebugEvent(sender, args);
        }

        private void chkDrawHitboxes_Checked(object sender, RoutedEventArgs e)
        {
            MMDebugEventArgs args = new MMDebugEventArgs();
            args.DrawHitboxes = true;
            HandleDebugEvent(sender, args);
        }

        private void chkDrawHitboxes_Unchecked(object sender, RoutedEventArgs e)
        {
            MMDebugEventArgs args = new MMDebugEventArgs();
            args.DrawHitboxes = false;
            HandleDebugEvent(sender, args);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Console.WriteLine($">>> MMDebugWindow Closed event (Thread ID: {Thread.CurrentThread.ManagedThreadId})");

            Dispatcher.InvokeShutdown();
        }
    }

    public class MMDebugEventArgs : EventArgs
    {
        public bool? RestartLevel;
        public bool? PauseLevel;
        public bool? DrawHitboxes;
    }

    public delegate void MMDebugEventHandler(object sender, MMDebugEventArgs e);

}
