using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.Mathematics;

namespace RobotMaster
{
    public class Exit
    {
        public int Screen;
        public Direction Direction;
        public int Destination;

        public Exit(int screen, Direction direction, int destination)
        {
            this.Screen = screen;
            this.Direction = direction;
            this.Destination = destination;
        }
    }
}
