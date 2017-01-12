using com.gseo.network;
using NativeWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.gseo.browser;
using WatchDog;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using com.gseo.browser.uri;
using System.Threading;
using System.Runtime.InteropServices;
using com.gseo.ad;
using com.gseo.persistent.local;
using com.gseo.persistent.remote;


namespace PCGuardWinForm
{
    public partial class MainForm : Form
    {
        private string _title = "PC Guard";
        private string _validSSID = "gseo_members";

        private bool IS_ENABLED_WATCHDOG = false; // Watch Dog Enabler
        private string _watchdogLockFileName = "watchdoglock"; // Watch Dog Lock flag, A lock file is used to temporarily disable the watchdog from monitoring and reviving the monitored application for debugging purposes.
        private WatchdogWatcher _watchdogWatcher = null;
        private string _watchdogName = "WatchDog";

        private object _getLocker = new object(); //抓取browser history的鎖
        private object _publishLocker = new object(); //抓取發佈到遠端的鎖

        delegate void SetTextCallback(string text);

      
        public MainForm()
        {
            InitializeComponent();
            
            if (IS_ENABLED_WATCHDOG)
            {
                int watchDogMonitorInterval = 5000;

                try
                {
                    watchDogMonitorInterval = Convert.ToInt32(ConfigurationManager.AppSettings["WatchDogMonitorInterval"]);
                    if (watchDogMonitorInterval != 0)
                    {
                        watchDogMonitorInterval = 5000;
                    }
                }
                catch (Exception)
                {
                    watchDogMonitorInterval = 5000;
                }
                _watchdogWatcher = new WatchdogWatcher(_watchdogName, _watchdogName + ".exe", watchDogMonitorInterval);  
            }
        }

        /// <summary>
        /// Form Load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

            MaximizeBox = false;

            Initial(); //初始化UI
            Run(); //執行監控
        }

        /// <summary>
        /// Resize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                Minimized();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                //Normal();
                Minimized();
            }
        }

        /// <summary>
        /// Form closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //按下關閉鈕變最小化
            e.Cancel = true;
            MainForm_Resize(null, null);
        }

        /// <summary>
        /// Winform初始化
        /// </summary>
        private void Initial()
        {

            //設定工作列標題與氣泡文字
            notifyIcon1.Text = _title;
            notifyIcon1.Click += (sender, e) =>
            {
                Normal();
            };
            ShowMsg(_title + " is still running");


            //Show Current SSID選單項目
            MenuItem m1 = new MenuItem();
            m1.Index = 1;
            m1.Text = "Show Current SSID";
            m1.Click += (sender, e) =>
            {
                WifiNetwork wifi = new WifiNetwork();
                MessageBox.Show("Current SSID is " + wifi.GetCurrentConnected().SSID);
            };

            //Lock/UnLock Watchdog選單項目
            MenuItem m2 = new MenuItem();
            m2.Checked = false;
            m2.Index = 2;
            if (!File.Exists(_watchdogLockFileName))
                m2.Text = "UnLock Watchdog";
            else
                m2.Text = "Lock Watchdog";
            m2.Click += (sender, e) =>
            {
                try
                {
                    if (!File.Exists(_watchdogLockFileName))
                    {
                        File.Create(_watchdogLockFileName);
                        m2.Text = "Lock Watchdog";
                        m2.Checked = true;
                        MessageBox.Show("Unlock WatchDog");
                    }
                    else
                    {
                        File.Delete(_watchdogLockFileName);
                        m2.Text = "UnLock Watchdog";
                        m2.Checked = false;
                        MessageBox.Show("Lock WatchDog");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception : " + ex.ToString());
                }
            };
            
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(m1);
            contextMenu.MenuItems.Add(m2);
            notifyIcon1.ContextMenu = contextMenu;

            Minimized();
        }

        /// <summary>
        /// 啟動監察
        /// </summary>
        private void Run()
        {
            Thread netThread = new Thread(new ThreadStart(StartNetworkMonitoring));
            netThread.Start();

            Thread ieThread = new Thread(new ThreadStart(StartIEMonitoring));
            ieThread.Priority = ThreadPriority.BelowNormal;
            ieThread.Start();

            Thread chromeThread = new Thread(new ThreadStart(StartChromeMonitoring));
            chromeThread.Priority = ThreadPriority.BelowNormal;
            chromeThread.Start();

            Thread firefoxThread = new Thread(new ThreadStart(StartFirefoxMonitoring));
            firefoxThread.Priority = ThreadPriority.BelowNormal;
            firefoxThread.Start();
        }

        /// <summary>
        /// 啟動無線網路的監控
        /// </summary>
        private void StartNetworkMonitoring()
        {
            WifiNetwork wifi = new WifiNetwork();

            while (true)
            {
                string ssid = wifi.GetCurrentConnected().SSID;
                SetText("Current SSID is " + ssid);

                //Show alert ex: 非gseo_members無網連接時警示非法
                bool IsWifiConnected = !"NONE".Equals(ssid);
                bool IsValid = _validSSID.Equals(ssid);
                if (IsWifiConnected && !IsValid)
                {
                    ShowMsg("您正使用的是非gseo_members的非法連線");
                }

                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// 啟動IE history儲存到local DB
        /// </summary>
        private void StartIEMonitoring()
        {
            IBrowser browser = new InternetExplorer();
            LocalDB localdb = new LocalDB("ie");
            HistoryExporter export = new HistoryExporter();
            Person person = new Person();

            while (true)
            {
                CatchBrowserHistory(browser, localdb);
                PublishToCentral(export, localdb, person);

                Thread.Sleep(15000);
            }
        }

        /// <summary>
        /// 啟動Chrome history儲存到local DB
        /// </summary>
        private void StartChromeMonitoring()
        {
            IBrowser browser = new Chrome();
            LocalDB localdb = new LocalDB("chrome");
            HistoryExporter export = new HistoryExporter();
            Person person = new Person();

            while (true)
            {
                CatchBrowserHistory(browser, localdb);
                PublishToCentral(export, localdb, person);

                Thread.Sleep(15000);
            }
        }

        /// <summary>
        /// 啟動Firefox history儲存到local DB
        /// </summary>
        private void StartFirefoxMonitoring()
        {
            IBrowser browser = new Firefox();
            LocalDB localdb = new LocalDB("firefox");
            HistoryExporter export = new HistoryExporter();
            Person person = new Person();

            while (true)
            {
                CatchBrowserHistory(browser, localdb);
                PublishToCentral(export, localdb, person);

                Thread.Sleep(15000);
            }
        }


        /// <summary>
        /// 截取browser的歷史瀏覽記錄，並儲存在Local DB
        /// </summary>
        /// <param name="browser">瀏覽器</param>
        /// <param name="localdb">本地資料庫</param>
        private void CatchBrowserHistory(IBrowser browser, LocalDB localdb)
        {
            IEnumerable<URL> browserList;
            lock (_getLocker)
            {
                browserList = browser.GetHistory();
                if (browserList != null && browserList.ToList().Count > 0)
                {
                    localdb.CreateTable();
                    localdb.InsertBulkHistory(browserList.ToList());
                }
            }
        }

        /// <summary>
        /// Local DB資料發佈到遠端儲存下來
        /// 一次抓100筆本地資料，拋送遠端記錄下來，最後將此100筆從本地刪除
        /// </summary>
        /// <param name="export">匯出資料物件</param>
        /// <param name="localdb">本地資料庫</param>
        /// <param name="person">個人物件</param>
        private void PublishToCentral(HistoryExporter export, LocalDB localdb, Person person)
        {
            lock (_publishLocker)
            {
                if (export.IsAvailable())
                {
                    List<URL> dbList = localdb.GetHistorys();
                    long maxNum = 0;
                    if (dbList != null && dbList.Count > 0)
                    {
                        foreach (URL uu in dbList)
                        {
                            export.Export(uu, person.GetADAccount(), person.GetDepartment());
                            if (uu.Num > maxNum)
                                maxNum = uu.Num;
                        }
                        localdb.Delete(maxNum);
                    }
                }
            }
        }

        /// <summary>
        /// 顯示泡泡訊息
        /// </summary>
        /// <param name="msg">訊息內文</param>
        private void ShowMsg(string msg)
        {
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = msg;
            notifyIcon1.ShowBalloonTip(2000);
        }

        /// <summary>
        /// UI上寫訊息
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            if (this.lblMsg.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblMsg.Text = text;
            }
        }

        /// <summary>
        /// 縮到工作列
        /// </summary>
        private void Minimized()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(500);

            this.ShowInTaskbar = false;
            this.Hide();
        }

        /// <summary>
        /// 回復原視窗
        /// </summary>
        private void Normal()
        {
            notifyIcon1.Visible = false;
            this.ShowInTaskbar = true;
            this.Show();
        }

        
    }
}
