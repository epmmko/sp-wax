using System;
using System.Windows.Forms;
using System.IO;

namespace waxPrecipitationFinalFormat
{
    //In this class, molar fraction values of solute
    public partial class SoluteFractionValues : Form
    {
        public SoluteFractionValues()
        {
            InitializeComponent();
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            //Current directory is called
            string CurrentDirectory=Directory.GetCurrentDirectory();

            /*The Solute fraction values are exported in textfile for 
            C++ exefile*/
            int numBofRows = 0;
            Int32.TryParse(this.GridViewSolute.Rows.Count.ToString(), out numBofRows);
            numBofRows = numBofRows - 1;
            for (int i=0;i< numBofRows;i++)
            {
               if( GridViewSolute.Rows[i].Cells[0].Value == null)
                {
                    numBofRows = i;
                    break;
                }
            }
           
            File.WriteAllText(CurrentDirectory+"\\SoluteFractionFile.txt", String.Empty);

            string Dir = CurrentDirectory + "\\SoluteFractionFile.txt";
            using (FileStream fW = new FileStream(Dir, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    for (int i = 0; i < numBofRows; i++)
                    {
                        //if(GridViewSolute.Rows[i].Cells[0].Value!=null)
                       // { 
                        WR.WriteLine(GridViewSolute.Rows[i].Cells[0].Value.ToString());

                            //In the last row, number "1" is put.
                            if (i == numBofRows-1)
                            {
                                WR.WriteLine("1");
                            }
                        //}
                    }
                }
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //User can upload Solute fraction values from prevouly saved .bin file
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BIN|*.bin";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                file = ofd.FileName;
            }
            else
            {
                return;
            }
            GridViewSolute.Rows.Clear();

            using (BinaryReader bw = new BinaryReader(File.Open(
                file, FileMode.Open)))
            {
                int A = bw.ReadInt32();
                int B = bw.ReadInt32();
                for (int i = 0; i < B; ++i)
                {
                    GridViewSolute.Rows.Add();
                    for (int j = 0; j < A; ++j)
                    {
                        if (bw.ReadBoolean())
                        {
                            GridViewSolute.Rows[i].Cells[j].Value =
                                bw.ReadString();
                        }
                        else bw.ReadBoolean();
                    }
                }
            }
        }

        //User can save the solute fraction data fro future use
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
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
            using (BinaryWriter bw = new BinaryWriter(File.Open(file,
                FileMode.Create)))
            {
                bw.Write(GridViewSolute.Columns.Count);
                bw.Write(GridViewSolute.Rows.Count);
                foreach (DataGridViewRow dgvR in GridViewSolute.Rows)
                {
                    for (int j = 0; j < GridViewSolute.Columns.Count; ++j)
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
    }
}
