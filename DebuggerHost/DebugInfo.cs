using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebuggerHost
{
    public class DebugInfo
    {
        public float megaX;
        public float megaY;
        public float megaRight;
        public float megaDown;
        public float velocityX;
        public float velocityY;
        public float cameraX;
        public float cameraY;

        public int RoomNum;
        public string ScreenStack;

        public bool IsJumping;
        public bool IsAbleToJump;
        public bool IsFalling;
        public bool IsClimbing;
        public bool IsAbleToClimb;
        public TimeSpan TotalGameTime;
        public TimeSpan ElapsedGameTime;
        public bool IsRunningSlowly;

    }
}
