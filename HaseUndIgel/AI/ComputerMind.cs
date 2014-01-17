using System;
using System.Collections.Generic;
using HaseUndIgel.BL;
using HaseUndIgel.Util;
using System.Linq;

namespace HaseUndIgel.AI
{
    public static class ComputerMind
    {
        /// <summary>
        /// макс. индекс! полухода (хода)
        /// т.е. 3 означает 4 полухода
        /// </summary>
        public const int MaxLevel = 5;

        /// <summary>
        /// после этого хода возможные варианты начинают резаться
        /// </summary>
        public const int MaxLevelToTryAllPaths = 3;

        /// <summary>
        /// штраф за нахождение на клетке зайца
        /// </summary>
        public const int HarePenalty = 6;

        /// <summary>
        /// штраф за капустный лист
        /// (умножается после прохождения первой трети игры на 2, затем,
        /// по прохождению второй трети игры - еще на два)
        /// </summary>
        public const int CabbagePenalty = 50;

        /// <summary>
        /// оценочная "стоимость" прохождения клетки
        /// </summary>
        public const int CellPenalty = 4;

        /// <summary>
        /// построить дерево решения и выбрать из него первый ход
        /// </summary>
        public static void MakeTurn(Board board)
        {
            if (board.Endspiel) return;

            SolutionNode root;
            SolutionNode.nodesCount = 0;
            using (new TimeLogger("Turns took"))
            {
                // получить все возможное множество ходов и заполнить
                // дерево решений
                root = new SolutionNode
                    {
                        children = new List<SolutionNode>()
                    };

                MakeAllProbableTurns(board, root, board.CurrentSpieler, 0);
            }

            // "решить" дерево и сделать ход
            SolutionNode turnRoot;
            using (new TimeLogger("Resolving tree took"))
            {
                turnRoot = SolutionNode.ResolveTree(root, board.CurrentSpieler);
                if (turnRoot == null)
                    return; // пат?
            }
            Logger.Info(SolutionNode.nodesCount.ToString() + " nodes tested");

            // таки сделать ход
            board.MakeTurn(turnRoot.token,
                           turnRoot.targetCell, turnRoot.gaveCarrot, false);
        }

        private static void MakeAllProbableTurns(Board board, SolutionNode root, Spieler pov,
            int level)
        {
            if (board.Endspiel) return;
            
            // если игрок пропускает ход...
            var spieler = board.CurrentSpieler;
            if (spieler.Freezed)
            {
                var newRoot = new SolutionNode
                    {
                        targetCell = -1,
                        children = new List<SolutionNode>()
                    };
                root.children.Add(newRoot);
                return;
            }

            // выбрать фишку для хода (актуально для игры вдвоем)
            int[] tokenIndicies;
            if (board.spielers.Length == 2)
            {    
                tokenIndicies = board.currentSpielerIndex == 0
                                    ? new[] {0, 1} : new[] {2, 3};
                // если это первый ход и все фишки на стартовой позиции - сокращаем выбор
                if (board.tokens[tokenIndicies[0]].Position == board.tokens[tokenIndicies[1]].Position)
                    tokenIndicies = new [] {tokenIndicies[0]};
            }
            else
                tokenIndicies = new [] {board.currentSpielerIndex};

            // если игрок стоит на капусте одной из фишек -
            // отдать капусту, получить моркву и пропустить ход
            if (spieler.GiveCabbage)
            {
                // добавить узел и спуститься ниже
                var spielerTokens = board.GetSpielerTokens(board.currentSpielerIndex);
                var tokenOnCabbage = spielerTokens.First(t => board.cells[t.Position].CellType == CellType.Cabbage);
                var tokenOnCabbageIndex = board.tokens.FindIndex(t => t == tokenOnCabbage);
                MakeTurnAndRootDown(board, root, pov, level, tokenOnCabbageIndex, tokenOnCabbage.Position, false);
                return;
            }

            // пробуем ходить каждой фишкой
            foreach (var tokenIndex in tokenIndicies)
            {
                var token = board.tokens[tokenIndex];

                // возможен ли ход фишкой?
                // мб невозможен - если фишка стоит на финише, если другая фишка стоит на капусте...
                if (token.Position == board.cells.Length) continue;

                var cell = board.cells[token.Position];

                // если стоим на морковке - пробуем два хода нулевой длины
                // (отдать 10 морковок и получить 10 морковок)
                if (cell.CellType == CellType.Carrot)
                {
                    if (spieler.CarrotsSpare > Board.CarrotsPerStay)
                    {
                        // если есть излишек моркови
                        var carrotsToFinish = tokenIndicies.Sum(i => Board.GetCarrotsPerCells(
                            board.cells.Length - board.tokens[i].Position));
                        // тогда попробуем отдать морковку...
                        if (carrotsToFinish < (board.CurrentSpieler.CarrotsSpare + 10))
                            MakeTurnAndRootDown(board, root, pov, level, tokenIndex, token.Position, true);
                    }
                    
                    // вариант с получением моркови интересен только при ее недостатке
                    // стараемся урезать дерево...
                    if (level <= MaxLevelToTryAllPaths || spieler.CarrotsSpare < 20)
                        MakeTurnAndRootDown(board, root, pov, level, tokenIndex, token.Position, false);
                }

                // пробуем ход на ежа (ход назад)
                if (token.Position > 1)
                {
                    // если ход на ежа не особо имеет смысл, а мы уже
                    // стараемся урезать дерево...
                    if (level <= MaxLevelToTryAllPaths || spieler.CarrotsSpare < 20)
                    {
                        for (var i = token.Position - 1; i > 0; i--)
                        {
                            if (board.cells[i].CellType != CellType.Hedgehog) continue;
                            // йож занят?
                            var igelPos = i;
                            if (board.tokens.Any(t => t.Position == igelPos)) break;
                            // пробуем ход на ежа
                            MakeTurnAndRootDown(board, root, pov, level, tokenIndex, i, false);
                        }
                    }
                }

                // пробуем ход на каждую последующую клетку, пока хватает моркови
                for (var i = token.Position + 1; i < board.cells.Length; i++)
                {
                    // стараемся урезать дерево...
                    // короткие либо слишком длинные ходы м.б. неинтересны
                    if (level > MaxLevelToTryAllPaths)
                    {
                        var isOk = board.cells[i].CellType == CellType.Cabbage ||
                                    i == board.cells.Length - 1;
                        if (!isOk)
                        {
                            // если моркови много, а ход короткий и не на финиш и не на капусту - 
                            // - нафиг такой ход
                            var deltaCells = i - token.Position;
                            if ((spieler.CarrotsSpare > 9 && deltaCells == 1)
                                || (spieler.CarrotsSpare > 15 && deltaCells == 2))
                                continue;

                            // если ход отнимает слишком много морквы
                            var carrotsRequired = Board.GetCarrotsPerCells(deltaCells);
                            var deltaCarrot = spieler.CarrotsSpare - carrotsRequired;
                            if (deltaCarrot < 15 && spieler.CarrotsSpare > 55)
                                continue;
                        }
                    }

                    // клетка занята?
                    var cellPos = i;
                    if (cellPos < board.cells.Length - 1)
                        if (board.tokens.Any(t => t.Position == cellPos)) continue;

                    // ход вперед на ежа?
                    if (board.cells[i].CellType == CellType.Hedgehog) continue;
                    
                    // ход на капусту - а капусты нет?
                    if (board.cells[i].CellType == CellType.Cabbage 
                        && board.CurrentSpieler.CabbageSpare == 0) continue;

                    // попробовать ход
                    string error;
                    int deltaCarrots;
                    if (board.CheckTurn(board.currentSpielerIndex, tokenIndex, i, false, out error, out deltaCarrots))
                    {
                        // пробуем такой ход
                        MakeTurnAndRootDown(board, root, pov, level, tokenIndex, i, false);
                        continue;
                    }

                    // для хода не хватает моркови?
                    if (deltaCarrots >= spieler.CarrotsSpare)
                        break;
                }
            }
        }

        private static void MakeTurnAndRootDown(Board board, SolutionNode root, Spieler pov, int recursionLevel,
            int tokenIndex, int position, bool gaveCarrot)
        {
            var nextBoard = board.MakeShallowCopy();
            nextBoard.MakeTurn(tokenIndex, position, gaveCarrot, true);
            MakeNewRootAndGoDown(nextBoard, root, pov, recursionLevel, tokenIndex, position, gaveCarrot);
        }

        private static void MakeNewRootAndGoDown(Board nextBoard, SolutionNode root, Spieler pov, int recursionLevel,
            int tokenIndex, int targetCell, bool gaveCarrot)
        {
            var newRoot = new SolutionNode
            {
                targetCell = (short)targetCell,
                children = new List<SolutionNode>(),
                gaveCarrot = gaveCarrot,
                token = (short)tokenIndex                
            };
            root.children.Add(newRoot);
            // рекурсивно углубиться
            if (recursionLevel < MaxLevel)
                MakeAllProbableTurns(nextBoard, newRoot, pov, recursionLevel + 1);
            if (newRoot.children.Count == 0)
                newRoot.Board = nextBoard; // для листа запоминаем доску
        }

        public static int GetScore(Board board, Spieler pov)
        {
            if (board.Winner == pov) return int.MaxValue;
            
            var score = 0;

            var povTokens = board.GetSpielerTokens(board.spielers.FindIndex(s => s.Id == pov.Id));
            var povPosition = povTokens.Sum(t => t.Position);
            var povCabbage = pov.GiveCabbage ? pov.CabbageSpare - 1 : pov.CabbageSpare;
            var povCarrots = pov.CarrotsSpare;
            if (pov.GiveCabbage)
                povCarrots += 10;
            // относительная позиция
            var relPos = povPosition / (double) board.cells.Length / povTokens.Length;

            for (var i = 0; i < board.spielers.Length; i++)
            {
                var spieler = board.spielers[i];
                if (board.Winner == spieler) return int.MinValue;

                if (spieler == pov) continue;
                var spielerTokens = board.GetSpielerTokens(i);
                
                // разница в расстоянии
                var deltaCells = povPosition - spielerTokens.Sum(t => t.Position);
                score += deltaCells;
                
                // и в моркови
                var deltaCarrots = povCarrots -
                                   (spieler.GiveCabbage ? spieler.CarrotsSpare + 10 : spieler.CarrotsSpare);
                var deltaCarrotsScore = Math.Sign(deltaCarrots) * (int)Math.Round(Math.Sqrt(deltaCarrots));
                score += deltaCarrotsScore;

                // и в капусте
                var spielerCabbage = spieler.GiveCabbage ? spieler.CabbageSpare - 1 : spieler.CabbageSpare;
                var deltaCabbage = spielerCabbage - povCabbage;
                var kCabbage = relPos < 0.25 ? 20 : relPos < 0.45 ? 35 : relPos < 0.65 ? 65 : 140;
                score += deltaCabbage * kCabbage;
            }

            // штраф за низкую степень свободы
            if (povCarrots < 12)
                score -= (povCarrots - 14) * (povCarrots - 14);

            // результат как разница собственного и наилучшего счета
            return score;
        }

        /// <summary>
        /// оценить ситуацию на доске с позиции pov
        /// выдать численную оценку - чем больше, тем лучше
        /// </summary>        
        public static int GetScore2(Board board, Spieler pov)
        {
            if (board.Winner == pov) return int.MaxValue;

            var ownScore = GetSpielerScore(board, pov);
            var maxOtherScore = int.MinValue;

            foreach (var spieler in board.spielers)
            {
                if (spieler == pov) continue;
                var score = GetSpielerScore(board, spieler);
                if (score > maxOtherScore)
                    maxOtherScore = score;
            }

            // результат как разница собственного и наилучшего счета
            return ownScore - maxOtherScore;
        }

        //public static int GetSpielerScore(Board board, Spieler pov)
        //{
        //    var score = 0;

        //    // в морковках учтем те, что получит игрок после хода
        //    var spielerCarrots = pov.CarrotsSpare;
        //    var cellsPassed = 0;

        //    // фишки игрока (1 или 2)
        //    var tokens = board.GetSpielerTokens(board.spielers.FindIndex(s => s.Id == pov.Id));
        //    foreach (var token in tokens)
        //    {
        //        cellsPassed += token.Position;
        //        var curCell = board.cells[token.Position];
        //        // заяц в минус
        //        if (curCell.CellType == CellType.Hare)
        //            score -= HarePenalty;

        //        // бонус за номерную карту и позицию в гонке
        //        var ownPos = token.Position;
        //        var pos = board.tokens.Count(t => t.Position > ownPos) + 1;
        //        if (curCell.CellType == CellType.Number && pos == curCell.Points)
        //        {
        //            var delta = pos * Board.CarrotsPerPosition;
        //            spielerCarrots += delta;
        //        }

        //        // учесть морковки, получаемые за капусту
        //        if (pov.GiveCabbage && curCell.CellType == CellType.Cabbage)
        //        {
        //            var delta = pos * Board.CarrotsPerCabbage;
        //            spielerCarrots += delta;
        //        }
        //    }

        //    // посчитаем, сколько клеток игрок сможет пройти за свои морковки


        //    return score;
        //}

        public static int GetSpielerScore(Board board, Spieler pov)
        {
            // морковки идут в плюс
            var spielerCarrots = pov.CarrotsSpare;
            var score = 0;
            var percentMade = 0f;

            var tokens = board.GetSpielerTokens(board.spielers.FindIndex(s => s.Id == pov.Id));
            foreach (var token in tokens)
            {
                var range = token.Position;
                // пройденное расстояние идет в плюс
                score += CellPenalty * range;
                var curCell = board.cells[token.Position];
                // заяц в минус
                if (curCell.CellType == CellType.Hare)
                    score -= HarePenalty;

                // позиция в гонке...
                var ownPos = token.Position;
                var pos = board.tokens.Count(t => t.Position > ownPos) + 1;
                if (curCell.CellType == CellType.Number && pos == curCell.Points)
                {
                    var delta = pos * Board.CarrotsPerPosition;
                    spielerCarrots += delta;
                }

                // учесть морковки, получаемые за капусту
                if (pov.GiveCabbage && curCell.CellType == CellType.Cabbage)
                {
                    var delta = pos * Board.CarrotsPerCabbage;
                    spielerCarrots += delta;
                }

                percentMade += range / (float) board.cells.Length;
            }
            
            score += spielerCarrots;
            if (tokens.Length == 2)
                score += spielerCarrots;

            // если после хода моркови - кот наплакал...
            if (spielerCarrots < 13)
                score -= 20;
            if (spielerCarrots < 3)
                score -= 30;
            if (spielerCarrots < 2)
                score -= 30;
            if (spielerCarrots < 1)
                score -= 30000;

            // штраф за капусту
            if (board.tokens.Length > board.spielers.Length)
                percentMade /= 2f;

            if (pov.CabbageSpare > 0 && !pov.GiveCabbage)
            {
                var cabbageToCount = pov.GiveCabbage ? pov.CabbageSpare - 1 : pov.CabbageSpare;
                var k = percentMade > 0.85f ? 10 : percentMade > 0.65f ? 6 : percentMade > 0.4f ? 3 : percentMade > 0.28f ? 2 : 1;
                if (spielerCarrots > 50 && cabbageToCount > 1)
                    score -= spielerCarrots;
                else if (spielerCarrots > 100)
                    score -= spielerCarrots;

                score -= k * CabbagePenalty * pov.CabbageSpare;
            }

            // не слишком ли много моркови?
            if (percentMade > 0.5f) // && spielerCarrots > 40)
            {
                var carrotsToFinish = tokens.Sum(t => Board.GetCarrotsPerCells(
                    board.cells.Length - t.Position));
                if (carrotsToFinish < (spielerCarrots + Board.CarrotsPerFinish))
                    score -= pov.CarrotsSpare * 10;
                else
                {
                    if (spielerCarrots >= carrotsToFinish)
                        score += 300; // морковки хватит, чтобы финишировать
                    else
                    {
                        // учесть соотношение моркови на руках / моркови до финиша
                        var progressPercent = spielerCarrots / (double) carrotsToFinish;
                        score += (int) Math.Round(350 * Math.Pow(progressPercent, 0.75 / percentMade));
                    }
                }
            }

            return score;
        }
    }
}
