using System;
using System.IO;
using System.Text.Json;

namespace EventTimerOverlay
{
    public static class SettingsManager
    {
        private static readonly string FilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EventTimerOverlay", "settings.json");

        public static OverlaySettings Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new OverlaySettings();

                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<OverlaySettings>(json)
                       ?? new OverlaySettings();
            }
            catch
            {
                return new OverlaySettings();
            }
        }

        public static void Save(OverlaySettings settings)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // swallow errors (safe for live event use)
            }
        }
    }
}