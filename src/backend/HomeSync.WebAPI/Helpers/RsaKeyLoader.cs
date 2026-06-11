using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace HomeSync.WebAPI.Helpers;

public static class RsaKeyLoader
{
    public static RsaSecurityKey LoadPrivateKey(string pemPath)
    {
        if (!File.Exists(pemPath)) throw new FileNotFoundException($"Private key {pemPath} yolunda bulunamadı");
        var pem = File.ReadAllText(pemPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey LoadPublicKey(string pemPath)
    {
        if (!File.Exists(pemPath)) throw new FileNotFoundException($"Public key {pemPath} yolunda bulunamadı");
        var pem = File.ReadAllText(pemPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return new RsaSecurityKey(rsa);
    }

}
