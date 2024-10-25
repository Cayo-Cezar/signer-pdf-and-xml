using poc.Utils;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

class Program
{
    static void Main()
    {
        string xmlPathToSign = @"";
        string signedXmlPath = @"";
        string certPath = @"";
        string certPassword = "";

        Console.WriteLine("Caminho do XML a ser assinado: " + xmlPathToSign);
        Console.WriteLine("Caminho para salvar o XML assinado: " + signedXmlPath);
        Console.WriteLine("Caminho do certificado: " + certPath);
        Console.WriteLine("Senha do certificado: " + certPassword);

        if (!File.Exists(xmlPathToSign))
        {
            Console.WriteLine("Arquivo do XML para assinar não existe no caminho informado, verifique.");
            return;
        }

        if (!File.Exists(certPath))
        {
            Console.WriteLine("Certificado para assinar não existe no caminho informado, verifique.");
            return;
        }

        var doc = new XmlDocument();
        using var client = new WebClient();
        {
            var xmlBytes = client.DownloadData(xmlPathToSign);
            doc.LoadXml(Encoding.UTF8.GetString(xmlBytes));
            var cert = new X509Certificate2(File.ReadAllBytes(certPath), certPassword);
            Xml.SignXmlDocumentWithCertificate(doc, cert);
            File.WriteAllText(signedXmlPath, doc.OuterXml);
            Console.WriteLine("XML Assinado com sucesso!");
        }
    }
}
