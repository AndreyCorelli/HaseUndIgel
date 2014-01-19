using System.Drawing;

namespace HaseUndIgel.BL
{
    public class Cell
    {
        public CellType CellType { get; set; }

        public int Index { get; set; }

        public int Points { get; set; }

        public Point Location { get; set; }

        public Point NumberLocation { get; set; }

        public Cell() {}

        public Cell(CellType cellType)
        {
            CellType = cellType;
        }

        public Cell(CellType cellType, int points)
        {
            CellType = cellType;
            Points = points;
        }
    }
}
