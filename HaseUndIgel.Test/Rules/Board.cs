using System;
using System.Collections.Generic;
using HaseUndIgel.BL;
using NUnit.Framework;

namespace HaseUndIgel.Test.Rules
{
    [TestFixture]
    public class NuBoard
    {
        private static readonly List<Board> boardsSrc = new List<Board>();
        private static readonly List<Board> boardsExpected = new List<Board>();

        [TestFixtureSetUp]
        public void Setup()
        {
            #region Board[0] - 5 steps
            var src = new Board(2, 0);
            var dest = new Board(2, 0);
            boardsSrc.Add(src);
            boardsExpected.Add(dest);
            // step 1
            src.MakeTurn(0, 3, false, false);
            dest.spielers[0].CarrotsSpare -= 6;
            dest.tokens[0].Position = 3;

            // step 2
            src.MakeTurn(2, 4, false, false);
            dest.spielers[1].CarrotsSpare -= 10;
            dest.tokens[2].Position = 4;

            // step 3
            src.MakeTurn(1, 5, false, false);
            dest.spielers[0].CarrotsSpare -= 15;
            dest.tokens[1].Position = 5;

            // step 4
            src.MakeTurn(3, 1, false, false);
            dest.spielers[1].CarrotsSpare -= 1;
            dest.tokens[3].Position = 1;

            // step 5 - take 10 carrots
            src.MakeTurn(0, 3, false, false);
            dest.spielers[0].CarrotsSpare += 10;
            dest.tokens[0].Position = 3;
            #endregion

            #region Board[1] - 8 steps
            src = new Board(2, 0);
            src.tokens[0].Position = 29;
            src.tokens[1].Position = 30;
            src.spielers[0].CarrotsSpare = 31;
            src.spielers[0].CabbageSpare = 2;

            src.tokens[2].Position = 13;
            src.tokens[3].Position = 11;
            src.spielers[1].CarrotsSpare = 24;
            src.spielers[1].CabbageSpare = 1;
            src.currentSpielerIndex = 0;

            dest = src.MakeShallowCopy();
            boardsSrc.Add(src);
            boardsExpected.Add(dest);
            
            // step 1
            src.MakeTurn(1, 28, false, false);
            dest.spielers[0].CarrotsSpare += 20;
            dest.tokens[1].Position = 28;

            // step 2
            src.MakeTurn(2, 14, false, false);
            dest.spielers[1].CarrotsSpare -= 1;
            dest.tokens[2].Position = 14;

            // step 3
            src.MakeTurn(1, 23, false, false);
            dest.spielers[0].CarrotsSpare += 50;
            dest.tokens[1].Position = 23;

            // step 4
            src.MakeTurn(3, 15, false, false);
            dest.spielers[1].CarrotsSpare -= 10;
            dest.tokens[3].Position = 15;

            // step 5
            src.MakeTurn(1, 25, false, false);
            dest.spielers[0].CarrotsSpare -= 3;
            dest.tokens[1].Position = 25;

            // step 6
            src.MakeTurn(2, 19, false, false);
            dest.spielers[1].CarrotsSpare += 25;
            dest.tokens[2].Position = 19;

            // step 7
            src.MakeTurn(1, 25, false, false);
            dest.spielers[0].CarrotsSpare += 20;
            dest.spielers[0].CabbageSpare -= 1;
            dest.tokens[1].Position = 25;

            #endregion
        }

        [Test]
        public void TestMakeTurn()
        {
            var errorList = new List<string>();
            for (var i = 0; i < boardsSrc.Count; i++)
            {
                var src = boardsSrc[i];
                var dest = boardsExpected[i];

                for (var j = 0; j < src.spielers.Length; j++)
                {
                    if (src.spielers[j].CarrotsSpare != dest.spielers[j].CarrotsSpare)
                        errorList.Add(string.Format("board[{0}]: spieler[{1}] carrots is {2}, should be {3}",
                                                    i, j, src.spielers[j].CarrotsSpare, dest.spielers[j].CarrotsSpare));
                    if (src.spielers[j].CabbageSpare != dest.spielers[j].CabbageSpare)
                        errorList.Add(string.Format("board[{0}]: spieler[{1}] !cabbages is {2}, should be {3}",
                                                    i, j, src.spielers[j].CabbageSpare, dest.spielers[j].CabbageSpare));
                }
                for (var j = 0; j < src.tokens.Length; j++)
                {
                    if (src.tokens[j].Position != dest.tokens[j].Position)
                        errorList.Add(string.Format("board[{0}]: token[{1}] pos is {2}, should be {3}",
                            i, j, src.tokens[j].Position, dest.tokens[j].Position));
                }
            }

            Assert.AreEqual(0, errorList.Count,
                string.Join(Environment.NewLine, errorList));
        }
    }
}
