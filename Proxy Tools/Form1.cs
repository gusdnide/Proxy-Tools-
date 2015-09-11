using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
namespace Proxy_Tools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        void Salvar(ListBox lst)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "";
            sfd.Filter = "Arquivos de Texto (*.txt)|*.txt";
            sfd.Title = "Procure...";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] g = new string[lst.Items.Count];
                for (int i = 0; i < lst.Items.Count; i++)
                {
                    g[i] = lst.Items[i].ToString();
                }
                File.WriteAllLines(sfd.FileName, g);
            }
        }
        void Copiar(ListBox lst)
        {
            string strPraCopiar = "";
            foreach (string str in lst.Items)
            {
                strPraCopiar = strPraCopiar + str + Environment.NewLine;
            }
            if (strPraCopiar != string.Empty) Clipboard.SetText(strPraCopiar); 
        }
        void PegarLista()
        {
            ListBox lst = listBox1;
            Thread t1 = new Thread(() => PegarProxys(lst, "01"));
            Thread t2 = new Thread(() => PegarProxys(lst, "02"));
            Thread t3 = new Thread(() => PegarProxys(lst, "03"));
            Thread t4 = new Thread(() => PegarProxys(lst, "04"));
            Thread t5 = new Thread(() => PegarProxys(lst, "05"));
            t1.IsBackground = true;
            t2.IsBackground = true;
            t3.IsBackground = true;
            t4.IsBackground = true;
            t5.IsBackground = true;
            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            PegarLista();
            button1.Enabled = false;
            button2.Enabled = true;
        }
        void PegarProxys(ListBox list, string Page = "01")
        {
            string site = "http://www.samair.ru/proxy/proxy-" + Page + ".htm";
            WebClient wc = new WebClient();
            string gg = wc.DownloadString(site);
            Regex ip = new Regex(@"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):\d+");
            MatchCollection result = ip.Matches(gg);
            int g = result.Count;
            for (int t = 0; t < g; t++)
            {
                this.Invoke((MethodInvoker)(() => list.Items.Add(result[t].ToString())));
            }
            
        }
        void ThreadVerificar()
        {
            Thread t = new Thread(() => Verificar(listBox1));
            t.IsBackground = true;
            t.Start();
        }
        void Verificar(ListBox lst)
        {
            ProxyTools pTools = new ProxyTools();
     
            foreach (string str in lst.Items)
            {
                ProxyTools.Proxy Proxy = new  ProxyTools.Proxy();
                Proxy = Proxy.PegarProxy(str);
                if (pTools.Verificar(Proxy.IP, Proxy.Port))
                {
                    this.Invoke((MethodInvoker)(() => listBox2.Items.Add(str)));
                }
                this.Invoke((MethodInvoker)(() => this.Text = "Proxy Tools Scaneando: " + str));
            }
            MessageBox.Show("Completo!");
            this.Invoke((MethodInvoker)(() => button1.Enabled = true));
            this.Invoke((MethodInvoker)(() => button2.Enabled = false));
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Salvar(listBox2);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Copiar(listBox2);
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Salvar(listBox1);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            listBox2.Items.Clear();
            ThreadVerificar();
        }
    }
    public class ProxyTools
    {
        public struct Proxy
        {
            public string IP;
            public string Port;
            public string IPPort() { return IP + ":" + Port; }
            public Proxy PegarProxy(string IPPort)
            {
                Proxy px = new Proxy();
                string[] Resultados = Separar(":", IPPort);
                if (Resultados.Count() > 0)
                {
                    px.IP = Resultados[0];
                    px.Port = Resultados[1];
                }
                return px;
            }
        }
        struct APIProxy
        {
          public  enum _Tipo
            {
                Transparent,
                Anonymous,
                HighAnonymous
            };
            public double response_time;
            public int speed;
            public string Localizacao;
            public _Tipo Tipo;
            public _Tipo Parse(int ID)
            {
                switch (ID)
                {
                    case 0: return _Tipo.Transparent;
                    case 1: return _Tipo.Anonymous;
                    case 2: return _Tipo.HighAnonymous;
                }
                return _Tipo.Transparent;
            }
        }      
        string PegarAPI(string IP, string Port)
        {
          
                Stream dataStream;
                WebRequest request = WebRequest.Create("http://api.proxyipchecker.com/pchk.php");
                request.Method = "POST";

                string postData = "ip=" + IP + "&port=" + Port;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
          
        }
        APIProxy PegarResultado(string str)
        {
            APIProxy APIRetorno = new APIProxy();
            string[] Resultados = Separar(";", str);
            APIRetorno.response_time = double.Parse(Resultados[0]);
            APIRetorno.speed = int.Parse(Resultados[1]);
            APIRetorno.Localizacao = Resultados[2];
            APIRetorno.Tipo = APIRetorno.Parse(int.Parse(Resultados[3]));
            return APIRetorno;
        }
        public static string[] Separar(string separator, string source)
        {
            return source.Split(new string[] { separator }, StringSplitOptions.None);
        }
        public  bool Verificar(string IP, string Port)
        {
            string Resposta = PegarAPI(IP, Port);
            
                APIProxy AP = new APIProxy();
                AP = PegarResultado(Resposta);
                if (AP.response_time > 0.5)
                {
                    return true;
                }
        
            return false;
        }
    }
}
