using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.GameComponents;
using RobotMaster.Controls;
using RobotMaster.Entities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RobotMaster.GameScreens
{
    public class StageSelectScreen : GameScreen
    {
        #region Fields and Properties

        Rectangle[] bossPortraits = new Rectangle[9];          // Location of boss portraits on screen
        Rectangle[] bossPortraitsSource = new Rectangle[9];    // Location of boss portraits in image

        Point cursorOffset;                                  // Offset of cursor from boss portrait image
        Rectangle cursorPos;
        Rectangle cursorSource;                                // Location of coursor in image
        Texture2D StageSelectItems;     // Other images to use on screen

        int selectedIndex;
        public int SelectedIndex { get { return selectedIndex; } }

        ScreenManager manager;
        BackgroundComponent background;

        #endregion


        #region Constructors

        public StageSelectScreen(Game game, ScreenManager screenManager)
            : base(game)
        {
            Content = Game.Content;
            manager = screenManager;
            selectedIndex = 0;
        }

        #endregion

        #region Methods

        protected override void LoadContent()
        {
            StageSelectItems = Content.Load<Texture2D>(@"Backgrounds\SelectScreenItems");
            background = new BackgroundComponent(GameRef, Content.Load<Texture2D>(@"Backgrounds\SelectScreen"), DrawMode.Fill);

            // Boss portrait locations in source image
            for (int i = 0; i < bossPortraitsSource.Length; i++)
            {
                bossPortraitsSource[i] = new Rectangle(i * 32, 128, 32, 32);
            }
            cursorSource = new Rectangle(0, 160, 42, 42);
            cursorPos = new Rectangle(32, 32, 42, 42);
            cursorOffset = new Point(-5, -5);

            // Boss portrait locations on screen
            bossPortraits[0] = new Rectangle(32, 32, 32, 32);    // Blade man
            bossPortraits[1] = new Rectangle(112, 32, 32, 32);   // Solar man
            bossPortraits[2] = new Rectangle(192, 32, 32, 32);   // Sheep man

            bossPortraits[3] = new Rectangle(32, 96, 32, 32);   // Chill man
            bossPortraits[4] = new Rectangle(192, 96, 32, 32);   // Commando man

            bossPortraits[5] = new Rectangle(32, 160, 32, 32);   // Nitro man
            bossPortraits[6] = new Rectangle(112, 160, 32, 32);   // Strike man
            bossPortraits[7] = new Rectangle(192, 160, 32, 32);   // Pump man
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.KeyReleased(Keys.Enter) ||
                InputManager.KeyReleased(Keys.A) ||
                InputManager.ButtonPressed(PlayerIndex.One, Buttons.A))
            {
                switch (SelectedIndex)
                {
                    case 0:
                        manager.PushScreen(GameRef.GamePlayScreen);
                        GameRef.GamePlayScreen.StartLevel(0);
                        break;
                    case 1:
                        manager.PushScreen(GameRef.GamePlayScreen);
                        GameRef.GamePlayScreen.StartLevel(1);
                        break;
                    case 2:
                        manager.PushScreen(GameRef.GamePlayScreen);
                        GameRef.GamePlayScreen.StartLevel(2);
                        break;
                    case 3:
                        manager.PushScreen(GameRef.GamePlayScreen);
                        GameRef.GamePlayScreen.StartLevel(3);
                        break;
                    default:
                        Game.Exit();
                        break;
                }
            }

            if (InputManager.KeyPressed(Keys.Left))
            {
                switch (SelectedIndex)
                {
                    case 0:
                    case 1:
                    case 2:
                        if (--selectedIndex < 0) selectedIndex = 2;
                        break;
                    case 3:
                    case 4:
                        if (--selectedIndex < 3) selectedIndex = 4;
                        break;
                    case 5:
                    case 6:
                    case 7:
                        if (--selectedIndex < 5) selectedIndex = 7;
                        break;
                }
            }

            if (InputManager.KeyPressed(Keys.Right))
            {
                switch (SelectedIndex)
                {
                    case 0:
                    case 1:
                    case 2:
                        if (++selectedIndex > 2) selectedIndex = 0;
                        break;
                    case 3:
                    case 4:
                        if (++selectedIndex > 4) selectedIndex = 3;
                        break;
                    case 5:
                    case 6:
                    case 7:
                        if (++selectedIndex > 7) selectedIndex = 5;
                        break;
                }
            }

            if (InputManager.KeyPressed(Keys.Up))
            {
                switch (SelectedIndex)
                {
                    case 0: selectedIndex = 5; break;
                    case 1: selectedIndex = 6; break;
                    case 2: selectedIndex = 7; break;

                    case 3: selectedIndex = 0; break;
                    case 4: selectedIndex = 2; break;

                    case 5: selectedIndex = 3; break;
                    case 6: selectedIndex = 1; break;
                    case 7: selectedIndex = 4; break;
                }
            }

            if (InputManager.KeyPressed(Keys.Down))
            {
                switch (SelectedIndex)
                {
                    case 0: selectedIndex = 3; break;
                    case 1: selectedIndex = 6; break;
                    case 2: selectedIndex = 4; break;

                    case 3: selectedIndex = 5; break;
                    case 4: selectedIndex = 7; break;

                    case 5: selectedIndex = 0; break;
                    case 6: selectedIndex = 1; break;
                    case 7: selectedIndex = 2; break;
                }
            }

            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            Game1.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                Game1.Scale);

            background.Draw(Game1.SpriteBatch);

            // Draw boss portraits stuff
            for (int i = 0; i < bossPortraits.Length; i++ )
            {
                Game1.SpriteBatch.Draw(StageSelectItems, bossPortraits[i], bossPortraitsSource[i], Color.White);
            }

            // Draw cursor on and off every second
            if (gameTime.TotalGameTime.TotalMilliseconds % 500 < 250)
            {
                cursorPos.X = bossPortraits[selectedIndex].X + cursorOffset.X;
                cursorPos.Y = bossPortraits[selectedIndex].Y + cursorOffset.Y;
                Game1.SpriteBatch.Draw(StageSelectItems, cursorPos, cursorSource, Color.White);
            }

            base.Draw(gameTime);

            Game1.SpriteBatch.End();
        }

        public override string ToString()
        {
            return "StageSelectScreen";
        }

        #endregion
    }
}

