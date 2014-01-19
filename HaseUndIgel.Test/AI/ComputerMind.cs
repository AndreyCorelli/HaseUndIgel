using System;
using System.Collections.Generic;
using HaseUndIgel.AI;
using HaseUndIgel.BL;
using NUnit.Framework;

namespace HaseUndIgel.Test.AI
{
    [TestFixture]
    public class NuComputerMind
    {
        private static readonly List<Board> boards = new List<Board>();

        private static readonly List<int> expectedScores = new List<int>();

        [TestFixtureSetUp]
        public void Setup()
        {
            #region [0] Условия примерно равные, игрок 1 обгоняет, капусты одинаково, первый лидирует
            var board = new Board(2, 1);
            board.tokens[0].Position = 16;
            board.tokens[1].Position = 24;
            board.spielers[0].CarrotsSpare = 13;
            board.spielers[0].CabbageSpare = 1;

            board.tokens[2].Position = 8;
            board.tokens[3].Position = 10;
            board.spielers[1].CarrotsSpare = 61;
            board.spielers[1].CabbageSpare = 1;
            board.currentSpielerIndex = 1;

            boards.Add(board);
            expectedScores.Add(-1);
            #endregion

            #region [1] Моркови одинаково, прогресс больше у первого, капусты меньше у второго, лидирует второй
            board = new Board(2, 1);
            board.tokens[0].Position = 16;
            board.tokens[1].Position = 24;
            board.spielers[0].CarrotsSpare = 31;
            board.spielers[0].CabbageSpare = 2;

            board.tokens[2].Position = 8;
            board.tokens[3].Position = 10;
            board.spielers[1].CarrotsSpare = 21;
            board.spielers[1].CabbageSpare = 1;
            board.currentSpielerIndex = 0;

            boards.Add(board);
            expectedScores.Add(-1);
            #endregion

            #region [2] Первый продвинулся дальше, но у него мало очков хода, лидирует второй
            board = new Board(2, 1);
            board.tokens[0].Position = 5;
            board.tokens[1].Position = 24;
            board.spielers[0].CarrotsSpare = 4;
            board.spielers[0].CabbageSpare = 0;

            board.tokens[2].Position = 8;
            board.tokens[3].Position = 10;
            board.spielers[1].CarrotsSpare = 56;
            board.spielers[1].CabbageSpare = 0;
            board.currentSpielerIndex = 0;

            boards.Add(board);
            expectedScores.Add(-1);
            #endregion

            #region [3] Первый не проследил за капустой, лидирует второй
            board = new Board(2, 1);
            board.tokens[0].Position = 29;
            board.tokens[1].Position = 30;
            board.spielers[0].CarrotsSpare = 31;
            board.spielers[0].CabbageSpare = 2;

            board.tokens[2].Position = 13;
            board.tokens[3].Position = 11;
            board.spielers[1].CarrotsSpare = 21;
            board.spielers[1].CabbageSpare = 1;
            board.currentSpielerIndex = 0;

            boards.Add(board);
            expectedScores.Add(-1);
            #endregion

            #region [4] Первый прошел чуть больше, у второго чуть больше моркови - лидирует первый
            board = new Board(2, 1);
            board.tokens[0].Position = 24;
            board.tokens[1].Position = 10;
            board.spielers[0].CarrotsSpare = 32;
            board.spielers[0].CabbageSpare = 0;

            board.tokens[2].Position = 11;
            board.tokens[3].Position = 16;
            board.spielers[1].CarrotsSpare = 52; // +20 за номер
            board.spielers[1].CabbageSpare = 0;
            board.currentSpielerIndex = 0;

            boards.Add(board);
            expectedScores.Add(1);
            #endregion

            #region [5] POV имеет преимущество, комп тоже так думал, да в суп попал
            board = new Board(2, 1);
            board.currentSpielerIndex = 1;
            board.tokens[0].Position = 14;
            board.tokens[1].Position = 0;
            board.tokens[2].Position = 12;
            board.tokens[3].Position = 0;
            board.spielers[0].CarrotsSpare = 57;
            board.spielers[0].CabbageSpare = 2;
            board.spielers[0].GiveCabbage = false;
            board.spielers[1].CarrotsSpare = 14;
            board.spielers[1].CabbageSpare = 1;
            board.spielers[1].GiveCabbage = false;

            boards.Add(board);
            expectedScores.Add(1);
            #endregion

            #region [6] POV (комп) думает, что попал
            board = new Board(2, 1);
            board.currentSpielerIndex = 1;
            board.tokens[0].Position = 17;
            board.tokens[1].Position = 3;
            board.tokens[2].Position = 6;
            board.tokens[3].Position = 10;
            board.spielers[0].CarrotsSpare = 12;
            board.spielers[0].CabbageSpare = 1;
            board.spielers[0].GiveCabbage = false;
            board.spielers[1].CarrotsSpare = 22;
            board.spielers[1].CabbageSpare = 2;
            board.spielers[1].GiveCabbage = false;

            boards.Add(board);
            expectedScores.Add(-1);
            #endregion
        }

        [Test]
        public void TestScoresAreAsExpected()
        {
            // проверить, что оценки компа по знаку совпадают с оценками
            // человека - "эксперта"
            var errorList = new List<string>();

            for (var i = 0; i < boards.Count; i++)
            {
                var board = boards[i];
                var score = expectedScores[i];
                var aiScore = ComputerMind.GetScore(board, board.CurrentSpieler);

                if (Math.Sign(score) != Math.Sign(aiScore))
                    errorList.Add(string.Format("Доска [{0}]: AI={1}, User={2}",
                        i, aiScore, score));
            }

            Assert.AreEqual(0, errorList.Count, string.Join(Environment.NewLine, errorList));
        }
    }
}
