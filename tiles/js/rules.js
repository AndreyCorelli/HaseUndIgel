/*
    // путешествие на эту клетку стоит resourcePerCell[numStep] * people ресурсов
    this.TileSand = 0;  
    
    // путешествие на эту клетку стоит вдвое меньше ресурсов, чем на клетку песка
    this.TileGrass = 1;

    // встав на эту клетку, игрок теряет people -= 1. После чего клетка "очищается" от снайпера
    this.TileSniper = 2;

    // стоя на этой клетке игрок может либо собрать (people x 2) ресурсов,
    // либо "пригласить" в команду +2 people
    this.TileShelter = 3;
    
    // стартовая клетка для всех игроков
    this.TileStart = 4;

    // конечная клетка для всех игроков
    this.TileFinish = 5;

    // бандитос... деремся / теряем ресурсы
    this.TileRaiders = 6;
*/


function Rules(board) {
    this.board = board;

    // constants
    this.SpielerChoiceResource = 1;
    this.SpielerChoicePeople = 2;
    this.MaxStepsInTurn = 3;
    this.resourcePerCell = [1, 2, 3];
    this.SpielerPeopleFromShelter = 2;
    this.SpielerResourceFromShelter = 2;
    this.co

    // spieler choice
    this.spielerChoiceShelter = 0;

    // whether spieler can step on the tile or not
    this.checkSpielerCanStepOnCell = function (targetCell) {
        var spieler = this.board.spielers[this.board.currentSpieler];
        var spielerOldCell = this.board.getCellTypeByRowCol(spieler.y, spieler.x);
        var cellType = this.board.getCellTypeByRowCol(targetCell.y, targetCell.x);

        this.showHintOnCell(spieler, targetCell);

        // almost free jumps from metro to metro
        if (cellType = this.board.TileMetro && spielerOldCell == this.board.TileMetro) {
            return this.getResourceCostForTargetCell(spieler, targetCell) < spieler.resource;
        }

        // check range and resources
        // if it is not a metro cell - range should be 1 or null
        var isAdjacent = Math.abs(spieler.y - targetCell.y) <= 1
            && Math.abs(spieler.x - targetCell.x) <= 1;

        if (!isAdjacent) return false;

        // stay on cell - first turn only?
        //if (spieler.y == targetCell.y && spieler.x == targetCell.x) {        }

        // check resource cost
        var cost = this.getResourceCostForTargetCell(spieler, targetCell);
        if (cost > spieler.resource) return false;

        return true;
    }

    // show hint when user steps on a cell
    this.showHintOnCell = function (spieler, targetCell) {
        var cost = this.getResourceCostForTargetCell(spieler, targetCell);
        var cellType = this.board.getCellTypeByRowCol(targetCell.y, targetCell.x);
        var costStr = (cost < 0 ? ' Вы получите ' : ' Вы отдадите ') + cost + ' ед. ресурсов';

        if (spieler.x == targetCell.x && spieler.y == targetCell.y) {
            this.board.showHelpOnCell('Оставаясь на клетке, вы получите по единице ресурсов на члена отряда (по две ед. стоя на "чистой" клетке). ' + costStr);
            return;
        }

        if (cellType == this.board.TileMetro) {
            this.board.showHelpOnCell('Вход в метро. С этой клетки вы можете перейти на любую клетку ' +
                'метро, потратив столько ед. ресурсов, сколько клеток отделяет вас от выбранной клетки. Если ваш отряд меньше 6, Сталкер забирает одного члена отряда. ' + costStr);
            return;
        }

        if (cellType == this.board.TileSniper) {
            this.board.showHelpOnCell('Поле контролирует снайпер. Проходя клетку, вы теряете одного члена отряда и освобождаете поле от снайпера.' + costStr);
            return;
        }

        if (cellType == this.board.TileRaiders) {
            this.board.showHelpOnCell('Поле занято бандитами! Большой отряд (свыше 8 человек) пройдет свободно. Средний отряд (более 5 человек) потеряет одного, маленький отряд - двоих. ' + costStr);
            return;
        }

        if (cellType == this.board.TileShelter) {
            this.board.showHelpOnCell('Убежище удерживает немногочисленный отряд. Вы можете отобрать у них припасы (' + this.SpielerResourceFromShelter +
                ' ед. на каждого в отряде) либо присоединить ' + this.SpielerPeopleFromShelter + ' человек в отряд. ' + costStr);
            return;
        }

        if (cellType == this.board.TileGrass) {
            this.board.showHelpOnCell('"Чистая" клетка. Ход на такую клетку стоит в два раза меньше ресурсов (половина ед. ресурса на каждого члена отряда за первый ход, единица за второй и 3/2 ед. за третий ход). ' + costStr);
            return;
        }

        if (cellType == this.board.TileSand) {
            this.board.showHelpOnCell('Пустошь. Ход на такую клетку стоит единицу ресурса на каждого члена отряда за первый ход, две единицы за второй и 3 ед. за третий ход. ' + costStr);
            return;
        }

        this.board.showHelpOnCell(costStr);
    }

    // make step
    this.moveSpielerOnCell = function (coords, turnEnded) {
        var spieler = this.board.spielers[this.board.currentSpieler];
        
        // charge cell's cost
        var cost = this.getResourceCostForTargetCell(spieler, coords);
        
        // apply cell's effects
        var cellType = this.board.getCellTypeByRowCol(coords.y, coords.x);
        // sniper!
        if (cellType == this.board.TileSniper) {
            // kill one man
            spieler.people -= 1;
            // free cell of the sniper
            this.board.cells[coords.y][coords.x] = this.board.TileSand;
        }
        // shelter!
        else if (cellType == this.board.TileShelter) {
            console.log('step on shelter, choice: ', this.spielerChoiceShelter);
            if (this.spielerChoiceShelter == 0) {
                // choice is not made yet
                this.askSpielerChoiceShelter();
                return;
            }

            // recruit?
            if (this.spielerChoiceShelter == this.SpielerChoicePeople) {
                spieler.people += this.SpielerPeopleFromShelter;
            } else if (this.spielerChoiceShelter == this.SpielerChoiceResource) {
                spieler.resource += Math.floor(spieler.people * this.SpielerResourceFromShelter);
            }
            // remove shelter from the map
            this.board.cells[coords.y][coords.x] = this.board.TileSand;
        }
        // raiders! fight and loose people... or not
        else if (cellType == this.board.TileRaiders) {
            if (spieler.people > 8) {
                // free cell of the raiders
                this.board.cells[coords.y][coords.x] = this.board.TileSand;
            } else if (spieler.people > 5) {
                spieler.people -= 1;
                this.board.cells[coords.y][coords.x] = this.board.TileSand;
            } else if (spieler.people > 2) {
                spieler.people -= 2;
            } else
                spieler.people = 0;
        }
        // metro!
        else if (cellType == this.board.TileMetro) {
            if (this.board.getCellTypeByRowCol(spieler.y, spieler.x) != this.board.TileMetro)
                if (spieler.people < 6)
                    spieler.people -= 1; // Stalker has charged his fee
        }

        // do change coords and charge fee
        spieler.x = coords.x;
        spieler.y = coords.y;
        spieler.resource -= cost;
        this.board.spielerSelectedCell = new Point(-1, -1);

        // forget last turn variables
        this.board.spielerDecidedOnEndTurn = 0;
        this.spielerChoiceShelter = 0;
        
        // FIN?
        this.checkWinLoss(spieler);

        // step number...
        spieler.curStep = spieler.curStep + 1;
        // if turn has ended ...
        if (!turnEnded && spieler.curStep > this.MaxStepsInTurn)
            turnEnded = true;            
        if (turnEnded) {
            // reset spieler's turn
            spieler.curStep = 1;
            // give turn
            this.board.switchNextSpieler();
        }
        // redraw
        this.board.drawBoard();
    }

    // check is it win or loss?
    this.checkWinLoss = function (spieler) {
        var cell = this.board.getCellTypeByRowCol(spieler.y, spieler.x);
        if (cell == this.board.TileFinish)
            this.board.showFinalTitles(spieler);
    }

    // how much resource needed to step on cell?
    this.getResourceCostForTargetCell = function (spieler, coords) {
        var tile = this.board.getCellTypeByRowCol(coords.y, coords.x);

        // stay on cell?
        if (coords.x == spieler.x && coords.y == spieler.y) {
            if (tile == this.board.TileGrass)
                return -2 * spieler.people;
            return spieler.people;;
        }

        // move on metro?
        // 1 res per cell
        if (tile == this.board.TileMetro) {
            var oldTile = this.board.getCellTypeByRowCol(spieler.y, spieler.x);
            if (oldTile == this.board.TileMetro)
                return Math.max(Math.abs(spieler.y - coords.y),
                    Math.abs(spieler.x - coords.x));
        }

        // pay per move
        var cost = this.resourcePerCell[spieler.curStep - 1] * spieler.people;
        
        if (tile == this.board.TileGrass)
            cost = Math.floor(cost / 2);
        return cost;
    }

    // ask spieler what he chooses
    this.askSpielerChoiceShelter = function () {
        customConfirm('Пополнить запасы либо набрать рекрутов?', 'Сделайте выбор', 'Запасы', 'Рекруты',
            function (choiceIndex) {
                console.log('function (choiceIndex) called');
                board.rules.spielerChoiceShelter = board.rules.SpielerChoiceResource;
                if (choiceIndex == 1)
                    board.rules.spielerChoiceShelter = board.rules.SpielerChoicePeople;
                board.rules.moveSpielerOnCell(board.spielerSelectedCell, board.rules.spielerDecidedOnEndTurn);
            });
        console.log('customConfirm called');
    }
}