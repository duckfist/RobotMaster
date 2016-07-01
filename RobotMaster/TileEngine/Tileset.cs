using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotMaster.TileEngine
{
    public class Tileset
    {
        public static int FrameLength = 10;
        public static int FrameTimer = 0;
        public static int CurrentFrame = 0;

        public static void Tick()
        {
            if (++FrameTimer >= FrameLength)
            {
                FrameTimer = 0;
                if (++CurrentFrame == int.MaxValue)
                {
                    CurrentFrame = 0;
                }
            }
        }

        #region Fields and Properties
        Texture2D image;
        int tileWidthInPixels;
        int tileHeightInPixels;
        int tilesWide;
        int tilesHigh;
        Rectangle[] sourceRectangles;
        TileCollision[] collisionTypes;
        int[] animations;
        

        public Texture2D Texture
        {
            get { return image; }
            private set { image = value; }
        }
        public int TileWidth
        {
            get { return tileWidthInPixels; }
            private set { tileWidthInPixels = value; }
        }
        public int TileHeight
        {
            get { return tileHeightInPixels; }
            private set { tileHeightInPixels = value; }
        }
        public int TilesWide
        {
            get { return tilesWide; }
            private set { tilesWide = value; }
        }
        public int TilesHigh
        {
            get { return tilesHigh; }
            private set { tilesHigh = value; }
        }
        public Rectangle[] SourceRectangles
        {
            get { return (Rectangle[])sourceRectangles.Clone(); }
        }
        public TileCollision[] CollisionTypes
        {
            get { return collisionTypes; }
        }
        public int[] Animations
        {
            get { return animations; }
        }
        #endregion

        #region Constructor
        public Tileset(Texture2D image, int tilesWide, int tilesHigh, int tileWidth, int tileHeight, TileCollision[] collisionTypes, int[] animations)
        {
            Texture = image;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            TilesWide = tilesWide;
            TilesHigh = tilesHigh;
            int tiles = tilesWide * tilesHigh;
            this.collisionTypes = collisionTypes;
            this.animations = animations;

            sourceRectangles = new Rectangle[tiles];
            int tile = 0;
            for (int y = 0; y < tilesHigh; y++)
                for (int x = 0; x < tilesWide; x++)
                {
                    sourceRectangles[tile] = new Rectangle(
                        x * tileWidth,
                        y * tileHeight,
                        tileWidth,
                        tileHeight);
                    tile++;
                }
        }
        #endregion

        #region Methods
        #endregion
    }
}
