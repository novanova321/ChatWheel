using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ChatWheel.Models;

public class ChatWheelMessage
{
	public string scope;
	public string message;
}

public readonly struct Database
{
	private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, "ChatWheel");
	public static readonly string PLAYER_SETTINGS_PATH = Path.Combine(CONFIG_PATH, "playerSettings.json");
	public static readonly string CHATWHEEL_PATH = Path.Combine(CONFIG_PATH, "chatwheel.json");
	public static readonly string CHUNKMAP_PATH = Path.Combine(CONFIG_PATH, "chunkMap.json");
	private static readonly Dictionary<string, string> CHUNKMAP = new()
	{
		{"14,8", "Farbane southeast near waygate"},
		{"8,8", "Farbane southwest near waygate"},
		{"Curse_Builder_Corner01_Territory", "CF northwest"},
		{"Curse_Builder_Corner02_Territory", "CF northeast"},
		{"Curse_Corner01", "Frog Boss"},
		{"Curse_Edge04", "Cyril-Matka"},
		{"Curse_Edge05_Territory", "CF middle south"},
		{"Curse_Edge06", "CF near Behemoth cave"},
		{"Curse_SpiderCave01_Territory", "Spider Cave"},
		{"Cursed_InCorner01_Territory", "CF north"},
		{"Dev_Island_Chunk", "Dev Island"},
		{"Dunley_Builder_Corner01_Territory", "Dunley southwest corner"},
		{"Dunley_Builder_Corner02_Territory", "Dunley southeast "},
		{"Dunley_Builder_Corner04", "Quartz Quarry"},
		{"Dunley_Builder_Edge01_Territory", "Dunley east"},
		{"Dunley_Builder_Edge02_Territory", "Iron Mine east beside waygate"},
		{"Dunley_Builder_Edge03_Territory", "Iron Mine west beside waygate"},
		{"Dunley_Builder_Edge04_Territory", "around Maja"},
		{"Dunley_Corner05_DraculasRuin", "Dunley north below Gloom waygate"},
		{"Dunley_Edge01_HorseTrack", "Horse Track"},
		{"Dunley_Edge05_BigFarm", "Dunley near Lumber Mill"},
		{"Dunley_Edge06_FarmCotton_Territory", "Dunley southeast Cotton Farm"},
		{"Dunley_Edge08_DraculasRuin", "Styx south"},
		{"Dunley_Group_IronMine_Mid01_Territory", "Iron Mine"},
		{"Dunley_Group_IronMine_Mid02_Territory", "Iron Mine north"},
		{"Dunley_Group_Middle01", "Dunley middle"},
		{"Dunley_Group_Middle02", "Dunley-Farbane Cave"},
		{"Dunley_Group_Middle03", "Dunley near Market"},
		{"Dunley_Group_Middle04", "Dunley north near Iron Cave"},
		{"Dunley_GroupWerewolf_Chunk01_Territory", "Leandra"},
		{"Dunley_GroupWerewolf_Chunk02", "Colosseum-Werewolf area"},
		{"Dunley_GroupWerewolf_Chunk03", "Maja north"},
		{"Dunley_GroupWerewolf_Chunk04_Territory", "Werewolf"},
		{"Dunley_GroupWerewolf_Chunk05_Territory", "Werewolf east"},
		{"Dunley_InCorner01_IronVeins_Territory", "Monastery waygate"},
		{"Dunley_Mid02_Colosseum", "Colosseum"},
		{"Dunley_Mid02_Colosseum_Territory", "Colosseum"},
		{"Dunley_Mid03_BigFort", "Dunley around Octavian"},
		{"Dunley_Mid04_VillageChurch_Territory", "Mosswick"},
		{"Dunley_Mid05_Lake", "Iron Mine west"},
		{"Dunley_Mid06_AnimalFarm", "Iron Mine east"},
		{"Dunley_Mid07_Waypoint_Territory", "Beatrice north"},
		{"Dunley_Mid08_VillageChurch_Territory", "Beatrice"},
		{"Dunley_Mid09_BigChurch_Territory", "Monastery"},
		{"Dunley_Mid09_Waypoint", "Octavian waygate"},
		{"DunleyCursed_BridgeGroup01_Chunk01", "CF west waygate"},
		{"DunleySilver_BridgeGroup01_Chunk01_Territory", "Dunley-SL road"},
		{"DunleySilver_BridgeGroup01_Chunk02", "SL waygate"},
		{"DunleyWerewolf_Chunk01", "Leandra"},
		{"ElrisCursed_Bridge_Group01_Chunk01", "Foulrot waygate"},
		{"ElrisCursed_Bridge_Group01_Chunk02", "Rift #2 north"},
		{"ElrisCursed_Bridge01_Chunk02", "Mortium north"},
		{"Farbane_BackgroundChunk02", "Farbane far south middle"},
		{"Farbane_Builder_Corner01_Territory", "Farbane southwest corner"},
		{"Farbane_Builder_Corner02_Territory", "Farbane near west Dunley bridge"},
		{"Farbane_Builder_Corner03_Territory", "Below Goreswine Right Cemetery"},
		{"Farbane_Builder_Corner04_Territory", "Farbane south of Goreswine Left Cemetery"},
		{"Farbane_Builder_Edge01_Territory", "Finn"},
		{"Farbane_Builder_Edge02_Territory", "Talzur"},
		{"Farbane_Builder_Edge03_Territory", "Farbane southeast"},
		{"Farbane_Builder_Edge04_Territory", "Farbane north, east of Quincey"},
		{"Farbane_Builder_Edge05_Territory", "Farbane north, west of Quincey"},
		{"Farbane_Builder_Edge06_Territory", "Clive north"},
		{"Farbane_Builder_Edge07_Territory", "Farbane southwest"},
		{"Farbane_Builder_Edge08_Territory", "Farbane southeast"},
		{"Farbane_Builder_InCorner01_Territory", "Farbane near west Merchant"},
		{"Farbane_Builder_InCorner03_Territory", "Bear Cave west"},
		{"Farbane_Builder_InCorner04_Territory", "Farbane southwest area"},
		{"Farbane_Builder_InCorner05_Territory", "Polora south"},
		{"Farbane_Corner03_GrizzlyBearCave_Territory", "Bear Cave"},
		{"Farbane_Corner04_Medow_Territory", "Polora"},
		{"Farbane_Corner06_Sulfurquarry", "Clive"},
		{"Farbane_Edge05", "Farbane south middle"},
		{"Farbane_Edge06_BanditFort_Territory", "Quincey"},
		{"Farbane_InCorner04_Graveyard_Territory", "Goreswine Left Cemetery"},
		{"Farbane_InCorner05_Graveyard_Territory", "Goreswine Right Cemetery"},
		{"Farbane_InCorner06_Territory", "Farbane west edge"},
		{"Farbane_Mid02_HauntedGraveyard_Territory", "Nicholaus"},
		{"Farbane_Mid02_Waypoint_Territory", "near Keely waygate"},
		{"Farbane_Mid05_BanditEncampment_Territory", "Nicholaus-Keely road"},
		{"Farbane_Mid07_Territory", "Farbane east"},
		{"Farbane_Mid09_Territory", "Copper Mine north"},
		{"Farbane_Mid10_Territory", "Farbane west"},
		{"Farbane_Mid11_Quarry_Territory", "Copper Mine"},
		{"Farbane_Mid12_BanditForge_Territory", "Grayson"},
		{"Farbane_Mid13_BanditTailor_Territory", "Keely"},
		{"Farbane_Mid14_Territory", "Farbane west"},
		{"Farbane_Mid15_Territory", "Farbane middle"},
		{"Farbane_Mid16_Territory", "Farbane east"},
		{"Farbane_Mid17_LumberCamp_Territory", "Rufus"},
		{"Farbane_Mid18_Waypoint_Territory", "near Grayson waygate"},
		{"Farbane_Snow_TransitionGroup01_Chunk01_Territory", "Farbane-Snow road"},
		{"Farbane_Snow_TransitionGroup01_Chunk02_Territory", "Terrorclaw Cave"},
		{"Farbane_Snow_TransitionGroup01_Chunk03_Territory", "Snow waygate"},
		{"Farbane_Snow_TransitionGroup01_Chunk04_Territory", "Snow northeast"},
		{"FarbaneDunley_BridgeGroup01_Chunk01_Territory", "Farbane-Dunley east bridge"},
		{"FarbaneDunley_BridgeGroup01_Chunk02_Territory", "Dunley southeast"},
		{"FarbaneDunley_BridgeGroup02_Chunk01_Territory", "Farbane-Dunley west bridge"},
		{"FarbaneDunley_BridgeGroup02_Chunk02_Territory", "Dunley southwest"},
		{"FarbaneSilver_Group_Bridge01_Territory", "Polora waygate"},
		{"FarbaneSilver_Group_Bridge02", "SL south"},
		{"Gloomrot_Corner01", "Bottom Right Gloom"},
		{"Gloomrot_Corner02", "Gloom northeast corner"},
		{"Gloomrot_Corner03", "Gloom west waygate"},
		{"Gloomrot_Corner04", "Gloom northwest corner"},
		{"Gloomrot_Edge01_DraculasRuin", "Styx"},
		{"Gloomrot_Edge02_DraculasRuin", "Gloom south waygate"},
		{"Gloomrot_Edge03", "Gloom northeast"},
		{"Gloomrot_Edge04", "Ziva north"},
		{"Gloomrot_Edge05", "Voltatia"},
		{"Gloomrot_Edge06", "Adam east"},
		{"Gloomrot_Edge07", "Adam"},
		{"Gloomrot_Edge08", "Domina south"},
		{"Gloomrot_Edge09", "Gloom west waygate"},
		{"Gloomrot_Edge10", "Gloom Merchant"},
		{"Gloomrot_Edge11", "Gloom northwest corner"},
		{"Gloomrot_Mid01", "Adam"},
		{"Gloomrot_Mid02", "Sulphur Mine east"},
		{"Gloomrot_Mid03", "Gloom Sulphur Mine"},
		{"Gloomrot_Mid04", "Domina-Angram area"},
		{"Gloomrot_Mid05", "Henry south"},
		{"Gloomrot_Mid06", "Henry"},
		{"Gloomrot_Mid07", "Henry east"},
		{"Gloomrot_Mid08", "Gloom north waygate"},
		{"Gloomrot_Mid09", "Ziva"},
		{"GloomrotCursed_BridgeGroup01_Chunk01", "Gloom-CF road"},
		{"Noctem_Corner01", "Rift #7 east"},
		{"Noctem_Corner02", "Small Garden"},
		{"Noctem_Corner03", "Rift #1"},
		{"Noctem_Corner04", "Dracula's Garden"},
		{"Noctem_Edge01", "Rift #7"},
		{"Noctem_Edge02", "Mortium South"},
		{"Noctem_Edge03", "Dracula's Garden"},
		{"Noctem_Edge04", "Rift #4"},
		{"Noctem_Edge05", "Mortium top left"},
		{"Noctem_Edge06", "Rift #6"},
		{"Noctem_Edge07_boss", "Dracula's Throne Room"},
		{"Noctem_Edge07_bossDead", "Dracula's Red Portal Room"},
		{"Noctem_Edge07_ruined", "Shadow Realm Portal Room"},
		{"Noctem_Mid01", "Rift #5"},
		{"Noctem_Mid02", "Mortium middle"},
		{"Noctem_Mid03", "Rift #2"},
		{"Noctem_Mid04", "Rift #3"},
		{"Noctem_Mid05", "Mortium top right around Rift #6"},
		{"Silver_Builder_Corner01_Territory", "Mairwyn south"},
		{"Silver_Builder_Corner02", "SL-Gloom road"},
		{"Silver_Builder_Corner03_Territory", "SL southwest corner"},
		{"Silver_Builder_Corner04_Territory", "City north plot"},
		{"Silver_Builder_Mountain_Edge01_Territory", "SL waygate north"},
		{"Silver_City01", "City Docks"},
		{"Silver_City02", "City south"},
		{"Silver_City03", "City north/east"},
		{"Silver_City04_Territory", "City northwest"},
		{"Silver_Edge01_Silvermine", "Silver Mine"},
		{"Silver_Edge01_SilverMine", "Silver Mine"},
		{"Silver_Edge02", "Mairwyn"},
		{"Silver_Edge03", "Solarus"},
		{"Silver_Mid01", "Morian"},
		{"Silver_Mid02", "Baron-Solarus area"},
		{"Silver_Mid03", "SL waygate"},
		{"Snow_WildlingGroup01_Edge01", "Rift #3 north"},
		{"Start_Vault", "The Starting Area"},
		{"Strongblade_Corner02", "Oak west of Stavros"},
		{"Strongblade_Corner03", "Oak Beach"},
		{"Strongblade_Corner04", "Dantos"},
		{"Strongblade_Corner05", "Megara"},
		{"Strongblade_Edge01", "Oak south"},
		{"Strongblade_Edge02", "Oak southwest corner"},
		{"Strongblade_Edge03", "Oak east"},
		{"Strongblade_Edge04", "Oak east waygate"},
		{"Strongblade_InCorner01", "Oak west waygate"},
		{"Strongblade_InCorner02", "Stavros"},
		{"Strongblade_Mid01", "Lucile"},
		{"Strongblade_Mid02", "Dantos south"},
		{"Strongblade_SilverGloomTransition01", "Jakira"},
		{"Strongblade_SilverTransition01", "SL-Oak road"},
		{"Strongblande_SilverGloomTransition01", "Jakira"},
		{"Wild_Group_StartGraveyard03", "The Graveyard"},
		{"Wild_Group_StartGraveyard03_Copy", "The Graveyard"},
		{"Wild_Group_StartGraveyard04_Copy", "The Graveyard"}
	};
	public static Dictionary<string, ChatWheelMessage> CHATWHEEL = new Dictionary<string, ChatWheelMessage>();
	public static Dictionary<string, bool> PLAYER_SETTINGS = new Dictionary<string, bool>();

	public static void InitConfig()
	{
		string json;
		Dictionary<string, string> dict;

		CHATWHEEL.Clear();
		PLAYER_SETTINGS.Clear();

		if (File.Exists(CHUNKMAP_PATH))
		{
			json = File.ReadAllText(CHUNKMAP_PATH);
			dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

			foreach (var kvp in dict)
			{
				CHUNKMAP[kvp.Key] = kvp.Value;
			}
		}
		else
		{
			SaveChunkMap();
		}

		if (File.Exists(CHATWHEEL_PATH))
		{
			json = File.ReadAllText(CHATWHEEL_PATH);
			dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);


			foreach (var kvp in dict)
			{
				var words = kvp.Value.Split(',', 2);
				CHATWHEEL[kvp.Key] = new ChatWheelMessage { scope = words[0], message = words[1] };
			}
		}
		else
		{
			SaveChatWheel();
		}

		if (File.Exists(PLAYER_SETTINGS_PATH))
		{
			json = File.ReadAllText(PLAYER_SETTINGS_PATH);
			Dictionary<string, bool> dict2 = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);


			foreach (var kvp in dict2)
			{
				PLAYER_SETTINGS[kvp.Key] = kvp.Value;
			}
		}
	}

	public static void ReloadChunkMap()
	{
		string json;
		Dictionary<string, string> dict;
		if (File.Exists(CHUNKMAP_PATH))
		{
			json = File.ReadAllText(CHUNKMAP_PATH);
			dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

			foreach (var kvp in dict)
			{
				CHUNKMAP[kvp.Key] = kvp.Value;
			}
		}
	}

	public static void SaveChatWheel()
	{
		Dictionary<string, string> dic = new() { };

		foreach (var kvp in CHATWHEEL)
		{
			dic[kvp.Key] = $"{kvp.Value.scope},{kvp.Value.message}";
		}

		WriteConfig(CHATWHEEL_PATH, dic);
	}

	public static void WriteConfig(string path, Dictionary<string, string> dict)
	{
		if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);
		var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(path, json);
	}

	public static void WriteConfig(string path, Dictionary<string, bool> dict)
	{
		if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);
		var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(path, json);
	}

	static public void SaveChunkMap()
	{
		WriteConfig(CHUNKMAP_PATH, CHUNKMAP);
	}

	public static void UpdateChunkMap(string chunk, string name)
	{
		CHUNKMAP[chunk] = name;
		SaveChunkMap();
	}

	public static string GetChunkCommonName(string chunk)
	{
		return CHUNKMAP.ContainsKey(chunk) ? CHUNKMAP[chunk] : chunk.Replace("_", " ");
	}

	static public void SavePlayerSettings()
	{
		WriteConfig(PLAYER_SETTINGS_PATH, PLAYER_SETTINGS);
	}

	static public void DisableChatWheel(string platformId)
	{
		PLAYER_SETTINGS[platformId] = false;
		SavePlayerSettings();
	}

	static public void EnableChatWheel(string platformId)
	{
		PLAYER_SETTINGS[platformId] = true;
		SavePlayerSettings();
	}

	static public bool IsChatWheelEnabled(string platformId)
	{
		return PLAYER_SETTINGS.GetValueOrDefault(platformId, true);
	}
}
