using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace poc.Utils
{
    public class XmlSigner
    {
        public static void SignXmlDocumentWithCertificate(XmlDocument doc, X509Certificate2 cert)
        {
            var signedXml = CreateSignedXml(doc, cert);
            AppendSignatureToDocument(doc, signedXml);
        }

        public static void CreateConfigFile(string filePath)
        {
            const string initialData = "xmlPathToSign;\nsignedXmlPath;\ncertPath;\ncertPassword;";
            try
            {
                File.WriteAllText(filePath, initialData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static ConfigFile ReadConfigFile(string filePath)
        {
            var lines = ReadFileLines(filePath);
            return ParseConfigFile(lines);
        }

        public static bool ValidateConfigs(ConfigFile configFile)
        {
            try
            {
                CheckXmlPathToSign(configFile);
                CheckSignedXmlPath(configFile);
                CheckCertPath(configFile);
                CheckCertPassword(configFile);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static void CheckXmlPathToSign(ConfigFile configFile)
        {
            if (string.IsNullOrEmpty(configFile.XmlPathToSign))
                throw new ArgumentException("Caminho do XML para assinar não informado");

            if (!File.Exists(configFile.XmlPathToSign))
                throw new FileNotFoundException("Arquivo do XML para assinar não existe no caminho informado");
        }

        private static void CheckSignedXmlPath(ConfigFile configFile)
        {
            if (string.IsNullOrEmpty(configFile.SignedXmlPath))
                throw new ArgumentException("Caminho para salvar XML assinado não informado");

            string directory = Path.GetDirectoryName(configFile.SignedXmlPath);
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("Diretório para salvar XML assinado não existe");
        }

        private static void CheckCertPath(ConfigFile configFile)
        {
            if (string.IsNullOrEmpty(configFile.CertPath))
                throw new ArgumentException("Caminho do certificado não informado");

            if (!File.Exists(configFile.CertPath))
                throw new FileNotFoundException("Certificado para assinar não existe no caminho informado");
        }

        private static void CheckCertPassword(ConfigFile configFile)
        {
            if (string.IsNullOrEmpty(configFile.CertPassword))
                throw new ArgumentException("Senha do certificado não informada");
        }

        private static SignedXml CreateSignedXml(XmlDocument doc, X509Certificate2 cert)
        {
            var signedXml = new SignedXml(doc) { SigningKey = cert.PrivateKey };
            var reference = new Reference { Uri = "" };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            return signedXml;
        }

        private static void AppendSignatureToDocument(XmlDocument doc, SignedXml signedXml)
        {
            var signatureNode = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(signatureNode, true));
        }

        private static string[] ReadFileLines(string filePath) => File.ReadAllText(filePath).Split('\n');

        private static ConfigFile ParseConfigFile(string[] lines)
        {
            string xmlPath = GetLineValue(lines, 0);
            string signedXmlPath = GetLineValue(lines, 1);
            string certPath = GetLineValue(lines, 2);
            string certPassword = GetLineValue(lines, 3);

            return new ConfigFile(xmlPath, signedXmlPath, certPath, certPassword);
        }

        private static string GetLineValue(string[] lines, int index) =>
            index >= 0 && index < lines.Length ? lines[index].Trim().Replace(";", "") : null;

        public record ConfigFile(string XmlPathToSign, string SignedXmlPath, string CertPath, string CertPassword);
    }
}
