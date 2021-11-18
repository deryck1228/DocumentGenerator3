using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.DocumentDelivery
{
    public class DeliverDocumentService_email
    {
        public DocumentData DocumentData { get; set; }
        public async void SendViaEmail()
        {
            var config = new ConfigurationBuilder()
                 .AddEnvironmentVariables()
                 .Build();
            var apiKey = config["SendGridAPIKey"];
            var fromEmailAddress = config["FromEmailAddress"];

            var emailSettings = (DeliverySettings_email)DocumentData.originalPayload.delivery_method.settings;

            Stream stream = new MemoryStream(0);

            stream.Write(DocumentData.fileContents, 0, (int)DocumentData.fileContents.Length);
            stream.Seek(0, SeekOrigin.Begin);

            string documentName = DocumentData.originalPayload.document_name + DocumentData.originalPayload.document_type;
            string mimeType = "";

            //TODO refactor to improve for Open/Close

            switch (DocumentData.originalPayload.document_type)
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".docx":
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmailAddress, emailSettings.from_name);
            var subject = emailSettings.subject_line;
            var to = new EmailAddress(emailSettings.to_email, emailSettings.to_name);
            var plainTextContent = emailSettings.body_text;
            var htmlContent = emailSettings.body_text;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await msg.AddAttachmentAsync(documentName, stream, mimeType);
            var emailResponse = await client.SendEmailAsync(msg);
        }
    }
}
