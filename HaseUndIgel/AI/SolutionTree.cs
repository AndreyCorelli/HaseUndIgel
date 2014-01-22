using System.Collections.Generic;
using System.Linq;
using System.Text;
using HaseUndIgel.BL;
using HaseUndIgel.Util;

namespace HaseUndIgel.AI
{
    /// <summary>
    /// корневой узел дерева
    /// </summary>
    public class SolutionNode
    {
        public static int nodesCount = 0;

        /// <summary>
        /// фишка, который был совершен ход
        /// </summary>
        public short token;

        /// <summary>
        /// ячейка, на которую походили
        /// </summary>
        public short targetCell;

        /// <summary>
        /// если стоим на морковке - морковь была отдана в банк
        /// </summary>
        public bool gaveCarrot;

        /// <summary>
        /// узлы-потомки - для потомков хранится только счет (оценка)
        /// </summary>
        public List<SolutionNode> children;

        /// <summary>
        /// ссылка на доску нужна для листьев, для оценки позиции
        /// </summary>
        public Board Board { get; set; }

        public SolutionNode()
        {
            nodesCount++;
        }

        /// <summary>
        /// обойти дерево, найдя его первую ветвь по определенным критериям
        /// </summary>
        public static SolutionNode ResolveTree(SolutionNode root, Spieler pov, Board board)
        {
            if (root.children == null || root.children.Count == 0) 
                return null;
            if (root.children.Count == 1) return root.children[0];

            var rootScore = root.children.Select(c => new Cortege2<SolutionNode, int>(c, GetBranchScore(c, true, pov))).ToList();
            // скорректировать счет оценкой ситуации !после первого же хода!
            if (rootScore.Count > 1)
                for (var i = 0; i < rootScore.Count; i++)
                {
                    var deltaScore = ComputerMind.GetScoreForProbableTurn(board, rootScore[i].a);
                    rootScore[i] = new Cortege2<SolutionNode, int>(rootScore[i].a, rootScore[i].b + deltaScore);
                }
            
            // выбрать наибольший из меньших результатов
            var ind = rootScore.IndexOfMin(s => -s.b);
            root.BestIndex = ind;
            ShowChainAndLastBoard(root);
            
            var nodeBest = rootScore[ind].a;
            return nodeBest;
        }

        private static int GetBranchScore(SolutionNode branchRoot, bool chooseMin, Spieler pov)
        {
            if (branchRoot.children == null || branchRoot.children.Count == 0)
            {
                // посчитать счет для узла
                return ComputerMind.GetScore(branchRoot.Board, pov);
            }            

            var extrScore = chooseMin ? int.MaxValue : int.MinValue;
            var ci = 0;
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var child in branchRoot.children)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var score = GetBranchScore(child, !chooseMin, pov);
                if ((chooseMin && (score < extrScore)) ||
                    (!chooseMin && (score > extrScore)))
                {
                    extrScore = score;
                    branchRoot.BestIndex = ci;
                }
                ci++;
            }
            
            return extrScore;
        }

        public override string ToString()
        {
            return "t[" + token + "]->" + targetCell + (gaveCarrot ? "(-m)" : ""); // +"/" + score + "/";
        }

        #region Debug

        public int BestIndex;

        public static void ShowChainAndLastBoard(SolutionNode root)
        {
            var turnsString = new StringBuilder();
            while (true)
            {
                turnsString.AppendFormat("t[{0}]->{1}{2}. ", root.token, root.targetCell, root.gaveCarrot ? ", c-" : "");
                if (root.Board != null)
                {
                    Logger.Info(root.Board.ToString());
                    Logger.Info("Score: " + ComputerMind.GetScore(root.Board, root.Board.CurrentSpieler));
                }

                if (root.children.Count > 0)
                    root = root.children[root.BestIndex];
                else break;
            }
            Logger.Info(turnsString.ToString());
        }

        #endregion
    }    
}
