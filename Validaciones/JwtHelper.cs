using System.Text.Json;

public static class JwtHelper
{
    public static int GetUserId(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
            throw new Exception("Token JWT inválido.");

        var payload = parts[1];
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var jsonBytes = Convert.FromBase64String(payload);
        var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

        using var doc = JsonDocument.Parse(json);
        return int.Parse(doc.RootElement.GetProperty("sub").GetString()!);
    }
}