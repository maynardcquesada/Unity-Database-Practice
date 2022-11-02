using System;
using System.IO;
using System.Threading;
using UnityEngine;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;

public class GmailSender : MonoBehaviour
{
    public void GmailStart(string emailTo)
    {
        string[] Scopes = { GmailService.Scope.GmailSend };
        string ApplicationName = "Unity Database Practice 1";
        string emailBody = "test";
        string emailSub = "test";
        SendEmailAsync(Scopes, ApplicationName, emailTo, emailSub, emailBody);
    }

    private string Base64UrlEncode(string input)
    {
        var data = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private void SendEmailAsync(string[] Scopes, string appName, string emailTo, string emailSub, string emailBody)
    {
        try
        {
            UserCredential credential;
            //read your credentials file
            using (FileStream stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                print(stream);
                string credPath = "token.json";
                print(credPath);
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromStream(stream).Secrets,Scopes,"user",CancellationToken.None,new FileDataStore(credPath, true)).Result;
                print(credential);
            }

            string message = $"To: {emailTo}\r\nSubject: {emailSub}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n<h1>{emailBody}</h1>";
            ////call your gmail service
            var service = new GmailService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = appName });
            var msg = new Google.Apis.Gmail.v1.Data.Message();
            msg.Raw = Base64UrlEncode(message.ToString());
            service.Users.Messages.Send(msg, "me").Execute();
        }
        catch (Google.GoogleApiException e)
        {
            print(e.Message);
        }
    }
}
