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

namespace RobotMaster.GameScreens
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class GameScreen 
        : DrawableGameComponent
    {
        #region Fields and Properties

        List<GameComponent> childComponents;
        protected ContentManager Content;
        protected Game1 GameRef;

        public List<GameComponent> Components
        {
            get { return childComponents; }
        }

        #endregion

        #region Constructors
        
        public GameScreen(Game game)
            : base(game)
        {
            childComponents = new List<GameComponent>();
            GameRef = (Game1)game;
        }
        #endregion

        #region Game Component Methods
        public override void Initialize()
        {
            base.Initialize();
        }
        
        public override void Update(GameTime gameTime)
        {
            foreach (GameComponent component in childComponents)
            {
                if (component.Enabled)
                    component.Update(gameTime);
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            DrawableGameComponent drawComponent;
            foreach (GameComponent component in childComponents)
            {
                if (component is DrawableGameComponent)
                {
                    drawComponent = component as DrawableGameComponent;
                    if (drawComponent.Visible)
                        drawComponent.Draw(gameTime);
                }
            }
            base.Draw(gameTime);
        }
        #endregion

        #region Methods
        internal protected virtual void ScreenChange(object sender, ScreenEventArgs e)
        {
            if (e.GameScreen == this)
                Show();
            else
                Hide();
        }
        protected void Show()
        {
            Visible = true;
            Enabled = true;
            foreach (GameComponent component in childComponents)
            {
                component.Enabled = true;
                if (component is DrawableGameComponent)
                    ((DrawableGameComponent)component).Visible = true;
            }
        }
        protected void Hide()
        {
            Visible = false;
            Enabled = false;
            foreach (GameComponent component in childComponents)
            {
                component.Enabled = false;
                if (component is DrawableGameComponent)
                    ((DrawableGameComponent)component).Visible = false;
            }
        }
        #endregion
    }
}
