using System.Collections.Generic;
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
        public static SolutionNode ResolveTree(SolutionNode root, Spieler pov)
        {
            if (root.children == null || root.children.Count == 0) 
                return null;
            if (root.children.Count == 1) return root.children[0];

            var rootScore = root.children.Select(c => new Cortege2<SolutionNode, int>(c, GetBranchScore(c, true, pov))).ToList();
            
            // выбрать наибольший из меньших результатов
            var ind = rootScore.IndexOfMin(s => -s.b);
            
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
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var child in branchRoot.children)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var score = GetBranchScore(child, !chooseMin, pov);
                if ((chooseMin && (score < extrScore)) ||
                    (!chooseMin && (score > extrScore)))
                {
                    extrScore = score;
                }
            }
            
            return extrScore;
        }

        public override string ToString()
        {
            return "t[" + token + "]->" + targetCell + (gaveCarrot ? "(-m)" : ""); // +"/" + score + "/";
        }
    }    
}
