using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace RobotMaster.GameComponents
{
    public enum DrawMode { Center, Fill }
    public class BackgroundComponent
    {
        #region Fields and Properties

        Rectangle screenRectangle;
        Rectangle destination;
        Texture2D image;
        DrawMode drawMode;
        public bool Visible
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public BackgroundComponent(Game game, Texture2D image, DrawMode drawMode)
        {
            Visible = true;
            this.image = image;
            this.drawMode = drawMode;
            screenRectangle = new Rectangle(
                0,
                0,
                Game1.WINDOW_WIDTH,
                Game1.WINDOW_HEIGHT
                );
            switch (drawMode)
            {
                case DrawMode.Center:
                    destination = new Rectangle(
                        (screenRectangle.Width - image.Width) / 2,
                        (screenRectangle.Height - image.Height) / 2,
                        image.Width,
                        image.Height);
                    break;
                case DrawMode.Fill:
                    destination = new Rectangle(
                       screenRectangle.X,
                       screenRectangle.Y,
                       screenRectangle.Width,
                       screenRectangle.Height);
                    break;
            }
        }

        #endregion

        #region Methods

        public void Draw(SpriteBatch spriteBatch, int alpha = 255)
        {
            if (Visible)
                spriteBatch.Draw(image, destination, new Color(alpha, alpha, alpha, alpha));
        }

        public void Draw(SpriteBatch spriteBatch, Color color, int alpha = 255)
        {
            if (Visible)
                spriteBatch.Draw(image, destination, new Color(color.R, color.G, color.B, alpha));
        }


        #endregion
    }
}
