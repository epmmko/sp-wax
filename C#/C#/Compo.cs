using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace waxPrecipitationFinalFormat
{
    /*In this class, user inserts normalized molar composition of n-alkane
    system. Data can alos be plotted, saved and loaded. */
    public partial class Compo : Form
    {
        /*This public variable is used to access number of 
        components from MultiComp class*/
        public string msg;

        public Compo(MultiComp InputFom1)
        {
            InitializeComponent();

            /*public variable K contains the number of n-alkane 
            components from MultiComp class*/
            msg = InputFom1.k;
            int numBofRows = 0;
            Int32.TryParse(msg, out numBofRows);

            /*the first collumn of data grid table will be filled with
            carbon number components based on the inserted number of components*/
            for (int i = 0; i < numBofRows; i++)
            {
                int Hl = i + 1;
                DataGridViewRow row = (DataGridViewRow)GridViewCompo.
                    Rows[i].Clone();
                row.Cells[0].Value = "C" + Hl;
                GridViewCompo.Rows.Add(row);
            }
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            //Current directory is called
            string CurrentDirectory = Directory.GetCurrentDirectory();

            //msg constains the number of components
            int numBofRows = 0;
            Int32.TryParse(msg, out numBofRows);

            //The composition data is exported to textfile 
            File.WriteAllText(CurrentDirectory+"\\Data.txt", String.Empty);
            string Dir = CurrentDirectory + "\\Data.txt";
            using (FileStream fW = new FileStream(Dir, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    for (int i = 0; i < numBofRows; i++)
                    {
                        WR.WriteLine(GridViewCompo.Rows[i].Cells[1].
                            Value.ToString());
                    }
                }
            }
        }

        //Window closes
        private void ButtonOk_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        //User can open a saved file
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
            GridViewCompo.Rows.Clear();

            using (BinaryReader bw = new BinaryReader(File.Open(
                file, FileMode.Open)))
            {
                int A = bw.ReadInt32();
                int B = bw.ReadInt32();
                for (int i = 0; i < B; ++i)
                {
                    GridViewCompo.Rows.Add();
                    for (int j = 0; j < A; ++j)
                    {
                        if (bw.ReadBoolean())
                        {
                            GridViewCompo.Rows[i].Cells[j].Value = 
                                bw.ReadString();
                        }
                        else bw.ReadBoolean();
                    }
                }
            }
        }

        //user gets to save the inserted compsosition for future use
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
            using (BinaryWriter bw = new BinaryWriter(File.Open(file,
                FileMode.Create)))
            {
                bw.Write(GridViewCompo.Columns.Count);
                bw.Write(GridViewCompo.Rows.Count);
                foreach (DataGridViewRow dgvR in GridViewCompo.Rows)
                {
                    for (int j = 0; j < GridViewCompo.Columns.Count; ++j)
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

        //In this method, one can plot the data
        Chart chart;
        private void ButtonPlot_Click(object sender, EventArgs e)
        {
            //Plotting the composition data
            chart = new Chart();
            ChartArea chartArea = new ChartArea("chart1");
            chart.ChartAreas.Add(chartArea);
            Series series = new Series();
            chart.Series.Add(series);

            int numBofRows=0;
            Int32.TryParse(msg, out numBofRows);
            double[] values = new double[numBofRows+1];

            //data are read from the table for plotting
            for (int h=0;h< numBofRows; h++)
            {
                DataGridViewRow row = (DataGridViewRow)GridViewCompo
                    .Rows[h].Clone();

                values[h + 1] = GridViewCompo.Rows[h].Cells[1].Value ==
                    DBNull.Value ? 0D : Convert.ToDouble(GridViewCompo.
                    Rows[h].Cells[1].Value);

                GridViewCompo.Rows.Add(row);
            }

            //X and Y values are assigned
            for (int i = 0; i < numBofRows; i++)
            {
                chart1.Series["Series1"].Points.AddXY(i, values[i]);
            }

            //Data are plotted
            chart1.Series["Series1"].ChartType = SeriesChartType.FastPoint;

            chart1.Series["Series1"].Color = Color.Red;

            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new 
                System.Drawing.Font("Arial", 11F);

            chart1.ChartAreas[0].AxisY.LabelStyle.Font = new 
                System.Drawing.Font("Arial", 11F);

            chart1.ChartAreas[0].AxisX.Title = "Carbon Number";

            chart1.ChartAreas[0].AxisX.TitleFont= new 
                System.Drawing.Font("Arial", 13F, FontStyle.Bold);

            chart1.ChartAreas[0].AxisY.Title = 
                "Nomralized n-alkane mollar composition, [-]";

            chart1.ChartAreas[0].AxisY.TitleFont = new 
                System.Drawing.Font("Arial", 13F, FontStyle.Bold);

            chart1.ChartAreas[0].AxisX.Interval = 5;

            chart1.ChartAreas[0].AxisX.Maximum = numBofRows;

            chart1.ChartAreas[0].AxisX.Minimum = 0;

            chart1.ChartAreas[0].AxisY.Interval =0.05;

            chart.Legends.Add("num");

            chart1.Series[0].LegendText = "N-alkane composition";
        }

        private void GridViewCompo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
