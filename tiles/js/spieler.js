

function Spieler(index) {
    // variables
    this.name = 'spieler';
    this.index = index;
    this.people = 6;
    this.resource = 60;
    this.x = -1;
    this.y = -1;
    // current step in a turn
    this.curStep = 1;
}