using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RobotMaster.Mathematics;

namespace RobotMaster.Entities
{
    public class DebugRectangle
    {
        int width;
        int height;

        public DebugRectangle(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public DebugRectangle(Vector2 size)
        {
            this.width = (int)size.X;
            this.height = (int)size.Y;
        }

        public static Texture2D GetRectTexture(int width, int height)
        {

            Texture2D t = new Texture2D(Game1.graphics.GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < width; ++i)
            {
                data[i] = Color.White;
                data[(height - 1) * width + i] = Color.White;
            }
            for (int i = 1; i < height; i++)
            {
                data[i * width] = Color.White;
                data[i * width - 1] = Color.White;
            }
            t.SetData(data);
            return t;
        }
        public static Texture2D GetRectTexture(Vector2 size)
        {
            return GetRectTexture((int)size.X, (int)size.Y);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, Texture2D tex)
        {
            Rectangle rect = new Rectangle((int)Math.Ceiling(position.X), (int)Math.Ceiling(position.Y), width, height);
            spriteBatch.Draw(tex, rect, color);
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, Color color, Texture2D tex)
        {
            Rectangle rect = new Rectangle(x, y, width, height);
            spriteBatch.Draw(tex, rect, color);
        }

    }
}
