using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuatiTimeWebApi.Services;

public record SessionPayload(string Username, string Password, string UserId);

public class CookieEncryptionService
{
    private readonly byte[] _key;

    public CookieEncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Cookie:EncryptionKey"]
            ?? throw new InvalidOperationException("Cookie:EncryptionKey not configured");
        // Derive a 32-byte key from whatever string is configured
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
    }

    public string Encrypt(SessionPayload payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var plaintext = Encoding.UTF8.GetBytes(json);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

        var result = new byte[aes.IV.Length + ciphertext.Length];
        aes.IV.CopyTo(result, 0);
        ciphertext.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }

    public SessionPayload? Decrypt(string cookieValue)
    {
        try
        {
            var data = Convert.FromBase64String(cookieValue);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = data[..16];
            var ciphertext = data[16..];
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plaintext = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            var json = Encoding.UTF8.GetString(plaintext);

            return JsonSerializer.Deserialize<SessionPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
