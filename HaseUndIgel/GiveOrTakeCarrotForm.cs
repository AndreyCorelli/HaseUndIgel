using System;
using System.Windows.Forms;

namespace HaseUndIgel
{
    public partial class GiveOrTakeCarrotForm : Form
    {
        public bool ShouldGave { get; private set; }

        public GiveOrTakeCarrotForm()
        {
            InitializeComponent();
        }

        private void BtnTakeClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void BtnGaveClick(object sender, EventArgs e)
        {
            ShouldGave = true;
            DialogResult = DialogResult.OK;
        }
    }
}
