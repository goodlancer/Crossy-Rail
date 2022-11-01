using System.Collections.Generic;

namespace Watermelon
{
    public static partial class GameSettingsPrefs
    {
        private static Dictionary<string, object> settings = new Dictionary<string, object>()
        {
            { "music", 1.0f },
            { "sound", 1.0f },
            { "vibration", true },

            { "current level", 1 },
            { "no_ads", false }
        };
    }
}