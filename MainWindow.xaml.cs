using System;
using System.Collections.Generic;
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

namespace PoliDown {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private static string didattica = "https://didattica.polito.it/pls/portal30/";
        private static string elearning = "https://elearning.polito.it/main/videolezioni/";
        private void DownloadButton_Click(object sender, RoutedEventArgs e) {
            string s = UrlBox.Text;
            if (s.StartsWith(didattica)) {
                DownloadDidattica(s, didattica);
            }else if (s.StartsWith(elearning)) {
                DownloadElearning(s, elearning);
            }else {
                MessageBox.Show("No valid Url inserted. Please insert Polito Lessons page url");
                return;
            }
            
        }

        private void DownloadDidattica(string url, string urlBase) {
            WebClient c = new WebClient();
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(c.DownloadString(url));
            List<HtmlNode> lessonsList = page.DocumentNode.Descendants().Where(
                x => x.Name == "div" && x.Attributes["id"] != null
                ).Where(x => x.Attributes["id"].Value == "navbar_left_menu").ToList();
            Dictionary<string, string> lessonsPageList = new Dictionary<string, string>();
            lessonsList.ForEach(x => x.Descendants().Where(y => y.Name == "a").ToList().ForEach(z => lessonsPageList.Add(z.InnerText, z.Attributes["href"].Value)));
        }
        private void DownloadElearning(string url, string urlBase) {
            WebClient c = new WebClient();
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(c.DownloadString(url));
            List<HtmlNode> lessonsList = page.DocumentNode.Descendants().Where(
                x => x.Name == "div" && x.Attributes["id"] != null
                ).Where(x => x.Attributes["id"].Value == "lessonList").ToList().First().Descendants().Where(x => x.Name == "li" && x.Attributes["class"] != null).Where(x=>x.Attributes["class"].Value=="h5").ToList();
            Dictionary<string, string> lessonsPageList = new Dictionary<string, string>();
            lessonsList.ForEach(x => x.Descendants().Where(y => y.Name == "a").ToList().ForEach(z => lessonsPageList.Add(z.InnerText, z.Attributes["href"].Value)));
        }
    }
}
//.First().Descendants().Where(x => x.Name == "li" && x.ParentNode.Attributes["id"] == null).ToList();
