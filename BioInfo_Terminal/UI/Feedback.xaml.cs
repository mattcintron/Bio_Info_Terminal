using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace BioInfo_Terminal.UI
{
    /// <summary>
    ///     Interaction logic for Update.xaml
    /// </summary>
    public partial class Feedback
    {
        public Feedback()
        {
            InitializeComponent();
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var feedBackUrl = "http://140.176.10.7/bioinfo/feedback/";
            if (string.IsNullOrWhiteSpace(TbFeedback.Text)) return;

            var message = TbFeedback.Text;
            var usr = TbUserName.Text;
            var pl = string.Join(",", " version 1");

            var body = string.Format(TbSubject.Text + " \n{0}{1}\n{2}\n", message, usr, pl);
            var data = Encoding.UTF8.GetBytes(body);
            BtnSubmit.Content = "Submitting...";

            using (var wc = new WebClient())
            {
                try
                {
                    var _ = await wc.UploadDataTaskAsync(new Uri(feedBackUrl + "?sf"), data);
                    MessageBox.Show(@"Thank you for your feedback.", @"Success",
                        MessageBoxButtons.OK, MessageBoxIcon.None);
                    Close();
                }
                catch (Exception)
                {
                    MessageBox.Show(@"There was an error submitting your feedback. " +
                                    @"Please try again later or e-mail the developers directly.");
                }
            }

            BtnSubmit.Content = "Submit";
        }
    }
}