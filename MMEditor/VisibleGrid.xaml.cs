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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MMEditor
{
    /// <summary>
    /// Interaction logic for VisibleGrid.xaml
    /// </summary>
    public partial class VisibleGrid : UserControl
    {
        private int selectedRow = 0;
        private int selectedColumn = 0;

        private int numCellsX = 1;
        private int numCellsY = 1;
        private int cellWidth = 16;
        private int cellHeight = 16;


        public int SelectedColumn
        {
            get { return selectedColumn; }
            set { selectedColumn = value; }
        }
        public int SelectedRow
        {
            get { return selectedRow; }
            set { selectedRow = value; }
        }
        public int NumColumns
        {
            get { return numCellsX; }
        }
        public int NumRows
        {
            get { return numCellsY; }
        }

        public VisibleGrid(int cellWidth, int cellHeight, int numCellsX, int numCellsY)
        {
            InitializeComponent();
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.numCellsX = numCellsX;
            this.numCellsY = numCellsY;

            for (int i = 0; i < numCellsX; i++)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.Width = new GridLength(cellWidth);
                c.SharedSizeGroup = "allCols";
                MainGrid.ColumnDefinitions.Add(c);    
            }
            for (int i = 0; i < numCellsY; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = new GridLength(cellHeight);
                r.SharedSizeGroup = "allRows";
                MainGrid.RowDefinitions.Add(r);    
            }

            MainGrid.Width = cellWidth * numCellsX;
            MainGrid.Height = cellHeight * numCellsY;
            MainGrid.ShowGridLines = true;

            this.Width = MainGrid.Width;
            this.Height = MainGrid.Height;
        }

        public void OnClick()
        {
            Point mouse = MouseUtilities.CorrectGetPosition(this);

            int cellCol = (int)(mouse.X / cellWidth);
            int cellRow = (int)(mouse.Y / cellHeight);

            try
            {
                SelectionRect.Visibility = Visibility.Visible;
                Grid.SetColumn(SelectionRect, cellCol);
                Grid.SetRow(SelectionRect, cellRow);
                selectedColumn = cellCol;
                selectedRow = cellRow;
            }
            catch (Exception e)
            {
                SelectionRect.Visibility = Visibility.Hidden;
            }
        }

    }
}
