using System;
using System.Windows.Forms;

namespace HaseUndIgel
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void NewToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new NewSpielForm();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            
            boardControl.Initialize(dlg.Humans + dlg.Computers, dlg.Computers);
            boardControl.Invalidate();
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (boardControl.Board == null) return;
            new StatisticsForm(boardControl.Board).ShowDialog();
        }

        private void turnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (boardControl.Board == null) return;
            new TurnsForm(boardControl.Board).ShowDialog();
        }
    }
}
