namespace HaseUndIgel.BL
{
    public class Spieler
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsComputer { get; set; }

        public int CarrotsSpare { get; set; }

        public int CabbageSpare { get; set; }

        #region Hare effects
        /// <summary>
        /// пропускает след. ход
        /// </summary>
        public bool Freezed { get; set; }

        /// <summary>
        /// вернет морковок после хода
        /// </summary>
        public int CarrotsGranted { get; set; }

        /// <summary>
        /// отдает капусту в след. ход
        /// </summary>
        public bool GiveCabbage { get; set; }
        #endregion     
   
        public Spieler MakeShallowCopy()
        {
            return new Spieler
                {
                    Id = Id,
                    CarrotsSpare = CarrotsSpare,
                    CabbageSpare = CabbageSpare,
                    Freezed = Freezed,
                    CarrotsGranted = CarrotsGranted,
                    GiveCabbage = GiveCabbage
                };
        }
    }
}
