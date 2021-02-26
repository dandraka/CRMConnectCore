using Dandraka.XmlUtilities;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Dandraka.CRMConnectCore.Tests
{
    internal class TestsConfig
    {
        private dynamic config;

        private int maxKeyLength
        {
            get
            {
                Aes aes = Aes.Create();
                KeySizes[] ks = aes.LegalKeySizes;
                int max = ks[ks.Length - 1].MaxSize / 8;
                return max;
            }
        }
        private string key => (Environment.MachineName + Environment.UserName + "zEYgPgPt8EDbwqAfwwlxclX8SJulVkrj").Substring(0, maxKeyLength);

        public TestsConfig()
        {
            string configFile = "TestsConfig-secret.xml";
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                configFile);
            if (!File.Exists(path))
            {
                configFile = "TestsConfig.xml";
                path = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    configFile);
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Test configuration file {path} not found");
            }

            CheckConfig(path);

            this.config = XmlSlurper.ParseText(File.ReadAllText(path));
        }

        private void CheckConfig(string path)
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(path);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"INVALID CONFIG: Could not load {path} as XML.\r\n{ex.Message}");
            }            

            // check if data are present
            var urlNode = xmlDoc.SelectSingleNode("//*[local-name()='CRMUrl']");
            var clientIDNode = xmlDoc.SelectSingleNode("//*[local-name()='ClientID']");
            var clientSecretNode = xmlDoc.SelectSingleNode("//*[local-name()='ClientSecret']");
            var clientSecretEncryptedNode = xmlDoc.SelectSingleNode("//*[local-name()='ClientSecretEncrypted']");

            if (urlNode == null)
            {
                throw new ApplicationException($"INVALID CONFIG: Config/CRMUrl not found in config file {path}.");
            }
            if (clientIDNode == null)
            {
                throw new ApplicationException($"INVALID CONFIG: Config/ClientID not found in config file {path}.");
            }
            if (clientSecretNode == null && clientSecretEncryptedNode == null)
            {
                throw new ApplicationException($"INVALID CONFIG: At least one of Config/ClientSecret or Config/ClientSecretEncrypted must exist in config file {path}.");
            }

            // if plain text, encrypt it
            if (clientSecretNode != null)
            {
                string encr = Security.EncryptString(clientSecretNode.InnerText, this.key);
                xmlDoc.DocumentElement.RemoveChild(clientSecretNode);

                if (clientSecretEncryptedNode == null)
                {
                    clientSecretEncryptedNode = xmlDoc.CreateNode(XmlNodeType.Element, "ClientSecretEncrypted", "");
                    xmlDoc.DocumentElement.AppendChild(clientSecretEncryptedNode);
                }

                clientSecretEncryptedNode.InnerText = encr;
                using (var tw = new XmlTextWriter(path, Encoding.UTF8))
                {
                    xmlDoc.WriteContentTo(tw);
                    tw.Flush();
                }
            }
        }

        public string CRMUrl => config.CRMUrl;

        public string ClientID => config.ClientID;

        public SecureString ClientSecret =>
            new NetworkCredential("", Security.DecryptString(config.ClientSecretEncrypted, this.key)).SecurePassword;

    }
}
