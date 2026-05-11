using System.Collections.Generic;
using Stunlock.Core;

namespace ChatWheel.Services;

public class ChatWheelService
{
    public static Dictionary<int, string> EMOTE_MAP = new Dictionary<int, string> {
        {-658066984, "Beckon"},
        {-1462274656, "Bow"},
        {-26826346, "Clap"},
        {-53273186, "No"},
        {-452406649, "Point"},
        {-370061286, "Salute"},
        {-1064533554, "Surrender"},
        {-158502505, "Taunt"},
        {1177797340, "Wave"},
        {-1525577000, "Yes"},
        {808904257, "Sit"},
        {-578764388, "Shrug"},

        // {-133703992, "Thank You"},
        // {-1685517289, "Laugh"},
        // {889811193, "Cry"},
        // {604121141, "Dance1"},
        // {-925169006, "Dance2"},
    };

    public string GetEmoteName(PrefabGUID prefabGuid)
    {
        return EMOTE_MAP.GetValueOrDefault(prefabGuid.GuidHash, "Unknown Emote");
    }
}