using System.Text.Json;

namespace StripePayment.Key
{

    public static class SecretKeyManager
    {
        private static readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "key_secret.json");

        public static string GetKey(string key)
        {
            if (!File.Exists(_path))
                throw new FileNotFoundException("key_secret.json file not found.");

            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (data != null && data.TryGetValue(key, out var value))
                return value;

            throw new KeyNotFoundException($"Key '{key}' not found in user_secrets.json.");
        }
    }

}
