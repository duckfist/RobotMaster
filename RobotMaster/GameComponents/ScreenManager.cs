using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using RobotMaster.ScreenEvents;
using RobotMaster.GameScreens;

namespace RobotMaster.GameComponents
{
    public class ScreenManager : GameComponent
    {
        #region Fields and Properties Region
        protected Stack<GameScreen> gameScreens = new Stack<GameScreen>();
        public event EventHandler<ScreenEventArgs> OnScreenChange;

        const int startDrawOrder = 5000;
        const int drawOrderInc = 100;
        int drawOrder;
        public GameScreen CurrentScreen
        {
            get { return gameScreens.Peek(); }
        }
        #endregion
        public GameScreen PreviousScreen
        {
            get { return gameScreens.ElementAt(1); }
        }

        #region Constructor
        public ScreenManager(Game game)
            : base(game)
        {
            drawOrder = startDrawOrder;
        }
        #endregion

        #region Methods Region
        public void PopScreen()
        {
            RemoveScreen();
            drawOrder -= drawOrderInc;
            if (OnScreenChange != null)
                OnScreenChange(this, new ScreenEventArgs(gameScreens.Peek()));
        }
        private void RemoveScreen()
        {
            GameScreen screen = (GameScreen)gameScreens.Peek();
            OnScreenChange -= screen.ScreenChange; 
            Game.Components.Remove(screen);
            gameScreens.Pop();
        }
        public void PushScreen(GameScreen newScreen)
        {
            drawOrder += drawOrderInc;
            newScreen.DrawOrder = drawOrder;
            AddScreen(newScreen);
            if (OnScreenChange != null)
                OnScreenChange(this, new ScreenEventArgs(newScreen));
        }
        private void AddScreen(GameScreen newScreen)
        {
            gameScreens.Push(newScreen);
            Game.Components.Add(newScreen);
            OnScreenChange += newScreen.ScreenChange;
        }
        public void ChangeScreens(GameScreen newScreen)
        {
            while (gameScreens.Count > 0)
                RemoveScreen();
            newScreen.DrawOrder = startDrawOrder;
            drawOrder = startDrawOrder;
            AddScreen(newScreen);
            if (OnScreenChange != null)
                OnScreenChange(this, new ScreenEventArgs(newScreen));
        }

        public override string ToString()
        {
            string stack = "";
            foreach (GameScreen screen in gameScreens)
            {
                stack += screen.ToString() + "\n";
            }
            return stack;
        }
        #endregion
    }
}
