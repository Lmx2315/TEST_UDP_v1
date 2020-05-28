using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using PlotWrapper;
using System.Windows.Forms;



namespace test_UDP_v1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void ShowMessageMethod(string msg);

        System.Windows.Threading.DispatcherTimer Timer1 = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer Timer2 = new System.Windows.Threading.DispatcherTimer();

        Plot3 fig1 = new Plot3("time", "x", "y");
        IPAddress my_ip;
        UInt16 my_port;
        IPAddress dest_ip;
        UInt16 dest_port;
        UInt32 timer = 0;
        string FLAG_DISPAY = "";
        public MainWindow()
        {
            InitializeComponent();
            Timer1.Tick += new EventHandler(Timer1_Tick);
            Timer1.Interval = new TimeSpan(0, 0, 0, 0, 100);

            Timer2.Tick += new EventHandler(Timer2_Tick);
            Timer2.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            my_ip = IPAddress.Parse(my_ip_box.Text);
            my_port = UInt16.Parse(my_port_box.Text);
            dest_ip = IPAddress.Parse(adr_ip_box.Text);
            dest_port = UInt16.Parse(adr_port_box.Text);
            timer = 0;
            Debug.WriteLine("...");
            if ((Convert.ToString(button.Content)) == "START")
            {
                Timer1.Start();
                Timer2.Start();
                button.Content = "STOP";
            }
            else
            {
                Timer1.Stop();
                Timer2.Stop();
                button.Content = "START";
            }

        }


        private void Timer1_Tick(object sender, EventArgs e)
        {
          if (FLAG_DISPAY == "")  SENDER();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (FLAG_DISPAY=="1")
            {
                // Start a Stopwatch
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                fig1.PlotData(time_series);
                fig1.Show();
                FLAG_DISPAY = "";
    //            Debug.WriteLine("*");
            }

        }
        int INDEX = 0;
        private void SENDER()
        {
            try
            { //send packet
             //   Debug.WriteLine("send data");
                UdpClient client = new UdpClient();
                client.Connect(adr_ip_box.Text, Convert.ToInt32(adr_port_box.Text));
                byte[] data;
                //   string msg = "hello!";
                //   data = Encoding.UTF8.GetBytes(msg);
                BUF = Convert.ToInt32(textBox_buf_n.Text);
                Int32[] Z = new Int32[BUF];
                Z=GEN_F0();//расчитываем отсчёты
                while (INDEX< BUF)
                {
                    data = FORMER(Z);
                    int number_bytes = client.Send(data, data.Length);
       //             Debug.WriteLine("INDEX:"+ INDEX);
                }
                INDEX = 0;
                client.Close();
            }
            catch (Exception excp)
            {
                Debug.WriteLine("чё-то не так");
                Console.WriteLine(excp.Message);
            }

        }

        double[] time_series;
        int BUF = 512;
        Int32[] GEN_F0()
        {
            double FREQ = 3;
            double Fclk = 12;
            double pi = 3.1416;
            double A = 30000;

            BUF = Convert.ToInt32(textBox_buf_n.Text);

            Int32[] Z      = new Int32[BUF];
            Int32[] smpl_i = new Int32[BUF];
            Int32[] smpl_q = new Int32[BUF];           

            int N_sampl =BUF;

            time_series = new Double[N_sampl];

            for (int i = 0; i < N_sampl; i++)
            {
                smpl_q[i] = Convert.ToInt32(Math.Floor(A * Math.Cos(2 * pi * FREQ * (i / Fclk))));
                smpl_i[i] = Convert.ToInt32(Math.Floor(A * Math.Sin(2 * pi * FREQ * (i / Fclk))));
                time_series[i] = Convert.ToDouble(smpl_i[i]);
                Z[i] = (smpl_q[i] << 16) + (smpl_i[i]&0xffff);
            }         

            FLAG_DISPAY = "1";
         

            return Z;
        }

    

        byte [] FORMER (Int32[] z)
        {
            byte[] m = new byte[1446];//транспортный массив
            int N_sampl;
            timer++;
            //------заголовок пакета UDP
            m[0] = 0;
            m[1] = 0;//тут номер канала 
            m[2] = Convert.ToByte((timer >> 24) & 0xff);//тут время пакета
            m[3] = Convert.ToByte((timer >> 16) & 0xff);// --//--
            m[4] = Convert.ToByte((timer >> 8) & 0xff);// --//--
            m[5] = Convert.ToByte((timer >> 0) & 0xff);// --//--
            //--------------------------

            BUF = Convert.ToInt32(textBox_buf_n.Text);
            N_sampl = BUF - INDEX;
            if (N_sampl > 360) N_sampl = 360; //столько отсчётов помещается в один 1446 байтный UDP пакет
            N_sampl = N_sampl + INDEX;
            int k = 6;
            for (int i = INDEX; i < N_sampl; i++)
            {
        //        Debug.WriteLine(z[i]);
                m[k++] = Convert.ToByte((z[i] >>24) & 0xff);
                m[k++] = Convert.ToByte((z[i] >>16) & 0xff);

                m[k++] = Convert.ToByte((z[i] >> 8) & 0xff);
                m[k++] = Convert.ToByte((z[i] >> 0) & 0xff);
            }

            INDEX = N_sampl;
     //       for (int i = 0; i < 1446; i++) Debug.WriteLine(m[i]);
            return m;
        }

    }
}
