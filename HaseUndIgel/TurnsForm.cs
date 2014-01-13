using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using FastGrid;
using HaseUndIgel.BL;

namespace HaseUndIgel
{
    public partial class TurnsForm : Form
    {
        public TurnsForm()
        {
            InitializeComponent();
        }

        public TurnsForm(Board board) : this()
        {
            SetupGrid();
            MakeColorMarks(board);
            var turns = board.turns.Select((t, i) => new SpielerTurnRow(i, t, board)).ToList();
            grid.DataBind(turns);
        }

        private void SetupGrid()
        {
            grid.CellHeight = 26;

            grid.Columns.Add(new FastColumn("TurnIndex", "№") { ColumnWidth = 40 });

            grid.Columns.Add(new FastColumn("Name", "Name") { ColumnMinWidth = 65 });

            grid.Columns.Add(new FastColumn("IsComputer", "AI") { ColumnWidth = 32, ImageList = imageListRobot });

            grid.Columns.Add(new FastColumn("TokenIndex", "*") { ColumnWidth = 32, ImageList = imageListColor });

            grid.Columns.Add(new FastColumn("TurnCode", "Turn") { ColumnWidth = 62 });

            grid.Columns.Add(new FastColumn("Carrots", "Carrots") { ColumnWidth = 72 });

            grid.Columns.Add(new FastColumn("NewCabbages", "Cabbage") { ColumnWidth = 56 });
            
            grid.CalcSetTableMinWidth();
        }

        private void MakeColorMarks(Board board)
        {
            // заполнить список цветов
            foreach (var token in board.tokens)
            {
                var img = new Bitmap(imageListColor.ImageSize.Width,
                                        imageListColor.ImageSize.Height);
                using (var br = new SolidBrush(Token.TokenColor[token.ColorIndex]))
                using (var g = Graphics.FromImage(img))
                {
                    g.FillEllipse(br, 1, 1, imageListColor.ImageSize.Height - 2,
                        imageListColor.ImageSize.Height - 2);
                }
                imageListColor.Images.Add(img);
            }
        }
    }

    class SpielerTurnRow
    {
        public int TurnIndex { get; set; }

        public string Name { get; set; }

        public bool IsComputer { get; set; }

        public int TokenIndex { get; set; }

        public string TurnCode { get; set; }

        public string Carrots { get; set; }

        public int NewCabbages { get; set; }

        public SpielerTurnRow(int i, SpielerTurn turn, Board board)
        {
            TurnIndex = i;
            var spieler = board.spielers[turn.SpielerIndex];
            Name = spieler.Name;
            IsComputer = spieler.IsComputer;
            TokenIndex = turn.TokenIndex;
            TurnCode = turn.OldCell + " -> " + turn.TargetCell;
            Carrots = turn.DeltaCarrots == 0
                          ? turn.NewCarrots.ToString()
                          : turn.NewCarrots.ToString() + (turn.DeltaCarrots < 0 ? "(" : "(+") +
                            turn.DeltaCarrots.ToString() + ")";
            NewCabbages = turn.NewCabbages;
        }
    }
}
