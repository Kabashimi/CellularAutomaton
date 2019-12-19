using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CA
{
    public partial class Form1 : Form
    {
        Engine engine;

        public Form1()
        {
            InitializeComponent();

            engine = new Engine();
            engine.DrawBitmap();
            pictureBox1.Image = engine.bitmap;
            Console.WriteLine("Initalastion done");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var args = (MouseEventArgs)e;
            if (engine.AddSelectedGrain(args.Location.X, args.Location.Y))
            {
                //add grain to list
                String grainLabel = "Id: " + engine.selectedGrains[engine.selectedGrains.Count - 1] + " - ";
                float grainPercentage = engine.CalculatePercentage(engine.selectedGrains[engine.selectedGrains.Count - 1]);
                grainLabel += grainPercentage + "%";
                listBox1.Items.Add(grainLabel);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            String nucleons = textBox1.Text;
            int nucleonsNumber;
            if (!Int32.TryParse(nucleons, out nucleonsNumber))
            {
                Console.WriteLine("Nucleons number is not a valid number.");
            }
            else
            {
                if (nucleonsNumber > (Engine.spaceDim * Engine.spaceDim))
                {
                    Console.WriteLine("Nucleons number is too high.");
                }
                else
                {
                    engine.SetRandomNucleons(nucleonsNumber);
                    pictureBox1.Image = engine.bitmap;
                    Console.WriteLine("Nucleation done");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int growthType = comboBox2.SelectedIndex;
            String probabilityString = textBox4.Text;
            int probability;
            Int32.TryParse(probabilityString, out probability);
            engine.probability = probability;
            engine.neighborhoodType = growthType;
            timer1.Enabled = !timer1.Enabled;
            if (timer1.Enabled)
            {
                button2.Text = "Stop";
            }
            else
            {
                button2.Text = "Growth";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Grow start");
            engine.Grow();
            pictureBox1.Image = engine.bitmap;
            pictureBox1.Refresh();
            if (engine.growthDone)
            {
                button2.Text = "Growth";
                timer1.Enabled = false;
            }
        }

        private void txtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text file|*.txt";
            saveFileDialog1.Title = "Save microstructure as a text file";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                // System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog1.OpenFile());
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        //this.button2.Image.Save(fs,System.Drawing.Imaging.ImageFormat.Jpeg);
                        this.engine.ExportToTxt(sw);
                        break;
                }

                sw.Close();
            }
        }

        private void txtToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sr = new System.IO.StreamReader(openFileDialog1.FileName);
                    this.engine.ImportFromTxt(sr);
                }
                catch (System.Security.SecurityException ex)
                {
                    MessageBox.Show("Nieprawidłowy format pliku.");
                    Console.WriteLine($"Security error.\n\nError message: {ex.Message}\n\n" + $"Details:\n\n{ex.StackTrace}");
                }

                this.pictureBox1.Image = engine.bitmap;
            }
        }

        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Bitmap|*.bmp";
            saveFileDialog1.Title = "Save microstructure as a bitmap";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                engine.bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void bitmapToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap bmp = new Bitmap(openFileDialog1.FileName);
                    this.engine.ImportFromBmp(bmp);
                }
                catch (System.Security.SecurityException ex)
                {
                    MessageBox.Show("Nieprawidłowy format pliku.");
                    Console.WriteLine($"Security error.\n\nError message: {ex.Message}\n\n" + $"Details:\n\n{ex.StackTrace}");
                }

                this.pictureBox1.Image = engine.bitmap;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            String numberOfInclusions = textBox2.Text;
            String sizeOfInclusion = textBox3.Text;
            int inclusionsNumber, inclusionSize;
            if (!Int32.TryParse(numberOfInclusions, out inclusionsNumber))
            {
                Console.WriteLine("Inclusions number is not a valid number.");
            }
            else if (!Int32.TryParse(sizeOfInclusion, out inclusionSize))
            {
                Console.WriteLine("Inclusion number is not a valid number.");
            }
            else
            {
                int inclusionType = comboBox1.SelectedIndex;
                engine.SetRandomInclusions(inclusionsNumber, inclusionSize, inclusionType);
                Console.WriteLine("Inclusions insert done");
                this.pictureBox1.Image = engine.bitmap;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            engine.SetDualStructure();
            this.pictureBox1.Image = engine.bitmap;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            engine.SetDualStructure();
            this.pictureBox1.Image = engine.bitmap;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String grainsNumberString = textBox5.Text;
            int grainsNumber;
            if (!Int32.TryParse(grainsNumberString, out grainsNumber))
            {
                Console.WriteLine("Grains number is not a valid number.");
            }
            else
            {
                engine.setMCGrains(grainsNumber);
                engine.DrawBitmap();
                pictureBox1.Image = engine.bitmap;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            String iterationsNumberString = textBox6.Text;
            int iterationsNumber;
            if (!Int32.TryParse(iterationsNumberString, out iterationsNumber))
            {
                Console.WriteLine("Iterations number is not a valid number.");
            }
            else
            {
                for (int i = 0; i < iterationsNumber; i++)
                {
                    engine.MCGrow(pictureBox1);
                    engine.DrawBitmap();
                    pictureBox1.Image = engine.bitmap;
                    pictureBox1.Refresh();
                }

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            engine.showEnergy = false;
            pictureBox1.Image = engine.bitmap;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            engine.showEnergy = true;
            pictureBox1.Image = engine.bitmapEnergy;

        }

        private void button10_Click(object sender, EventArgs e)
        {
            int energyDistributionType = comboBox3.SelectedIndex;
            int minEnergy = System.Convert.ToInt32(textBox7.Text);
            int maxEnergy = System.Convert.ToInt32(textBox8.Text);
            int deviation = System.Convert.ToInt32(textBox9.Text);
            engine.DistributeEnergy(energyDistributionType, minEnergy, maxEnergy, deviation);
            Console.WriteLine("Distribution energy done");
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            int nucleonsLocation = comboBox4.SelectedIndex;
            int nucleationType = comboBox5.SelectedIndex;
            int nucleonsNumber = System.Convert.ToInt32(textBox11.Text);
            int iterationsNumber = System.Convert.ToInt32(textBox10.Text);
            for (int i = 0; i < iterationsNumber; i++)
            {
                engine.Recrystallize(nucleonsLocation, nucleationType, nucleonsNumber);
                if (engine.showEnergy)
                {
                    pictureBox1.Image = engine.bitmapEnergy;
                    pictureBox1.Refresh();
                }
                else
                {
                    pictureBox1.Image = engine.bitmap;
                    pictureBox1.Refresh();
                }
            }


        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
