using System.Net;

namespace HomeSync.WebAPI.Helpers;

internal static class ClientIpResolver
{
    public static string? GetNormalizedClientIp(HttpContext context)
    {
        // 1. Cloudflare Kontrolü
        // Eğer uygulama Cloudflare arkasındaysa, gerçek kullanıcı IP'si her zaman bu header'da gelir.
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
        {
            return NormalizeIp(cfIp.ToString());
        }

        // 2. Standart Reverse Proxy Kontrolü (Nginx, AWS ALB, Apache vb.)
        // Listenin EN BAŞINDAKİ değer her zaman gerçek kullanıcının IP'sidir.
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIps) && !string.IsNullOrWhiteSpace(forwardedIps))
        {
            var firstIp = forwardedIps.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(firstIp))
            {
                return NormalizeIp(firstIp);
            }
        }

        // 3. Fallback (Doğrudan Bağlantı)
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is null) return null;

        // IPv6 Loopback (::1) adresini yerel testlerde kafa karıştırmasın diye standart IPv4 loopback'e çeviriyoruz.
        return remoteIp.Equals(IPAddress.IPv6Loopback) ? "127.0.0.1" : NormalizeIp(remoteIp.ToString());
    }

    private static string NormalizeIp(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return ip;
        if (ip.StartsWith('[') && ip.Contains("]:")) return ip.Split("]:")[0].TrimStart('[');
        if (ip.Contains(':') && !ip.Contains("::") && ip.Split(':').Length == 2) return ip.Split(':')[0];
        return ip;
    }
}
