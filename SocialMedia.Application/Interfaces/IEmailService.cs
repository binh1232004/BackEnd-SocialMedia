using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces
{
    public interface IEmailService
    {
        // Method to generate OTP and store it in Redis
        Task<string> GenerateOtpAsync(string email);

        // Method to send email (used for sending OTP)
        Task SendEmailAsync(string toEmail, string subject, string message);

        // Method to verify the OTP entered by the user
        Task<bool> VerifyOtpAsync(string email, string otp);
    }
}
