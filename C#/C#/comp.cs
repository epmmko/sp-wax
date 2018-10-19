using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;


namespace CompositionData
{
    public partial class compData : Form
    {
        public compData()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numBofRows = 0;
            Int32.TryParse(TX1.Text, out numBofRows);

            for (int i = 0; i < numBofRows; i++)
            {
                int Hl = i + 1;
                DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[i].Clone();
                row.Cells[0].Value = "C" + Hl;
                dataGridView1.Rows.Add(row);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(@"C:\Users\aryas\source\repos\waxPrecipitationFinalFormat\CompositionData\Data.txt", String.Empty);
            string path = @"C:\Users\aryas\source\repos\waxPrecipitationFinalFormat\CompositionData\Data.txt";
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (TextWriter tw = new StreamWriter(fs))
                {
                    int numBofRows = 0;
                    Int32.TryParse(TX1.Text, out numBofRows);
                    for (int i = 0; i < numBofRows; i++)
                    {
                        tw.WriteLine(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
