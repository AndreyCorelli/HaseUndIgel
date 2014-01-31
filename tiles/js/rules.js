
function Rules(board) {
    this.board = board;

    // constants
    this.MaxStepsInTurn = 3;
    this.resourcePerCell = [1, 2, 3];

    // whether spieler can step on the tile or not
    this.checkSpielerCanStepOnCell = function (targetCell) {
        var spieler = this.board.spielers[this.board.currentSpieler];
        var spielerOldCell = this.board.getCellTypeByRowCol(spieler.y, spieler.x);
        var cellType = this.board.getCellTypeByRowCol(targetCell.y, targetCell.x);

        // check range and resources
        // if it is not a metro cell - range should be 1 or null
        var isAdjacent = Math.abs(spieler.y - targetCell.y) <= 1
            && Math.abs(spieler.x - targetCell.x) <= 1;

        if (!isAdjacent) return false;

        // check resource cost
        var cost = this.getResourceCostForTargetCell(spieler, targetCell);
        if (cost > spieler.resource) return false;

        return true;
    }

    // make step
    this.moveSpielerOnCell = function (coords, turnEnded) {
        var spieler = this.board.spielers[this.board.currentSpieler];
        spieler.x = coords.x;
        spieler.y = coords.y;
        
        // charge cell's cost
        var cost = this.getResourceCostForTargetCell(spieler, coords);
        spieler.resource -= cost;

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
        }
        
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

    // how much resource needed to step on cell?
    this.getResourceCostForTargetCell = function (spieler, coords) {
        var cost = this.resourcePerCell[spieler.curStep - 1] * spieler.people;
        var tile = this.board.getCellTypeByRowCol(coords.y, coords.x);
        if (tile == this.board.TileGrass)
            cost = Math.floor(cost / 2);
        return cost;
    }
}