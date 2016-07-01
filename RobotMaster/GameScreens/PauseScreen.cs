using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RobotMaster.GameComponents;
using RobotMaster.ScreenEvents;

namespace RobotMaster.GameScreens
{


    public class PauseScreen : TransitiveGameScreen
    {
        #region Field and Properties Region

        

        // Menu items and controls
        MenuComponent menu;
        BackgroundComponent background;
        SpriteFont spriteFont;
        string[] menuItems = { "New Game", "Load Game", "Exit" };

        #endregion

        #region Constructor Region

        public PauseScreen(Game game, ScreenManager manager)
            : base(game, manager)
        {
            Content = Game.Content;
        }

        #endregion

        #region Game Component Methods Region

        protected override void LoadContent()
        {
            spriteFont = Content.Load<SpriteFont>(@"Fonts\Mega");
            menu = new MenuComponent(spriteFont, menuItems);

            Vector2 menuPosition = new Vector2(
                (Game.Window.ClientBounds.Width - menu.Width) / 1.5f,
                (Game.Window.ClientBounds.Height - menu.Height) / 1.5f);

            menu.Position = menuPosition;

            background = new BackgroundComponent(GameRef, Content.Load<Texture2D>(@"Backgrounds\PauseScreen"), DrawMode.Center);

            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            if (InputManager.KeyPressed(Keys.D) && 
                fadeState == FadeState.Neutral)
            {
                ChangeFadeState(FadeState.FadeOut);
            }

            menu.Update();
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            // Draw previous screen
            if (fadeState != FadeState.Neutral)
                manager.PreviousScreen.Draw(gameTime);


            // Draw pause screen
            Game1.SpriteBatch.Begin(
                SpriteSortMode.Deferred, // waits until .End() is called to draw
                BlendState.AlphaBlend,
                SamplerState.PointClamp, // Necessary for smooth scaling!
                null,
                null,
                null,
                Game1.Scale);

            background.Draw(Game1.SpriteBatch, new Color(color, color, color), alpha);
            

            base.Draw(gameTime);
            Game1.SpriteBatch.End();
        }

        public override string ToString()
        {
            return "PauseScreen";
        }

        #endregion
    }
}
