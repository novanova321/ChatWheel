using BepInEx.Logging;
using ProjectM;
using Unity.Entities;
using ChatWheel.Services;

namespace ChatWheel;

internal static class Core
{
	public static World Server { get; } = GetWorld("Server") ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
	public static EntityManager EntityManager { get; } = Server.EntityManager;
	public static PrefabCollectionSystem PrefabCollectionSystem { get; internal set; }
	public static ChatWheelService ChatWheelService { get; } = new();
	public static ManualLogSource Log { get; } = Plugin.PluginLog;

	internal static void InitializeAfterLoaded()
	{
		if (_hasInitialized) return;
		PrefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		_hasInitialized = true;
		Log.LogInfo($"{nameof(InitializeAfterLoaded)} completed");
	}
	private static bool _hasInitialized = false;

	private static World GetWorld(string name)
	{
		foreach (var world in World.s_AllWorlds)
		{
			if (world.Name == name)
			{
				return world;
			}
		}

		return null;
	}
}
