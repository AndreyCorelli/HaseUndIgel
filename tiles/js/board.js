var board;

function Board() {
    // spiel rules
    this.rules = new Rules(this);

    // spielers
    this.spielers = new Array();

    // constants
    this.SpielModeEdit = 0;
    this.SpielModeSpiel = 1;
    this.rows = 12;
    this.cols = 10;
    this.tileSz = 48;
    this.tileSz2 = 24;
    this.tileSz4 = 12;
    this.boardPadLeft = 0;
    this.boardPadTop = 0;

    // variables
    this.currentPaletteItemIndex = -1;
    this.boardCodeContainer = '';
    this.currentSpieler = -1;
    this.spielMode = this.SpielModeEdit;
    this.spielerSelectedCell = new Point(-1, -1);

    // constants - tile types
    this.TileSand = 0;
    this.TileGrass = 1;
    this.TileSniper = 2;
    this.TileShelter = 3;
    this.TileStart = 4;
    this.TileFinish = 5;

    this.tileImage = {};
    this.tileImage[this.TileSand] = 'tile_small_sand.png';
    this.tileImage[this.TileGrass] = 'tile_small_grass.png';
    this.tileImage[this.TileSniper] = 'tile_small_sniper.png';
    this.tileImage[this.TileShelter] = 'tile_small_shelter.png';
    this.tileImage[this.TileStart] = 'tile_small_start.png';
    this.tileImage[this.TileFinish] = 'tile_small_finish.png';

    this.spielerImage = new Array(4);
    this.spielerImage[0] = "spieler_a.png";
    this.spielerImage[1] = "spieler_b.png";
    this.spielerImage[2] = "spieler_c.png";
    this.spielerImage[3] = "spieler_d.png";

    // cells array
    this.cells = new Array(this.rows);
    for (var i = 0; i < this.rows; i++) {
        this.cells[i] = new Array(this.cols);
        for (var j = 0; j < this.cols; j++) {
            this.cells[i][j] = this.TileGrass;
        }
    }

    // coords of Start - End
    this.cellStartX = 0;
    this.cellStartY = 0;
    this.cellEndX = 0;
    this.cellEndY = 0;

    // control panel
    this.controlPanelIdImgSpieler = '';
    this.controlPanelIdImgSpielerNext = '';
    this.controlPanelIdBtnMakeTurn = '';
    this.controlPanelIdLabelPeople = '';
    this.controlPanelIdLabelResource = '';

    // draw board as is
    this.drawBoard = function () {

        // update control panel: who's making turn etc
        this.updateControlPanel();

        // draw board itself
        var canvas = $('div#boardCanvas');
        var canvasCoords = canvas.position();
        this.boardPadLeft = canvasCoords.left;
        this.boardPadTop = canvasCoords.top + 30;
        var strInnerHtml = '';

        for (var row = 0; row < this.rows; row++)
            for (var col = 0; col < this.cols; col++) {
                // coords
                var coords = this.getXYByRowCol(row, col);
                var xCoord = coords.x;
                var yCoord = coords.y;

                // img file
                var tileIndex = this.cells[row][col];
                var tileImgStr = this.tileImage[tileIndex];

                // img tag itself
                var strStyle = 'style="left: ' + xCoord + 'px; top: ' + yCoord + 'px"';
                var strTag = '<img src="pic/' + tileImgStr + '" class="tile" onclick="board.processCellClick()"' + strStyle + '/>\n';
                strInnerHtml = strInnerHtml + strTag;
            }

        // add images for spielers
        for (var i = this.spielers.length - 1; i >= 0; i--) {
            var spieler = this.spielers[i];            
            // coords
            var coords = this.getXYByRowCol(spieler.y, spieler.x);
            var xCoord = coords.x;
            var yCoord = coords.y;
            // tag
            var tileImgStr = this.spielerImage[i];
            var strStyle = 'style="left: ' + xCoord + 'px; top: ' + yCoord + 'px"';
            var strTag = '<img src="pic/' + tileImgStr + '" class="tile" onclick=""' + strStyle + '/>\n';
            strInnerHtml = strInnerHtml + strTag;
        }

        canvas.html(strInnerHtml);

        if (this.boardCodeContainer)
            this.drawBoardSrc();
    };

    // get cell type (int, enum) from tile's image
    this.getCellTypeByRowCol = function (row, col) {
        return this.cells[row][col];
    }

    // select cell to make turn or to change tile
    // processSpielerCellClick() V changeFieldItem()
    this.processCellClick = function () {
        if (this.spielMode == this.SpielModeSpiel) {
            this.processSpielerCellClick();
            return;
        }

        if (this.spielMode == this.SpielModeEdit) {
            this.changeFieldItem();
            return;
        }
    }

    // render board as tile images in a DIV (canvas)
    this.drawBoardSrc = function () {
        var canvas = $('div#' + this.boardCodeContainer);
        var innerText = '    var boardCells = new Array(' + this.rows + ');<br/>';
        for (var i = 0; i < this.rows; i++) {
            var strRow = '    boardCells[' + i + '] = new Array(' + this.cells[i].join() + ');<br/>'
            innerText = innerText + strRow;
        }
        canvas.html(innerText);
    }

    // draw palette that is used for choosing board tiles
    this.drawPalette = function () {
        var canvas = $('div#boardPalette');
        var strInnerHtml = ''; // 

        for (var i = 0; i < Object.keys(this.tileImage).length; i++) {
            var tileImgStr = this.tileImage[i];
            var strTag = '<img src="pic/' + tileImgStr + '" class="palette" ' +
                'onclick="board.choosePaletteItem(' + i + ')"' + '/>\n';
            strInnerHtml = strInnerHtml + strTag;
        }
        canvas.html(strInnerHtml);
    }

    // update spieler's control panel
    // (Go! button, score etc)
    this.updateControlPanel = function () {
        if (this.currentSpieler < 0) return;

        // spieler's image
        if (this.controlPanelIdImgSpieler && this.controlPanelIdImgSpielerNext) {
            var imgSpieler = $('img#' + this.controlPanelIdImgSpieler);
            imgSpieler.attr("src", 'pic/' + this.spielerImage[this.currentSpieler]);
            // the next one
            var nextSpieler = this.currentSpieler + 1;
            if (nextSpieler == this.spielers.length) nextSpieler = 0;
            imgSpieler = $('img#' + this.controlPanelIdImgSpielerNext);
            imgSpieler.attr("src", 'pic/' + this.spielerImage[nextSpieler]);
        }

        // resources
        if (this.controlPanelIdLabelPeople && this.controlPanelIdLabelResource) {
            $('span#' + this.controlPanelIdLabelPeople).text(this.spielers[this.currentSpieler].people);
            $('span#' + this.controlPanelIdLabelResource).text(this.spielers[this.currentSpieler].resource);
        }
    }

    // select cell to make turn
    this.processSpielerCellClick = function () {
        // turn could be deprecated
        this.spielerSelectedCell = new Point(-1, -1);

        var e = window.event;
        var srcEl = $(e.srcElement ? e.srcElement : e.target);

        // check turn - is it allowed?
        var isAllowed = this.checkTurn(srcEl);

        // deselect other cells
        this.removeSelectionOnTiles();

        // highlight cell
        var opacity = 0.85;
        if (isAllowed) {
            var coords = this.getImgTileXY(srcEl);
            // the cell is selected
            this.spielerSelectedCell = new Point(coords.x, coords.y);
            opacity = 0.5; //srcEl.parent().css('background-color', 'white');
        }
        srcEl.css('opacity', opacity);
    }

    // draw all cells as not selected
    this.removeSelectionOnTiles = function () {
        var canvas = $('div#boardCanvas');
        canvas.children().css('opacity', '');
    }

    // get cell coords (cell, selected by user)
    this.getCellSelectedBySpieler = function (img) {
        // get target cell coords
        if (!img) {
            var canvas = $('div#boardCanvas');
            img = canvas.children("img[style*='opacity']")[0];
        }
        return this.getImgTileXY(img);
    }

    // choose cell type from palette
    this.choosePaletteItem = function (itemIndex) {
        var e = window.event;
        var srcEl = e.srcElement ? e.srcElement : e.target;
        this.currentPaletteItemIndex = itemIndex;

        $(srcEl).parent().children('img').each(function (i) {
            $(srcEl).parent().children(i).css("border", "0px");
        });

        $(srcEl).css("border", "solid black 2px");
    }

    // change a cell on the field
    this.changeFieldItem = function () {
        if (this.currentPaletteItemIndex < 0) return;
        var e = window.event;
        var srcEl = $(e.srcElement ? e.srcElement : e.target);
        var coords = this.getImgTileXY(srcEl);
        this.cells[coords.y][coords.x] = this.currentPaletteItemIndex;
        this.drawBoard();
    }

    // make N spielers
    this.initSpielers = function (spielerCount) {

        // find Start - Finish cell
        for (var row = 0; row < this.rows; row++)
            for (var col = 0; col < this.cols; col++) {
                if (this.cells[row][col] == this.TileStart) {
                    this.cellStartX = col;
                    this.cellStartY = row;
                }
                else if (this.cells[row][col] == this.TileFinish) {
                    this.cellEndX = col;
                    this.cellEndY = row;
                }
            }

        // spielers...
        this.spielers = new Array(spielerCount);
        for (var i = 0; i < spielerCount; i++) {
            this.spielers[i] = new Spieler(i);
            // spieler coords
            this.spielers[i].x = this.cellStartX;
            this.spielers[i].y = this.cellStartY;
        }

        this.currentSpieler = 0;
        this.spielMode = this.SpielModeSpiel;
    }

    // get x-y (col-row) for an img (tile)
    this.getImgTileXY = function (img) {
        // get img order amongst other images
        var childIndex = img.index();
        // get X-Y from index
        var y = Math.floor(childIndex / this.cols);
        var x = childIndex - this.cols * y;
        return new Point(x, y);
    }

    // [i][j] => (x, y)
    this.getXYByRowCol = function (row, col) {
        var xCoord = this.tileSz * col;
        var yCoord = (this.tileSz - this.tileSz4) * row;
        var rowEven = (row % 2 == 0);
        if (!rowEven)
            xCoord += this.tileSz2;
        return new Point(xCoord + this.boardPadLeft, yCoord + this.boardPadTop);
    }

    // can spieler step on the cell? isn't he freezed? ...
    this.checkTurn = function (img) {
        var coords = this.getCellSelectedBySpieler(img);
        // check if spieler can step on this field
        return this.rules.checkSpielerCanStepOnCell(coords);
    }

    // spieler has realy made his turn
    this.spielerConfirmedTurn = function () {
        if (!this.spielerSelectedCell || this.spielerSelectedCell.x < 0) return false;
        // make turn according the Rules
        this.rules.moveSpielerOnCell(this.spielerSelectedCell, false);
    }

    // give turn to the next spieler
    this.switchNextSpieler = function () {
        this.currentSpieler = this.currentSpieler + 1;
        if (this.currentSpieler == this.spielers.length)
            this.currentSpieler = 0;
    }
}