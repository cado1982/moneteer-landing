namespace Moneteer.Landing.V2.Helpers
{
    public class SmtpConnectionInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
    }
}
