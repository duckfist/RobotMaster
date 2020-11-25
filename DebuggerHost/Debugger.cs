using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MMDebugger;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace DebuggerHost
{
    public class Debugger
    {
        public static MMDebugWindow DebugWindow = new MMDebugWindow();

        public static bool IsGameplayScreenEnabled
        {
            set
            {
                DebugWindow.IsGameplayScreenEnabled = value;
            }
            get
            {
                return DebugWindow.IsGameplayScreenEnabled;
            }
        }

        public static bool IsBaseGameEnabled
        {
            set
            {
                DebugWindow.IsBaseGameEnabled = value;
            }
            get
            {
                return DebugWindow.IsBaseGameEnabled;
            }
        }

        public static void Show(MMDebugEventHandler handleDebugEvents)
        {
            DebugWindow = new MMDebugWindow();

            DebugWindow.ShowActivated = false;      // Prevent from stealing focus!
            DebugWindow.HandleDebugEvent = new MMDebugEventHandler(handleDebugEvents);
            DebugWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        public static void WriteGameplayScreenDebug(DebugInfo debugInfo)
        {
            DebugWindow.MMPositionText = String.Format("({0:0.00},{1:0.00})", debugInfo.megaX, debugInfo.megaY);
            DebugWindow.MMPositionOppositeText = String.Format("({0:0.00},{1:0.00})", debugInfo.megaRight, debugInfo.megaDown);
            DebugWindow.MMVelocity = String.Format("({0:0.00},{1:0.00})", debugInfo.velocityX, debugInfo.velocityY);
            DebugWindow.CameraPositionText = String.Format("({0:0.00},{1:0.00})", debugInfo.cameraX, debugInfo.cameraY);
            DebugWindow.RoomNumber = String.Format("{0}", debugInfo.RoomNum);
            DebugWindow.IsAbleToJump = debugInfo.IsAbleToJump;
            DebugWindow.IsClimbing = debugInfo.IsClimbing;
            DebugWindow.IsJumping = debugInfo.IsJumping;
            DebugWindow.IsFalling = debugInfo.IsFalling;
        }

        public static void WriteGameDebug(DebugInfo debugInfo)
        {

            DebugWindow.ScreenStackText = debugInfo.ScreenStack;
            DebugWindow.TotalGameTime = debugInfo.TotalGameTime;
            DebugWindow.ElapsedGameTime = debugInfo.ElapsedGameTime;
            DebugWindow.IsRunningSlowly = debugInfo.IsRunningSlowly;
        }


        public static void Exit(Thread thread)
        {
            Console.WriteLine($">>> Debugger.Exit called on thread {Thread.CurrentThread.ManagedThreadId} for thread {thread.ManagedThreadId}");

            if (thread.IsAlive)
            {
                System.Windows.Threading.Dispatcher.FromThread(thread).InvokeShutdown();
            }
        }
    }
}
