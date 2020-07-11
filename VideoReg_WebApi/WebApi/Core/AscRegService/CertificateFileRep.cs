using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;

namespace WebApi.Core
{
    public interface ICertificateRep
    {
        public X509Certificate2 GetCertificate();
    }

    public class CertificateFileRep : ICertificateRep
    {
        private readonly X509Certificate2 cert;

        public CertificateFileRep(ILogger<CertificateFileRep> log)
        {
            //const string sertificatePath = "public.crt";
            const string certificatePath = "server.pfx";
            try 
            {
                this.cert = new X509Certificate2(certificatePath, "");
                // this.cert = new X509Certificate2(sertificatePath, "v1336pwd");
            }
            catch(Exception e)
            {
                log.LogError($"problem with the certificate {certificatePath} . Error {e.Message}");
                throw;
            }   
        }

        public X509Certificate2 GetCertificate() => cert;
    }
}
