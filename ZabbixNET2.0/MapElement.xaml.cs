using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zabbix;

//test
namespace ZabbixNET
{
    /// <summary>
    /// Логика взаимодействия для MapElement.xaml
    /// </summary>
    public partial class MapVisualElement : UserControl
    {
       // public string Caption { get; set; }
        public MapElement data{get;set;}
        public Map map;
        public Stream image_on;
        private ZabbixConnection zabbixConnection;
        public Color clFrom = Color.FromArgb(0, 0, 255, 0);
        public Color clTo = Color.FromArgb(0, 0, 155, 0);
        private GradientStopCollection[] states=new GradientStopCollection[4];
        public MapVisualElement(MapElement element,Map parentMap,ZabbixConnection connection)
        {
            InitializeComponent();
           // this.ToolTip = element.url ;
            data = element;
            map = parentMap;
            label.Content = data.label;
            zabbixConnection = connection;
            StringBuilder tooltip = new StringBuilder();
            StringBuilder problems = new StringBuilder();
            StringBuilder works = new StringBuilder();


            states[0] = new GradientStopCollection();
            states[1] = new GradientStopCollection();
            states[2] = new GradientStopCollection();
            states[3] = new GradientStopCollection();

            states[0].Add(new GradientStop(Color.FromArgb(255, 50, 255, 50), 0.0));

            states[0].Add(new GradientStop(Color.FromArgb(255, 00, 56, 34), 1.0));
            states[1].Add(new GradientStop(Colors.Red, 0.0));
            states[1].Add(new GradientStop(Colors.DarkRed, 1.0));
            states[2].Add(new GradientStop(Colors.Yellow, 0.0));
            states[2].Add(new GradientStop(Colors.OrangeRed, 1.0));
            states[3].Add(new GradientStop(Colors.Gray, 0.0));
            states[3].Add(new GradientStop(Colors.DarkGray, 1.0));
            
            this.SetState(0);
            
            tooltip.Append("Триггеры:\n");
            problems.Append("\tПроблемы:\n");
            works.Append("\tНет проблем:\n");
            
            if (data.triggers != null)
            {
                int WorkTrigger = data.triggers.Length;
                foreach (Zabbix.Trigger tr in data.triggers)
                {
                    if (tr.value!="0")
                    {
                        //problems.Append("\t\t" + tr.description.Replace("{HOSTNAME}", tr.host.host) + "[" + tr.lastchangeDateTime.ToString() + "]\n");
                        tltp.Items.Add(new { text = tr.description.Replace("{HOSTNAME}", tr.host.host), value = tr.value });
                    }
                    //else { works.Append("\t\t" +tr.description.Replace("{HOSTNAME}", tr.host.host) + "[" + tr.lastchangeDateTime.ToString() + "]\n"); }
                    WorkTrigger -= Int32.Parse(tr.value);
                }
                foreach (Zabbix.Trigger tr in data.triggers)
                {
                    if (tr.value == "0")
                    {
                        //problems.Append("\t\t" + tr.description.Replace("{HOSTNAME}", tr.host.host) + "[" + tr.lastchangeDateTime.ToString() + "]\n");
                        tltp.Items.Add(new { text = tr.description.Replace("{HOSTNAME}", tr.host.host), value = tr.value });
                    }
                    //else { works.Append("\t\t" +tr.description.Replace("{HOSTNAME}", tr.host.host) + "[" + tr.lastchangeDateTime.ToString() + "]\n"); }
                    //WorkTrigger -= Int32.Parse(tr.value);
                }
                if (WorkTrigger == data.triggers.Length) { SetState(0); }else
                    if (WorkTrigger == 0) { SetState(1); }
                    else
                    { SetState(2); }
            }
            if (data.elementtype != "0")
            { this.SetState(3); } 
            //this.ToolTip = "Триггеры:\n"+problems+works;
            if (!File.Exists("images/"+data.iconid_off))
            {
                image_on = zabbixConnection.GetImageByID(data.iconid_off);
                FileStream fl = File.Create("images/" + data.iconid_off);
                image_on.CopyTo(fl);
                fl.Close();
            }
            BitmapImage bi = new BitmapImage();
            byte[] bytes = File.ReadAllBytes("images/" + data.iconid_off);
            MemoryStream ms = new MemoryStream(bytes);
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            image.Source = bi;


            //myBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
        }

        public void SetState(int state)
        {
            LinearGradientBrush br = (LinearGradientBrush)bgrd.Fill;
            br.GradientStops = states[state];
        }
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Canvas.SetLeft(label, (this.bgrd.ActualWidth - label.ActualWidth) / 2);
        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (label.ActualWidth > bgrd.ActualWidth) { bgrd.Width = label.ActualWidth + 10; };

            Canvas.SetLeft(label, (this.bgrd.ActualWidth - label.ActualWidth) / 2);
            Canvas.SetLeft(image, (this.bgrd.ActualWidth - image.ActualWidth) / 2);
            
        }
    }
}
