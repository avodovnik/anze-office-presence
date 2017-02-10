using Microsoft.Lync.Model;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace PresenceNotifier
{
    public partial class frmMain : Form
    {
        private const string EndpointConfigurationName = "endpoint";
        private string _endpoint = null;
        private LyncClient _lyncClient = null;

        public frmMain()
        {
            InitializeComponent();

            // load the configuration
            _endpoint = ConfigurationManager.AppSettings[EndpointConfigurationName];
            System.Diagnostics.Debug.WriteLine("Endpoint configured as {0}", _endpoint);
            txtEndpoint.Text = _endpoint;

            SetupLyncClient();
        }

        private bool ConfirmExit()
        {
            var result = MessageBox.Show("Are you sure you want to exit the Presence Notifier?", "Confirmation",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        private void cExitMenuItem_Click(object sender, EventArgs e)
        {
            if (ConfirmExit())
            {
                Application.Exit();
                notifyIcon.Dispose();
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.notifyIcon.ShowBalloonTip(2000, "Application still running...",
                "The App is still running. To exit, use the context menu.", ToolTipIcon.Info);
            e.Cancel = true;

            this.WindowState = FormWindowState.Minimized;
            frmMain_Resize(sender, null);
        }

        private void SetupLyncClient()
        {
            // spin up the Lync client
            _lyncClient = LyncClient.GetClient(false);

            _lyncClient.StateChanged += _lyncClient_StateChanged;

            // assume user signed in first time
            _lyncClient_StateChanged(null, null);
            Contact_ContactInformationChanged(null, null);
        }

        private void _lyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            // if we're signed in
            if (_lyncClient.State == ClientState.SignedIn)
            {
                // attach a presence monitor
                _lyncClient.Self.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
            }
        }

        private void Contact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            // check to see if client is signed-in
            if (_lyncClient.State != ClientState.SignedIn)
            {
                NotifyAvailability("Offline");
                // this is important, because if the user signs out, we actually get notified, then die
                return;
            }

            var availability = ((ContactAvailability)_lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability))
                                .ToString();
            NotifyAvailability(availability);
        }

        private void NotifyAvailability(string presence)
        {
            // yay cross-thread!
            var renderUpdatedPresence = new Action(() =>
            {
                this.lblCurrentStatus.Text = presence;
            });

            if (this.InvokeRequired)
            {
                this.Invoke(renderUpdatedPresence);
            } else
            {
                renderUpdatedPresence();
            }

            // notify the endpoint
        }
    }
}
