using System;
using System.Windows.Forms;

namespace waxPrecipitationFinalFormat
{
    public partial class FirstPage : Form
    {
        public FirstPage()
        {
            InitializeComponent();

            //The initial window cannot be Maximize/Minimize
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void ButtonMultiComponent_Click(object sender, EventArgs e)
        {
            //Multi-component system window open
            MultiComp CdF = new MultiComp();
            CdF.Show();
        }

        private void ButtonBinary_Click(object sender, EventArgs e)
        {
            //Binary system window open
            BinarySolute CF = new BinarySolute();
            CF.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void FirstPage_Load(object sender, EventArgs e)
        {

        }

        private void licensingStatementToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }
}
