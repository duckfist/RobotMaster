using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;

using RobotMaster.GameComponents;
using RobotMaster.Controls;

namespace RobotMaster.GameScreens
{

    public class TitleScreen : GameScreen
    {
        #region Fields and Properties
        ScreenManager manager;
        ControlManager Controls;
        Label label;
        PlayerIndex playerIndex;
        #endregion


        #region Constructors
        public TitleScreen(Game game, ScreenManager screenManager)
            : base(game)
        {
            Content = GameRef.Content;
            manager = screenManager;
            Controls = new ControlManager();
        }
        #endregion


        #region Methods
        protected override void LoadContent()
        {
            SpriteFont spriteFont = Content.Load<SpriteFont>(@"Fonts\Mega");
            label = new Label(spriteFont);
            label.Text = "Press Start";
            label.Size = spriteFont.MeasureString(label.Text);
            label.Position = new Vector2(
                (GameRef.Window.ClientBounds.Width - label.Size.X) / 2,
                (int)((GameRef.Window.ClientBounds.Height - label.Size.Y) / 2));
            Controls.Add(label); base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.WasReleased(Session.PlayerInControl, Buttons.Start, Keys.Enter, out playerIndex))
            {
                InputManager.Flush();
                Session.PlayerInControl = playerIndex;
#if XBOX360
                if (!Guide.IsVisible)
                    StorageDevice.BeginShowSelector(GuideCallback, null);
#else
                StorageDevice.BeginShowSelector(GuideCallback, null);
#endif
            }
            base.Update(gameTime);
        }

        private void GuideCallback(IAsyncResult result)
        {
            StorageDevice selectedDevice;
            if (result.IsCompleted)
            {
                //selectedDevice = Guide.EndShowStorageDeviceSelector(result);
                selectedDevice = StorageDevice.EndShowSelector(result);
                if (selectedDevice != null)
                {
                    Session.StorageDevice = selectedDevice;
                    manager.PushScreen(GameRef.StartScreen);
                }
            }
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

            Controls.Draw(Game1.SpriteBatch);
            base.Draw(gameTime);
            Game1.SpriteBatch.End();
        }

        public override string ToString()
        {
            return "TitleScreen";
        }

        #endregion
    }
}