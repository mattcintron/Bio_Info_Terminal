using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using BioInfo_Terminal.Methods.Dialog_Handling;

namespace BioInfo_Terminal.Methods.Operations
{
    internal class EmailOperations : IOperation
    {
        private readonly ChemOperations _chemOperations;

        private int _dialougeTarget;

        internal EmailOperations()
        {
            Users = string.Empty;
            _dialougeTarget = 0;
            _chemOperations = new ChemOperations();
            FillEmailRecords();
        }

        internal EmailOperations(ChemOperations co)
        {
            Users = string.Empty;
            _dialougeTarget = 0;
            _chemOperations = co;
            FillEmailRecords();
        }

        internal string EmailContent { get; set; } //all current saved data txt to email
        internal Dictionary<string, string> EmailRecords { get; set; } //all curent email records 
        internal Dictionary<int, string> EmailIDs { get; set; } //all curent email records 
        internal List<string> Emails { get; set; } //all usable emails
        internal string Users { get; set; } //string containing target users

        public void FillValues(Dictionary<string, string> values)
        {
            if (values["Users"] != string.Empty)
                Users = values["Users"];
        }

        public string RunOperations(string id, string text, ref int dialougeTarget)
        {
            if (id == "Send_Email")
            {
                var response = ProcessEmail_Request(text);
                dialougeTarget = _dialougeTarget;
                return response;
            }

            return null;
        }

        private string TargetData(ref string text, ChemOperations chemOperations, bool data)
        {
            if (data)
            {
                EmailContent = string.Empty;

                EmailContent += "Chemical is " + chemOperations.Chemical + "\n";
                if (text.Contains("mw") || text.Contains("molecular weight") || text.Contains("full"))
                    EmailContent += "the Molecular Weight is " + chemOperations.MolecularWeight + "\n";
                if (text.Contains("formula") || text.Contains("full"))
                    EmailContent += "the Molecular Formula is " + chemOperations.ChemicalFormula + "\n";
                if (text.Contains("synonym") || text.Contains("full"))
                    EmailContent += "the Synonym's are " + chemOperations.Synonyms + "\n";
                if (text.Contains("cid") || text.Contains("full"))
                    EmailContent += "the current CID's are " + chemOperations.CiDs + "\n";
                if (text.Contains("keys") || text.Contains("full"))
                    EmailContent += "the current INCHI keys are " + chemOperations.InchiKeys + "\n";

                text = text.Replace("mw", "");
                text = text.Replace(" molecular weight", "");
                text = text.Replace("formula", "");
                text = text.Replace("synonym", "");
                text = text.Replace(" cid", "");
                text = text.Replace("keys", "");
                text = text.Replace("full", "");
                text = text.Replace(",", "");
            }

            return EmailContent;
        }

        internal string ReportEmailList()
        {
            var users = ReportFullList();
            return users;
        }

        private string ReportFullList()
        {
            var users = string.Empty;
            users += " Matt ID 1    ---";
            users += " Serhiy ID 2  ---";
            users += " Marathe ID 4  ---";
            users += " Myrtle ID 5  ---";
            users += " Gudmundsson ID 6  ---";
            users += " michael ID 7  ---";
            users += " Humphreys ID 8  ---";
            users += " Robert ID 9  ---";
            users += " Olah ID 11  ---";
            users += " vanderwall ID 12  ---";
            users += " Bruce ID 13  ---";
            users += " Colleen ID 14  ---";
            users += " Bethanne ID 15  ---";
            users += " Nelson ID 16  ---";
            users += " Yan ID 17  ---";
            users += " jan lucas ID 18  ---";
            users += " Gayle ID 19  ---";
            users += " Petia ID 20  ---";
            users += " Joelle ID 21  ---";
            users += " Burke ID 22  ---";
            users += " Scavetta ID 23  ---";
            users += " Rostyslav ID 24  ---";
            users += " Thayasivam ID 25  ---";

            return users;
        }

        internal string EmailInternaly(string emailadress, string user)
        {
            try
            {
                var message = new MailMessage();
                var smtpClient = new SmtpClient
                {
                    Host = "mailhost.net.bms.com", // smtp server address here…
                    Port = 25,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };
                message.IsBodyHtml = true;
                var fromAddress = new MailAddress("DonNotReply@AlexaBioInfo.com");
                message.From = fromAddress;
                message.To.Add(emailadress);
                message.Subject = "Bio Info Email Test";
                message.IsBodyHtml = true;
                message.Body = message.Body = "Dear " + user + "<br /> <br />" +
                                              "Hello from Bio Info AI - you have sent and email with no Data specified or their was none available to send." +
                                              "<br /> <br />"
                                              + "Have A nice Day" + "<br /> " +
                                              "BioInfo AI system";
                smtpClient.Send(message);
                return "Email Sent";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal void ClearData()
        {
            EmailContent = string.Empty;
            Users = string.Empty;
        }

        internal void FillEmailRecords()
        {
            EmailIDs = new Dictionary<int, string>();
            EmailIDs.Add(1,"matt");
            EmailIDs.Add(2, "serhiy");
            EmailIDs.Add(3,"marathe");
            EmailIDs.Add(4,"myrtle");
            EmailIDs.Add(5,"gudmundsson");
            EmailIDs.Add(6,"humphreys");
            EmailIDs.Add(7,"robert");
            EmailIDs.Add(8,"hillman");
            EmailIDs.Add(9,"olah");
            EmailIDs.Add(10,"vanderwall");
            EmailIDs.Add(11,"bruce");
            EmailIDs.Add(12,"colleen");
            EmailIDs.Add(13,"bethanne");
            EmailIDs.Add(14,"nelson");
            EmailIDs.Add(15,"yan");
            EmailIDs.Add(16,"jan lucas");
            EmailIDs.Add(17,"gayle");
            EmailIDs.Add(18,"petia");
            EmailIDs.Add(19,"joelle");
            EmailIDs.Add(20,"burke");
            EmailIDs.Add(21,"scavetta");
            EmailIDs.Add(22,"rostyslav");
            EmailIDs.Add(23,"thayasivam");
            EmailIDs.Add(24,"luciano");
            EmailIDs.Add(25, "lois");


            EmailRecords = new Dictionary<string, string>();
            EmailRecords.Add("matt", "Matthew.Cintron@bms.com");
            EmailRecords.Add("serhiy", "serhiy.hnatyshyn @bms.com"); 
            EmailRecords.Add("marathe", "punit.marathe@bms.com");
            EmailRecords.Add("myrtle", "Myrtle.Davis@bms.com");
            EmailRecords.Add("gudmundsson", "olafur.gudmundsson@bms.com");
            EmailRecords.Add("humphreys", "william.humphreys@bms.com");
            EmailRecords.Add("robert", "robert.penhallow@bms");
            EmailRecords.Add("hillman", "Mark.Hillman@bms.com");
            EmailRecords.Add("olah", "Timothy.Olah@bms.com");
            EmailRecords.Add("vanderwall", "Dana.Vanderwall@bms.com");
            EmailRecords.Add("bruce", "Bruce.Car@bms.com");
            EmailRecords.Add("colleen", "Colleen.Mcnaney@bms.com");
            EmailRecords.Add("bethanne", "bethanne.warrack@bms.com");
            EmailRecords.Add("nelson", "David.Nelson@bms.com");
            EmailRecords.Add("yan", "yan.he@bms.com");
            EmailRecords.Add("jan lucas", "Jan-Lucas.Ott@bms.com");
            EmailRecords.Add("gayle", "Gayle.Hart@bms.com");
            EmailRecords.Add("petia", "petia.shipkova@bms.com");
            EmailRecords.Add("joelle", "joelle.onorato@bms.com");
            EmailRecords.Add("burke", "burkew9@students.rowan.edu");
            EmailRecords.Add("scavetta", "scavettaj0@students.rowan.edu");
            EmailRecords.Add("rostyslav", "hnatyshyr4@students.rowan.edu");
            EmailRecords.Add("thayasivam", "Thayasivam@rowan.edu");
            EmailRecords.Add("luciano", "luciano.mueller@bms.com");
            EmailRecords.Add("lois", "Lois.Lehman-McKeeman@bms.com");
        }

        #region Send Email - Operations

        internal string EmailInfo(string emailadress, string content, string user)
        {
            try
            {
                var message = new MailMessage {IsBodyHtml = true};
                var smtpClient = new SmtpClient
                {
                    Host = "mailhost.net.bms.com", //smtp server address here…
                    Port = 25,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };
                return LaunchEmail(emailadress, content, user, message, smtpClient);
            }
            catch
            {
                string senderID = "bioinformaticsmanager@gmail.com";
                string senderPassword = "ty56ty56";
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                SmtpClient smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",  //smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(senderID, senderPassword),
                    Timeout = 30000,
                };
                return LaunchEmail(emailadress, content, user, message, smtpClient);
            }
        }

        private static string LaunchEmail(string emailadress, string content, string user, MailMessage message,
            SmtpClient smtpClient)
        {
            message.IsBodyHtml = true;
            var fromAddress = new MailAddress("DonNotReply@BioInfo.com");
            message.From = fromAddress;
            message.To.Add(emailadress);
            message.Subject = "Chemical Info Result";
            message.IsBodyHtml = true;
            if (content == null || content.Contains("state the target User to email") ||
                content.Contains("Email Send- failed unknown user"))
                content = " No Data to email- please perform a request to receive processed data \n " +
                          "example: Get information on chemical sodium";
            content = content.Replace("\n", "<br />");
            message.Body = "Dear " + user + "<br /> <br />" +
                           content + "<br /> <br />" +
                           "<br />" +
                           "Have A nice Day" + "<br /> " +
                           "BioInfo AI system";
            smtpClient.Send(message);
            return "Email Sent";
        }

        private void EmailList(string user, ref string response)
        {
            user = user.Replace("user ", "");
            var isNumeric = int.TryParse(user, out _);
            if (isNumeric)
            {
                int target = Convert.ToInt32(user);
                if (EmailIDs.ContainsKey(target))
                {
                    user = EmailIDs[target];
                    EmailInfo(EmailRecords[user], EmailContent, user);
                    response += user + " ";
                }
                return;
            }
            if (EmailRecords.ContainsKey(user))
            {
                EmailInfo(EmailRecords[user], EmailContent, user);
                response += user + " ";
            }
        }

        internal string SendEmail(bool data, string text, ChemOperations chemOperations)
        {
            if (string.IsNullOrEmpty(Users))
                return "Email Send-failed unknown user";
            Users = Users.ToLower();
            var response = "email sent To:";

            EmailContent = TargetData(ref text, chemOperations, data);
            EmailList(Users, ref response);
            if (response == "email sent To:")
                return " Email Send- failed unknown user please try emailing a target user with an ID . \n" +
                       "1- Matt.   \n" +
                       "2- Serhiy. \n" +
                       "        Example say- Email User 1 data-  to email user matt";
            return response;
        }

        internal string SendDictationEmail(string emailAddress, string text)
        {
            var response = "email sent To: " + emailAddress;
            var items = emailAddress.Split(new[] {"@"}, StringSplitOptions.None);
            var user = items[0];

            EmailInfo(emailAddress, text, user);
            if (response == "email sent To:")
                return " Email Send- failed unknown user please try emailing a target user with an ID . \n" +
                       "1- Matt.   \n" +
                       "2- Serhiy. \n" +
                       "        Example say- Email User 1 data-  to email user matt";
            return response;
        }

        public string ProcessEmail_Request(string text)
        {
            var response = string.Empty;
            if (text.Contains("show full email") || text.Contains("show full users") || text.Contains("show all users"))
            {
                response = ReportEmailList();
                _dialougeTarget = 0;
                return response;
            }

            //try
            //{
                var data = false;
                var addWords = false;

                var result = CheckProgress_Emails(ref text, ref response, ref data, ref addWords);
                if (result) return response;
                response = SendEmail(data, text, _chemOperations);

                _dialougeTarget = 0;
                ClearData();
                return response;
        //}
        //    catch
        //    {
        //        response =
        //            "I'm afraid I don't know who you are trying to email, please identify the user and try again";

        //        _dialougeTarget = 0;
        //        return response;
        //    }
}

        internal bool CheckProgress_Emails(ref string text, ref string response, ref bool data, ref bool addWords)
        {
            if (text.Contains("data"))
            {
                data = true;
                text = text.Replace("data", "");
            }

            if (_dialougeTarget == 2 && string.IsNullOrEmpty(Users))
            {
                Users = text;
                return false;
            }

            if (string.IsNullOrEmpty(Users))
            {
                _dialougeTarget = 2;
                response = "Ok please state the target User to email";
                return true;
            }

            return false;
        }

        #endregion
    }
}