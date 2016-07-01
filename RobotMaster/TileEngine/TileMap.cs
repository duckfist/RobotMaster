using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.GameComponents;
using RobotMaster.Mathematics;
using RobotMaster.Entities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace RobotMaster.TileEngine
{
    public class TileMap
    {
        #region Fields and Properties
        public static DebugRectangle debugRectangle;
        public static Texture2D debugRectTex;

        List<Tileset> tilesets;
        List<MapLayer> mapLayers;
        public List<Room> rooms;
        public int CurrentRoomNum;
        public int SpawnRoom;
        public Point SpawnTile;

        int mapWidth;
        int mapHeight;

        public int WidthInPixels
        {
            get { return mapWidth * Engine.TileWidth; }
        }
        public int HeightInPixels
        {
            get { return mapHeight * Engine.TileHeight; }
        }
        public int WidthInTiles
        {
            get { return mapWidth; }
        }
        public int HeightInTiles
        {
            get { return mapHeight; }
        }
        public int WidthInScreens
        {
            get { return mapWidth / Engine.ViewportWidthTiles; }
        }
        public int HeightInScreens
        {
            get { return mapHeight / Engine.ViewportHeightTiles; }
        }
        public List<Tileset> Tilesets
        {
            get { return tilesets; }
        }
        public List<Room> Rooms
        {
            get { return rooms; }
        }
        public Room CurrentRoom
        {
            get
            {
                return rooms[CurrentRoomNum];
            }
        }
        #endregion

        #region Constructors
        public TileMap(List<Tileset> tilesets, List<MapLayer> layers, List<Room> rooms, int spawnRoom, Point SpawnTile)
        {
            // Record tilemap's dimensions based on maplayers
            mapWidth = layers[0].Width;
            mapHeight = layers[0].Height;

            this.tilesets = tilesets;
            this.mapLayers = layers;

            this.rooms = rooms;
            this.SpawnRoom = spawnRoom;
            this.CurrentRoomNum = SpawnRoom;
            this.SpawnTile = SpawnTile;
        }

        public TileMap(Tileset tileset, MapLayer layer, List<Room> rooms, int spawnRoom, Point SpawnTile)
        {
            mapWidth = layer.Width;
            mapHeight = layer.Height;

            tilesets = new List<Tileset>();
            tilesets.Add(tileset);

            mapLayers = new List<MapLayer>();
            mapLayers.Add(layer);

            this.rooms = rooms;
            this.SpawnRoom = spawnRoom;
            this.CurrentRoomNum = SpawnRoom;
            this.SpawnTile = SpawnTile;
        }
        #endregion

        #region Methods

        public static void LoadContent()
        {
            debugRectangle = new DebugRectangle(16,16);
            debugRectTex = DebugRectangle.GetRectTexture(16,16);
        }

        internal void AddLayer(MapLayer layer)
        {
            if (layer.Width != mapWidth || layer.Height != mapHeight)
                throw new Exception("Error in layer size.");

            mapLayers.Add(layer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">Location in the current level.</param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public Tile GetTileAt(Vector2 v, int layerIndex)
        {
            Point p = Engine.VectorToCell(v);
            return mapLayers[layerIndex].GetTile(p);
        }


        public Tile GetTileAtIndex(Point p, int layerIndex)
        {
            return mapLayers[layerIndex].GetTile(p);
        }
        
        //public List<Tile> GetTilesAtSegment(Vector2 pt1, Vector2 pt2, bool horizontal, int layerIndex)
        //{
        //    Point p1 = Engine.VectorToCell(pt1);
        //    Point p2 = Engine.VectorToCell(pt2);

        //    List<Point> points = new List<Point>();

        //    if (horizontal)
        //    {
        //        if (p1.X > p2.X)
        //        {
        //            for (int i = 0; i <= p1.X - p2.X; i++)
        //            {
        //                points.Add(new Point(p2.X + i, p2.Y));
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < p2.X - p1.X; i++)
        //            {
        //                points.Add(new Point(p1.X + i, p1.Y));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (p1.Y > p2.Y)
        //        {
        //            for (int i = 0; i <= p1.Y - p2.Y; i++)
        //            {
        //                points.Add(new Point(p2.X, p2.Y + i));
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i <= p2.Y - p1.Y; i++)
        //            {
        //                points.Add(new Point(p1.X, p1.Y + i));
        //            }
        //        }
        //    }

        //    List<Tile> tilesFound = new List<Tile>();
        //    foreach (Point p in points)
        //    {
        //        tilesFound.Add(mapLayers[layerIndex].GetTile(p));
        //    }

        //    return tilesFound;
            
        //}

        /// <summary>
        /// Gets the tile collision type at a location in this tilemap's array.
        /// Configured so that tiles outside of the left/right bounds are solid,
        /// and the tiles outside the bottom/top bounds are passable.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public TileCollision GetCollision(int x, int y, int layerIndex)
        {
            if (x < 0 || x >= mapWidth)
            {
                return TileCollision.Impassable;
            }
            if (y < 0 || y >= mapHeight)
            {
                return TileCollision.Passable;
            }

            return mapLayers[layerIndex].GetTile(x,y).Collision;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destination = new Rectangle(0, 0, Engine.TileWidth, Engine.TileHeight);
            Tile tile;

            foreach (MapLayer layer in mapLayers)
            {

                Point camUpLeft = Engine.VectorToCell(Session.Camera.Position);
                Point camLowRight = Engine.VectorToCell(Session.Camera.Position + Engine.ViewportVector);
                Point min = new Point(); // Least tile location (begin drawing)
                Point max = new Point(); // Greatest tile location (end drawing)

                min.X = camUpLeft.X;
                min.Y = camUpLeft.Y;
                max.X = (int)Math.Min(camLowRight.X + 1, mapWidth);
                max.Y = (int)Math.Min(camLowRight.Y + 1, mapHeight);

                for (int y = min.Y; y < max.Y; y++)
                {
                    destination.Y = y * Engine.TileHeight;
                    //if (Session.IsScrolling) destination.Y -= Engine.ViewportHeight;

                    for (int x = min.X; x < max.X; x++)
                    {
                        tile = layer.GetTile(x, y);

                        if (tile.TileIndex == -1)   // Don't draw transparent tiles!
                            continue;

                        destination.X = x * Engine.TileWidth;

                        // DEBUG! Highlights the tile!
                        //if (tile.StandingOn)
                        //{
                        //    spriteBatch.Draw(
                        //        tilesets[tile.Tileset].Texture,
                        //        destination,
                        //        tilesets[tile.Tileset].SourceRectangles[47],
                        //        Color.Red);
                        //    tile.StandingOn = false;
                        //}
                        //else 
                        //{
                            // Animate tile if it has animation
                            int aniFrame = 0;
                            int aniLength = tilesets[tile.Tileset].Animations[tile.TileIndex];
                            if (aniLength > 1)
                            {
                                aniFrame = Tileset.CurrentFrame % aniLength;
                            }

                            // Draw the tile
                            spriteBatch.Draw(
                                tilesets[tile.Tileset].Texture,
                                destination,
                                tilesets[tile.Tileset].SourceRectangles[tile.TileIndex + aniFrame],
                                Color.White);
                        //}

                            if (Session.DebugHitboxes && tile.Collision != TileCollision.Passable)
                            {
                                debugRectangle.Draw(spriteBatch, destination.X, destination.Y, new Color(0,255,0), debugRectTex);
                            }
                    }
                }
            }
        }
        #endregion

    }


}
