using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using RobotMaster.Mathematics;
using RobotMaster.GameScreens;
using RobotMaster.GameComponents;
using RobotMaster.TileEngine;
using RobotMaster.Entities;

namespace RobotMaster.TileEngine
{
    [Serializable]
    public class SerializedLevel
    {
        // TODO: Support multiple layers, multiple textures

        // for tileset. Todo multiple tilesets?
        public int TilesWide;
        public int TilesHigh;
        public int TileWidth;
        public int TileHeight;

        public string TexturePath;              // Tileset png location
        public List<int> TilesetCollisionKey;   // Which Tiles are collidable, spikes, etc.
        public List<int> TilesetAnimationKey;
        public List<List<int>> Map;             // 2D array representing tilemap
        public List<List<int>> EnemyMap;
        public List<List<int>> ObstacleMap;
        public List<SerializedRoom> Rooms;
        public int SpawnRoom;
        public Point SpawnTile;

        public SerializedLevel()
        {
            TilesetCollisionKey = new List<int>();
            TilesetAnimationKey = new List<int>();
            Map = new List<List<int>>();
            EnemyMap = new List<List<int>>();
            ObstacleMap = new List<List<int>>();
            Rooms = new List<SerializedRoom>();
        }
    }

    public class Level
    {
        public Tileset tileset;
        public Texture2D tilesetTexture;         // Graphic for the tileset
        public Texture2D DebugTileRect;
        public TileCollision[] tilesetCollision; // Collision map key for the tileset
        public int[] tilesetAnimationKey;
        public int levelNumber;                  // Current level

        public TileMap map;
        public List<Enemy> Enemies;
        public List<Obstacle> Obstacles;
        public List<MMEffect> Effects;

        public Level(int levelNumber)
        {
            this.levelNumber = levelNumber;
        }

        public void LoadContent(ContentManager content)
        {
            SerializedLevel levelFile = Load(levelNumber);

            // TODO: List of tileset graphics!
            // Load tileset graphic
            string path = Directory.GetCurrentDirectory() + "\\" + levelFile.TexturePath;
            //string path = "Content\\Tiles\\Tiles_QuickMan.png";
            //tilesetTexture = content.Load<Texture2D>(path);
            //FileStream stream = new FileStream(path, FileMode.Open);
            //tilesetTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);

            tilesetTexture = content.Load<Texture2D>(levelFile.TexturePath);

            // TODO: List of collision keys for other tilesets!
            // Construct collision key
            tilesetCollision = new TileCollision[levelFile.TilesetCollisionKey.Count];
            for (int i = 0; i < tilesetCollision.Length; i++)
            {
                tilesetCollision[i] = (TileCollision)levelFile.TilesetCollisionKey[i];
            }

            // Construct animation key
            tilesetAnimationKey = new int[levelFile.TilesetAnimationKey.Count];
            for (int i = 0; i < tilesetAnimationKey.Length; i++)
            {
                tilesetAnimationKey[i] = levelFile.TilesetAnimationKey[i];
            }

            // TODO: List of tilesets!
            // Make Tileset object
            tileset = new Tileset(tilesetTexture,
                levelFile.TilesWide,
                levelFile.TilesHigh,
                levelFile.TileWidth,
                levelFile.TileHeight,
                tilesetCollision,
                tilesetAnimationKey);
            List<Tileset> tilesets = new List<Tileset>();
            tilesets.Add(tileset);

            
            // Deserialize and construct the tilemap
            List<List<int>> serializedMap = levelFile.Map;
            List<SerializedRoom> serializedRooms = levelFile.Rooms;
            List<Room> deserializedRooms = new List<Room>();

            // Construct each Room in the TileMap
            for (int i = 0; i < serializedRooms.Count; i++)
            {
                SerializedRoom serializedRoom = serializedRooms[i];

                // Build the Room's exit
                List<Exit> exits = new List<Exit>();
                for (int j = 0; j < serializedRoom.ExitDestinationRooms.Count; j++)
                {
                    Exit theExit = new Exit(serializedRoom.ExitScreenLocations[j],
                        (Direction)serializedRoom.ExitDirections[j],
                        serializedRoom.ExitDestinationRooms[j]);
                    exits.Add(theExit);
                }
                Room roomDeserialized = new Room(serializedRoom.RoomNumber,
                                                 serializedRoom.RoomLocation,
                                                 serializedRoom.RoomWidth,
                                                 exits);
                deserializedRooms.Add(roomDeserialized);
            }

            // Construct each Tile in the MapLayer
            List<MapLayer> layers = new List<MapLayer>();
            MapLayer theLayer = new MapLayer(serializedMap[0].Count, serializedMap.Count); // TODO: Several map layers!
            for (int y = 0; y < theLayer.Height; y++)
            {
                for (int x = 0; x < theLayer.Width; x++)
                {
                    Tile tile = new Tile(serializedMap[y][x], 0, new Point(x, y));
                    theLayer.SetTile(x, y, tile);
                }
            }
            layers.Add(theLayer);

            // Finally, construct the finished TileMap.
            map = new TileMap(tilesets, layers, deserializedRooms, levelFile.SpawnRoom, levelFile.SpawnTile);

            // Create enemy objects and special objects
            List<List<int>> serializedObstacles = levelFile.ObstacleMap;
            List<List<int>> serializedEnemies = levelFile.EnemyMap;
            Obstacles = new List<Obstacle>();
            Enemies = new List<Enemy>();
            Effects = new List<MMEffect>();

            for (int y = 0; y < map.HeightInTiles; y++)
            {
                for (int x = 0; x < map.WidthInTiles; x++)
                {
                    int enemyID = serializedEnemies[y][x];
                    if (enemyID >= 0)
                    {
                        Enemies.Add(Enemy.CreateEnemy((EnemyTypes)enemyID, x, y));
                    }

                    int obstacleID = (serializedObstacles.Count <= 0) ? -1 : serializedObstacles[y][x];
                    if (obstacleID >= 0)
                    {
                        Obstacles.Add(Obstacle.CreateObstacle((ObstacleTypes)obstacleID, x, y));
                    }
                }
            }



        }

        public static SerializedLevel Load(int levelIndex)
        {
            String path = String.Format("Levels\\level" + levelIndex + ".xml");

            //// Open a storage container
            //IAsyncResult result = Session.StorageDevice.BeginOpenContainer("MegaMan10x", null, null);

            //// Wait for WaitHandle to be signaled
            //result.AsyncWaitHandle.WaitOne();

            //StorageContainer container = Session.StorageDevice.EndOpenContainer(result);

            //// Close the wait handle.
            //result.AsyncWaitHandle.Close();

            SerializedLevel level = null;

            //string filename = "savegame.sav";
            try
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedLevel));
                level = (SerializedLevel)serializer.Deserialize(stream);
                stream.Close();
            }
            catch (FileNotFoundException e)
            {
                
            }
            //container.Dispose();

            return level;
        }

        public static void Write(int levelIndex, SerializedLevel level)
        {
            String path = String.Format("level" + levelIndex + ".xml");

            //// Open a storage container
            //IAsyncResult result = Session.StorageDevice.BeginOpenContainer("MegaMan10x", null, null);

            //// Wait for WaitHandle to be signaled
            //result.AsyncWaitHandle.WaitOne();

            //StorageContainer container = Session.StorageDevice.EndOpenContainer(result);

            //// Close the wait handle.
            //result.AsyncWaitHandle.Close();

            ////string filename = "savegame.sav";
            //if (container.FileExists(path))
            //    container.DeleteFile(path);
            FileStream stream = new FileStream(path, FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedLevel));
            serializer.Serialize(stream, level);
            stream.Close();
            //container.Dispose();
        }
    }
}
