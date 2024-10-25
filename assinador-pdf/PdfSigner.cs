using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using System.ComponentModel.Design;
using System.Reflection.PortableExecutable;

namespace SignPdf
{
    class PDFSigner
    {
        public void Sign(string sourceDocument, string destinationPath, string privateKeyPath, string keyPassword, string reason, string location)
        {
            using (FileStream privateKeyStream = new FileStream(privateKeyPath, FileMode.Open, FileAccess.Read))
            {
                Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, keyPassword.ToCharArray());

                
                string alias = null;
                foreach (string tAlias in pk12.Aliases)
                {
                    if (pk12.IsKeyEntry(tAlias))
                    {
                        alias = tAlias;
                        break;
                    }
                }

                var pk = pk12.GetKey(alias).Key;
                var ce = pk12.GetCertificateChain(alias);
                var chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];
                for (int k = 0; k < ce.Length; ++k)
                    chain[k] = ce[k].Certificate;

                // Leitor e carimbador
                using (PdfReader reader = new PdfReader(sourceDocument))
                using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    StampingProperties properties = new StampingProperties();
                    PdfSigner signer = new PdfSigner(reader, fout, properties);

                    PdfSignatureAppearance appearance = signer.GetSignatureAppearance()
                        .SetReason(reason)
                        .SetLocation(location);

                    IExternalSignature pks = new PrivateKeySignature(pk, "SHA-512");
                    signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
                }
            }
        }
    }

}