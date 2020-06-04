using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

namespace WebApi.Core
{
    public class CertificateFileRep : ICertificateRep
    {
        private readonly X509Certificate2 cert;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Ignore.</exception>
        public CertificateFileRep()
        {
            const string sertificatePath = "public.crt";
            this.cert = new X509Certificate2(sertificatePath, "v1336pwd");       
        }

        public X509Certificate2 GetCertificate() => cert;
    }

    public interface ICertificateRep
    {
        public X509Certificate2 GetCertificate();
    }
}
