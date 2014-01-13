namespace HaseUndIgel.BL
{
    /// <summary>
    /// все ходы записаны
    /// </summary>
    public class SpielerTurn
    {
        public int SpielerIndex { get; set; }

        public int TokenIndex { get; set; }

        public int OldCell { get; set; }

        public int TargetCell { get; set; }

        public int NewCarrots { get; set; }

        public int NewCabbages { get; set; }
        
        public int DeltaCarrots { get; set; }
    }
}
