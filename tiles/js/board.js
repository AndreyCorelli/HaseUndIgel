var board;

function Board() {
    // spielers
    this.spielers = new Array();

    // variables
    this.currentPaletteItemIndex = -1;
    this.boardCodeContainer = '';

    // constants
    this.rows = 12;
    this.cols = 10;
    this.tileSz = 48;
    this.tileSz2 = 24;
    this.tileSz4 = 12;

    // constants - tile types
    this.TileGrass = 0;
    this.TileSand = 1;
    this.TileSniper = 2;
    this.TileShelter = 3;
    this.TileStart = 4;
    this.TileFinish = 5;

    this.tileImage = {};
    this.tileImage[this.TileGrass] = 'tile_small_grass.png';
    this.tileImage[this.TileSand] = 'tile_small_sand.png';
    this.tileImage[this.TileSniper] = 'tile_small_sniper.png';
    this.tileImage[this.TileShelter] = 'tile_small_shelter.png';
    this.tileImage[this.TileStart] = 'tile_small_start.png';
    this.tileImage[this.TileFinish] = 'tile_small_finish.png';

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

    // draw board as is
    this.drawBoard = function () {
        var canvas = $('div#boardCanvas');
        var strInnerHtml = '';

        for (var row = 0; row < this.rows; row++)
            for (var col = 0; col < this.cols; col++) {
                // coords
                var xCoord = this.tileSz * col;
                var yCoord = (this.tileSz - this.tileSz4) * row;
                var rowEven = (row % 2 == 0);
                if (!rowEven)
                    xCoord += this.tileSz2;

                // img file
                var tileIndex = this.cells[row][col];
                var tileImgStr = this.tileImage[tileIndex];

                // img tag itself
                var strStyle = 'style="left: ' + xCoord + 'px; top: ' + yCoord + 'px"';
                var strTag = '<img src="pic/' + tileImgStr + '" class="tile" onclick="board.changeFieldItem()"' + strStyle + '/>\n';
                strInnerHtml = strInnerHtml + strTag;
            }
        canvas.html(strInnerHtml);

        if (this.boardCodeContainer)
            this.drawBoardSrc();
    };

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

        // get img order amongst other images
        var childIndex = srcEl.index();
        // get X-Y from index
        var y = Math.floor(childIndex / this.cols);
        var x = childIndex - this.cols * y;
        this.cells[y][x] = this.currentPaletteItemIndex;
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
    }
}