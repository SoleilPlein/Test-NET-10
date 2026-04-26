using System.Text.Json;

namespace Test_NET_10;

public interface IAnimationPreferenceStore
{
    bool TryLoad(out AnimationSpeedProfile profile);

    bool Save(AnimationSpeedProfile profile);
}

public sealed class JsonAnimationPreferenceStore(string filePath) : IAnimationPreferenceStore
{
    private readonly string filePath = filePath;

    public bool TryLoad(out AnimationSpeedProfile profile)
    {
        profile = AnimationSpeedProfile.Normal;

        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            string json = File.ReadAllText(filePath);
            PreferencePayload? payload = JsonSerializer.Deserialize<PreferencePayload>(json);

            if (payload is null || string.IsNullOrWhiteSpace(payload.AnimationProfile))
            {
                return false;
            }

            if (!Enum.TryParse(payload.AnimationProfile, ignoreCase: true, out AnimationSpeedProfile parsed))
            {
                return false;
            }

            profile = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Save(AnimationSpeedProfile profile)
    {
        try
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var payload = new PreferencePayload
            {
                AnimationProfile = profile.ToString()
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class PreferencePayload
    {
        public string AnimationProfile { get; init; } = string.Empty;
    }
}
