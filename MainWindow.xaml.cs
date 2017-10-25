using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using HtmlAgilityPack;
using OpenQA.Selenium.PhantomJS;
using System.Security.Policy;

namespace PoliDown {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private CookieCollection PolitoLogin()
        {
            LoginWindow log = new LoginWindow();
            log.ShowDialog();
            
            if(log.DialogResult==true){
                MessageBox.Show("Login successful");
                return log.jar;
            }
            MessageBox.Show("Login Unsuccessful");
            return null;
        }
        
        private void LoadButton_Click(object sender, RoutedEventArgs e) {
            string s = UrlBox.Text;
            if (s != "")
            {
                string[] spl = s.Split(new[] {'/'});
                //LoadElearning(s, s.Replace(spl.Last(),""));
                LoadDidattica(s, s.Replace(spl.Last(),""),PolitoLogin());
            }
        }

        private void LoadDidattica(string url, string urlBase, CookieCollection jar = null)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.CookieContainer = new CookieContainer();
            foreach (Cookie c in jar)
            {
                request.CookieContainer.Add(c);
            }
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
            request.UseDefaultCredentials = false;
            request.PreAuthenticate = false;
            
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (var s = response.GetResponseStream())
            {
                using (var rs = new StreamReader(s))
                {
                    string str = rs.ReadToEnd();
                }
            }
        }

        private void LoadElearning(string url, string urlBase, CookieContainer jar = null)
        {
            WebClient c = new WebClient();
            
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(c.DownloadString(url));
            List<HtmlNode> lessonsList = page.DocumentNode.Descendants().Where(
                    x => x.Name == "div" && x.Attributes["id"] != null
                ).Where(x => x.Attributes["id"].Value == "lessonList").ToList().First().Descendants()
                .Where(x => x.Name == "li" && x.ParentNode.Attributes["class"] != null)
                .Where(x => x.ParentNode.Attributes["class"].Value == "lezioni").ToList();

            ObservableCollection<DownloadListElement> downloadList = new ObservableCollection<DownloadListElement>();
            DownloadList.ItemsSource = downloadList;
            BottomGrid.Visibility = Visibility.Visible;

            Dictionary<string, string> lessonsPageList = new Dictionary<string, string>();
            lessonsList.ForEach(x => x.Descendants().Where(y => y.Name == "a" && y.Attributes["class"]==null).ToList()
                .ForEach(z => lessonsPageList.Add(z.InnerText, z.Attributes["href"].Value)));
            lessonsPageList.ToList().ForEach(x =>
                {
                    DownloadList.Dispatcher.Invoke(()=>downloadList.Add(new DownloadListElement(x.Key, x.Value)));
                });
                
                
            downloadList.ToList().ForEach(async(x) =>
            {
                var str = await RetreiveVideoUrlElearning(urlBase + "/" + x.fileUrl);
                downloadList.ElementAt(downloadList.IndexOf(x)).fileUrl = str;
                DownloadList.Dispatcher.Invoke(() =>
                {
                    x.CanDownload = true;
                });
            });
        }

        private async Task<string> RetreiveVideoUrlElearning(string pageUrl)
        {
            WebClient c = new WebClient();
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(await c.DownloadStringTaskAsync(pageUrl));
            return page.DocumentNode.Descendants().Where(x => x.Name == "a" && x.Attributes["id"] != null)
                .Where(x => x.Attributes["id"].Value == "aflowplayer").ToList().First().Attributes["href"].Value;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            List<DownloadListElement> downloadList = DownloadList.ItemsSource as List<DownloadListElement>;
            if (downloadList == null)
            {
                return;
            }
            string saveFolder;
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;
                dialog.Description = "Select the videos destination folder!";
                var result =dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    saveFolder = dialog.SelectedPath;
                }
                else
                {
                    return;
                }
            }
            
            downloadList.Where(a=>a.WillDownload && !a.CanDownload).AsParallel().ForAll(async (y) =>
            {
                WebClient videoCli = new WebClient();
                videoCli.DownloadProgressChanged += (send, args) =>
                {
                    DownloadList.Dispatcher.Invoke(() =>
                    {
                        var l = DownloadList.ItemsSource as ObservableCollection<DownloadListElement>;
                        l.ElementAt(l.IndexOf(y)).DownloadPercentage = args.ProgressPercentage;
                    });
                };
                await videoCli.DownloadFileTaskAsync(new Uri(y.fileUrl), saveFolder + "\\" + y.FileName +"."+y.fileUrl.Split('.').Last());
            });
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
