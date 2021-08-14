using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SecurityApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Queue<ListView> listQueue = new Queue<ListView>();
        double gridThick = 0.2;
        SolidColorBrush gridBrushes = Brushes.Gray;

        ListView[,] listView = new ListView[5, 3];
        ListViewItem[,] listViewItem = new ListViewItem[5, 3];

        TextBlock[,] Tit_textBlock = new TextBlock[5, 3];
        TextBlock[,] Cont_textBlock = new TextBlock[5, 3];


        public MainWindow()
        {
            InitializeComponent();
        }

        #region 최소화, 최대화 애니메이션
        const int GWL_STYLE = -16;
        const uint WS_POPUP = 0x80000000;
        const uint WS_CAPTION = 0x00C00000;
        const uint WS_SYSMENU = 0x00080000;
        const uint WS_MINIMIZEBOX = 0x00020000;
        const uint WS_MAXIMIZEBOX = 0x00010000;
        const uint WS_THICKFRAME = 0x00040000;
        const uint WM_NCCALCSIZE = 0x83;

        [DllImport("user32")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, uint newLong);

        #endregion


        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;

            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void StartAnimation()
        {
            // 더블애니메이션 (페이드 인 페이드 아웃)
            DoubleAnimation dba1 = new DoubleAnimation();  // 애니메이션 생성
            dba1.From = 0;   // start 값
            dba1.To = 1;   // end 값
            dba1.Duration = new Duration(TimeSpan.FromSeconds(0.3));  // 1.5초 동안 실행
            // 애니메이션 실행
            this.BeginAnimation(OpacityProperty, dba1);   // 변경할 속성값, 대상애니매이션

        }

        private void EndAnimation()
        {
            // 더블애니메이션 (페이드 인 페이드 아웃)
            DoubleAnimation dba1 = new DoubleAnimation();  // 애니메이션 생성
            dba1.From = 1;   // start 값
            dba1.To = 0;   // end 값
            dba1.Duration = new Duration(TimeSpan.FromSeconds(0.3));  // 1.5초 동안 실행

            // 애니메이션 종료 이벤트 ( ※ BeginAnimation 이전에 있어야 동작함)
            dba1.Completed += (s, a) =>
            {
                this.Close();
            };

            // 애니메이션 실행
            this.BeginAnimation(OpacityProperty, dba1);   // 변경할 속성값, 대상애니매이션
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_CAPTION);

            CenterWindowOnScreen();
            StartAnimation();
            Thread pySecurity = new Thread(new ThreadStart(Get_Python_Security_List));
            pySecurity.IsBackground = true;
            pySecurity.Start();

            RegistryKey myKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            String value = (String)myKey.GetValue("hdev_security", "null");

            if(value != "null")
            {
                boot.IsChecked = true;
                run.appName = "hdev1004";
                run.regKey = "Run";
                run.SetRegKey("hdev_security", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }



            for (int row = 2; row < 5; row += 2)
            {
                for (int colum = 0; colum < 3; colum++)
                {
                    listView[row, colum] = new ListView();
                    listViewItem[row, colum] = new ListViewItem();
                    listViewItem[row, colum].BorderBrush = gridBrushes;
                    listViewItem[row, colum].BorderThickness = new Thickness(gridThick);
                    listViewItem[row, colum].Background = Brushes.Transparent;
                    listViewItem[row, colum].Height = getSize.ActualHeight;
                    listView[row, colum].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ListView_MouseLeftButtonDown);
                    listView[row, colum].PreviewMouseLeftButtonUp += new MouseButtonEventHandler(ListView_MouseLeftButtonUp);


                    Tit_textBlock[row, colum] = new TextBlock();
                    Tit_textBlock[row, colum].Foreground = Brushes.LightGray;
                    Tit_textBlock[row, colum].HorizontalAlignment = HorizontalAlignment.Left;
                    Tit_textBlock[row, colum].VerticalAlignment = VerticalAlignment.Top;
                    Tit_textBlock[row, colum].Margin = new Thickness(10, 10, 0, 0);
                    Tit_textBlock[row, colum].IsHitTestVisible = false;
                    Tit_textBlock[row, colum].FontSize = 12;
                    Tit_textBlock[row, colum].Width = 200;
                    //Tit_textBlock[row, colum].TextWrapping = TextWrapping.Wrap;
                    Tit_textBlock[row, colum].TextTrimming = TextTrimming.CharacterEllipsis;
                    Tit_textBlock[row, colum].Text = "🔄";

                    Cont_textBlock[row, colum] = new TextBlock();
                    Cont_textBlock[row, colum].Foreground = Brushes.Gray;
                    Cont_textBlock[row, colum].HorizontalAlignment = HorizontalAlignment.Left;
                    Cont_textBlock[row, colum].VerticalAlignment = VerticalAlignment.Top;
                    Cont_textBlock[row, colum].Margin = new Thickness(10, 38, 0, 0);
                    Cont_textBlock[row, colum].IsHitTestVisible = false;
                    Cont_textBlock[row, colum].FontSize = 12;
                    Cont_textBlock[row, colum].Width = 200;
                    Cont_textBlock[row, colum].Height = 70;

                    Cont_textBlock[row, colum].TextWrapping = TextWrapping.Wrap;
                    Cont_textBlock[row, colum].TextTrimming = TextTrimming.CharacterEllipsis;
                    Cont_textBlock[row, colum].Text = "🔄";

                    Grid.SetColumn(listView[row, colum], colum);
                    Grid.SetRow(listView[row, colum], row);

                    Grid.SetColumn(Tit_textBlock[row, colum], colum);
                    Grid.SetRow(Tit_textBlock[row, colum], row);

                    Grid.SetColumn(Cont_textBlock[row, colum], colum);
                    Grid.SetRow(Cont_textBlock[row, colum], row);


                    listView[row, colum].Items.Add(listViewItem[row, colum]);
                    MainWindow_Grid.Children.Add(listView[row, colum]);
                    MainWindow_Grid.Children.Add(Tit_textBlock[row, colum]);
                    MainWindow_Grid.Children.Add(Cont_textBlock[row, colum]);

                }
            }

        }
        int curRow = 2;
        int curCol = 0;
        bool isLoad = false;
        private void ListView_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            ListView listview = (ListView)sender;
            listQueue.Enqueue(listview);
            Point p = FindListViewPoint(listview);

            curRow = (int)p.X;
            curCol = (int)p.Y;

            if (listQueue.Count > 1)
            {
                ListView getListView = listQueue.Dequeue();
                ListViewItem listviewItem = (ListViewItem)getListView.Items.GetItemAt(0);
                listviewItem.IsSelected = false;

            }
        }
        private void ListView_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (isLoad == false) return;

            ListView listview = (ListView)sender;
            Point p = FindListViewPoint(listview);

            curRow = (int)p.X;
            curCol = (int)p.Y;
            Point mousePoint = e.GetPosition(listview);
            

            int x = 0;
            int res = 0;
            if (curRow == 2) x = 0;
            else if (curRow == 4) x = 1;

            res = 3 * x + curCol;
            if((mousePoint.X > 0 && mousePoint.X < listview.ActualWidth) && (mousePoint.Y > 0 && mousePoint.Y < listview.ActualHeight))  
                Process.Start(se[res].Link);
        }

        private Point FindListViewPoint(ListView getListView)
        {
            int r = 2, c = 0;
            for (r = 2; r < 5; r += 2)
            {
                for (c = 0; c < 3; c++)
                {
                    if (getListView.Equals(listView[r, c]))
                    {
                        return new Point(r, c);
                    }
                }
            }

            return new Point(r, c);

        }

        security[] se = new security[7];
        #region 파이썬 보안 리스트 받아오기
        private void Get_Python_Security_List()
        {
            try
            {
                String getStr;
                String[] getStrList;
                String[] getStrLine;

                int c = 2, r = 0;
                int idx = 0;

                while (true)
                {
                    try
                    {
                        ProcessStartInfo start = new ProcessStartInfo();
                        start.FileName = "Resources/security.exe";
                        start.UseShellExecute = false;
                        start.RedirectStandardOutput = true;
                        start.WindowStyle = ProcessWindowStyle.Hidden;
                        start.CreateNoWindow = true;

                        using (Process process = Process.Start(start))
                        {
                            using (StreamReader reader = process.StandardOutput)
                            {
                                getStr = reader.ReadToEnd();
                                break;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Thread.Sleep(5000);
                        MessageBox.Show("외부 파일 실행오류(101) : " + err);
                        continue;
                    }
                }
                getStrList = getStr.Split('\n');
                //2 4
                //0 1 2
                foreach (string line in getStrList)
                {

                    getStrLine = line.Split(',');
                    se[idx] = new security();
                    se[idx].Tit = getStrLine[0];
                    se[idx].Date = getStrLine[1];
                    se[idx].Week = getStrLine[2];
                    se[idx].Link = getStrLine[3];
                    se[idx].Cont = getStrLine[4];
                    idx += 1;
                    if (idx == 6) break;

                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    date1.Text = se[0].Date + " (" + se[0].Week + ")";
                    date2.Text = se[3].Date + " (" + se[3].Week + ")";

                    Tit_textBlock[2, 0].Text = se[0].Tit;
                    Tit_textBlock[2, 1].Text = se[1].Tit;
                    Tit_textBlock[2, 2].Text = se[2].Tit;

                    Tit_textBlock[4, 0].Text = se[3].Tit;
                    Tit_textBlock[4, 1].Text = se[4].Tit;
                    Tit_textBlock[4, 2].Text = se[5].Tit;


                    Cont_textBlock[2, 0].Text = se[0].Cont;
                    Cont_textBlock[2, 1].Text = se[1].Cont;
                    Cont_textBlock[2, 2].Text = se[2].Cont;

                    Cont_textBlock[4, 0].Text = se[3].Cont;
                    Cont_textBlock[4, 1].Text = se[4].Cont;
                    Cont_textBlock[4, 2].Text = se[5].Cont;

                }));
                isLoad = true;
            }
            catch (Exception err)
            {
                MessageBox.Show("외부 파일 실행오류(102) : " + err);
            }


        }
        #endregion
        bool isMove = true;
        bool isMax = false;
        
        #region  메뉴 모음
        private void menu01_MouseEnter(object sender, MouseEventArgs e)
        {
            isMove = false;
            menu01.Source = new BitmapImage(new Uri("/SecurityApp;component/Resources/최소화2.png", UriKind.Relative));
        }

        private void menu02_MouseEnter(object sender, MouseEventArgs e)
        {
            isMove = false;
            menu02.Source = new BitmapImage(new Uri("/SecurityApp;component/Resources/최대화2.png", UriKind.Relative));
        }

        private void menu03_MouseEnter(object sender, MouseEventArgs e)
        {
            isMove = false;
            menu03.Source = new BitmapImage(new Uri("/SecurityApp;component/Resources/닫기2.png", UriKind.Relative));
        }


        private void menu01_MouseLeave(object sender, MouseEventArgs e)
        {
            isMove = true;
            menu01.Source = new BitmapImage(new Uri("/SecurityApp;component/Resources/최소화.png", UriKind.Relative));
        }

        private void menu02_MouseLeave(object sender, MouseEventArgs e)
        {
            isMove = true;
            menu02.Source = new BitmapImage(new Uri("/SecurityApp;component/Resources/최대화.png", UriKind.Relative));
        }

        private void menu03_MouseLeave(object sender, MouseEventArgs e)
        {
            isMove = true;
            menu03.Source = new BitmapImage(new Uri("/SecurityApp;component/Resources/닫기.png", UriKind.Relative));
        }

        private void menu01_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void menu02_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMax == false)
            {
                isMax = true;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                isMax = false;
                this.WindowState = WindowState.Normal;
            }
            for (int row = 2; row < 5; row += 2)
            {
                for (int colum = 0; colum < 3; colum++)
                {
                    listViewItem[row, colum].Height = getSize.ActualHeight;
                }
            }
        }


        bool isEndClick = false;
        private void menu03_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEndClick == false)
            {
                EndAnimation();
                isEndClick = true;
            }
            
        }
        #endregion

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isMove)
                this.DragMove();
        }


        Install run = new Install();
        private void boot_Checked(object sender, RoutedEventArgs e)
        {
            run.appName = "hdev1004";
            run.regKey = "Run";
            run.SetRegKey("hdev_security", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");

        }

        private void boot_Unchecked(object sender, RoutedEventArgs e)
        {
            run.DelRegKey("hdev_security");
        }

        private void bootText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            boot.IsChecked = !boot.IsChecked;
        }

        private void bootText_MouseEnter(object sender, MouseEventArgs e)
        {
            isMove = false;
        }
    }

}
