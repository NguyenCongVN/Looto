namespace Looto.Models.HostScanner
{
    /// <summary>Data about host.</summary>
    public struct HostData
    {
        /// <summary>Host domain name or IP address.</summary>
        public string Host { get; set; }
        /// <summary>Existance of this host.</summary>
        public bool Exists { get; set; }


        /// <summary>
        /// OS type of host
        /// </summary>
        public string OS { get; set; }

        /// <summary>Create new host data.</summary>
        /// <param name="host">Host domain name or IP address.</param>
        /// <param name="exists">Existance of this host.</param>
        /// <param name="os">OS type of this host</param>
        public HostData(string host, bool exists , string os = "")
        {
            Host = host;
            OS = os;
            Exists = exists;
        }
    }
}
