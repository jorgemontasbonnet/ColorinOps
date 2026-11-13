using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Mantenimiento.Api.Helpers;

public class MailHelper
{

    private readonly IConfiguration _config;
    public MailHelper(IConfiguration config)
    {
        _config = config;
    }
    public void SendMail(string to, string subject, string body)
    {
        SendMail(new[] { to }, subject, body, null, null);
    }

    public void DoSend(MailMessage msg)
    {
        var host = _config["MailHost"];
        var port = Convert.ToInt32(_config["MailPort"]);
        var userName = _config["MailUser"];
        var password = _config["MailPwd"];
        var usarSsl = (!string.IsNullOrWhiteSpace(_config["MailSsl"]) && _config["MailSsl"] == "true");


        var client = new SmtpClient(host, port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(userName, password),
            EnableSsl = usarSsl
        };

        client.Send(msg);
    }

    public void SendMail(string to, string subject, string body, byte[]? atts, string? attName)
    {
        SendMail([to], subject, body, atts, attName);
    }

    public void SendMail(IEnumerable<string> tos, string subject, string body, byte[]? atts, string? attName)
    {
        var msg = new MailMessage();

        var from = _config["MailFrom"];

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new Exception("MailFrom configuration is not set.");
        }

        foreach (var t in tos)
        {
            msg.To.Add(t);
        }

        msg.Body = body;
        msg.Subject = subject;

        if (atts != null && atts.Length != 0)
        {
            msg.Attachments.Add(new Attachment(new MemoryStream(atts), attName));
        }
        msg.IsBodyHtml = true;
        msg.From = new MailAddress(from);

        DoSend(msg);
    }
}