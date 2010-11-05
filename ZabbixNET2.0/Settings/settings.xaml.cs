using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZabbixNET
{
    /// <summary>
    /// Логика взаимодействия для settings.xaml
    /// </summary>
    public partial class settings : Window
    {
        private settingsController sControll;
        public settings(settingsController controller)
        {
            InitializeComponent();
            sControll = controller;
            this.DataContext = sControll;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            sControll.save();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            sControll.reload();
            this.Close();
        }
    }
}
