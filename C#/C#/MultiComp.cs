using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;


namespace waxPrecipitationFinalFormat
{
    
    public partial class MultiComp : Form
    {
        // This public variable will be used in other classes
        public string k;
        public MultiComp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*Three default values are choosen for
            temperature step, and accuracy limit*/
            TextBoxTempStep.Text = "0.5";
            TextBoxMaxAccuracy.Text = "0.0001";
            TextBoxWATaccuracy.Text = "0.001";

            /*Initially three radioButton options are available for the user 
            to choose from. Everything else is freezed*/
            flowLayoutPanel6.Enabled = false;
            TrackBarCompo.Enabled = false;
            FlowLayoutPanelPrecWAT.Enabled = false;
            FlowLayoutPanelInputs.Enabled = false;
            FlowLayoutPanelOneTemp.Enabled = false;
            FlowLayoutPanelCCN.Enabled = false;
            BottonRun.Enabled = false;
            ButtonPlot.Enabled = false;

            //Making sure that char location is clear
            ChartPrec.Series.Clear();

        }

        



        /*Normalized molar composition of the total fluid is inserted 
        through a new window (Compo)*/
        private void ButtonCompoInsert_click(object sender, EventArgs e)
        {
           // Number of components is assigned to the public parameter (K)
            k = TextBoxNumComp.Text;

            //Composition window will be open by this command
            Compo CF = new Compo(this);
            CF.Show();
        }

        /* When this botton is clicked, all variables are written to textfiles
           and those textfiles are read by the C++ file to generated different
           SLE calculations*/
        private void BottonRun_Click(object sender, EventArgs e)
        {
            /*Track bar in solid-phase composition is activated when,
            "Run-Simulation" is clicked*/

            //Calling current directory
            string CurrentDirectory = Directory.GetCurrentDirectory();

            /*General inputs are used in all three versions of 
            ulti -component system*/
            File.WriteAllText(CurrentDirectory+"\\GeneralInputs.txt",
                String.Empty);
            string Dir = CurrentDirectory+"\\GeneralInputs.txt";

            using (FileStream fW = new FileStream(Dir, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    WR.WriteLine(TextBoxWaxContent.Text);
                    WR.WriteLine(TextBoxAlpha.Text);
                    WR.WriteLine(TextBoxSmallestCNum.Text);
                    WR.WriteLine(TextBoxNumComp.Text);
                }
            }

            /*This radio button is for precipitation curve and WAT
             calculations*/
            if (RadioButtonPCAW.Checked)
            {
                // Guidance for user to be familier with this option
                label32.Text = "solid phase weight fraction=[mass of solid" +
                    " phase] / [mass of liquid phase and liquid phase]." +
                    ""+"\n"+"Solid phase only contains n-alkanes while liquid" +
                    " phase containts both n-alkane and non n-alkane " +
                    "components."+"\n"+"WAT= the temperature below which " +
                    "preciptiation occures, [°K]";
        
                /* This text is appears when "Run-Simulation" is clicked */
                label28.Text = "Temperature choice";

                //Current directory
                File.WriteAllText(CurrentDirectory+
                    "\\PrecipitatationCurveWAT.txt", String.Empty);

                //Inputs are written in the textfiles for further C++ code
                Dir = CurrentDirectory + "\\PrecipitatationCurveWAT.txt";

                     using (FileStream fw = new FileStream(Dir, 
                         FileMode.OpenOrCreate))
                    {
                            using (TextWriter WR = new StreamWriter(fw))
                        {
                        WR.WriteLine(TextBoxNumData.Text);
                        WR.WriteLine(TextBoxStartTemp.Text);
                        WR.WriteLine(TextBoxTempStep.Text);
                        WR.WriteLine(TextBoxMaxAccuracy.Text);
                        WR.WriteLine(TextBoxWATaccuracy.Text);
                        }
                    }
                 // .exe file is run for precipitation curve calculations
                Process.Start(CurrentDirectory + "\\SPWAXPrecipitation.exe");

            } //end of if (cBPCW.Checked)

            if (RadioButtonBOTSC.Checked)
            {
                //Solid-phase trancbar is disabled
                TrackBarCompo.Enabled = false;

                //Current directory is called
                File.WriteAllText(CurrentDirectory+"\\TempCase.txt", 
                    String.Empty);
                Dir = CurrentDirectory + "\\TempCase.txt";

                /*The desired temperature is written to textfile for C++ 
                code execution*/
                using (FileStream FW = new FileStream(Dir,
                    FileMode.OpenOrCreate))
                {
                    using (TextWriter WR = new StreamWriter(FW))
                    {
                        WR.WriteLine(TextBoxDisTemp.Text);
                    }
                }

                //.exe file is run for one temperature calculation 
                Process.Start(CurrentDirectory+"\\SPWAXOneTemperatureCase.exe");
            } 

            if(RadioButtonCCN.Checked)
            {
                //Solid-phase trancbar is disabled
                TrackBarCompo.Enabled = false;

                label32.Text = "By this option, relative concentration " +
                 " gradient of n-alkanes are calculated based on deposit" +
                 " surrounding temperatures (interface and wall)" +
                 "and CCN is determined. Please refer to developer's manual " +
                 "for more information.";

                //Current directory is called
                File.WriteAllText(CurrentDirectory+"\\TempCaseCCN.txt",
                    String.Empty);
                Dir = CurrentDirectory+"\\TempCaseCCN.txt";

                /*High and low temps are written to textfile for C++ 
                  code execution*/
                using (FileStream fW = 
                    new FileStream(Dir, FileMode.OpenOrCreate))
                {
                    using (TextWriter WR = new StreamWriter(fW))
                    {
                        WR.WriteLine(TextBoxHighTemp.Text);
                        WR.WriteLine(TextBoxLowTemp.Text);

                    }
                }

                //.exe file is run for CCN determination 
                Process.Start(CurrentDirectory+"\\SPWAXCCN.exe");
            }
        }

        Chart chart;

        /*Public variables to read several SLE characteristics for one
        temperature calculations*/
        public string[] Param = new string[2];
        public string[] ArrayLiqM = new string[100];
        public string[] ArrayLiqW = new string[100];
        public string[] ArraySolW = new string[100];
        public string[] ArraySolM = new string[100];
        public string[] ArrayCons = new string[100];
        public string[] ArrayDis = new string[100];

        /*By this method, SLE characteristics of one temperature calculations
          are assgined to arrays*/
        public void arrayRetMolarLiquid()
        {
            //Current directory is called
            string currentDirectory = Directory.GetCurrentDirectory();

            //Number of componnets is taken and saved to NumComp
            int NumComp;
            string Kp = TextBoxNumComp.Text;
            Int32.TryParse(Kp, out NumComp);

            //weight composition of liquid phase
            string[] LiqW_Lines = System.IO.File.ReadAllLines(currentDirectory
                +"//LiquidWComposition.txt");
            for (int i = 0; i < NumComp; i++)
            {
                Param = LiqW_Lines[i].Split(new string[] { "\t" },
                    StringSplitOptions.RemoveEmptyEntries);
                ArrayLiqW[i] = Param[1];
            }

            //molar composition of liquid phase
            string[] LiqM_Lines = System.IO.File.ReadAllLines(currentDirectory
                + "//LiquidMolarComposition.txt");
            for (int i = 0; i < NumComp; i++)
            {
                Param = LiqM_Lines[i].Split(new string[] { "\t" },
                    StringSplitOptions.RemoveEmptyEntries);
                ArrayLiqM[i] = Param[1];
            }

            //weight composition of solid phase
            string[] SolidW_Lines = System.IO.File.ReadAllLines(currentDirectory
                + "//SolidWCompositionOnetemp.txt");

            for (int i = 0; i < NumComp; i++)
            {
                Param = SolidW_Lines[i].Split(new string[] { "\t" },
                    StringSplitOptions.RemoveEmptyEntries);
                ArraySolW[i] = Param[1];
            }

            //molar composition of solid phase
            string[] SolidM_Lines = System.IO.File.ReadAllLines(currentDirectory
                + "//SolidMolarComposition.txt");

            for (int i = 0; i < NumComp; i++)
            {
                Param = SolidM_Lines[i].Split(new string[] { "\t" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                ArraySolM[i] = Param[1];
            }

            //local Concentration in n-alkane system
            string[] Cons_Lines = System.IO.File.ReadAllLines(currentDirectory
                + "//RelativeConcentration.txt");
            for (int i = 0; i < NumComp; i++)
            {
                Param = Cons_Lines[i].Split(new string[] { "\t" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                ArrayCons[i] = Param[1];
            }

            //Dissolved mass
            string[] Dis_Lines = System.IO.File.ReadAllLines(currentDirectory + 
                "//DissolvedMass.txt");
            for (int i = 0; i < NumComp; i++)
            {
                Param = Dis_Lines[i].Split(new string[] { "\t" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                ArrayDis[i] = Param[1];
            }
        }

        //This flag is used to plot the data correctly 
        public int Flag = 0;

        private void ButtonPlot_Click(object sender, EventArgs e)
        {
            double TempStart;
            double TempStep;
            int Param;
            string SeriesNum;

            //Current directory is called
            string CurrentDirectory = Directory.GetCurrentDirectory();

            /*If radio button of precipitation and WAT is chosen, the following
            commands are executed*/
            if (RadioButtonPCAW.Checked)
            {

                /* WAT is read from the textfile generated by C++ code*/
                string[] WATS = System.IO.File.ReadAllLines(CurrentDirectory
                    + "//WAT.txt");

                //This method returns data point number of precipitation curve
                ActualTempNum();

                double[] Val = new double[Convert.ToInt32(Actual_Num) + 1];

                //Starting temperature and temperature step are assigned
                TempStart = Convert.ToDouble(TextBoxStartTemp.Text);
                TempStep = Convert.ToDouble(TextBoxTempStep.Text);

                /*In order to plot multiple lines, the series name should be 
                  different, I used random numbers to name them to do the task*/
                Random Rnd = new Random();
                int Randd = Rnd.Next(1, 1000);
                SeriesNum = "Series" + Randd;

                //Precipitation Curve plotting
                ChartPrec.Series.Add(SeriesNum);
                chart = new Chart();
                ChartArea chartArea = new ChartArea("chart2");
                chart.ChartAreas.Add(chartArea);
                Series Series = new Series();
                chart.Series.Add(Series);

                //Precipitation values are read here and assinged to the array
                string[] lines = System.IO.File.ReadAllLines(CurrentDirectory +
                    "//Wax weight fraction.txt");

                //Converting the values from string to double
                for (int h = 0; h < Actual_Num; h++)
                {
                    Val[h] = Convert.ToDouble(lines[h]);
                }

                //X and Y values are given for plotting
                for (int i = 0; i < Actual_Num; i++)
                {
                    ChartPrec.Series[SeriesNum].Points.AddXY(TempStart
                        + i * TempStep, Val[i]);
                }
                //Precipitation curve is plotted
                ChartPrec.Series[SeriesNum].ChartType = SeriesChartType.FastLine;

                ChartPrec.ChartAreas[0].AxisX.LabelStyle.Font =
                    new System.Drawing.Font("Arial", 11F);

                ChartPrec.ChartAreas[0].AxisY.LabelStyle.Font =
                    new System.Drawing.Font("Arial", 11F);

                ChartPrec.ChartAreas[0].AxisX.Title = "Temperature, [K]";

                ChartPrec.ChartAreas[0].AxisX.TitleFont =
                    new System.Drawing.Font("Arial", 12F, FontStyle.Bold);

                ChartPrec.ChartAreas[0].AxisY.Title =
                    "Weight Fraction of" + "\n" + " Precipitated Wax, [-]";

                ChartPrec.ChartAreas[0].AxisY.TitleFont =
                    new System.Drawing.Font("Arial", 12F, FontStyle.Bold);

                ChartPrec.ChartAreas[0].AxisX.Maximum =
                    TempStart + Actual_Num * TempStep;

                ChartPrec.ChartAreas[0].AxisX.Minimum = TempStart;

                ChartPrec.Legends.Add(SeriesNum);

                //This flag is also used to name the different simulation runs
                ChartPrec.Series[SeriesNum].LegendText = "Simulation Results- Run" + Flag;

                //Wax Appearance Temperature reporting
                if (WATS[1] == "0")
                {
                    label20.Text = "WAT has not been reached, " +
                        "please choose different temperature settings";
                }

                else
                {
                    label20.Text = " WAT is: " + WATS[1] + " [°K]";
                }

                /*This if loop is made, so Experimental results wont be plotted
                  multiple times when PlotButton is clicked multiple times for 
                  different simulation cases*/
                 if (CheckBoxExpData.Checked)
                 {
                //if (Flag == 0)
                //{
                    string SeriesName = "Series" + Randd + 1;
                    ChartPrec.Series.Add(SeriesName);
                    string[] Exp1 = new string[2];
                    string[,] Exp2 = new string[2, 200];
                    int NumTem = 0;

                    /*Exp data will be read from table inserted by
                    the user and exported to the text file*/

                    /*Please note that the first line in the textfile is the 
                      number of data points which is stored at Explines[0]*/
                    string[] Explines = System.IO.File.ReadAllLines(CurrentDirectory
                        + "//ExpData.txt");
                    Int32.TryParse(Explines[0], out NumTem);

                    //X and Y  arrays for plotting
                    double[] Xs = new double[NumTem];
                    double[] Ys = new double[NumTem];

                    // The data are read from the textfile for plotting
                    for (int i = 1; i < NumTem + 1; i++)
                    {
                        Exp1 = Explines[i].Split(new string[] { "\t" },
                            StringSplitOptions.RemoveEmptyEntries);
                        Exp2[0, i] = Exp1[0];
                        Exp2[1, i] = Exp1[1];
                    }

                    //X and Y values are assgined for plotting
                    for (int i = 1; i < NumTem + 1; i++)
                    {
                        Xs[i - 1] = Convert.ToDouble(Exp2[0, i]);
                        Ys[i - 1] = Convert.ToDouble(Exp2[1, i]);

                        ChartPrec.Series[SeriesName].Points.AddXY(Xs[i - 1],
                            Ys[i - 1]);
                    }

                    ChartPrec.Series[SeriesName].ChartType =
                        SeriesChartType.FastPoint;

                    ChartPrec.Series[SeriesName].Color = Color.LightSalmon;

                    ChartPrec.Series[SeriesName].MarkerSize = 8;

                    ChartPrec.Legends.Add(SeriesName);

                    ChartPrec.Series[SeriesName].LegendText =
                        "Experimental Data";
                //}
            }
                
                //Flag parameter adds up for the desired task
                Flag = Flag + 1;
            }

            //One temperature case radio button is on
            if (RadioButtonBOTSC.Checked)
            {
                ChartOneTemp.Series.Clear();

                /*This varible will he assgined to based on the chosen item
                in the comboBox*/
                string Option;
                Option = outputComboBox.SelectedItem.ToString();

                /*solid weight fraction is read from the text fileat the given 
                 temp*/

                string[] WaxWeight = System.IO.File.ReadAllLines(CurrentDirectory +
                 "//OneTemp Wax weight fraction.txt");
                    label29.Text = "Solid wax weight fraction is: " + WaxWeight[0];

                //Based on the chosen item, correct result data are plotted
                if (Option == "Liquid phase mole-composition (n-alkane system)")
                {
                    //Comment section
                    label32.Text = "Y-axis (each bar) =[mole of ith n-alkane in" +
                        " liquid phase] / [total" +
                        " mole of n-alkanes in liquid phase]" + "\n" +
                        "Summation of all n-alkane compositions (bars) will be" +
                        " equal (Normalized)";
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = false;
                    /* When this method is called, the generated data from 
                    project2.exe file will be exported to TextFiles*/
                    arrayRetMolarLiquid();

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp];
                    double[] Ys = new double[NumComp];

                    //Liquid mole-composition plotting
                    ChartOneTemp.Series.Add("Series25");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart25");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);

                    //X and Y values
                    for (int i = 0; i < NumComp; i++)
                    {
                        Xs[i] = i + 1;
                        Ys[i] = Convert.ToDouble(ArrayLiqM[i]);

                        ChartOneTemp.Series["Series25"].Points.AddXY(Xs[i],
                            Ys[i]);
                    }

                    //Plotting Liquid mole-composition data 
                    ChartOneTemp.Series["Series25"].ChartType =
                        SeriesChartType.Column;

                    ChartOneTemp.Series["Series25"].Color = Color.BlueViolet;

                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title =
                        "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Liquid mole-composition" +"\n"+"(n-alkane system)";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisX.Maximum = NumComp; 

                    ChartOneTemp.ChartAreas[0].AxisX.Minimum = 5;

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 0.4; 

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0;

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param * 5 + 5;
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                }

                //Based on the chosen item, correct result data are plotted
                if (Option == "Solid phase mole-composition")
                {
                    //Comment section
                    label32.Text = "Y-axis (each bar) =[mole of ith n-alkane in" +
                        " solid phase] / [total" +
                        " mole of n-alkanes in solid phase]" + "\n" +
                        "Summation of all n-alkane compositions (bars) will be" +
                        " equal (Normalized)";

                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = false;

                    /* When this method is called, the generated data from 
                    project2.exe file will be exported to TextFiles*/
                    arrayRetMolarLiquid();

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp]; 
                    double[] Ys = new double[NumComp];

                    //Solid phase mole-composition plotting
                    ChartOneTemp.Series.Clear();
                    ChartOneTemp.Series.Add("Series45");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart45");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);

                    //X and Y values
                    for (int i = 0; i < NumComp; i++)
                    {
                        Xs[i] = i + 1;
                        Ys[i] = Convert.ToDouble(ArraySolM[i]);

                        ChartOneTemp.Series["Series45"].Points.AddXY(Xs[i],
                            Ys[i]);
                    }

                    //Plotting Solid phase mole-composition data 
                    ChartOneTemp.Series["Series45"].ChartType =
                        SeriesChartType.Column;

                    ChartOneTemp.Series["Series45"].Color =
                        Color.PaleVioletRed;

                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title = "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Solid mole-composition";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisX.Maximum = NumComp;

                    ChartOneTemp.ChartAreas[0].AxisX.Minimum = 5;

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 0.4;

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0;

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param * 5 + 5;
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                }

                //Based on the chosen item, correct result data are plotted
                if (Option == "Liquid phase weight-composition (n-alkane system)")
                {
                    //Comment section
                    label32.Text = "Y-axis (each bar) =[weight of ith n-alkane in" +
                        " liquid phase] / [total" +
                        " weight of n-alkanes in liquid phase]" + "\n" +
                        "Summation of all n-alkane compositions (bars) will be" +
                        " equal (Normalized)";
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = false;

                    /* When this method is called, the generated data from 
                       project2.exe file will be exported to TextFiles*/
                    arrayRetMolarLiquid();

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp];
                    double[] Ys = new double[NumComp];

                    //Liquid weight-composition plotting
                    ChartOneTemp.Series.Clear();
                    ChartOneTemp.Series.Add("Series75");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart75");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);

                    // X and Y value assignment
                    for (int i = 0; i < NumComp; i++)
                    {
                        Xs[i] = i + 1;
                        Ys[i] = Convert.ToDouble(ArrayLiqW[i]);
                        ChartOneTemp.Series["Series75"].Points.AddXY(Xs[i], Ys[i]);
                    }

                    //Plotting Liquid weight-composition data 
                    ChartOneTemp.Series["Series75"].ChartType = SeriesChartType.Column;

                    ChartOneTemp.Series["Series75"].Color = Color.ForestGreen;

                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title = "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Liquid weight-composition"+"\n"+"(n-alkane system)";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisX.Maximum = NumComp; 

                    ChartOneTemp.ChartAreas[0].AxisX.Minimum = 5;

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 0.4; 

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0;

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param * 5 + 5;
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                }

                //Based on the chosen item, correct result data are plotted
                if (Option == "Solid phase weight-composition")
                {
                     label32.Text = "Y-axis (each bar) =[weight of ith n-alkane" +
                        " in solid phase] / [total" +
                     " weight of n-alkanes in solid phase]" + "\n" +
                     "Summation of all n-alkane compositions (bars) will be" +
                     " equal (Normalized)";
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = false;

                    /* When this method is called, the generated data from 
                       project2.exe file will be exported to TextFiles*/
                    arrayRetMolarLiquid();

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp];
                    double[] Ys = new double[NumComp];

                    //Solid weight-compositionn plotting
                    ChartOneTemp.Series.Clear();
                    ChartOneTemp.Series.Add("Series85");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart85");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);

                    // X and Y value assignment
                    for (int i = 0; i < NumComp; i++)
                    {
                        Xs[i] = i + 1;
                        Ys[i] = Convert.ToDouble(ArraySolW[i]);
                        ChartOneTemp.Series["Series85"].Points.AddXY(Xs[i],
                            Ys[i]);
                    }

                    //Plotting Solid weight-compositionn data 
                    ChartOneTemp.Series["Series85"].ChartType =
                        SeriesChartType.Column;

                    ChartOneTemp.Series["Series85"].Color =
                        Color.Orange;

                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title =
                        "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F,
                        FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Solid weight-composition";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisX.Maximum = NumComp; 

                    ChartOneTemp.ChartAreas[0].AxisX.Minimum = 5;

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 0.4;

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0;

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param * 5 + 5;
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                }

                //Based on the chosen item, correct result data are plotted
                if (Option == "Dissolved mass of each component")
                {
                    label32.Text = "Y-axis= dissolved mass of ith n-alkane" +
                        " in liquid phase "+
                        "based on the assumption that the total mass of the" +
                        " system" +
                        " (paraffin and non-paraffinic components in liquid and" +
                        " in solid phases) is equal to 1 kg";
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = false;

                    /* When this method is called, the generated data from 
                       project2.exe file will be exported to TextFiles*/
                    arrayRetMolarLiquid();

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp];
                    double[] Ys = new double[NumComp];

                    //Dissolved mass inliquid phase plotting
                    ChartOneTemp.Series.Clear();
                    ChartOneTemp.Series.Add("Series95");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart95");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);

                    // X and Y value assignment
                    for (int i = 0; i < NumComp; i++)
                    {
                        Xs[i] = i + 1;
                        Ys[i] = Convert.ToDouble(ArrayDis[i]);

                        ChartOneTemp.Series["Series95"].Points.AddXY(Xs[i],
                            Ys[i]);
                    }

                    //Plotting Dissolved mass inliquid phase data 
                    ChartOneTemp.Series["Series95"].ChartType =
                        SeriesChartType.FastPoint;

                    ChartOneTemp.Series["Series95"].Color = Color.Black;

                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title =
                        "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Dissolved mass in liquid phase"+"\n"+"[kg]";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 0.02;

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param * 5 + 5;
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }
                }

                //Based on the chosen item, correct result data are plotted
                if (Option == "Relative concentration of each component" +
                    " (n-alkane system)")
                {
                    label32.Text = "Y-axis= [mass of ith n-alkane that" +
                        " is dissolved" +
                        " in liquid phase] / [volume of n-alkanes in" +
                        " liquid phase]";
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = false;

                    /* When this method is called, the generated data from 
                       project2.exe file will be exported to TextFiles*/
                    arrayRetMolarLiquid();

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp];
                    double[] Ys = new double[NumComp];


                    //Local concentration plotting
                    ChartOneTemp.Series.Clear();
                    ChartOneTemp.Series.Add("Series05");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart05");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);


                    // X and Y value assignment
                    for (int i = 0; i < NumComp; i++)
                        {
                            if (Convert.ToDouble(ArrayCons[i]) > 0)
                            {
                                Xs[i] = i + 1;
                                Ys[i] = Convert.ToDouble(ArrayCons[i]);
                                ChartOneTemp.Series["Series05"]
                                .Points.AddXY(Xs[i], Ys[i]);
                            }
                        }

                    //Plotting Local concentratione data 
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = true;

                    ChartOneTemp.Series["Series05"].ChartType =
                        SeriesChartType.FastPoint;

                    ChartOneTemp.Series["Series05"].Color =
                        Color.DarkOliveGreen;

                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title = "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Relative concentration" + "\n" + " n-alkane system" + "\n" + "[kg/m3]";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 100;

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0.00001;

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param * 5 + 5;
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }
                }

                if (Option == "Equilibrium Constants")
                {

                    label32.Text = "Y-axis= [mole fraction of ith carbon number" +
                        " in solid phase]/[mole fraction of ith carbon number" +
                        " in liquid phase]"+"\n"+"X_S/X_L";
                    /* When this method is called, the generated data from 
                       project2.exe file will be exported to TextFiles*/
                    string[] Equil_Const = System.IO.File.ReadAllLines(
                        CurrentDirectory + "//K_values.txt");

                    //Number of componnets are required for plotting
                    int NumComp;
                    Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                    // X and Y arrays
                    double[] Xs = new double[NumComp];
                    double[] Ys = new double[NumComp];

                    //Local concentration plotting
                    ChartOneTemp.Series.Clear();
                    ChartOneTemp.Series.Add("Series5");
                    chart = new Chart();
                    ChartArea chartArea = new ChartArea("chart5");
                    chart.ChartAreas.Add(chartArea);
                    Series series = new Series();
                    chart.Series.Add(series);

                    // X and Y value assignment
                    for (int i = 0; i < NumComp; i++)
                    {
                        if (Convert.ToDouble(Equil_Const[i]) > 0)
                        { 
                        Xs[i] = i + 1;
                        Ys[i] = Convert.ToDouble(Equil_Const[i]);
                        ChartOneTemp.Series["Series5"].Points.AddXY(Xs[i], Ys[i]);
                        }
                    }

                    //Plotting Local concentratione data 
                    ChartOneTemp.ChartAreas[0].AxisY.IsLogarithmic = true;

                    ChartOneTemp.Series["Series5"].ChartType =
                        SeriesChartType.Column;

                    ChartOneTemp.Series["Series5"].Color = Color.DarkKhaki
                        ;
                    ChartOneTemp.ChartAreas[0].AxisX.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Font =
                        new System.Drawing.Font("Arial", 11F);

                    ChartOneTemp.ChartAreas[0].AxisX.Title = "Carbon Number";

                    ChartOneTemp.ChartAreas[0].AxisX.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.Title =
                        "Equilibrium Constants, [-]";

                    ChartOneTemp.ChartAreas[0].AxisY.TitleFont =
                        new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                    ChartOneTemp.ChartAreas[0].AxisY.LabelStyle.Format =
                        "0.000E00";

                    ChartOneTemp.ChartAreas[0].AxisX.Maximum = NumComp;

                    ChartOneTemp.ChartAreas[0].AxisX.Minimum = 5;

                    ChartOneTemp.ChartAreas[0].AxisY.Maximum = 1000000000;

                    ChartOneTemp.ChartAreas[0].AxisY.Minimum = 0.000000001;

                    ChartOneTemp.ChartAreas[0].AxisX.Interval = 5;

                    //XAxis scale maximum value
                    Int32.TryParse(TextBoxNumComp.Text, out Param);
                    if (Convert.ToDouble(Param) / 5.0 == 0)
                    {
                        ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }

                    else
                    {
                        Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                        Param = Param*5 + 5;
                            ChartOneTemp.ChartAreas[0].AxisX.Maximum = Param;
                    }
                }
            }

            //If radioButton of CCN case is on, the following tasks are done
            if (RadioButtonCCN.Checked)
            {
                //Number of components are needed for plotting
                int NumComp;
                Int32.TryParse(TextBoxNumComp.Text, out NumComp);

                //X and Y arrays
                double[] Xs = new double[NumComp]; 
                double[] Ys = new double[NumComp];

                /*The following arrays are defined to store concentration 
                  gradient values*/
                string[,] Grad = new string[2, 200];
                string[] Gradd = new string[2];

                //The values of concentration values are read from the textfile
                 string[] Grad_Lines = System.IO.File.ReadAllLines(
                     CurrentDirectory + "//RelativeConcentrationGradient.txt");
                for (int i = 0; i < NumComp; i++)
                {
                    Gradd = Grad_Lines[i].Split(new string[] { "\t" }, 
                        StringSplitOptions.RemoveEmptyEntries);
                    Grad[0, i] = Gradd[1];
                }

                //Plotting concentration gradient plot
                ChartCCN.Series.Clear();
                ChartCCN.Series.Add("Seriesgrad");
                chart = new Chart();
                ChartArea chartArea = new ChartArea("Seriesgrad");
                chart.ChartAreas.Add(chartArea);
                Series series = new Series();
                chart.Series.Add(series);

                
                //Assigning X and Y values
                for (int i = 0; i < NumComp; i++)
                {
                    Xs[i] = i + 1;
                    Ys[i] = Convert.ToDouble(Grad[0, i]);

                    ChartCCN.Series["Seriesgrad"].Points.AddXY(Xs[i], Ys[i]);
                }

                /*Plotting local concentration gradient 
                values for CCN determination*/
                // ChartCCN.Series["Seriesgrad"].ChartType = SeriesChartType.Column;

                // ChartCCN.Series["Seriesgrad"].Color = Color.BlueViolet;

                //  ChartCCN.ChartAreas[0].AxisX.Interval = 5;

                ChartCCN.Series["Seriesgrad"].ChartType =
                       SeriesChartType.Column;
           
                ChartCCN.Series["Seriesgrad"].MarkerSize = 10;

                ChartCCN.Series["Seriesgrad"].Color = Color.PaleVioletRed;
                    ;
                ChartCCN.ChartAreas[0].AxisX.LabelStyle.Font =
                    new System.Drawing.Font("Arial", 11F);

                ChartCCN.ChartAreas[0].AxisY.LabelStyle.Font =
                    new System.Drawing.Font("Arial", 11F);

                ChartCCN.ChartAreas[0].AxisX.Title = "Carbon Number";

                ChartCCN.ChartAreas[0].AxisX.TitleFont =
                    new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                ChartCCN.ChartAreas[0].AxisY.Title =
                    "Relative Concentration Gradient" +"\n"+ "of N-alkanes, [kg/m3]";

                ChartCCN.ChartAreas[0].AxisY.TitleFont =
                    new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

                ChartCCN.ChartAreas[0].AxisX.Interval = 5;

                //CCN is calculated to be reported at the time of plotting
                int CCNParam;
                for (int jj=0; jj<NumComp;jj++)
                {
                    if (Ys[jj]>0)
                    {
                        CCNParam = jj+1;
                        CCNReport.Text = "CCN is: "+Convert.ToString(CCNParam);
                        break;
                    }
                }

                //Maximum value of X-Axis scale  
                Int32.TryParse(TextBoxNumComp.Text, out Param);
                if (Convert.ToDouble(Param) / 5.0 == 0)
                {
                    ChartCCN.ChartAreas[0].AxisX.Maximum = Param;
                }

                else
                {
                    Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                    Param = Param * 5 + 5;
                    ChartCCN.ChartAreas[0].AxisX.Maximum = Param; ;
                }
            }
        }


        private void PCAW_CheckedChanged(object sender, EventArgs e)
        {
            FlowLayoutPanelPrecWAT.Enabled = true;
            FlowLayoutPanelInputs.Enabled=true;
            FlowLayoutPanelOneTemp.Enabled = false;
            FlowLayoutPanelCCN.Enabled = false;
            TrackBarCompo.Enabled = true;
        }

        private void BOTSC_CheckedChanged(object sender, EventArgs e)
        {
            FlowLayoutPanelPrecWAT.Enabled = false;
            FlowLayoutPanelInputs.Enabled = true;
            FlowLayoutPanelOneTemp.Enabled = true;
            FlowLayoutPanelCCN.Enabled = false;
            TrackBarCompo.Enabled = false;
        }

        //This method returns the number of data points of precipitation curve
        public int Actual_Num;
        public void ActualTempNum()
        {
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string[] WATS = System.IO.File.ReadAllLines(CurrentDirectory +
                "//WAT.txt");
            Int32.TryParse(WATS[0], out Actual_Num);
        }

        /*In this method, values of SolidWComposition.txt are read and put in 
        the array*/
        public string[,] Array = new string[100, 200];
        public string[] Arrayy = new string[200];

        public void arrayRet()
        {
            string CurrentDirectory = Directory.GetCurrentDirectory();
            ActualTempNum();
            int NumComp;
            string Kp = TextBoxNumComp.Text;
            Int32.TryParse(Kp, out NumComp);
            string[] Solid_Lines = System.IO.File.ReadAllLines(CurrentDirectory
                + "//SolidWComposition.txt");

            for (int i = 0; i < Actual_Num; i++)
            {
                Arrayy = Solid_Lines[i].Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                for (int K = 0; K < NumComp; K++)
                {
                    Array[K, i] = Arrayy[K];
                }
            }
        }

        //Trackbar for solid-phase composition plotting
        private void TrackBarCompo_Scroll(object sender, EventArgs e)
        {
            //Number of n-alkane components
            ActualTempNum();

            //Assigning Maximum and Minimum values of TrackBarCompo
            this.TrackBarCompo.Maximum = Actual_Num;
            this.TrackBarCompo.Minimum = 0;
            int NumComp;
            Int32.TryParse(TextBoxNumComp.Text, out NumComp);

            //X and Y arrays
            double[] Xs = new double[NumComp]; 
            double[] Ys = new double[NumComp];

            //Solid-phase composition array assignment  
            arrayRet();

            string Hlp;

            //Capturing TrackBarCompo value
            string TrackVal = TrackBarCompo.Value.ToString();

            //Reading the temperature from the TrachBar and show it
            label23.Text = "T= " + Convert.ToString(Convert.ToDouble
                (TextBoxStartTemp.Text) + (Convert.ToDouble(TrackVal)) *
                Convert.ToDouble(TextBoxTempStep.Text)) + "[K]";

            //Plotting Soild-phase composition 
            ChartSolidCompo.Series.Clear();
            ChartSolidCompo.Series.Add("Series1");
            chart = new Chart();
            ChartArea chartArea = new ChartArea("chart3");
            chart.ChartAreas.Add(chartArea);
            Series series = new Series();
            chart.Series.Add(series);

            //Reading TrackBar value
            Hlp = TrackBarCompo.Value.ToString();
            int j;
            Int32.TryParse(Hlp, out j);

            //X and Y array assignment
            for (int i = 0; i < NumComp; i++)
            {
                Xs[i] = i + 1;
                Ys[i] = Convert.ToDouble(Array[i, j]);

                ChartSolidCompo.Series["Series1"].Points.AddXY(Xs[i], Ys[i]);
            }

            //Plotting Normalized Weight Fraction of solid phase
            int Param;
            ChartSolidCompo.Series["Series1"].ChartType = SeriesChartType.Column;

            ChartSolidCompo.ChartAreas[0].AxisX.LabelStyle.Font =
                new System.Drawing.Font("Arial", 11F);

            ChartSolidCompo.ChartAreas[0].AxisY.LabelStyle.Font =
                new System.Drawing.Font("Arial", 11F);

            ChartSolidCompo.ChartAreas[0].AxisX.Title = "Temperature, [K]";

            ChartSolidCompo.ChartAreas[0].AxisX.TitleFont =
                new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

            ChartSolidCompo.ChartAreas[0].AxisY.Title =
                "Normalized Solid Weight Fraction, [-]";

            ChartSolidCompo.ChartAreas[0].AxisY.TitleFont =
                new System.Drawing.Font("Arial", 13F, FontStyle.Bold);

            ChartSolidCompo.ChartAreas[0].AxisX.Maximum = NumComp;

            ChartSolidCompo.ChartAreas[0].AxisX.Minimum = 5;

            ChartSolidCompo.ChartAreas[0].AxisX.Interval = 5;

            ChartSolidCompo.ChartAreas[0].AxisY.Minimum = 0;

            ChartSolidCompo.ChartAreas[0].AxisY.Maximum = 0.4;

            ChartSolidCompo.ChartAreas[0].AxisY.Minimum = 0;

            //Number of components are captured
            Int32.TryParse(TextBoxNumComp.Text, out Param);

            //Maximum value of X-Axis scale  
            if (Convert.ToDouble(Param) / 5.0 == 0)
            {
                ChartSolidCompo.ChartAreas[0].AxisX.Maximum = Param;
            }

            else
            {
                Param = Convert.ToInt32(Convert.ToDouble(Param) / 5.0);
                Param = Param * 5 + 5;
                ChartSolidCompo.ChartAreas[0].AxisX.Maximum = Param;
            }

            if (RadioButtonCCN.Checked)
            {

            }
        }

        //Erasing plot button
        private void Erase_Click(object sender, EventArgs e)
        {
            ChartPrec.Series.Clear();
            label20.Text = " ";
        }

        //Openning a new window to insert experimental data
        private void ButtonExpDataInsert_click(object sender, EventArgs e)
        {
                //ButtonExpDataInsert which open its window
                Exp CF = new Exp(this);
                CF.Show();
        }

        private void RadioButtonCCN_CheckedChanged(object sender, EventArgs e)
        {
            TrackBarCompo.Enabled = false;
            FlowLayoutPanelPrecWAT.Enabled = false;
            FlowLayoutPanelInputs.Enabled = true;
            FlowLayoutPanelOneTemp.Enabled = false;
            FlowLayoutPanelCCN.Enabled = true;
        }

        //The inpput data could be uploaded from the previously saved .bon file 
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
            string[] Val = new string[12];
            using (BinaryReader bw = new BinaryReader(File.Open(
                file, FileMode.Open)))
            {
                String S;
                for (int j = 0; j < 12; ++j)
                {
                        if (bw.ReadBoolean())
                        {
                            Val[j] = bw.ReadString();
                        }
                        else bw.ReadBoolean();
                }

                //Correct input values are assigned to textbox(es)
                TextBoxWaxContent.Text=Val[0];
                TextBoxAlpha.Text= Val[1];
                TextBoxNumComp.Text= Val[2];
                TextBoxSmallestCNum.Text= Val[3];
                TextBoxNumData.Text = Val[4];
                TextBoxStartTemp.Text = Val[5];
                TextBoxTempStep.Text = Val[6];
                TextBoxMaxAccuracy.Text = Val[7];
                TextBoxWATaccuracy.Text = Val[8];
                TextBoxDisTemp.Text = Val[9];
                TextBoxLowTemp.Text = Val[10];
                TextBoxHighTemp.Text = Val[11];
            }
        }

        //The input data can be saved by "save as" button.
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
            string[] Val = new string[12];
            using (BinaryWriter bw = new BinaryWriter(File.Open(filee,
                FileMode.Create))) 
            {
                Val[0]=  TextBoxWaxContent.Text;
                Val[1] = TextBoxAlpha.Text;
                Val[2] = TextBoxNumComp.Text;
                Val[3] = TextBoxSmallestCNum.Text;
                Val[4] = TextBoxNumData.Text;
                Val[5] = TextBoxStartTemp.Text;
                Val[6] = TextBoxTempStep.Text;
                Val[7] = TextBoxMaxAccuracy.Text;
                Val[8] = TextBoxWATaccuracy.Text;
                Val[9] = TextBoxDisTemp.Text;
                Val[10] = TextBoxLowTemp.Text;
                Val[11] = TextBoxHighTemp.Text;

                //The Va[] array is saved
                for (int j = 0; j < 12; ++j)
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //This button saves precipittion curve chart
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            //Calling current directory
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string Dir = CurrentDirectory + "\\Precipitation.png";
            this.ChartPrec.SaveImage(Dir, ChartImageFormat.Png);
        }

        private void ButtonSaveOT_Click(object sender, EventArgs e)
        {
            //To name the saved chart correctly, the right option
            //has to be called
            string Option;
            Option = outputComboBox.SelectedItem.ToString();

            //Calling current directory
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string Dir = CurrentDirectory + "\\"+ Option+".png";
            this.ChartOneTemp.SaveImage(Dir, ChartImageFormat.Png);
        }

        private void ButtonSaveCompo_Click(object sender, EventArgs e)
        {
            //Calling current directory
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string Dir = CurrentDirectory + "\\SolidPhaseComposition.png";
            this.ChartSolidCompo.SaveImage(Dir, ChartImageFormat.Png);
        }

        private void ButtonSaveCCN_Click(object sender, EventArgs e)
        {
            //Calling current directory
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string Dir = CurrentDirectory + "\\RelativeConcentrationGradient.png";
            this.ChartCCN.SaveImage(Dir, ChartImageFormat.Png);
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        //Calling User's manual
        private void usersManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("User's manual.pdf");
        }

        //Calling Developer's manual
        private void developersManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
           System.Diagnostics.Process.Start("Developer's manual.pdf");
        }

        private void MultiComponentSystem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string CurrentDirectory = Directory.GetCurrentDirectory();

            /*General inputs are used in all three versions of 
ulti -component system*/
            File.WriteAllText(CurrentDirectory + "\\GeneralInputs.txt",
                String.Empty);
            string Dir = CurrentDirectory + "\\GeneralInputs.txt";

            using (FileStream fW = new FileStream(Dir, FileMode.OpenOrCreate))
            {
                using (TextWriter WR = new StreamWriter(fW))
                {
                    WR.WriteLine(TextBoxWaxContent.Text);
                    WR.WriteLine(TextBoxAlpha.Text);
                    WR.WriteLine(TextBoxSmallestCNum.Text);
                    WR.WriteLine(TextBoxNumComp.Text);
                }
            }
            Process.Start(CurrentDirectory + "\\SPWaxKInitialization.exe");
            BottonRun.Enabled = true;
            ButtonPlot.Enabled = true;
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
