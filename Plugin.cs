using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ChatWheel.Models;
using HarmonyLib;
using VampireCommandFramework;

namespace ChatWheel;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;
	public static ManualLogSource LogInstance { get; private set; }

	public override void Load()
	{
		PluginLog = Log;

		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
		LogInstance = Log;
		Database.InitConfig();

		// Harmony patching
		Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

		// Register all commands in the assembly with VCF
		CommandRegistry.RegisterAll();
	}

	public override bool Unload()
	{
		CommandRegistry.UnregisterAssembly();
		Harmony?.UnpatchSelf();
		return true;
	}
}
