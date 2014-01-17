using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using HaseUndIgel.BL;
using HaseUndIgel.Util;

namespace HaseUndIgel.AI
{
    /// <summary>
    /// корневой узел дерева
    /// </summary>
    public class SolutionNode
    {
        /// <summary>
        /// фишка, который был совершен ход
        /// </summary>
        public short token;

        /// <summary>
        /// ячейка, на которую походили
        /// </summary>
        public short targetCell;

        /// <summary>
        /// результирующая оценка - очки
        /// </summary>
        public int score;

        /// <summary>
        /// если стоим на морковке - морковь была отдана в банк
        /// </summary>
        public bool gaveCarrot;

        /// <summary>
        /// узлы-потомки - для потомков хранится только счет (оценка)
        /// </summary>
        public List<SolutionNode> children;

        /// <summary>
        /// обойти дерево, найдя его первую ветвь по определенным критериям
        /// </summary>
        public static SolutionNode ResolveTree(SolutionNode root)
        {
            if (root.children == null || root.children.Count == 0) 
                return null;
            if (root.children.Count == 1) return root.children[0];

            //SimplifyChild(root);
            var rootScore = root.children.Select(c => new Cortege2<SolutionNode, int>(c, GetBranchScore(c, true))).ToList();
            
            // выбрать наибольший из меньших результатов
            var ind = rootScore.IndexOfMin(s => -s.b);
            root.CheckTreeFuckedUp(ind);
            //root.DrawTreeInFile(ind);
            
            var nodeBest = rootScore[ind].a;
            return nodeBest;
        }

        private static int GetBranchScore(SolutionNode branchRoot, bool chooseMin)
        {
            if (branchRoot.children == null || branchRoot.children.Count == 0)
                return branchRoot.score;            

            var extrScore = chooseMin ? int.MaxValue : int.MinValue;
            // ReSharper disable LoopCanBeConvertedToQuery
            var ci = 0;
            foreach (var child in branchRoot.children)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var score = GetBranchScore(child, !chooseMin);
                if ((chooseMin && (score < extrScore)) ||
                    (!chooseMin && (score > extrScore)))
                {
                    extrScore = score;
                    branchRoot.bestChild = ci;
                }
                ci++;
            }
            branchRoot.score = extrScore;
            return extrScore;
        }

        public override string ToString()
        {
            return "t[" + token + "]->" + targetCell + (gaveCarrot ? "(-m)" : "") + "/" + score + "/";
        }

        #region Debug

        public int bestChild/*, level*/;
        public Point coords;
        private static int[] levelCoords;

        const int nodeSz = 25;
        const int nodePad = 25;
        const int nodeFullSz = nodeSz + nodePad;

        private void CheckTreeFuckedUp(int bestChildIndex)
        {
            var nodeBest = children[bestChildIndex];
            var nodeList = new List<SolutionNode> { nodeBest };
            //var nodeBestUntouch = nodeBest;
            while (nodeBest.children.Count > 0)
            {
                nodeBest = nodeBest.children[nodeBest.bestChild];
                nodeList.Add(nodeBest);
            }
            //nodeBest = nodeBestUntouch;
            var routeStr = string.Join("; ", nodeList.Select(n => n.ToString()));

            // одна из возможных ошибок - комп отдает морковку, когда в том нет нужды
            if (nodeList.Any(n => n.gaveCarrot))
                throw new Exception("Stupid turn (give carrot): " + routeStr);
            
            // другая возможная ошибка - ходы повторяются
            for (var i = 0; i < nodeList.Count; i++)
            {
                var nodeSrc = nodeList[i];
                for (var j = i + 2; j < nodeList.Count; j++)
                {
                    var nodeDest = nodeList[j];
                    if (nodeSrc.token == nodeDest.token &&
                        nodeSrc.targetCell == nodeDest.targetCell)
                    {
                        var isOnCarrotOrOnCabbage = Board.debugCells[nodeSrc.targetCell].CellType == CellType.Carrot
                                                    || Board.debugCells[nodeSrc.targetCell].CellType == CellType.Cabbage;
                        if (!isOnCarrotOrOnCabbage)
                            throw new Exception("Doubled turn (give carrot): " + routeStr);
                    }
                }
            }

            // третья ошибка - игрок ходит два раза подряд
            for (var i = 0; i < nodeList.Count - 1; i++)
            {
                var nodeSrc = nodeList[i];
                var nodeDest = nodeList[i + 1];
                if (nodeDest.token / 2 == nodeSrc.token / 2)
                    throw new Exception("Spieler turns twice: " + routeStr);
            }
        }

        public void DrawTreeInFile(int bestIndex)
        {
            // упорядочить узлы
            levelCoords = new int[4];

            foreach (var t in children)
                ArrangeNodes(t, 0);

            var w = levelCoords.Max() * nodeFullSz;
            var h = 4 * nodeFullSz;

            using (var bmp = new Bitmap(w, h))
            using (var gr = Graphics.FromImage(bmp))
            using (var brVoid = new SolidBrush(Color.Beige))
            using (var f = new Font(FontFamily.GenericSansSerif, 9))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.FillRectangle(brVoid, 0, 0, w, h);

                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    DrawNode(child, null, gr, f, brVoid, i == bestIndex);
                }

                var path = ExecutablePath.ExecPath + "\\tree.png";
                bmp.Save(path, ImageFormat.Png);
            }
        }

        public void ArrangeNodes(SolutionNode node, int level)
        {
            var col = levelCoords[level]++;
            col = col * (1 << (3 - level));

            node.coords = new Point(col, level);                        

            foreach (var child in node.children)
            {
                ArrangeNodes(child, level + 1);
            }
        }

        public void DrawNode(SolutionNode node, SolutionNode parent, Graphics gr, Font f, Brush brushVoid, 
            bool isBestChild = false)
        {
            using (var brushBlack = new SolidBrush(Color.Black))
            using (var pen = new Pen(isBestChild ? Color.Red : Color.Black, 2))
            {
                if (parent != null)
                    gr.DrawLine(pen,
                        node.coords.X * nodeFullSz + nodeSz / 2, node.coords.Y * nodeFullSz,
                        parent.coords.X * nodeFullSz + nodeSz / 2, node.coords.Y * nodeFullSz - nodeSz);

                gr.FillEllipse(brushVoid, node.coords.X * nodeFullSz, node.coords.Y * nodeFullSz,
                    nodeSz, nodeSz);

                gr.DrawString(node.score.ToString(), f, brushBlack,
                    node.coords.X * nodeFullSz + nodeSz / 2,
                    node.coords.Y * nodeFullSz + nodeSz / 2, new StringFormat
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                gr.DrawEllipse(pen, node.coords.X * nodeFullSz, node.coords.Y * nodeFullSz,
                    nodeSz, nodeSz);                
            }

            for (var i = 0; i < node.children.Count; i++)
            {
                DrawNode(node.children[i], node, gr, f, brushVoid, i == node.bestChild);
            }
        }

        private static void SimplifyChild(SolutionNode node)
        {
            if (node.children.Count > 2)
                node.children.RemoveRange(2, node.children.Count - 2);
            foreach (var child in node.children)
                SimplifyChild(child);
        }

        #endregion
    }    
}
