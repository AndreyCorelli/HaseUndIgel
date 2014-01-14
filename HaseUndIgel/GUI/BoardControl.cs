using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using HaseUndIgel.BL;
using HaseUndIgel.Util;

namespace HaseUndIgel.GUI
{
    public class BoardControl : Control
    {
        #region Constants
        private const int StatusStripHeight = 60;
        private const int CellMargin = 20;
        private const int TokenSelectorSz = 22;
        private const int TokenSelectorSpan = 12;

        private static readonly Dictionary<CellType, int> imageByCellType
            = new Dictionary<CellType, int>
                {
                    {CellType.Carrot, 0},
                    {CellType.Hedgehog, 1},
                    {CellType.Hare, 2},
                    {CellType.Cabbage, 3},
                };
        #endregion

        #region Drawing variables
        private Bitmap[] pictures;
        private int cellShapeSz, cellSpanX, cellSpanY;
        private Font cellFont;
        private Point[] tokenSelectors;

        #endregion

        #region Spiegel variables

        private int selectedCellIndex = -1;

        private int carrotsNeeded;

        private string lastError;

        #endregion

        public Board Board { get; private set; }

        public BoardControl()
        {
            cellFont = new Font(Font, FontStyle.Bold);
            LoadPictures();
            DoubleBuffered = true;
        }

        public void Initialize(int spielersTotal, int spielersComp)
        {
            Board = new Board(spielersTotal, spielersComp);
            selectedCellIndex = -1;
            ArrangeCells();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if (Board == null || tokenSelectors == null) return;
            if (Board.Endspiel) return;
            if (tokenSelectors.Length > 1 && Board.selectedTokenIndex < 0)
                return; // фишка не выбрана

            // выбрать ячейку
            var cellIndex = Board.cells.FindIndex(s =>
                Math.Sqrt((s.Location.X - e.X) * (s.Location.X - e.X) + (s.Location.Y - e.Y) * (s.Location.Y - e.Y))
                    <= cellShapeSz);
            if (cellIndex < 0)
            {
                if (selectedCellIndex >= 0)
                {
                    selectedCellIndex = -1;
                    Invalidate();
                }
                return;
            }
            if (cellIndex == selectedCellIndex) return;

            // проверить доступность хода
            var cellPos = cellIndex;
            var isOk = Board.CheckTurn(Board.currentSpielerIndex, Board.selectedTokenIndex,
                                       cellPos, false, out lastError, out carrotsNeeded);
            if (!isOk)
            {
                
            }
            else
            {
                // подтвердить выбор
                selectedCellIndex = cellIndex;
            }
            
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (Board == null) return;
            if (e.Button != MouseButtons.Left) return;

            // сделать ход?
            if (Board.Endspiel) return;
            if (selectedCellIndex > 0 && Board.selectedTokenIndex >= 0)
            {
                var gaveCarrot = false;
                // запросить действие: получить / отдать 10 морковок?
                //var targetCell = Board.cells[selectedCellIndex];
                //GiveOrTakeCarrotForm
                var rst = Board.MakeTurn(Board.selectedTokenIndex,
                               selectedCellIndex, gaveCarrot, false);
                selectedCellIndex = -1;
                
                Invalidate();
                MessageBox.Show(rst);
                return;
            }

            // выбор маркера?
            if (tokenSelectors.Length > 1)
            {
                var selectorIndex = tokenSelectors.FindIndex(s =>
                                                        Math.Sqrt((s.X - e.X)*(s.X - e.X) + (s.Y - e.Y)*(s.Y - e.Y)) <=
                                                        TokenSelectorSz);
                if (selectorIndex >= 0)
                {
                    var tokens = Board.GetSpielerTokens();
                    var selToken = tokens[selectorIndex];
                    Board.selectedTokenIndex = Board.tokens.IndexOf(selToken);
                    Invalidate();
                    return;
                }
            }

            // выбор маркера щелчком по клетке?
            var cellIndex = Board.cells.FindIndex(s =>
                Math.Sqrt((s.Location.X - e.X) * (s.Location.X - e.X) + (s.Location.Y - e.Y) * (s.Location.Y - e.Y))
                    <= cellShapeSz);
            if (cellIndex >= 0)
            {
                var spielerToken = Board.GetSpielerTokens().FirstOrDefault(t => t.Position == cellIndex);
                if (spielerToken != null)
                {
                    Board.selectedTokenIndex = Board.tokens.IndexOf(spielerToken);
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            if (Board == null) return;

            // нарисовать поле
            ArrangeCells();
            
            // сглаживание
            var oldMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // ячейки
            using (var brushes = new BrushStorage())
            using (var pens = new PenStorage())
            {
                for (var i = 0; i < Board.cells.Count(); i++)
                    DrawCell(i, e.Graphics, brushes, pens);    

                // стата
                DrawSpielerStat(e.Graphics, brushes, pens);
            }
            
            e.Graphics.SmoothingMode = oldMode;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.ButtonFace, 0, 0, Width, Height);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeCells();
            Invalidate();
        }

        private void ArrangeCells()
        {
            if (Board == null) return;
            // упорядочить ячейки
            var height = Height - StatusStripHeight;
            var width = Width;
            var widthField = width - CellMargin - CellMargin;
            var heightField = height - CellMargin - CellMargin;

            var m = (int) Math.Round((Board.cells.Length / 2 + 3)/(1 + width/(float) height));
            var n = Board.cells.Length / 2 - m + 3;

            if (n < 2 || m < 2) return;

            var cellW = widthField / n;
            var cellH = heightField / m;
            var cellFullSz = Math.Min(cellW, cellH);
            cellShapeSz = (int) Math.Floor(cellFullSz*0.75);

            if (cellShapeSz < 7) return;

            cellSpanX = (int)Math.Round((widthField - cellShapeSz * n) / (n - 1.0));
            cellSpanY = (int)Math.Round((heightField - cellShapeSz * m) / (m - 1.0));

            var cellSizeX = cellShapeSz + cellSpanX;
            var cellSizeY = cellShapeSz + cellSpanY;

            var cellIndex = 0;
            for (; cellIndex < m; cellIndex++)
            {
                var x = n - 1;
                var y = cellIndex;
                Board.cells[cellIndex].Location =
                    new Point(CellMargin + x * cellSizeX + cellSizeX / 2, y * cellSizeY + cellSizeY / 2);
            }
            
            for (var i = 1; i < n; i++)
            {
                var x = n - 1 - i;
                var y = m - 1;
                Board.cells[cellIndex++].Location =
                    new Point(CellMargin + x * cellSizeX + cellSizeX / 2, y * cellSizeY + cellSizeY / 2);
            }

            for (var i = 1; i < m; i++)
            {
                var x = 0;
                var y = m - 1 - i;
                Board.cells[cellIndex++].Location =
                    new Point(CellMargin + x * cellSizeX + cellSizeX / 2, y * cellSizeY + cellSizeY / 2);
            }

            var xStart = 1;
            for (; cellIndex < Board.cells.Length; cellIndex++)
            {
                var x = xStart++;
                var y = 0;
                Board.cells[cellIndex].Location =
                    new Point(CellMargin + x * cellSizeX + cellSizeX / 2, y * cellSizeY + cellSizeY / 2);
            }
        }
    
        private void DrawCell(int cellIndex, Graphics g, BrushStorage brushes, PenStorage pens)
        {
            var cell = Board.cells[cellIndex];
            
            var ellipseRect = new Rectangle(cell.Location.X - cellShapeSz/2, cell.Location.Y - cellShapeSz/2,
                                            cellShapeSz, cellShapeSz);
            // залить фон
            g.FillEllipse(brushes.GetBrush(Color.White), ellipseRect);

            // граница
            g.DrawEllipse(pens.GetPen(Color.Black, cellShapeSz > 22 ? 3 : 2,
                cellIndex == selectedCellIndex ? DashStyle.Dot : DashStyle.Solid), ellipseRect);

            // содержимое
            int imageIndex;
            if (imageByCellType.TryGetValue(cell.CellType, out imageIndex))
            {
                var bmp = pictures[imageIndex];
                g.DrawImage(bmp, ellipseRect);
            }
            else
            {
                var textColor = cell.CellType == CellType.Cabbage
                                ? Color.DarkGreen
                                : cell.CellType == CellType.Carrot ? Color.Brown : Color.Black;
                var text = cell.CellType == CellType.Cabbage
                               ? "CB" : cell.CellType == CellType.Carrot ? "Cr" : cell.CellType == CellType.Finish
                                           ? "fin" : cell.CellType == CellType.Hare ? "H" : cell.CellType == CellType.Hedgehog
                                                       ? "Ig" : cell.CellType == CellType.Number ? cell.Points.ToString() : "St";
                g.DrawString(text, cellFont, brushes.GetBrush(textColor), cell.Location.X, cell.Location.Y,
                    new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            }

            // фишка
            var tokenOnCell = Board.tokens.FirstOrDefault(t => t.Position == cellIndex);
            if (tokenOnCell != null)
                g.FillEllipse(brushes.GetBrush(Color.FromArgb(128, Token.TokenColor[tokenOnCell.ColorIndex])), ellipseRect);
        }

        private void DrawSpielerStat(Graphics g, BrushStorage brushes, PenStorage pens)
        {
            var top = Height - StatusStripHeight + 3;
            var left = 20;

            var spieler = Board.spielers[Board.currentSpielerIndex];
            var text = "Spieler: " + spieler.Name + ", carrots: " + spieler.CarrotsSpare + ", cabbages: " +
                       spieler.CabbageSpare;
            g.DrawString(text, cellFont, brushes.GetBrush(Color.Black), left, top);

            var textSize = g.MeasureString(text, cellFont);
            top += (int)textSize.Height + 3;

            // выбор маркера
            var tokens = Board.GetSpielerTokens();
            tokenSelectors = tokens.Select((t, i) =>
                new Point(left + i * (TokenSelectorSpan + TokenSelectorSz) + TokenSelectorSpan / 2, top + TokenSelectorSz / 2)).ToArray();

            for (var i = 0; i < tokens.Length; i++)
            {
                var isSelected = Board.selectedTokenIndex >= 0 && 
                    Board.tokens[Board.selectedTokenIndex] == tokens[i];
                var color = Token.TokenColor[tokens[i].ColorIndex];
                var coords = tokenSelectors[i];

                var rect = new Rectangle(coords.X - TokenSelectorSz/2, coords.Y - TokenSelectorSz/2,
                                         TokenSelectorSz, TokenSelectorSz);
                g.FillEllipse(brushes.GetBrush(color), rect);
                if (isSelected)
                    g.DrawEllipse(pens.GetPen(Color.Black, 3), rect);
            }

            // нужно моркови / послед. ошибка
            var notes = new List<string>();
            if (carrotsNeeded != 0)
                notes.Add(carrotsNeeded > 0 ? carrotsNeeded.ToString() 
                    : "+" + (-carrotsNeeded).ToString());
            if (!string.IsNullOrEmpty(lastError))
                notes.Add(lastError);

            if (notes.Count > 0)
            {
                top += TokenSelectorSz + 3;
                left = 20;
                g.DrawString(string.Join(", ", notes), Font, brushes.GetBrush(Color.Black), left, top);
            }
        }
    
        private void LoadPictures()
        {
            try
            {
                pictures = new[]
                {
                    (Bitmap) Image.FromFile(ExecutablePath.ExecPath + "\\img\\pic_carrot.png"),
                    (Bitmap) Image.FromFile(ExecutablePath.ExecPath + "\\img\\pic_hedgehog.png"),
                    (Bitmap) Image.FromFile(ExecutablePath.ExecPath + "\\img\\pic_bunny.png"),
                    (Bitmap) Image.FromFile(ExecutablePath.ExecPath + "\\img\\pic_cabbage.png")
                };
            }
            catch // design mode!
            {
            }
        }
    }
}
