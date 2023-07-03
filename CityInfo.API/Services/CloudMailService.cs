namespace CityInfo.API.Services
{
    public class CloudMailService : ILocalMailService
    {
        private  string _mailTo = string.Empty ;
        private  string _mailFrom = string.Empty ;

        public CloudMailService(IConfiguration configuration)
        {
            string mailTo = configuration["mailSettings:mailToAddress"];
            string mailFrom = configuration["mailSettings:mailFromAddress"];

            _mailTo = mailTo;
            _mailFrom = mailFrom;
        }

        public void Send(string subject, string body)
        {
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, " + $"Wth {nameof(CloudMailService)}.");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
        }
    }
}
