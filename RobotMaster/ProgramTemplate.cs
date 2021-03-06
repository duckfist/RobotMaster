﻿using System;

namespace RobotMaster
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class ProgramTemplate
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1Template())
                game.Run();
        }
    }
#endif
}
