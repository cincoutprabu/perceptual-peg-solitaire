//Board.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-16
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerceptualPegSolitaire.Entities
{
    public class Board : List<List<int>>
    {
        #region Constants

        public static int Rows = 7;
        public static int Columns = 7;

        public static Dictionary<char, int> Chars = new Dictionary<char, int>()
        {
            {' ', -1},
            {'1', 1},
            {'0', 0}
        };

        #endregion

        #region Methods

        public Board CreateCopy()
        {
            Board newBoard = new Board();

            foreach (var row in this)
            {
                List<int> newRow = new List<int>();
                foreach (var elem in row)
                {
                    newRow.Add(elem);
                }
                newBoard.Add(newRow);
            }

            return newBoard;
        }

        #endregion

        #region Helper-Methods

        public static Board BuildBoard(string[] level)
        {
            Board board = new Board();

            foreach (string row in level)
            {
                var boardRow = new List<int>();
                foreach (char ch in row) boardRow.Add(Chars[ch]);
                board.Add(boardRow);
            }

            return board;
        }

        #endregion
    }
}
