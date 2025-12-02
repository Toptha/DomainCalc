using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;

namespace DomainCalc
{
    public partial class Form1 : Form
    {
        private WebView2 webView;

        private const string VirtualHostName = "appassets";
        private readonly string wwwrootPath;

        public Form1()
        {
            wwwrootPath = Path.Combine(Application.StartupPath, "wwwroot");
            InitializeComponent();            // designer-generated method (keep this)
            SetupWebViewAndUi();              // <-- your custom setup moved here
            _ = InitializeAsync();            // start async init
        }

        // renamed from InitializeComponent to avoid collision with designer
        private void SetupWebViewAndUi()
        {
            // If the designer already placed controls on the form, you can either use that control
            // or create/add it here. Example: create WebView2 programmatically if designer didn't.
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            // If you also used the designer to create panels/containers, add appropriately.
            this.Controls.Add(webView);

            // you can also set Form properties here if you didn't via designer
            this.ClientSize = new System.Drawing.Size(1000, 720);
            this.Text = "WebView2 — Scientific Calculator (hosted in wwwroot)";
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private async Task InitializeAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);

                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

                if (!Directory.Exists(wwwrootPath))
                {
                    MessageBox.Show($"wwwroot folder not found at: {wwwrootPath}\nPut your index.html and assets there.", "wwwroot missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    VirtualHostName,
                    wwwrootPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                var startUri = $"https://{VirtualHostName}/index.html";
                webView.CoreWebView2.Navigate(startUri);

                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize WebView2: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                var status = e.WebErrorStatus;
                MessageBox.Show($"Navigation failed: {status}", "Navigation error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string msg = e.TryGetWebMessageAsString();
                Console.WriteLine($"Message from web: {msg}");
            }
            catch { }
        }
    }
}
