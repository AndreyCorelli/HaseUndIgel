using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HaseUndIgel.AI;
using HaseUndIgel.BL;
using NUnit.Framework;

namespace HaseUndIgel.Test.AI
{
    [TestFixture]
    public class NuSolutionTree
    {
        private readonly List<Board> leafBoards = new List<Board>();
        private Board boardInit;
        
        [TestFixtureSetUp]
        public void Setup()
        {
            boardInit = new Board(2, 0);
        }

        private void MakeNode(SolutionNode root, int level)
        {
            for (var i = 0; i < 2; i++)
            {
                var child = new SolutionNode
                    {
                        token = (short) level, 
                        targetCell = (short) i,
                        children = new List<SolutionNode>()
                    };
                root.children.Add(child);

                if (level < 3)
                    MakeNode(child, level + 1);
                else                
                    child.Board = AddBoard(0);
            }
        }

        private Board AddBoard(int targetScoreDelta)
        {
            var board = boardInit.MakeShallowCopy();
            board.spielers[0].CarrotsSpare += targetScoreDelta;
            leafBoards.Add(board);
            return board;
        }

        [Test]
        public void TestTreePath()
        {
            var root = new SolutionNode { children = new List<SolutionNode>() };
            MakeNode(root, 0);
            var bestNode = SolutionNode.ResolveTree(root, boardInit.spielers[boardInit.currentSpielerIndex], boardInit);
            Assert.IsNotNull(bestNode);
        }
    }
}
