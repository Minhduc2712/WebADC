//using ErpOnlineOrder.Application.DTOs.EmailDTOs;
//using ErpOnlineOrder.Application.Interfaces.Services;
//using MailKit.Security;
//using MimeKit;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ErpOnlineOrder.Application.Services
//{
//    public class EmailService : IEmailService
//    {
//        public EmailService(Emai settings)
//        {
//            _settings = settings.Value;
//        }

//        public async Task SendEmailAsync(string toEmail, string subject, string body)
//        {
//            var email = new MimeMessage();
//            email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Email));
//            email.To.Add(MailboxAddress.Parse(toEmail));
//            email.Subject = subject;

//            var builder = new BodyBuilder { HtmlBody = body };
//            email.Body = builder.ToMessageBody();

//            using var smtp = new SmtpClient();
//            try
//            {
//                // Kết nối đến máy chủ SMTP
//                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);

//                // Đăng nhập
//                await smtp.AuthenticateAsync(_settings.Email, _settings.Password);

//                // Gửi mail
//                await smtp.SendAsync(email);
//            }
//            finally
//            {
//                await smtp.DisconnectAsync(true);
//            }
//        }
//    }
//}



