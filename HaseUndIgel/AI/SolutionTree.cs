using System.Collections.Generic;
using System.Linq;
using System.Text;
using HaseUndIgel.Util;

namespace HaseUndIgel.AI
{
    /// <summary>
    /// корневой узел дерева
    /// </summary>
    public struct SolutionNode
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

        public int bestChild/*, level*/;

        /// <summary>
        /// обойти дерево, найдя его первую ветвь по определенным критериям
        /// </summary>
        public static SolutionNode? ResolveTree(SolutionNode root)
        {
            if (root.children == null || root.children.Count == 0) 
                return null;
            if (root.children.Count == 1) return root.children[0];
            var rootScore = root.children.Select(c => new Cortege2<SolutionNode, int>(c, GetBranchScore(c, true))).ToList();

            // выбрать наибольший из меньших результатов
            var ind = rootScore.IndexOfMin(s => -s.b);

            var rt = rootScore[ind].a;
            var sb = new StringBuilder(rt + "; ");
            while (rt.children.Count > 0)
            {
                rt = rt.children[rt.bestChild];
                sb.Append(rt + "; ");
            }

            var nodeBest = rootScore[ind].a;
            if (nodeBest.gaveCarrot)
            {
                bool fuckup = true;
            }
            return nodeBest;
        }

        private static int GetBranchScore(SolutionNode branchRoot, bool chooseMin)
        {
            if (branchRoot.children == null || branchRoot.children.Count == 0)
            {
                //if (!chooseMin)
                //{
                //    var isWrong = true;
                //}
                return branchRoot.score;
            }

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
            return extrScore;
        }

        public override string ToString()
        {
            return "t[" + token + "]->" + targetCell + (gaveCarrot ? "(-m)" : "") + "/" + score + "/";
        }
    }    
}
