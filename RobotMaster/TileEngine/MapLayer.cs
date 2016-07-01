using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RobotMaster.TileEngine
{
    public class MapLayer
    {
        #region Field and Property Region
        Tile[,] map;
        public int Width
        {
            get { return map.GetLength(1); }
        }

        public int Height
        {
            get { return map.GetLength(0); }
        }
        #endregion

        #region Constructor Region
        public MapLayer(Tile[,] map)
        {
            this.map = (Tile[,])map.Clone();
        }

        public MapLayer(int width, int height)
        {
            map = new Tile[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = new Tile(23, 1, new Point(x,y));    // Transparent tile
                }
            }
        }
        #endregion

        #region Method region
        public Tile GetTile(int x, int y)
        {
            return map[y, x];
        }

        public Tile GetTile(Point p)
        {
            try
            {
                return map[p.Y, p.X];
            }
            catch (Exception e)
            {
                return new Tile(-1, 0, p);
            }
        }

        public void SetTile(Point p, Tile tile)
        {
            map[p.Y, p.X] = tile;
        }


        public void SetTile(int x, int y, Tile tile)
        {
            map[y, x] = tile;
        }

        public void SetTile(int x, int y, int tileIndex, int tileset)
        {
            map[y, x] = new Tile(tileIndex, tileset, new Point(x, y));
        }

        #endregion
    }
}
