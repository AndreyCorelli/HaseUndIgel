using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using HaseUndIgel.AI;
using HaseUndIgel.BL;

namespace HaseUndIgel
{
    public partial class StatisticsForm : Form
    {
        public StatisticsForm()
        {
            InitializeComponent();
        }

        public StatisticsForm(Board board) : this()
        {
            SetupGrid();

            var statList = board.spielers.Select((s, i) => 
                new SpielerStatistics(board, s)
                    {
                        ColorIndex = i,
                        Turn = (i < board.currentSpielerIndex ? i - board.currentSpielerIndex + board.spielers.Length 
                            : i - board.currentSpielerIndex) + 1
                    }).ToList();

            MakeColorMarks(board);

            grid.DataBind(statList);
        }

        private void SetupGrid()
        {
            grid.CellHeight = 26;

            grid.Columns.Add(new FastColumn("Turn", "№") { ColumnWidth = 30 });

            grid.Columns.Add(new FastColumn("Name", "Name")
                { ColumnMinWidth = 65 });

            grid.Columns.Add(new FastColumn("IsComputer", "AI") 
                { ColumnWidth = 32, ImageList = imageListRobot });

            grid.Columns.Add(new FastColumn("ColorIndex", "*") { ColumnWidth = 32, ImageList = imageListColor });

            grid.Columns.Add(new FastColumn("CarrotsSpare", "Carrots")
                { ColumnWidth = 50 });

            grid.Columns.Add(new FastColumn("CabbageSpare", "Cabbage") { ColumnWidth = 56 });

            grid.Columns.Add(new FastColumn("Score", "Score") { ColumnWidth = 60 });
            grid.CalcSetTableMinWidth();
        }

        private void MakeColorMarks(Board board)
        {
            // заполнить список цветов
            for (var i = 0; i < board.spielers.Length; i++)
            {
                var spieler = board.spielers[i];
                var img = new Bitmap(imageListColor.ImageSize.Width,
                                        imageListColor.ImageSize.Height);

                if (board.tokens.Length == board.spielers.Length)
                {
                    var token = board.GetSpielerTokens(i)[0];
                    using (var br = new SolidBrush(Token.TokenColor[token.ColorIndex]))
                    using (var g = Graphics.FromImage(img))
                    {
                        g.FillEllipse(br, 1, 1, imageListColor.ImageSize.Height - 2,
                            imageListColor.ImageSize.Height - 2);
                    }
                    imageListColor.Images.Add(img);
                    continue;
                }

                // разделенная картинка - два цвета
                var tokens = board.GetSpielerTokens(i);
                using (var brA = new SolidBrush(Token.TokenColor[tokens[0].ColorIndex]))
                using (var brB = new SolidBrush(Token.TokenColor[tokens[1].ColorIndex]))
                using (var g = Graphics.FromImage(img))
                {
                    g.FillRectangle(brA, 1, 1, imageListColor.ImageSize.Width - 2,
                        imageListColor.ImageSize.Height - 2);
                    g.FillPolygon(brB, new[]
                    {
                        new Point(imageListColor.ImageSize.Width - 1, 1), 
                        new Point(imageListColor.ImageSize.Width - 1, imageListColor.ImageSize.Height - 1), 
                        new Point(1, imageListColor.ImageSize.Height - 1), 
                    });
                }
                imageListColor.Images.Add(img);
            }
        }
    }

    class SpielerStatistics
    {
        public string Name { get; set; }

        public bool IsComputer { get; set; }

        public int Turn { get; set; }

        public int ColorIndex { get; set; }

        public int Score { get; set; }

        public bool Freezed { get; set; }

        public int CarrotsSpare { get; set; }

        public int CabbageSpare { get; set; }

        public SpielerStatistics() {}

        public SpielerStatistics(Board board, Spieler spieler)
        {
            Name = spieler.Name;
            IsComputer = spieler.IsComputer;
            Freezed = spieler.Freezed;
            CarrotsSpare = spieler.CarrotsSpare;
            CabbageSpare = spieler.CabbageSpare;
            Score = ComputerMind.GetSpielerScore(board, spieler);
        }
    }
}
