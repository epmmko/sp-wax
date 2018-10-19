using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace waxPrecipitationFinalFormat
{
    public partial class BinarySolute : Form
    {
        public BinarySolute()
        {
            InitializeComponent();

            //This wondow does not Maximize/Minimize
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        //New window pops up to insert mole fraction of solute
        private void ButtonSoluteMole_Click_1(object sender, EventArgs e)
        {
            SoluteFractionValues CdF = new SoluteFractionValues();
            CdF.Show();
        }

        //This run botton saves the input data to textfiles and run the C++ exefile 
        private void ButtonRun_Click_1(object sender, EventArgs e)
        {
            //Current directory is called
            string CurrentDirectory = Directory.GetCurrentDirectory();

            //Input information of binary system is written to textfile
            File.WriteAllText(CurrentDirectory+"\\BinaryInfo.txt", String.Empty);
            string Dir = CurrentDirectory + "\\BinaryInfo.txt";

            using (FileStream fW = new FileStream(Dir, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    WR.WriteLine(TextBoxSolvent.Text);
                    WR.WriteLine(TextBoxSolute.Text);
                }
            }

            //Project4.exe is executed which is for binary system C++ file
            Process.Start(CurrentDirectory + "\\SPWAXBinary.exe");
        }

        private void ButtonPlot_Click_1(object sender, EventArgs e)
        {
            //Current directory is called 
            string CurrentDirectory = Directory.GetCurrentDirectory();

            Chart Chart;
            chart1.Series.Clear();

            /*In order to plot multiple times, series name should be different each time
              that is why random numbers are generated to name the series*/
            Random Rnd = new Random();
            int Randd = Rnd.Next(1, 1000);
            string Param = "Series" + Randd;

            //Plotting
            chart1.Series.Add(Param);
            Chart = new Chart();
            ChartArea chartArea = new ChartArea("chart1");
            Chart.ChartAreas.Add(chartArea);
            Series series = new Series();

            // Solute fraction values are read from textfiles
            string[] LinesYRaw = System.IO.File.ReadAllLines(CurrentDirectory+"\\SoluteFractionFile.txt");
            string[] LinesXRaw = System.IO.File.ReadAllLines(CurrentDirectory + "\\OutPutBinary.txt");

            //Declaring X and Y arrays for plotting 
            int NumData = LinesXRaw.Length;
            double[] X = new double[NumData];
            double[] Y = new double[NumData + 1];

            //X and Y values are assigned
            for (int h = 0; h < NumData; h++)
            {
                X[h] = Convert.ToDouble(LinesXRaw[h]);
                Y[h] = Convert.ToDouble(LinesYRaw[h]);
            }
            for (int i = 0; i < NumData; i++)
            {
                chart1.Series[Param].Points.AddXY(1000.0 / X[i], Y[i]);
            }

            //Plotting Simulation results
            chart1.ChartAreas[0].AxisY.IsLogarithmic = true;

            chart1.Series[Param].Color = Color.BlueViolet;
            
            chart1.Series[Param].ChartType = SeriesChartType.FastLine;

            chart1.Series[Param].BorderWidth = 8;

            chart1.Series[Param].LegendText = "Simulation Result";

            chart1.ChartAreas[0].AxisX.LabelStyle.Font = 
                new System.Drawing.Font("Arial", 11F);

            chart1.ChartAreas[0].AxisY.LabelStyle.Font = 
                new System.Drawing.Font("Arial", 11F);

            chart1.ChartAreas[0].AxisX.Title = "1000/T, [1/K]";

            chart1.ChartAreas[0].AxisX.TitleFont = new 
                System.Drawing.Font("Arial", 12F, FontStyle.Bold);

            chart1.ChartAreas[0].AxisY.Title = "Solute mole Fraction, [-]";

            chart1.ChartAreas[0].AxisY.TitleFont = 
                new System.Drawing.Font("Arial", 12F, FontStyle.Bold);

            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";

            chart1.ChartAreas[0].AxisX.Minimum = 3;

            chart1.ChartAreas[0].AxisX.Maximum = 4;

            //For plotting Experimental binary data, the previously random 
            chart1.Series.Add(Param+"1");

            int NumTem = 0;

            //Exp data are read from textfile
            string[] Exp1 = new string[2];
            string[,] Exp2 = new string[2, 100];
            string[] Explines = System.IO.File.ReadAllLines(CurrentDirectory + "\\ExpBinaryData.txt");
            Int32.TryParse(Explines[0], out NumTem);
            double[] Xs = new double[NumTem];
            double[] Ys = new double[NumTem];

            //The exp data are assigned to arrays
            for (int i = 1; i < NumTem + 1; i++)
            {
                Exp1 = Explines[i].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                Exp2[0, i] = Exp1[0];
                Exp2[1, i] = Exp1[1];
            }

            //X and Y are assigned
            for (int i = 1; i < NumTem + 1; i++)
            {
                Xs[i - 1] = Convert.ToDouble(Exp2[0, i]);
                Ys[i - 1] = Convert.ToDouble(Exp2[1, i]);

                chart1.Series[Param + "1"].Points.AddXY(1000.0 / Xs[i - 1], Ys[i - 1]);
            }

            chart1.Series[Param + "1"].LegendText = "Experimental Data";

            chart1.Series[Param + "1"].ChartType = SeriesChartType.FastPoint;

            chart1.Series[Param + "1"].Color = Color.LightSalmon;

            chart1.Series[Param + "1"].MarkerSize = 10;
        }

        //New window pops up to insert experimental binary data
        private void ButtonExpInsert_Click_1(object sender, EventArgs e)
        {
           ExpBinaryData CF = new ExpBinaryData();
            CF.Show();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
             string filee;
            SaveFileDialog ofdd = new SaveFileDialog();
            ofdd.Filter = "BIN|*.bin";
            if (ofdd.ShowDialog() == DialogResult.OK)
            {
                filee = ofdd.FileName;
            }

            else
            {
                return;
            }

            //Input data are saved to Val[]
            string[] Val = new string[2];
            using (BinaryWriter bw = new BinaryWriter(File.Open(filee,
                FileMode.Create))) 
            {
                Val[0]= TextBoxSolvent.Text;
                Val[1] = TextBoxSolute.Text;

                //The Va[] array is saved
                for (int j = 0; j < 2; ++j)
                {
                    object val = Val[j];
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

            //Val array could store the input data which have already been saved
            string[] Val = new string[2];
            using (BinaryReader bw = new BinaryReader(File.Open(
                file, FileMode.Open)))
            {
                String S;
                for (int j = 0; j < 2; ++j)
                {
                    if (bw.ReadBoolean())
                    {
                        Val[j] = bw.ReadString();
                    }
                    else bw.ReadBoolean();
                }

                //Correct input values are assigned to textbox(es)
                TextBoxSolvent.Text = Val[0];
                TextBoxSolute.Text = Val[1];
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //This button saves the chart
        private void ButtonSaveBinary_Click(object sender, EventArgs e)
        {
            //Calling current directory
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string Dir = CurrentDirectory + "\\BinarySystem.png";
            this.chart1.SaveImage(Dir, ChartImageFormat.Png);
        }

        private void usersManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("User's manual.pdf");
        }

        private void developersManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("Developer's manual.pdf");
        }

        private void licensingStatementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("Licensing Statement.pdf");
        }

        private void contactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Authors CF = new Authors();
            CF.Show();
        }
    }
}
