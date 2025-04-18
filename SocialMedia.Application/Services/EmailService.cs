using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using Newtonsoft.Json;
using SocialMedia.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace SocialMedia.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache; 

        public EmailService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        // Send OTP email to user
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);

                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"])); // Sửa ở đây
                mailMessage.Subject = subject;
                mailMessage.Body = new TextPart("plain") { Text = message };

                mailMessage.To.Add(new MailboxAddress("", toEmail));  // Sửa ở đây

                await smtpClient.SendAsync(mailMessage);
                await smtpClient.DisconnectAsync(true); 
            }
        }

        // Generate OTP and store it in MemoryCache with TTL of 2 minutes
        public async Task<string> GenerateOtpAsync(string email)
        {
            var otp = GenerateRandomOtp(); 
            var otpData = new { OTP = otp, Attempts = 0, CreatedAt = DateTime.UtcNow };

            var key = $"otp_{email}";
            _memoryCache.Set(key, otpData, TimeSpan.FromMinutes(2)); 

            return otp; 
        }


        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var key = $"otp_{email}";

            if (!_memoryCache.TryGetValue(key, out dynamic otpData))
                return false;

            var storedOtp = otpData.OTP;
            var attempts = otpData.Attempts;
            var createdAt = otpData.CreatedAt;

            if (attempts >= 5)
            {
                _memoryCache.Remove(key);
                return false;
            }

            if (DateTime.UtcNow > createdAt.AddMinutes(2))
            {
                _memoryCache.Remove(key);
                return false;
            }

            if (storedOtp != otp)
            {
                otpData.Attempts += 1;

                _memoryCache.Set(key, (object)otpData, TimeSpan.FromMinutes(2));

                return false;
            }

            _memoryCache.Remove(key);
            return true;
        }


        private string GenerateRandomOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[4];
                rng.GetBytes(randomBytes);
                var otp = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 1000000;
                return otp.ToString("D6");
            }
        }
    }
}




//using System;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using MimeKit;
//using MailKit.Net.Smtp;
//using Newtonsoft.Json;
//using SocialMedia.Application.Interfaces;
//using System.Globalization;
//using Microsoft.Extensions.Caching.StackExchangeRedis;
//using Microsoft.Extensions.Caching.Distributed;


//namespace SocialMedia.Application.Services
//{
//    public class EmailService : IEmailService
//    {
//        private readonly IConfiguration _configuration;
//        private readonly IDistributedCache _distributedCache;

//        public EmailService(IConfiguration configuration, IDistributedCache distributedCache)
//        {
//            _configuration = configuration;
//            _distributedCache = distributedCache;
//        }

//        // Send OTP email to user
//        public async Task SendEmailAsync(string toEmail, string subject, string message)
//        {
//            var emailSettings = _configuration.GetSection("EmailSettings");

//            using (var smtpClient = new SmtpClient())
//            {
//                await smtpClient.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
//                await smtpClient.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);

//                var mailMessage = new MimeMessage();
//                mailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"])); // Sửa ở đây
//                mailMessage.Subject = subject;
//                mailMessage.Body = new TextPart("plain") { Text = message };


//                mailMessage.To.Add(new MailboxAddress("", toEmail));  // Sửa ở đây

//                await smtpClient.SendAsync(mailMessage);
//                await smtpClient.DisconnectAsync(true); // Ngắt kết nối sau khi gửi
//            }
//        }

//        // Generate OTP and store it in Redis with TTL of 2 minutes
//        public async Task<string> GenerateOtpAsync(string email)
//        {
//            var otp = GenerateRandomOtp(); // Generate OTP, already formatted to 6 digits
//            var otpData = new { OTP = otp, Attempts = 0, CreatedAt = DateTime.UtcNow };

//            var key = $"otp_{email}";
//            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(otpData), new DistributedCacheEntryOptions
//            {
//                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // OTP expires after 2 minutes
//            });

//            return otp; // Directly return the OTP as it's already 6 digits
//        }

//        public async Task<bool> VerifyOtpAsync(string email, string otp)
//        {
//            var key = $"otp_{email}";
//            var cachedOtp = await _distributedCache.GetStringAsync(key);

//            if (string.IsNullOrEmpty(cachedOtp))
//                return false; // OTP doesn't exist or has expired

//            var otpData = JsonConvert.DeserializeObject<dynamic>(cachedOtp);
//            var storedOtp = otpData.OTP;
//            var attempts = otpData.Attempts;
//            var createdAt = otpData.CreatedAt;

//            if (attempts >= 5)
//            {
//                await _distributedCache.RemoveAsync(key); // Remove OTP after 5 attempts
//                return false;
//            }

//            if (DateTime.UtcNow > createdAt.AddMinutes(2)) // OTP expired after 2 minutes
//            {
//                await _distributedCache.RemoveAsync(key); // Remove expired OTP
//                return false;
//            }

//            if (storedOtp != otp)
//            {
//                otpData.Attempts += 1; // Increment attempt counter
//                                       // Set the updated OTP data back into Redis with extended TTL
//                var updatedOtpData = JsonConvert.SerializeObject(otpData);

//                // Chuyển đổi dữ liệu thành byte array trước khi lưu
//                var updatedOtpDataBytes = Encoding.UTF8.GetBytes(updatedOtpData);

//                await _distributedCache.SetAsync(key, updatedOtpDataBytes, new DistributedCacheEntryOptions
//                {
//                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // Extend TTL after each attempt
//                });
//                return false;
//            }

//            // OTP is valid, remove OTP after successful verification
//            await _distributedCache.RemoveAsync(key);
//            return true;
//        }

//        // Generate random 6-digit OTP
//        private string GenerateRandomOtp()
//        {
//            using (var rng = RandomNumberGenerator.Create())
//            {
//                var randomBytes = new byte[4];
//                rng.GetBytes(randomBytes);
//                var otp = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 1000000;
//                return otp.ToString("D6");
//            }
//        }
//    }
//}
