using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using RobotMaster.GameComponents;
using RobotMaster.ScreenEvents;

namespace RobotMaster.GameScreens
{
    public abstract class TransitiveGameScreen : GameScreen
    {
        // Fade information
        protected FadeState fadeState = FadeState.Neutral;
        protected int alpha = 0;
        protected int color = 0;  // For fade in/out of black
        protected double fadeTime = 1000;
        protected DateTime startFade = DateTime.MaxValue;
        protected TimeSpan fadeElapsed = TimeSpan.Zero;
        protected ScreenManager manager;

        public TransitiveGameScreen(Game game, ScreenManager manager)
            : base(game)
        {
            this.manager = manager;
        }

        internal protected override void ScreenChange(object sender, ScreenEventArgs e)
        {
            if (e.GameScreen == this)
            {
                fadeState = FadeState.FadeIn;
                startFade = DateTime.Now;
                Show();
            }
            else
                Hide();
        }

        internal protected void ChangeFadeState(FadeState state)
        {
            switch (state)
            {
                case FadeState.Neutral:
                    break;
                case FadeState.FadeIn:
                    break;
                case FadeState.FadeOut:
                    startFade = DateTime.Now;
                    break;
            }

            fadeState = state;
        }

        public override void Update(GameTime gameTime)
        {
            switch (fadeState)
            {
                case FadeState.FadeIn:
                    fadeElapsed = (DateTime.Now - startFade);

                    // Fade to black
                    if (fadeElapsed.TotalMilliseconds <= fadeTime / 2)
                    {
                        double normalized = fadeElapsed.TotalMilliseconds / (fadeTime / 2);
                        alpha = (byte)(byte.MaxValue * normalized);
                    }
                    // Fade to pause screen
                    else if (fadeElapsed.TotalMilliseconds <= fadeTime)
                    {
                        double normalized = (fadeElapsed.TotalMilliseconds - (fadeTime / 2)) / (fadeTime / 2);
                        color = (byte)(byte.MaxValue * normalized);
                    }
                    // Finished fading in
                    else
                    {
                        ChangeFadeState(FadeState.Neutral);
                    }
                    break;


                case FadeState.FadeOut:

                    fadeElapsed = (DateTime.Now - startFade);
                    // Fade to black
                    if (fadeElapsed.TotalMilliseconds <= fadeTime / 2)
                    {
                        double normalized = fadeElapsed.TotalMilliseconds / (fadeTime / 2);
                        color = byte.MaxValue - (byte)(byte.MaxValue * normalized);
                    }
                    // Fade to pause screen
                    else if (fadeElapsed.TotalMilliseconds <= fadeTime)
                    {
                        double normalized = (fadeElapsed.TotalMilliseconds - (fadeTime / 2)) / (fadeTime / 2);
                        alpha = byte.MaxValue - (byte)(byte.MaxValue * normalized);
                        if (alpha <= 0) alpha = 0;

                    }
                    else
                    {
                        // Finished fading out
                        color = 0;
                        fadeState = FadeState.Neutral;
                        manager.PopScreen();
                    }
                    break;
            }

            base.Update(gameTime);
        }
    }

    public enum FadeState { FadeIn, FadeOut, Neutral }
}