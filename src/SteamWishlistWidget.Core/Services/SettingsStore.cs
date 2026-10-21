using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services;

/// <summary>
/// Lê e grava as configurações na pasta de dados do usuário. O SteamID fica criptografado pelo
/// <see cref="ISecretProtector"/> do sistema; os demais campos ficam legíveis para facilitar ajustes à mão.
/// </summary>
public sealed class SettingsStore(string path, ISecretProtector protector) : ISettingsStore
{
    private const string ProtectedSteamIdKey = "SteamIdProtected";
    private const string LegacySteamIdKey = nameof(AppSettings.SteamId);
    private const int MaxFileSize = 64 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SettingsStore(ISecretProtector protector) : this(AppConstants.SettingsFile, protector)
    {
    }

    public AppSettings Load()
    {
        try
        {
            var file = new FileInfo(path);
            if (!file.Exists || file.Length > MaxFileSize)
                return new AppSettings();

            if (JsonNode.Parse(File.ReadAllText(path)) is not JsonObject json)
                return new AppSettings();

            // Versões antigas gravavam o SteamID em texto puro; migra para o formato protegido.
            var legacyId = json[LegacySteamIdKey]?.GetValue<string>();
            var protectedId = json[ProtectedSteamIdKey]?.GetValue<string>();
            json.Remove(LegacySteamIdKey);
            json.Remove(ProtectedSteamIdKey);

            var settings = json.Deserialize<AppSettings>() ?? new AppSettings();
            settings.SteamId = protector.TryUnprotect(protectedId) ?? legacyId;
            settings.Normalize();

            if (legacyId is not null)
                Save(settings);

            return settings;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException or InvalidOperationException)
        {
            ErrorLog.Write("Falha ao ler as configurações; usando o padrão.", ex);
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.SerializeToNode(settings.Clone().Normalize(), JsonOptions)!.AsObject();
        json.Remove(LegacySteamIdKey);
        if (settings.SteamId is not null)
            json[ProtectedSteamIdKey] = protector.Protect(settings.SteamId);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Grava num arquivo temporário e troca no fim, para nunca deixar o config pela metade.
        var temp = path + ".tmp";
        File.WriteAllText(temp, json.ToJsonString(JsonOptions));
        File.Move(temp, path, overwrite: true);
    }
}
