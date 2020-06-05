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
            const string sertificatePath = "public.crt";
            try 
            {
                this.cert = new X509Certificate2(sertificatePath, "v1336pwd");
            }
            catch(Exception e)
            {
                log.LogError($"problem with the certificate {sertificatePath} . Error {e.Message}");
                throw;
            }   
        }

        public X509Certificate2 GetCertificate() => cert;
    }
}
