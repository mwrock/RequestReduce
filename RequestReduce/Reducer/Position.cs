using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestReduce.Reducer
{
    public struct Position
    {
        public PositionMode PositionMode { get; set; }
        public int Offset { get; set; }
        public Direction Direction { get; set; }
    }
}
