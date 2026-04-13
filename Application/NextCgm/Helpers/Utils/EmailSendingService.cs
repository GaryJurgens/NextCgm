using MimeKit;
using MailKit.Net.Smtp; // <-- Use MailKit's SmtpClient, not System.Net.Mail


namespace NextCgm.Helpers.Utils
{

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, string? toName = null);
        Task<bool> SendVerificationCodeAsync(string email, string name, string verificationCode);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string email, string name, string verificationCode)
        {
            try
            {
                var subject = _configuration["SmtpSettings:VerificationEmailSubject"] ?? "Email Verification Code";
                var template = _configuration["SmtpSettings:VerificationEmailTemplate"] ??
                    "Hello {Name},\n\nYour verification code is: {VerificationCode}\n\nBest regards,\nThe NextCgm Team";

                var body = template
                    .Replace("{Name}", name)
                    .Replace("{VerificationCode}", verificationCode);

                return await SendEmailAsync(email, subject, body, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, string? toName = null)
        {
            try
            {
                var smtpHost = _configuration["SmtpSettings:Host"];

                var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var smtpUsername = _configuration["SmtpSettings:Username"];
                var smtpPassword = _configuration["SmtpSettings:Password"];
                var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");
                var fromEmail = _configuration["SmtpSettings:FromEmail"] ?? "noreply@nextcgm.co.za";
                var fromName = _configuration["SmtpSettings:FromName"] ?? "NextCgm";
                var timeout = int.Parse(_configuration["SmtpSettings:Timeout"] ?? "30000");

                _logger.LogInformation("Email settings: Host={Host}, Port={Port}, Username={Username}, SSL={EnableSsl}, Timeout={Timeout}ms",
                    smtpHost, smtpPort, smtpUsername, enableSsl, timeout);

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Email settings are not properly configured");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toName ?? to, to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();

                try
                {
                    client.Timeout = timeout;

                    _logger.LogInformation("Connecting to SMTP server {Host}:{Port} with SSL: {EnableSsl}",
                        smtpHost, smtpPort, enableSsl);

                    await client.ConnectAsync(smtpHost, smtpPort, enableSsl);

                    await client.AuthenticateAsync(smtpUsername, smtpPassword);

                    _logger.LogInformation("SMTP authentication successful. Sending email to: {To}", to);

                    await client.SendAsync(message);

                    _logger.LogInformation("Email sent successfully. Disconnecting from SMTP server.");

                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SMTP operation failed: {ErrorMessage}", ex.Message);
                    throw;
                }

                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }
    }

}
