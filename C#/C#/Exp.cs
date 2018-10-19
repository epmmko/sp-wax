using System;
using System.Windows.Forms;
using System.IO;

namespace waxPrecipitationFinalFormat
{
    /*In this class, user inserts exp data to a table. 
    Then the data will be exported to textfile for future C++ code use */
    public partial class Exp : Form
    {
        public Exp(MultiComp InputFom1)
        {
            InitializeComponent();
        }

        /* Apply button: in this method, Exp data is exported to text file 
        for C++ file future use */
        private void ButtonApply_1_Click(object sender, EventArgs e)
        {
            //Current directory is called
            string CurrentDirectory = Directory.GetCurrentDirectory();

            //Number of data points is captured
            int numBofRows = 0;
            Int32.TryParse(this.GridViewExp.Rows.Count.ToString(), 
                out numBofRows);
            numBofRows = numBofRows - 1;

            //A new experimental data will be exported into textfile
            File.WriteAllText(CurrentDirectory+"\\ExpData.txt", String.Empty);
            string Dir = CurrentDirectory + "\\ExpData.txt";

            using (FileStream fW = new FileStream(Dir, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    //In the first row, number of data points are inserted
                    WR.WriteLine(numBofRows-1);

                    //Then temperature and precipitation points are exported
                    for (int i = 0; i < numBofRows-1; i++)
                    {
                        WR.WriteLine(GridViewExp.Rows[i].Cells[0].
                            Value.ToString()+ "\t" + GridViewExp.Rows[i]
                            .Cells[1].Value.ToString());
                    }
                }
            }
        }

        //Exits from this widnow
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //User gets to open the saved file of exp data
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string F;
            OpenFileDialog Dial = new OpenFileDialog();
            Dial.Filter = "BIN|*.bin";

            if (Dial.ShowDialog() == DialogResult.OK)
            {
                F = Dial.FileName;
            }

            else
            {
                return;
            }

            GridViewExp.Rows.Clear();

            using (BinaryReader bw = new BinaryReader(File.Open(F, FileMode.Open)))
            {
                int A = bw.ReadInt32();
                int  B= bw.ReadInt32();

                for (int i = 0; i < B; ++i)
                {
                    GridViewExp.Rows.Add();
                    for (int j = 0; j < A; ++j)
                    {
                        if (bw.ReadBoolean())
                        {
                            GridViewExp.Rows[i].Cells[j].Value = bw.ReadString();
                        }

                        else bw.ReadBoolean();
                    }
                }
            }
        }

        //User gets to save the experimental data after inserted in the table
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file;
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "BIN|*.bin";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                file = ofd.FileName;
            }

            else
            {
                return;
            }

            using (BinaryWriter bw = new BinaryWriter(File.Open
                (file, FileMode.Create)))
            {
                bw.Write(GridViewExp.Columns.Count);
                bw.Write(GridViewExp.Rows.Count);

                foreach (DataGridViewRow dgvR in GridViewExp.Rows)
                {
                    for (int j = 0; j < GridViewExp.Columns.Count; ++j)
                    {
                        object val = dgvR.Cells[j].Value;
                        if (val == null)
                        {
                            bw.Write(false);
                            bw.Write(false);
                        }

                        else
                        {
                            bw.Write(true);
                            bw.Write(val.ToString());
                        }
                    }
                }
            }
        }

        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
