using System;
using System.Windows.Forms;
using System.IO;

namespace waxPrecipitationFinalFormat
{
    /*In this class, the experimental binary data will be inserted by user
    and data will be exported to textfile for C++ exefile*/
    public partial class ExpBinaryData : Form
    {
        public ExpBinaryData()
        {
            InitializeComponent();
        }

        //This buttons, export the inserted exp values to textfile
        private void ButtonApply_Click(object sender, EventArgs e)
        {
            //Current directory is called
            string CurrentDirectory = Directory.GetCurrentDirectory();

            //Number of exp data points are captured
            int numBofRows = 0;
            Int32.TryParse(this.GridViewBinaryExp.Rows.Count.ToString(), out numBofRows);
            numBofRows = numBofRows - 1;

            //Data will be exported to texfile
            File.WriteAllText(CurrentDirectory+"\\ExpBinaryData.txt", String.Empty);
            string path = CurrentDirectory + "\\ExpBinaryData.txt";
            using (FileStream fW = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    //The first row contains the number of data points
                    WR.WriteLine(numBofRows - 1);
                    
                    for (int i = 0; i < numBofRows - 1; i++)
                    {
                        WR.WriteLine(GridViewBinaryExp.Rows[i].Cells[0].Value.ToString() + "\t" + GridViewBinaryExp.Rows[i].Cells[1].Value.ToString());
                    }
                }
            }
        }

        //By clicking this button, the window will be closed
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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

            GridViewBinaryExp.Rows.Clear();

            using (BinaryReader bw = new BinaryReader(File.Open(F, FileMode.Open)))
            {
                int A = bw.ReadInt32();
                int B = bw.ReadInt32();

                for (int i = 0; i < B; ++i)
                {
                    GridViewBinaryExp.Rows.Add();
                    for (int j = 0; j < A; ++j)
                    {
                        if (bw.ReadBoolean())
                        {
                            GridViewBinaryExp.Rows[i].Cells[j].Value = bw.ReadString();
                        }

                        else bw.ReadBoolean();
                    }
                }
            }
        }

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
                bw.Write(GridViewBinaryExp.Columns.Count);
                bw.Write(GridViewBinaryExp.Rows.Count);

                foreach (DataGridViewRow dgvR in GridViewBinaryExp.Rows)
                {
                    for (int j = 0; j < GridViewBinaryExp.Columns.Count; ++j)
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
