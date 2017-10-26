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

        private async void LoadDidattica(string url, string urlBase, CookieCollection jar = null)
        {
            if (jar == null)
            {
                return;
            }
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
                    string str = await rs.ReadToEndAsync();
                    HtmlDocument page = new HtmlDocument();
                    page.LoadHtml(str);
                    List<HtmlNode> lessonsList = page.DocumentNode.Descendants().Where(
                            x => x.Name == "div" && x.Attributes["id"] != null
                        ).Where(x => x.Attributes["id"].Value == "lessonList").ToList().First().Descendants()
                        .Where((x) =>
                        {
                            bool b = x.Name == "a" && x.Attributes["href"] != null;
                            if (x.Attributes["class"] == null)
                            {
                                b = b && true;
                            }
                            else
                            {
                                b = b && (x.Attributes["class"].Value != "argoLink");
                            }
                            return b;
                            }).Where(x => x.Attributes["href"].Value.StartsWith(url.Split('/').Last().Split('?').First())).ToList();

                    ObservableCollection<DownloadListElement> downloadList = new ObservableCollection<DownloadListElement>();
                    DownloadList.ItemsSource = downloadList;
                    BottomGrid.Visibility = Visibility.Visible;
                    lessonsList.ForEach((x) => { DownloadList.Dispatcher.Invoke(()=>downloadList.Add(new DownloadListElement(x.InnerText, x.Attributes["href"].Value.Replace("amp;","")))); });

                    downloadList.ToList().ForEach(async (x) =>
                    {
                        var strin = await RetreiveVideoUrlElearning(urlBase + x.fileUrl,jar);
                        downloadList.ElementAt(downloadList.IndexOf(x)).fileUrl = strin;
                        DownloadList.Dispatcher.Invoke(() =>
                        {
                            x.CanDownload = true;
                        });
                    });
                }
            }
        }

        private async Task<string> RetreiveVideoUrlElearning(string pageUrl,CookieCollection jar = null)
        {
            HttpWebRequest request = HttpWebRequest.Create(pageUrl) as HttpWebRequest;
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
                    HtmlDocument page = new HtmlDocument();
                    page.LoadHtml(str);
                    return page.DocumentNode.Descendants().Where(x => x.Name == "a" && x.Attributes["id"] != null)
                        .Where(x => x.Attributes["id"].Value == "aflowplayer").ToList().First().Attributes["href"].Value;
                }
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<DownloadListElement> downloadList = DownloadList.ItemsSource as ObservableCollection<DownloadListElement>;
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
            
            downloadList.Where(a=>a.WillDownload && a.CanDownload).AsParallel().ForAll(async (y) =>
            {
                y.CanDownload = false;
                WebClient videoCli = new WebClient();
                videoCli.DownloadProgressChanged += (send, args) =>
                {
                    DownloadList.Dispatcher.Invoke(() =>
                    {
                        y.DownloadPercentage = args.ProgressPercentage;
                    });
                };
                try
                {
                    await videoCli.DownloadFileTaskAsync(new Uri(y.fileUrl),
                        saveFolder + "\\" + y.FileName + "." + y.fileUrl.Split('.').Last());
                }
                catch (Exception ex)
                {
                    y.CanDownload = true;
                }
            });
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
