using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RobotMaster.Mathematics;
using RobotMaster.GameComponents;

namespace RobotMaster.Entities
{
    public class MeterDisplay
    {
        protected static Texture2D texUnitBuster;
        protected static Texture2D texUnitBoss;
        protected static Texture2D texUnitWeapon;

        public static int Width = 8;

        Texture2D background;
        Texture2D unitTexture;
        AttributePair value;
        Vector2 Position;

        public int Height { get { return ((int)value.MaximumValue) * 2 + 1; } }

        public MeterDisplay(AttributePair value, Vector2 screenPos, MeterColor color)
        {
            this.value = value;
            this.Position = screenPos;

            int height = ((int)value.MaximumValue) * 2 + 1;

            background = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);

            //Color[] data = new Color[Width * height];
            //for (int i = 0; i < Width * height; ++i)
            //{
            //    data[i] = Color.Black;
            //}
            background.SetData(new Color[] { Color.Black });

            switch (color)
            {
                case MeterColor.Buster:
                    unitTexture = texUnitBuster;
                    break;
                case MeterColor.Boss:
                    unitTexture = texUnitBoss;
                    break;
                case MeterColor.Weapon:
                    unitTexture = texUnitWeapon;
                    break;
                    
                default: break;
            }
        }

        public static void LoadContent()
        {
            texUnitBuster = MegaMath.CropTexture2D(Entity.TextureObjects, new Rectangle(8, 89, 6, 1));
            texUnitWeapon = MegaMath.CropTexture2D(Entity.TextureObjects, new Rectangle(8, 90, 6, 1));
            texUnitBoss = MegaMath.CropTexture2D(Entity.TextureObjects, new Rectangle(8, 91, 6, 1));
        }

        public void Draw(SpriteBatch spriteBatch, AttributePair newValue)
        {
            int x = (int)Math.Ceiling(Position.X + Session.Camera.Position.X);
            int y = (int)Math.Ceiling(Position.Y + Session.Camera.Position.Y);
            int h = Height;

            Rectangle rect = new Rectangle(x, y, Width, Height);
            spriteBatch.Draw(background, rect, Color.Black);

            for (int i = 0; i < newValue.CurrentValue; i++)
            {
                rect = new Rectangle(x + 1, y + h - 2 * (i + 1), 6, 1);
                spriteBatch.Draw(unitTexture, rect, Color.White);
            }

            value = newValue;
        }

        public enum MeterColor
        {
            Buster,Boss,Weapon
        }
    }

}
