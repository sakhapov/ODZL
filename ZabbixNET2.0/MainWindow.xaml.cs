using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zabbix;


namespace ZabbixNET
{
    delegate void simplefunc();
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private settings sForm;
        private settingsController sControll;
        bool md;
        double dx;
        double dy;
        MapVisualElement shape;
        Map ActiveMap;
        public ZabbixConnection zApi;
        double zoom = 1;
        private int i = 0;
        Cursor lastCursor;
        bool MapScroll = false;
        double _x;
        double _y;
        public void Debug(string message)
        {
            
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadMaps()
        {
            lock (zApi.maps.SyncRoot)
            {
                trMaps.Items.Clear();
                foreach (Map map in zApi.maps)
                {
                    TreeViewItem itm = new TreeViewItem();
                    itm.Tag = map;
                    itm.Selected += delegate(object sender2, RoutedEventArgs e2) { ShowMap((Map)itm.Tag); };
                    itm.Header = map.name;
                    itm.FontStyle = FontStyles.Normal;
                    itm.HeaderTemplate = Resources["trMapItemTemplate"] as DataTemplate;
                    trMaps.Items.Add(itm);


                }
                ActiveMap = (Zabbix.Map)(((TreeViewItem)trMaps.Items[0]).Tag);
            }
        }

        private void LoadHosts()
        {
            //groups.Items.Clear();
            //foreach (HostGroup grp in zApi.hostgroups.result)
            //{
            //    groups.Items.Add(grp);
            //}
            //groups.SelectedIndex = 0;
        }

        private void Connect()
        {
            sControll = new settingsController();
            if (sControll.isFirstRun)
            {
                sForm = new settings(sControll);
                sForm.ShowDialog();
            }
            if (zApi != null) { zApi.stop(); }
            trMaps.Items.Clear();
            trTriggers.Items.Clear();

            zApi = new ZabbixConnection(sControll.host,sControll.user, sControll.pass);
            zApi.onUpdate += update_info;
            this.Cursor = Cursors.Wait;
            zApi.connect();
        }

        private void AddLink(link Lnk)
        {
            MapElement el1 = ActiveMap.getById(Lnk.selementid1);
            MapElement el2 = ActiveMap.getById(Lnk.selementid2);
            MapVisualElement vEl1 = (MapVisualElement)el1.Tag;
            MapVisualElement vEl2 = (MapVisualElement)el2.Tag;
            if (el1 == null || el2 == null) { MessageBox.Show("ELEMENT NULL"); }
            Line ln = new Line();
            System.Drawing.Color cl = System.Drawing.ColorTranslator.FromHtml("#" + Lnk.color);
            ln.Stroke = new SolidColorBrush(Colors.Black);
            if (Lnk.drawtype == "4")
            {
                ln.StrokeDashArray = new DoubleCollection();
                ln.StrokeDashArray.Add(3);
                ln.StrokeDashArray.Add(2);
            }
            ln.X1 = el1.x + 30;
            ln.X2 = el2.x + 30;
            ln.Y1 = el1.y + 30;
            ln.Y2 = el2.y + 30;
            //ln.ToolTip = Lnk.drawtype;
            ln.StrokeThickness = 2;
            Lnk.Tag = ln;
            mapCanvas.Children.Add(ln);
            Canvas.SetZIndex(ln, 10);

        }
        private void AddElement(MapElement element)
        {

            MapVisualElement rt = new MapVisualElement(element, ActiveMap, zApi);
            element.Tag = rt;
            rt.MouseDown += rectangle1_MouseDown;
            rt.MouseUp += map_MouseUp;
            Canvas.SetLeft(rt, element.x);
            Canvas.SetTop(rt, element.y);
            mapCanvas.Children.Add(rt);
            Canvas.SetZIndex(rt, 15);
            i++;
            // Index.Content = i.ToString();
        }
        private void ShowMap(Zabbix.Map map)
        {
            ActiveMap = map;
            mapCanvas.Children.Clear();
            mapCanvas.Width = map.width;
            mapCanvas.Height = map.height;
            foreach (Zabbix.MapElement elem in map.selements)
            {
                AddElement(elem);
            }
            foreach (Zabbix.link lnk in map.links)
            {
                AddLink(lnk);
            }

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Connect();

        }

        private void btnVerion_Click(object sender, RoutedEventArgs e)
        {
            //Debug("Apiversion:" + zApi.ApiVersion());
            MessageBox.Show("Версия Zabbix API:" + zApi.ApiVersion(), "Zabbix", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btnGetGroups_Click(object sender, RoutedEventArgs e)
        {
            LoadHosts();
        }

        private void rectangle1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            lastCursor = mapCanvas.Cursor;
            shape = sender as MapVisualElement;//)sender;
            md = shape != null;
            if (!md) { mapCanvas.Cursor = Cursors.Hand; }
            else
            {
                dx = e.GetPosition(shape).X;
                dy = e.GetPosition(shape).Y;
            }
            
            
        }

        private void map_MouseUp(object sender, MouseButtonEventArgs e)
        {
            md = false;
            ActiveMap.update();
            mapCanvas.Cursor = lastCursor;
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (md)
            {
                double newX = e.GetPosition(mapCanvas).X - dx;
                double newY = e.GetPosition(mapCanvas).Y - dy;
                shape.data.x = (int)newX;
                shape.data.y = (int)newY;
                Canvas.SetLeft(shape, newX);
                Canvas.SetTop(shape, newY);
                List<link> ln = shape.map.getAllElementLinks(shape.data.selementid);
                foreach (link l in ln)
                {
                    MapElement el1 = ActiveMap.getById(l.selementid1);
                    MapElement el2 = ActiveMap.getById(l.selementid2);
                    //  if (el1 == null || el2 == null) { MessageBox.Show("ELEMENT NULL"); }
                    Line line = (Line)l.Tag;
                    {
                        line.X1 = el1.x + 30;
                        line.Y1 = el1.y + 30;
                    }
                    {
                        line.X2 = el2.x + 30;
                        line.Y2 = el2.y + 30;
                    }
                }


            }
            else
                if (MapScroll)
                {
                    double _X = e.GetPosition(this).X;
                    double _Y = e.GetPosition(this).Y;
                    if (_X != _x && _Y != _y)
                    {
                        Scroller.ScrollToVerticalOffset(Scroller.VerticalOffset - (_Y - _y));
                        Scroller.ScrollToHorizontalOffset(Scroller.HorizontalOffset - (_X - _x));
                    }
                    _x = e.GetPosition(this).X;
                    _y = e.GetPosition(this).Y;


                }
        }


        private void mapCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnGetMaps_Click(object sender, RoutedEventArgs e)
        {
            LoadMaps();
        }



        private void mapCanvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            MapVisualElement elem = (MapVisualElement)e.Source;
            if (elem != null)
            {
                mapCanvas.ContextMenu.Items.Clear();
                mapCanvas.ContextMenu.Items.Insert(0, "ping");
                mapCanvas.ContextMenu.Items.Insert(0, "tracerote");
                if (elem.data.url != "")
                {
                    mapCanvas.ContextMenu.Items.Insert(0, elem.data.url);
                }
                mapCanvas.ContextMenu.Items.Insert(0, elem.data.label);
                MenuItem itm = new MenuItem();
                itm.Click += delegate(object sender2, RoutedEventArgs e2) { elem.SetState(0); };
                itm.Header = "SetState(0)";
                mapCanvas.ContextMenu.Items.Add(itm);
                itm = new MenuItem();
                itm.Click += delegate(object sender2, RoutedEventArgs e2) { elem.SetState(1); };
                itm.Header = "SetState(1)";
                mapCanvas.ContextMenu.Items.Add(itm);
                itm = new MenuItem();
                itm.Click += delegate(object sender2, RoutedEventArgs e2) { elem.SetState(2); };
                itm.Header = "SetState(2)";
                mapCanvas.ContextMenu.Items.Add(itm);
            }
            else
            {
                mapCanvas.ContextMenu.Items.Clear();
                MenuItem itm = new MenuItem();
                itm.Click += delegate(object sender2, RoutedEventArgs e2) { MessageBox.Show("Добавление линка пока не реализовано"); };
                itm.Header = "Add Link";
                mapCanvas.ContextMenu.Items.Add(itm);
                MenuItem itm2 = new MenuItem();
                itm2.Click += delegate(object sender2, RoutedEventArgs e2) { MessageBox.Show("Добавление объектов пока не реализовано"); };
                itm2.Header = "Add Device";
                mapCanvas.ContextMenu.Items.Add(itm2);
            }

        }





        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnSettings_click(object sender, RoutedEventArgs e)
        {
            sForm = new settings(sControll);
            sForm.ShowDialog();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void mapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta < 0) { zoom *= 1 / (1.1); }
            if (e.Delta > 0) { zoom *= 1.1; }

            ScaleTransform scaleTransform1 = new ScaleTransform(zoom, zoom, 0, 0);
            if (mapCanvas != null) { mapCanvas.RenderTransform = scaleTransform1; }
            mapCanvas.Width = ActiveMap.width * zoom;
            mapCanvas.Height = ActiveMap.height * zoom; ;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Connect();

        }

        private void update_info(UpdateInfoMessage info)
        {
            switch (info.status)
            {
                case "OK":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Подготовка к отображению карты";
                        LoadMaps();
                        this.Cursor = Cursors.Arrow;
                        Status.Content = "online";
                    }));
                    break;
                case "START":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Подключение к серверу";
                    }));
                    break;
                case "LOGIN":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Авторизация на сервере";
                    }));
                    break;
                case "HOSTS":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Получение списка узлов сети...";
                    }));
                    break;
                case "TRIGGERS":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Получение списка триггеров";
                        trHosts.ItemsSource = zApi.hostgroups;
                    }));
                    break;
                case "MAPS":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Инициализация карты сети";
                        //listBox1.ItemsSource = zApi.triggers.collection;
                        //foreach (Zabbix.Trigger tr in zApi.triggers)
                        //{
                        //    vTriggers.Items.Add(tr);
                        //}
                        vTriggers.ItemsSource = zApi.triggers;
                    }));
                    break;
                case "DEBUG":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "Сохранение отладочной информации";
                    }));
                    break;
                case "REFRESH":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Status.Content = "refresh#" + info.message;
                        //this.Title = zApi.triggers.result[0].value;
                        string actMap = ActiveMap.name;
                        LoadMaps();
                        foreach (TreeViewItem item in trMaps.Items)
                        {
                            if (item.Header.ToString() == actMap)
                            {
                                ActiveMap = (Map)item.Tag;
                                item.IsSelected = true;
                            }
                        }
                        ShowMap(ActiveMap);
                    }));
                    break;
                case "REFRESH_ERROR":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        errors.Content = info.message + " errors";
                        Status.Content = "При получении информации произошла ошибка";
                    }));
                    break;
                case "INFO":
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Index.Content = info.message; ;
                    }));
                    break;




                default:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
                    {
                        Index.Content = info.status;
                        errors.Content = info.message;
                    }));
                    break;

            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
            {
                //txtLog.Text += "[" + DateTime.Now.ToString() + "] "+info.sender.ToString()+"\n\t" + info.status + ": " + info.message + "\n\n";
                vLog.Items.Insert(0,new
                        {
                            time = DateTime.Now.ToString(),
                            sender = info.sender.ToString(),
                            status = info.status,
                            message = info.message
                        });
            }));
            

            //if (message == "OK")
            //{
            //    this.Dispatcher.Invoke(DispatcherPriority.Normal, new simplefunc(() =>
            //    {
            //        LoadMaps();
            //        this.Cursor = Cursors.Arrow;
            //        trHosts.ItemsSource = zApi.hostgroups.collection;
            //        listBox1.ItemsSource = zApi.triggers.collection;
            //    }));
            //}
            //else
            //{

            //}
        }



        private void Window_Closed(object sender, EventArgs e)
        {
            zApi.stop();
            Application.Current.Shutdown();
        }
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void vTriggers_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
              e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    Sort(header, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["headerArrowUP"] as DataTemplate;
                        
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                         Resources["headerArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }


                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }
        private void Sort(string sortBy, ListSortDirection direction)
        {
            try
            {
                ICollectionView dataView =
                  CollectionViewSource.GetDefaultView(vTriggers.ItemsSource);

                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription("value", direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
            catch { };
        }

        private void mapCanvas_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Canvas s = sender as Canvas;
            if (s != null)
            {
                Title = "down";
                MapScroll = true;
                lastCursor = mapCanvas.Cursor;
                mapCanvas.Cursor = Cursors.Hand;
                _x = e.GetPosition(this).X;
                _y = e.GetPosition(this).Y;
            }
        }

        private void mapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Title = "UP";
            MapScroll = false;
            mapCanvas.Cursor=lastCursor;

        }
    }
}
