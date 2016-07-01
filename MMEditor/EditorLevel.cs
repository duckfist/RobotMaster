using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;

using RobotMaster.TileEngine;
using RobotMaster.Entities;

namespace MMEditor
{
    class EditorLevel
    {
        public static bool UnsavedChanges = false;

        public static void Write(string file, SerializedLevel level)
        {
            String path = String.Format(file);

            //string filename = "savegame.sav";
            if (File.Exists(path))
                File.Delete(path);
            FileStream stream = new FileStream(path, System.IO.FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedLevel));
            serializer.Serialize(stream, level);
            stream.Close();
        }

        public static int IndexFromRowCol(int row, int col, int numCols)
        {
            return col + (row * numCols);
        }

        public static Point RowColFromIndex(int index, int numRows, int numCols)
        {
            return new Point(index % numCols, index / numCols);
        }

        public static string EnemyString(EnemyTypes enemy)
        {
            switch (enemy)
            {
                case EnemyTypes.MedusaHead:
                    return "MedusaHead";
                case EnemyTypes.SmallJumper:
                    return "SmallJumper";
                case EnemyTypes.Zoomer:
                    return "Zoomer";
                case EnemyTypes.CeilingSpider:
                    return "CeilingSpider";
                case EnemyTypes.TestBoss:
                    return "TestBoss";
            }

            return "";
        }

        public static string ObstacleString(ObstacleTypes obstacle)
        {
            switch (obstacle)
            {
                case ObstacleTypes.RisingPlatform:
                    return "RisingPlatform";
                case ObstacleTypes.FallingPlatform:
                    return "FallingPlatform";
                case ObstacleTypes.BossDoorVert:
                    return "BossDoorVert";
                case ObstacleTypes.BossDoorHoriz:
                    return "BossDoorHoriz";
            }

            return "";
        }
        
    }
}
