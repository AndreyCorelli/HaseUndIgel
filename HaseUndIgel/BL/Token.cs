using System.Drawing;

namespace HaseUndIgel.BL
{
    public class Token
    {
        public static Color[] TokenColor = new Color[]
            {
                Color.Red, Color.Green, Color.Blue, Color.DarkKhaki, Color.DarkOrange, Color.Gray
            };

        public int ColorIndex { get; set; }

        public int Position { get; set; }
    }
}
