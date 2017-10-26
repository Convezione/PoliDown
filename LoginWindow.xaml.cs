using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;

namespace PoliDown
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public CookieCollection jar = new CookieCollection();

        public LoginWindow()
        {
            InitializeComponent();
        }
        private string urlAction = "/idp/Authn/X509Mixed/UserPasswordLogin";
        private string method = "POST";
        private string contentType = @"application/x-www-form-urlencoded";
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            if (User.Text == "" || Password.Password == "")
            {
                MessageBox.Show("Insert user and password");
                return;
            }
            
            PhantomJSDriverService srv = PhantomJSDriverService.CreateDefaultService();
            srv.SuppressInitialDiagnosticInformation = true;
            srv.HideCommandPromptWindow = true;
            using (PhantomJSDriver chrDriver = new PhantomJSDriver(srv))
            {
                try
                {
                    chrDriver.Manage().Window.Minimize();
                }
                catch (Exception ex)
                {
                    //Evbb
                }
                chrDriver.Url = "https://idp.polito.it/idp/x509mixed-login";
                chrDriver.Navigate();
                IWebElement element = chrDriver.FindElementById("j_username");
                element.SendKeys(User.Text);
                element = chrDriver.FindElementById("j_password");
                element.SendKeys(Password.Password);
                element = chrDriver.FindElementsByTagName("button").Where((x) =>
                {
                    try
                    {
                        x.FindElement(By.Id("usernamepassword"));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }).First();
                
                element.Click();
                chrDriver.Url = "https://didattica.polito.it/portal/page/portal/home/Studente";
                chrDriver.Navigate();
                if (chrDriver.Manage().Cookies.GetCookieNamed("ShibCookie") != null)
                {
                    chrDriver.Manage().Cookies.AllCookies.ToList()
                        .ForEach(x =>
                            jar.Add(new System.Net.Cookie(x.Name, x.Value, x.Path, x.Domain)));
                    this.DialogResult = true;
                }
                else
                {
                    jar = null;
                    this.DialogResult = false;
                }
                this.Close();
            }
        }
    }
}
