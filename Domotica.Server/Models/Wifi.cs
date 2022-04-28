namespace Domotica.Server.Models
{
    public class Parameter
    {
        public Parameter(string ssId, string encryption, int quality)
        {
            SsId = ssId;
            Encryption = encryption;
            Quality = quality;
        }

        public int Quality { get; set; }
        public string Encryption { get; set; }
        public string SsId { get; set; }
    }

    public class Wifi
    {
        public List<Parameter> NetWorks { get; set; } = new List<Parameter>();
    }
}