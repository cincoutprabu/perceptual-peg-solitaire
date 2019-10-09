//Position.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-14
*/

using System;

namespace PerceptualPegSolitaire.Entities
{
    public enum Direction
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }

    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public Direction Direction { get; set; }

        public Position()
        {
            Row = -1;
            Column = -1;
            Direction = Direction.None;
        }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public Position(int row, int column, Direction direction)
        {
            Row = row;
            Column = column;
            Direction = direction;
        }

        public override string ToString()
        {
            return (Row + "," + Column);
        }
    }

}
