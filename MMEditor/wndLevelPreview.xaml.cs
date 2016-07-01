using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using RobotMaster.TileEngine;

namespace MMEditor
{
    /// <summary>
    /// Interaction logic for wndLevelPreview.xaml
    /// </summary>
    public partial class wndLevelPreview : Window
    {

        Image imgTileset;
        Image[,] imgTilesInGrid;
        int[,] map;

        public wndLevelPreview(Image tileset, int tilesetRows, int tilesetCols, List<int[,]> roomMaps, SerializedLevel serialized, int screenTilesX, int screenTilesY)
        {
            InitializeComponent();


            this.imgTileset = tileset;


            // Find the bounds for the entire level
            int maxTilemapHeight = 0;
            int maxTilemapWidth = 0;
            for (int i = 0; i < roomMaps.Count; i++)
            {
                SerializedRoom room = serialized.Rooms[i];
                int roomRight = (room.RoomWidth + room.RoomLocation.X) * screenTilesX;
                int roomBottom = (1 + room.RoomLocation.Y) * screenTilesY;
                if (roomRight > maxTilemapWidth)
                    maxTilemapWidth = roomRight;
                if (roomBottom > maxTilemapHeight)
                    maxTilemapHeight = roomBottom;
            }

            // Prepare tilemap array
            map = new int[maxTilemapWidth, maxTilemapHeight];
            for (int y = 0; y < maxTilemapHeight; y++)
            {
                for (int x = 0; x < maxTilemapWidth; x++)
                {
                    map[x, y] = -1;
                }
            }
            for (int i = 0; i < roomMaps.Count; i++)
            {
                SerializedRoom room = serialized.Rooms[i];

                // Look at the position of each room, and copy over to cooresponding sub-array in master tilemap
                int startTileX = room.RoomLocation.X * screenTilesX;
                int startTileY = room.RoomLocation.Y * screenTilesY;

                for (int y = startTileY; y < startTileY + 1 * screenTilesY; y++)
                {
                    for (int x = startTileX; x < startTileX + room.RoomWidth * screenTilesX; x++)
                    {
                        map[x,y] = roomMaps[i][x - startTileX, y - startTileY];
                    }
                }
            }

            // Construct grid
            grdLevel.Children.Clear();
            grdLevel.ColumnDefinitions.Clear();
            grdLevel.RowDefinitions.Clear();
            for (int x = 0; x < maxTilemapWidth; x++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(serialized.TileWidth * 1);
                grdLevel.ColumnDefinitions.Add(col);
            }
            for (int y = 0; y < maxTilemapHeight; y++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(serialized.TileHeight * 1);
                grdLevel.RowDefinitions.Add(row);
            }
            grdLevel.Width = grdLevel.ColumnDefinitions.Count * serialized.TileWidth * 1;
            grdLevel.Height = grdLevel.RowDefinitions.Count * serialized.TileHeight * 1;

            // Assign images to grid
            imgTilesInGrid = new Image[grdLevel.ColumnDefinitions.Count, grdLevel.RowDefinitions.Count];
            for (int y = 0; y < grdLevel.RowDefinitions.Count; y++)
            {
                for (int x = 0; x < grdLevel.ColumnDefinitions.Count; x++)
                {
                    // Make a new, blank image
                    Image cellTile = new Image();

                    Grid.SetColumn(cellTile, x);
                    Grid.SetRow(cellTile, y);
                    grdLevel.Children.Add(cellTile);
                    imgTilesInGrid[x, y] = cellTile;
                    RenderOptions.SetBitmapScalingMode(cellTile, BitmapScalingMode.NearestNeighbor);

                    // Set to image source from tileset
                    int tileIndex = map[x, y];
                    if (tileIndex > -1)
                    {
                        Microsoft.Xna.Framework.Point tileLoc = EditorLevel.RowColFromIndex(tileIndex, tilesetRows, tilesetCols);
                        Int32Rect cropRegion = new Int32Rect(tileLoc.X * serialized.TileWidth,
                                            tileLoc.Y * serialized.TileHeight,
                                            serialized.TileWidth,
                                            serialized.TileHeight);
                        cellTile.Source = new CroppedBitmap((BitmapSource)imgTileset.Source, cropRegion);
                    }
                    else
                    {
                        cellTile.Source = null;
                    }
                }
            }

            // Set a solid black
            grdLevel.Background = new SolidColorBrush(System.Windows.Media.Colors.Black);
        }
    }
}
