using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.GameComponents;
using RobotMaster.GameScreens;

namespace RobotMaster.ScreenEvents
{
    public class ScreenEventArgs : EventArgs
    {
        GameScreen gameScreen;

        public GameScreen GameScreen
        {
            get { return gameScreen; }
            private set { gameScreen = value; }
        }

        public ScreenEventArgs(GameScreen gameScreen)
        {
            GameScreen = gameScreen;
        }
    }
}
