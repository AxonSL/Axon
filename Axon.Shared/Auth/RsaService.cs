using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Axon.Shared.Auth;

public class RsaService
{
    private RSAParameters parameters;
    private RSACryptoServiceProvider csp = new();

    public RsaService(RSAParameters parameters)
    {
        this.parameters = parameters;
        csp.ImportParameters(parameters);
    }

    public RsaService(string xmlPath)
    {
        var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
        var stream = new FileStream(xmlPath,FileMode.Open, FileAccess.Read);
        parameters = (RSAParameters)xmlSerializer.Deserialize(stream);
        stream.Close();
        csp.ImportParameters(parameters);
    }

    public RSAParameters Key => parameters;

    public string KeyXml
    {
        get
        {
            var sw = new StringWriter();
            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(sw, parameters);
            return sw.ToString();
        }
    }

    public byte[] Encript(byte[] plainData)
        => csp.Encrypt(plainData, false);

    public string Encript(string plainText)
    {
        var data = Encoding.Unicode.GetBytes(plainText);
        var cypher = csp.Encrypt(data, false);
        return Convert.ToBase64String(cypher);
    }

    public byte[] Decript(byte[] plainData)
        => csp.Decrypt(plainData, false);

    public string Decript(string cypherText)
    {
        var data = Convert.FromBase64String(cypherText);
        var plain = csp.Decrypt(data, false);
        return Encoding.Unicode.GetString(plain);
    }
}
