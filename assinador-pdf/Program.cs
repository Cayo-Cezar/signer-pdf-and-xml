using System.ComponentModel.Design;

namespace SignPdf
{
    class Program
    {
        static void Main(string[] args)
        {
            string privateKeyPath = @"";
            string sourceDocument = @"";
            string destinationPath = @"";
            string keyPassword = "";
            string reason = "Razão da assinatura";
            string location = "Localização da assinatura";

            PDFSigner signer = new PDFSigner();
            signer.Sign(sourceDocument, destinationPath, privateKeyPath, keyPassword, reason, location);

            Console.WriteLine("PDF assinado com sucesso!");
        }
    }

}