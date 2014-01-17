using System;
using System.Collections.Generic;
using System.Linq;
using HaseUndIgel.AI;

namespace HaseUndIgel.BL
{
    public class Board
    {
        public static Cell[] debugCells;

        #region Constants

        public const int CarrotsPerHedgehog = 10;

        public const int CarrotsPerStay = 10;

        public const int CarrotsPerCabbage = 10;

        public const int CarrotsPerPosition = 10;

        public const int CarrotsPerFinish = 10;

        #endregion

        public Cell[] cells;

        public Spieler[] spielers;

        public Token[] tokens;

        public readonly List<SpielerTurn> turns = new List<SpielerTurn>();

        #region Spieler variables

        public bool Endspiel { get; private set; }

        public Spieler Winner { get; private set; }

        public int currentSpielerIndex;

        public int selectedTokenIndex = -1;

        public Spieler CurrentSpieler
        {
            get { return spielers[currentSpielerIndex]; }
        }

        #endregion

        #region Board Initialization
        public Board()
        {
        }

        public Board(int spielersTotal, int spielersComp)
        {
            var names = new []
                {
                    "Der Hase", "Die Igel", "Carmen", "Mario", "Bender", "Zoidberg"
                };

            cells = new []
                {
                    new Cell(CellType.Start),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Hare),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Number, 3),
                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Hare),
                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Carrot),

                    new Cell(CellType.Number, 1),
                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Cabbage),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Number, 4),
                    new Cell(CellType.Hedgehog),
                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Hare),
                    new Cell(CellType.Number, 3),

                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Cabbage),
                    new Cell(CellType.Number, 4),
                    new Cell(CellType.Hedgehog),
                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Cabbage),
                    new Cell(CellType.Number, 1),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Hedgehog),
                    new Cell(CellType.Number, 4),
                    new Cell(CellType.Number, 2),
                    new Cell(CellType.Hare),
                    new Cell(CellType.Carrot),
                    new Cell(CellType.Finish),
                };
            debugCells = cells;

            if (spielersTotal == 2)
            {
                spielers = new[]
                    {
                        new Spieler {CabbageSpare = 2, CarrotsSpare = 50, IsComputer = false, Name = names[0], Id = 0 },
                        new Spieler
                            {
                                CabbageSpare = 2,
                                CarrotsSpare = 50,
                                IsComputer = spielersComp > 0,
                                Name = names[1],
                                Id = 1
                            }
                    };
                tokens = new []
                    {
                        new Token {ColorIndex = 0, Position = 0},
                        new Token {ColorIndex = 1, Position = 0},
                        new Token {ColorIndex = 2, Position = 0},
                        new Token {ColorIndex = 3, Position = 0},
                    };
                return;
            }

            spielers = new Spieler[spielersTotal];
            tokens = new Token[spielersTotal];

            for (var i = 0; i < spielersTotal; i++)
            {
                spielers[i] = new Spieler
                    {
                        Id = i,
                        CabbageSpare = 2,
                        CarrotsSpare = 50,
                        IsComputer = i >= (spielersTotal - spielersComp),
                        Name = names[i]
                    };
                tokens[i] = new Token
                    {
                        ColorIndex = i,
                        Position = 0
                    };
            }
        }
        #endregion
    
        /// <summary>
        /// "скопировать" доску для проверки результатов пробного хода
        /// </summary>
        public Board MakeShallowCopy()
        {
            var board = new Board
                {
                    cells = cells,
                    currentSpielerIndex = currentSpielerIndex,
                    Endspiel = Endspiel,
                    spielers = new Spieler[spielers.Length],
                    tokens = new Token[tokens.Length]
                };
            
            for (var i = 0; i < spielers.Length; i++)
            {
                board.spielers[i] = spielers[i].MakeShallowCopy();
                if (tokens.Length == spielers.Length)
                    board.tokens[i] = new Token
                        {
                            ColorIndex = tokens[i].ColorIndex,
                            Position = tokens[i].Position
                        };
                else
                {
                    board.tokens[i * 2] = new Token
                    {
                        ColorIndex = tokens[i * 2].ColorIndex,
                        Position = tokens[i * 2].Position,
                    };
                    board.tokens[i * 2 + 1] = new Token
                    {
                        ColorIndex = tokens[i * 2 + 1].ColorIndex,
                        Position = tokens[i * 2 + 1].Position,
                    };
                }
            }

            return board;
        }

        public bool CheckTurn(int spielerIndex, int tokenIndex,
                              int targetPosition, bool gaveCarrot, out string error, out int deltaCarrots)
        {
            deltaCarrots = 0;
            error = string.Empty;

            // проверить допустимость хода
            var spieler = spielers[spielerIndex];
            var token = tokens[tokenIndex];
            var oldPos = token.Position;
            var cellsPassed = targetPosition - oldPos;
            var cell = cells[targetPosition];
            var spielerTokens = GetSpielerTokens(spielerIndex);

            // стоит на месте?
            if (cellsPassed == 0)
            {
                // снять с игрока "заморозку"
                if (spieler.Freezed)
                    return true;                

                // стоит на морковке?
                if (cell.CellType == CellType.Carrot)
                {
                    var isOk = spieler.CarrotsSpare >= CarrotsPerStay || !gaveCarrot;
                    if (!isOk)
                        error = "Нечего отдать";
                    else
                        deltaCarrots = gaveCarrot ? CarrotsPerStay : -CarrotsPerStay;
                    return isOk;
                }

                // стоит на капусте?
                if (spielerTokens.Any(t => cells[t.Position].CellType == CellType.Cabbage &&
                                           !spieler.GiveCabbage))
                {
                    error = "Нужно уйти с капусты";
                    return false;
                }
                if (cell.CellType == CellType.Cabbage) 
                {
                    if (spieler.GiveCabbage)
                    {
                        if (spieler.CabbageSpare == 0)
                        {
                            error = "Нет капусты";
                            return false; // недостижимо
                        }
                        var tokenPosition = 1 + tokens.Count(t => t.Position > oldPos);
                        deltaCarrots = tokenPosition * CarrotsPerCabbage;
                        return true;
                    }
                    error = "Нужно уйти с капусты";
                    return false;
                }
            }

            // клетка занята?
            if (tokens.Any(t => t.Position == targetPosition &&
                targetPosition != cells.Length - 1))
            {
                error = "Клетка занята";
                return false;    
            }

            if (spieler.Freezed)
            {
                error = "Игрок пропускает ход";
                return false; // заморожен?
            }

            // на ежа?
            if (cellsPassed < 0)
            {
                var isOk = cell.CellType == CellType.Hedgehog;
                if (!isOk)
                    error = "Ход назад возможен только на ежа";
                else
                {
                    deltaCarrots = -cellsPassed * CarrotsPerHedgehog;
                }
                return isOk;
            }
            
            // ход вперед
            var oldCell = cells[oldPos];
            if (oldCell.CellType == CellType.Cabbage && spieler.GiveCabbage)
            {
                error = "Ход пропускается (на капусте)";
                return false;
            }
            
            // должен уйти с капусты другой фишкой?
            var otherToken = spielerTokens.FirstOrDefault(t => t != token);
            if (otherToken != null)
            {
                var onCabbage = cells[otherToken.Position].CellType == CellType.Cabbage;
                if (onCabbage)
                {
                    error = "Нужно уйти с капусты";
                    return false;
                }
            }

            if (cell.CellType == CellType.Hedgehog)
            {
                error = "Занимать ежа разрешено только при ходе назад";
                return false;
            }

            if (cell.CellType == CellType.Cabbage && CurrentSpieler.CabbageSpare == 0)
            {
                error = "Нет капусты";
                return false;
            }

            // посчитать деньги, что получит игрок
            var additionalCarrots = 0;
            if (oldCell.CellType == CellType.Number)
            {
                var oldPosition = 1 + tokens.Count(t => t.Position > oldPos);
                if (oldPosition == oldCell.Points || (oldPosition > 4 && oldCell.Points == 1))
                    additionalCarrots = CarrotsPerPosition * oldPosition;
            }

            // посчитать деньги, что потребуются на ход
            var carrotsNeeded = GetCarrotsPerCells(cellsPassed);
            deltaCarrots = carrotsNeeded - additionalCarrots;

            if ((additionalCarrots + spieler.CarrotsSpare) < carrotsNeeded)
            {
                error = "Недостаточно моркови";
                return false;
            }

            // если это финиш - посчитать количество моркови на руках
            if (cell.CellType == CellType.Finish)
            {
                var oldPosition = 1 + tokens.Count(t => t.Position > oldPos);
                var carrotsMax = oldPosition * CarrotsPerFinish;
                if (spieler.CarrotsSpare > carrotsMax)
                {
                    error = "Избыток моркови для финиша (" + carrotsMax + ")";
                    return false;
                }
            }

            return true;
        }

        public string MakeTurn(int tokenIndex,
            int targetPosition, bool gaveCarrot, bool simulatorTurn)
        {
            var hadCarrots = CurrentSpieler.CarrotsSpare;
            var oldCell = tokens[tokenIndex].Position;

            var msg = MakeTurnWoSwitchSpieler(currentSpielerIndex, tokenIndex,
                                    targetPosition, gaveCarrot, simulatorTurn);
            
            // записать ход
            turns.Add(new SpielerTurn
                {
                    SpielerIndex  = currentSpielerIndex,
                    TargetCell = targetPosition,
                    TokenIndex = tokenIndex,
                    DeltaCarrots = CurrentSpieler.CarrotsSpare - hadCarrots,
                    NewCarrots = CurrentSpieler.CarrotsSpare,
                    NewCabbages = CurrentSpieler.CabbageSpare,
                    OldCell = oldCell
                });
            
            SwitchSpieler(!simulatorTurn);
            msg = msg + Environment.NewLine + "Ход: " + CurrentSpieler.Name;
            return msg;
        }

        private string MakeTurnWoSwitchSpieler(int spielerIndex, int tokenIndex,
            int targetPosition, bool gaveCarrot, bool simulatorTurn)
        {
            if (Endspiel) return "";

            var spieler = spielers[spielerIndex];
            var token = tokens[tokenIndex];
            var oldPos = token.Position;
            var cellsPassed = targetPosition - oldPos;
            var cell = cells[targetPosition];

            // стоит на месте?
            if (cellsPassed == 0)
            {
                // снять с игрока "заморозку"
                if (spieler.Freezed) return "Ход пропущен";
                
                // стоит на морковке?
                if (cell.CellType == CellType.Carrot)
                {
                    spieler.CarrotsSpare += (gaveCarrot ? -CarrotsPerStay : CarrotsPerStay);
                    return (gaveCarrot ? "Отдано " : "Получено ") + CarrotsPerStay + " моркови";
                }

                // стоит на капусте?
                if (cell.CellType == CellType.Cabbage && spieler.GiveCabbage)
                {
                    spieler.GiveCabbage = false;
                    spieler.CabbageSpare--;
                    var position = 1 + tokens.Count(t => t.Position > oldPos);
                    var deltaCarrots = CarrotsPerCabbage * position;
                    spieler.CarrotsSpare += deltaCarrots;
                    return "Капуста отдана, получено " + deltaCarrots + " моркови";
                }
            }

            // на ежа?
            if (cellsPassed < 0)
            {
                // добавить игроку морковок и переместить токен
                var deltaCarrots = -cellsPassed * CarrotsPerHedgehog;
                spieler.CarrotsSpare += deltaCarrots;
                token.Position = targetPosition;
                return "Получено " + deltaCarrots + " моркови";
            }

            // ход вперед
            // посчитать деньги, что получит игрок
            var oldCell = cells[oldPos];
            var additionalCarrots = 0;
            if (oldCell.CellType == CellType.Number)
            {
                var oldPosition = 1 + tokens.Count(t => t.Position > oldPos);
                if (oldPosition == oldCell.Points || (oldPosition > 4 && oldCell.Points == 1))
                    additionalCarrots = CarrotsPerPosition * oldPosition;
            }
            
            // посчитать деньги, что потребуются на ход
            var carrotsNeeded = GetCarrotsPerCells(cellsPassed);
            
            // если это финиш - таки финишировать
            var message = string.Empty;
            if (cell.CellType == CellType.Finish)
            {
                token.Position = targetPosition;
                var spielerFinished = tokens.Length == spielers.Length;
                if (!spielerFinished)
                {
                    // все фишки игрока финишировали?
                    var spielerTokens = GetSpielerTokens(spielerIndex);
                    if (spielerTokens.All(t => t.Position == spielers.Length - 1))
                        spielerFinished = true;
                }

                if (spielerFinished)
                {
                    Endspiel = true;
                    Winner = spieler;
                    message = spieler.Name + " финишировал!";
                }
            }

            var minusCarrots = carrotsNeeded - additionalCarrots;
            if (string.IsNullOrEmpty(message))
                message = (minusCarrots > 0 ? "Потрачено " : "Получено ") +
                          Math.Abs(minusCarrots) + " моркови";

            spieler.CarrotsSpare -= minusCarrots;
            token.Position = targetPosition;

            // если встал на капусту...
            if (cell.CellType == CellType.Cabbage)
                spieler.GiveCabbage = true;

            return message;
        }

        public static int GetCarrotsPerCells(int cells)
        {
            cells++;
            return cells*(cells - 1)/2;
        }
    
        public void SwitchSpieler(bool makeNextTurnAuto)
        {
            currentSpielerIndex++;
            if (currentSpielerIndex == spielers.Length)
                currentSpielerIndex = 0;
            selectedTokenIndex = -1;

            if (makeNextTurnAuto && spielers[currentSpielerIndex].IsComputer)
                    MakeTurnByComputer();
        }

        // походить за компьютер
        private void MakeTurnByComputer()
        {
            ComputerMind.MakeTurn(this);
        }
    
        public Token[] GetSpielerTokens(int spielerIndex)
        {
            return spielers.Length == tokens.Length
                       ? new[] {tokens[spielerIndex]}
                       : new[] {tokens[spielerIndex*2], tokens[spielerIndex*2 + 1]};
        }

        public Token[] GetSpielerTokens()
        {
            return GetSpielerTokens(currentSpielerIndex);
        }
    }
}
