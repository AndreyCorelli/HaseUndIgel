using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HaseUndIgel.Util;

namespace HaseUndIgel
{
    public partial class NewSpielForm : Form
    {
        public int Humans { get; private set; }

        public int Computers { get; private set; }

        public NewSpielForm()
        {
            InitializeComponent();
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            var errors = new List<string>();

            if (tbComputers.Text.Trim().ToIntSafe() == null ||
                tbPeople.Text.Trim().ToIntSafe() == null)
                errors.Add("Please, provide Numbers for Human and Computer players");
            else
            {
                var hums = tbPeople.Text.Trim().ToIntSafe();
                var comps = tbComputers.Text.Trim().ToIntSafe() ?? 0;
                if (hums <= 0)
                    errors.Add("Where should be at least One Human player");
                if (comps < 0 || comps < 0)
                    errors.Add("All numbers must be Positive");
                if (hums + comps > 6)
                    errors.Add("6 players maximum");
            }

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors), "Input is incorrect",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            Humans = tbPeople.Text.Trim().ToIntSafe() ?? 1;
            if (Humans < 1) Humans = 1;

            Computers = tbComputers.Text.Trim().ToIntSafe() ?? 0;
            if (Computers < 0) Computers = 0;

            DialogResult = DialogResult.OK;            
        }
    }
}
