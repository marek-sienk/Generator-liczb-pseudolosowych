using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomatKomórkowy
{
    public partial class Form1 : Form
    {
        int n; //szerokosc automatu
        int N; //dlugosc automatu
        string regula;
        int sasiedztwo;
        List<Dictionary<int, string>> listaRegul;
        Dictionary<int, string> reguly1;
        Dictionary<int, string> reguly2;
        Dictionary<int, string> reguly3;
        Dictionary<int, string> reguly4;
        int[][] automat;
        int h, nEnt; //zmienne do entropii
        int liczbaPrzebiegów; //zmienna do generowania pliku do testow

        public Form1()
        {
            InitializeComponent();

            reguly1 = new Dictionary<int, string>();
            for (int i = 0; i < 16; i++)
            {
                reguly1.Add(i, Convert.ToString(i, 2).PadLeft(4,'0'));
            }

            reguly2 = new Dictionary<int, string>();
            for (int i = 0; i < 64; i++)
            {
                reguly2.Add(i, Convert.ToString(i, 2).PadLeft(6,'0'));
            }

            reguly3 = new Dictionary<int, string>();
            for (int i = 0; i < 256; i++)
            {
                reguly3.Add(i, Convert.ToString(i, 2).PadLeft(8, '0'));
            }

            reguly4 = new Dictionary<int, string>();
            for (int i = 0; i < 1024; i++)
            {
                reguly4.Add(i, Convert.ToString(i, 2).PadLeft(10, '0'));
            }

            listaRegul = new List<Dictionary<int, string>>() { reguly1, reguly2, reguly3, reguly4 };

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DataSource = new BindingSource(reguly1, null);
            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Value";
            comboBox1.SelectedIndex = 0;
            regula = comboBox1.SelectedValue.ToString();
            label1.Text = regula;

            int[] sasiedztwa = new int[] { 1, 2, 3, 4 };
            comboBox2.DataSource = new BindingSource(sasiedztwa, null);
            comboBox2.SelectedIndex = 0;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

            label5.Text = "";
        }

        public int ZastosujRegule(int NrPrzebiegu, int NrBitu)
        {
            int suma = 0;
            for (int i = NrBitu - sasiedztwo; i <= NrBitu + sasiedztwo; i++)
            {
                if (i < 0)
                {
                    if (automat[NrPrzebiegu-1][n + i] == 1)
                        suma++;
                }
                else
                    if (i > n - 1)
                    {
                        if (automat[NrPrzebiegu-1][i - n] == 1)
                            suma++;
                    }
                    else
                        if (automat[NrPrzebiegu-1][i] == 1)
                            suma++;
            }
            char[] reverse = regula.ToCharArray();
            Array.Reverse(reverse);
            return Int32.Parse(new string(reverse).Substring(suma, 1));
        }

        public void GenerujPopPocz()
        {
            Random rand = new Random();
            automat = new int[N][];

            for (int i = 0; i < automat.Length; i++)
			{
                automat[i] = new int[n];
			}

            Action<string> updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Działanie automatu...");

            string dane2 = "";
            for (int i = 0; i < n; i++)
            {
                automat[0][i] = rand.Next(2);
                dane2 += automat[0][i].ToString();
            }

            FileStream fs = new FileStream(Application.StartupPath + @"\" + "DANE.txt", FileMode.Append);
            byte[] dane = Encoding.UTF8.GetBytes("**********************************************************************************" +
                    Environment.NewLine + "Dlugosc automatu : " + n + " Ilość przebiegów : " + N +
                    " Rozmiar sąsiedztwa : " + sasiedztwo + " Reguła : " + regula + Environment.NewLine +
                    "**********************************************************************************" +
                    Environment.NewLine + dane2 + Environment.NewLine);

            fs.Write(dane, 0, (dane.Length));
            fs.Close();
        }

        public void UruchomAutomatJednorodny()
        {
            //------------GENEROWANIE POPULACJI POCZATKOWEJ----------
            GenerujPopPocz();
            //----------------KONIEC GENEROWANIA------------------------------------------

            //-------------DZIAŁANIE AUTOMATU--------------------------
            FileStream fs = new FileStream(Application.StartupPath + @"\" + "DANE.txt", FileMode.Append);
            byte[] dane;
            string result = "";
            for (int j = 1; j < N; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    automat[j][k] = ZastosujRegule(j, k);
                    result += automat[j][k];
                }
                dane = Encoding.UTF8.GetBytes(result + Environment.NewLine);
                fs.Write(dane, 0, (dane.Length));
                result = "";

                progressBar1.Invoke((MethodInvoker)delegate { progressBar1.Increment(1); });
            }
            dane = Encoding.UTF8.GetBytes(Environment.NewLine);
            fs.Write(dane, 0, (dane.Length));
            fs.Close();

            Action<string> updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Koniec działania automatu");
            //------------------KONIEC DZIAŁANIA---------------------
        }

        public void UruchomAutomatNiejednorodny()
        {
            GenerujPopPocz(); //POPULACJA POCZATKOWA

            List<string> zbiorRegul = new List<string>((richTextBox1.Text.Split('\n')).ToList());//wydzielenie wpisanych regul
            int[] przypisaneReguly = new int[n];
            Random rand = new Random();

            for (int i = 0; i < n; i++)
            {
                przypisaneReguly[i] = rand.Next(zbiorRegul.Count);//losowanie regul dla danych komorek
            }

            FileStream fs = new FileStream(Application.StartupPath + @"\" + "DANE.txt", FileMode.Append);
            byte[] dane;
            string result = "";
            for (int j = 1; j < N; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    string[] regula_i_sasiedztwo = zbiorRegul[przypisaneReguly[k]].Split(' ');
                    regula = (listaRegul[Convert.ToInt32(regula_i_sasiedztwo[1]) - 1])[Convert.ToInt32(regula_i_sasiedztwo[0])]; //zmiana reguly
                    sasiedztwo = Convert.ToInt32(regula_i_sasiedztwo[1]);

                    automat[j][k] = ZastosujRegule(j, k);
                    result += automat[j][k];
                }
                dane = Encoding.UTF8.GetBytes(result + Environment.NewLine);
                fs.Write(dane, 0, (dane.Length));

                result = "";

                progressBar1.Invoke((MethodInvoker)delegate { progressBar1.Increment(1); });
            }
            dane = Encoding.UTF8.GetBytes(Environment.NewLine);
            fs.Write(dane, 0, (dane.Length));
            fs.Close();

            Action<string> updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Koniec działania automatu");
        }

        public void ObliczEntropie()
        {
            Action<string> updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Obliczanie entropii...");

            double min = 0, max = 0, avg = 0;

            h = Int32.Parse(textBox3.Text);
            Dictionary<string, int> Fragmenty = new Dictionary<string, int>();

            for (int i = 0; i < Math.Pow(2, h); i++)
            {
                Fragmenty.Add(Convert.ToString(i, 2).PadLeft((int)h, '0'), 0);
            }

            for (int k = 0; k < nEnt; k++)
            {
                Random rand = new Random();
                int badanyBit = rand.Next(n);

                string fragment = "";

                for (int j = 0; j < N; j++)
                {
                    fragment += automat[j][badanyBit] + "";

                    if ((j + 1) % h == 0)
                    {
                        Fragmenty[fragment]++;
                        fragment = "";
                    }
                }

                double suma = 0, praw = 0;
                foreach (var fr in Fragmenty)
                {
                    praw = (double)fr.Value / (double)Fragmenty.Values.Sum();

                    if (praw != 0)
                        suma += praw * Math.Log(praw, 2);
                }
                
                if (k == 0)
                {
                    min = -suma;
                    max = -suma;
                }
                else
                {
                    if ((-suma) < min)
                        min = -suma;
                    if ((-suma) > max)
                        max = -suma;
                }
                avg += -suma;

                progressBar1.Invoke((MethodInvoker)delegate { progressBar1.Increment(1); });
            }
            avg /= nEnt;
            updateAction = new Action<string>((value) => label12.Text = value);
            label12.Invoke(updateAction, avg.ToString());

            FileStream fs = new FileStream(Application.StartupPath + @"\" + "ENTROPIA.txt", FileMode.Append);
            byte[] dane = null;
            dane = Encoding.UTF8.GetBytes(regula + "\t" + avg + "\t" + min + "\t" + max + Environment.NewLine);

            fs.Write(dane, 0, (dane.Length));
            fs.Close();
            updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Koniec liczenia entropii");
        }

        private void ZmieńRegule(object sender, EventArgs e)
        {
            regula = comboBox1.SelectedValue.ToString();
            label1.Text = regula;
        }

        private void ZmienSasiedztwo(object sender, EventArgs e)
        {
            sasiedztwo = (int)comboBox2.SelectedValue;
            comboBox1.DataSource = new BindingSource(listaRegul[sasiedztwo-1], null);
        }

        private void StartAutomatu()
        {
            n = Int32.Parse(textBox1.Text);
            N = Int32.Parse(textBox2.Text);
            nEnt = Int32.Parse(textBox4.Text);
            progressBar1.Maximum = N - 1;
            if (checkBox1.Checked)
                progressBar1.Maximum += nEnt - 1;
            if (radioButton2.Checked)
                progressBar1.Maximum += liczbaPrzebiegów - 1;
            if (radioButton3.Checked)
                progressBar1.Maximum += liczbaPrzebiegów - 1;
            progressBar1.Value = 0;

            if (radioButton4.Checked)
                UruchomAutomatJednorodny();
            else
                UruchomAutomatNiejednorodny();

            if (checkBox1.Checked)
                ObliczEntropie();

            if (radioButton2.Checked)
                GenerujPlikNIST();

            if (radioButton3.Checked)
                GenerujPlikDieHard();

            button3.Enabled = true;
            Console.Beep(500, 250);
        }

        private void generujObrazClick(object sender, EventArgs e)
        {
            n = Int32.Parse(textBox1.Text);
            N = Int32.Parse(textBox2.Text);

            Form oknoObraz = new Form()
            {
                Name = "oknoObrazu",
                Size = new Size(300,300),
                AutoScroll = true
            };
            PictureBox obraz = new PictureBox
            {
                Name = "obraz",
                Size = new Size(n, N),
                Visible = true
            };

            obraz.Image = new Bitmap(obraz.Width, obraz.Height);

            oknoObraz.Controls.Add(obraz);
            oknoObraz.Visible = true;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (automat[i][j] == 1)
                        ((Bitmap)obraz.Image).SetPixel(j, i, Color.Black);
                }
            }
        }

        private void StartClick(object sender, EventArgs e)
        {
            button3.Enabled = false;
            Thread th = new Thread(StartAutomatu);
            th.Start();
        }
        private void GenerujPlikNIST()
        {
            Action<string> updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Generowanie pliku...");

            liczbaPrzebiegów = Int32.Parse(textBox5.Text);
            Random rand = new Random();
            List<int> bity = new List<int>();
            while(bity.Count<liczbaPrzebiegów)
            {
                int b = rand.Next(n);
                if (!bity.Contains(b))
                    bity.Add(b);
            }

            string fileName = Application.StartupPath + @"\" + "NIST";
            foreach (var para in richTextBox1.Text.Split('\n'))
            {
                fileName += "-" + para.Split(' ')[0] + "_" + para.Split(' ')[1];
            }
            fileName += "-(" + liczbaPrzebiegów + ")" + ".txt";

            string ciag;
            FileStream fs;
            if(radioButton4.Checked)
                fs = new FileStream(Application.StartupPath + @"\" + "NIST-" + sasiedztwo + "-" + comboBox1.Text + "-(" + liczbaPrzebiegów + ")" + ".txt", FileMode.Create);
            else
                fs = new FileStream(fileName , FileMode.Create);

            foreach (var bit in bity)
            {
                ciag = "";
                for (int j = 0; j < N; j++)
                {
                    //ciag += automat[j][bit];
                    byte[] dane = Encoding.UTF8.GetBytes(automat[j][bit].ToString());
                    fs.Write(dane, 0, (dane.Length));
                }
                //byte[] dane = Encoding.UTF8.GetBytes(ciag);
                //fs.Write(dane, 0, (dane.Length));

                progressBar1.Invoke((MethodInvoker)delegate { progressBar1.Increment(1); });
            }
            fs.Close();

            updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Koniec generowania pliku...");
        }
        private void GenerujPlikDieHard()
        {
            Action<string> updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Generowanie pliku...");

            liczbaPrzebiegów = Int32.Parse(textBox5.Text);
            Random rand = new Random();
            List<int> bity = new List<int>();
            while (bity.Count < liczbaPrzebiegów)
            {
                int b = rand.Next(n - 1);
                if (!bity.Contains(b))
                    bity.Add(b);
            }

            string fileName = Application.StartupPath + @"\" + "dih";
            if (!radioButton4.Checked)
                foreach (var para in richTextBox1.Text.Split('\n'))
                {
                    fileName += "-" + para.Split(' ')[0] + "_" + para.Split(' ')[1];
                }
            else
                fileName += "-" + sasiedztwo + "-" + comboBox1.Text;

            using (BinaryWriter binWriter =
                new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                BitArray myBits = new BitArray(8);
                byte[] bajt = new byte[1];
                int licznik = 0;
                foreach (var bit in bity)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (licznik < 8)
                        {
                            myBits.Set(licznik, Convert.ToBoolean(automat[j][bit]));
                            licznik++;
                        }
                        else
                        {
                            myBits.CopyTo(bajt,0);
                            binWriter.Write(bajt[0]);
                            licznik = 0;
                        }
                        
                    }
                }
                binWriter.Close();
            }
            
            updateAction = new Action<string>((value) => label5.Text = value);
            label5.Invoke(updateAction, "Koniec generowania pliku...");
        }
    }
}
