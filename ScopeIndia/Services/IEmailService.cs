using System.Threading.Tasks;

namespace ScopeIndia.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}