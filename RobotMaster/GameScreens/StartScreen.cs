using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RobotMaster.GameComponents;

namespace RobotMaster.GameScreens
{
    public class StartScreen : GameScreen
    {
        #region Field and Properties Region

        MenuComponent menu;
        BackgroundComponent background;
        SpriteFont spriteFont;
        string[] menuItems = { "New Game", "Load Game", "Exit" };
        ScreenManager manager;

        #endregion

        #region Constructor Region

        public StartScreen(Game game, ScreenManager manager)
            : base(game)
        {
            Content = Game.Content;
            this.manager = manager;
        }

        #endregion

        #region Game Component Methods Region

        protected override void LoadContent()
        {
            spriteFont = Content.Load<SpriteFont>(@"Fonts\Mega");
            menu = new MenuComponent(spriteFont, menuItems);

            Vector2 menuPosition = new Vector2(
                (Game1.WINDOW_WIDTH - menu.Width) / 1.5f,
                (Game1.WINDOW_HEIGHT - menu.Height) / 1.5f);

            menu.Position = menuPosition;

            background = new BackgroundComponent(GameRef, Content.Load<Texture2D>(@"Backgrounds\SelectScreen"), DrawMode.Fill);

            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            if (InputManager.KeyReleased(Keys.Enter) ||
                InputManager.ButtonPressed(PlayerIndex.One, Buttons.A))
            {
                switch (menu.SelectedIndex)
                {
                    case 0:
                        InputManager.Flush();
                        //manager.ChangeScreens(GameRef.StageSelectScreen);
                        manager.PushScreen(GameRef.StageSelectScreen);
                        break;
                    case 1:
                        InputManager.Flush();
                        //manager.ChangeScreens(GameRef.GamePlayScreen);
                        manager.PushScreen(GameRef.StageSelectScreen);
                        break;
                    case 2:
                        Game.Exit();
                        break;
                }
            }

            menu.Update();
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            Game1.SpriteBatch.Begin(
                SpriteSortMode.Deferred, // waits until .End() is called to draw
                BlendState.AlphaBlend,
                SamplerState.PointClamp, // Necessary for smooth scaling!
                null,
                null,
                null,
                Game1.Scale);

            background.Draw(Game1.SpriteBatch);
            menu.Draw(Game1.SpriteBatch);
            base.Draw(gameTime);

            Game1.SpriteBatch.End();
        }

        public override string ToString()
        {
            return "StartScreen";
        }

        #endregion
    }
}
