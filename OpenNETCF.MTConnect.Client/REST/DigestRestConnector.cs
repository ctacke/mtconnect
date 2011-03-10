using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace OpenNETCF.Web
{
    public class DigestRestConnector : RestConnector
    {
        private CredentialCache Credentials { get; set; }

        public DigestRestConnector(string deviceAddress, string username, string password, bool useSSL)
            : base(deviceAddress)
        {
            Credentials = new CredentialCache();

            NetworkCredential c = new NetworkCredential(username, password);
            Credentials.Add(
                new Uri(string.Format("http{0}://{1}", useSSL ? "s" : string.Empty, DeviceAddress)),
                "Digest", 
                c);
        }

        protected override CredentialCache GenerateCredentials()
        {
            return Credentials;
        }
    }
}
