using System.Collections.Generic;
using ChatWheel.Models;
using Stunlock.Core;
using VampireCommandFramework;
using System.Globalization;
using ProjectM.Terrain;
using ProjectM;
using Unity.Entities;
using Unity.Collections;

namespace ChatWheel.Commands;

[CommandGroup("chatwheel", "cw")]
class ChatWheelCommands
{
	public static Dictionary<string, ChatWheelMessage> TO_BIND = new Dictionary<string, ChatWheelMessage>();
	public static List<string> TO_UNBIND = new List<string>();


	[Command("bind", "b", description: "Binds a chat message on the emote wheel", adminOnly: false)]
	public static void BindCommand(ChatCommandContext ctx, string scope, string message)
	{
		scope = scope.ToLower();

		if (scope == "local" || scope == "l")
		{
			scope = "local";
		}
		else if (scope == "clan" || scope == "team" || scope == "c")
		{
			scope = "clan";
		}
		else
		{
			ctx.Reply("Invalid message scope. Please choose between local or clan.");
			return;
		}

		var platformId = ctx.Event.User.PlatformId.ToString();
		var cw = new ChatWheelMessage { message = message, scope = scope };

		TO_BIND[platformId] = cw;

		ctx.Reply("ChatWheel: Now choose the emote to bind it to (Hold ALT).");
	}

	[Command("unbind", "ub", description: "Unbinds a chat message on the emote wheel", adminOnly: false)]
	public static void UnbindCommand(ChatCommandContext ctx)
	{
		var platformId = ctx.Event.User.PlatformId.ToString();

		TO_UNBIND.Add(platformId);

		ctx.Reply("ChatWheel: Now choose the emote you want to unbind.");
	}


	[Command("list", "l", description: "Lists all bound messages", adminOnly: false)]
	public static void ListCommand(ChatCommandContext ctx)
	{
		var platformId = ctx.Event.User.PlatformId.ToString();
		var has = false;


		foreach (var kvp in Database.CHATWHEEL)
		{
			if (kvp.Key.StartsWith(platformId))
			{
				has = true;
				var guid = new PrefabGUID(int.Parse(kvp.Key.Split("|")[1]));
				var titleScope = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(kvp.Value.scope.ToLower());
				ctx.Reply($"Emote <color=#C82D36>{Core.ChatWheelService.GetEmoteName(guid)}</color> <color=#0ff>[{titleScope}]</color> <color=#8DD9FF>{kvp.Value.message}");
			}
		}

		if (!Database.IsChatWheelEnabled(platformId) && has)
		{
			ctx.Reply("<color=red>Note: ChatWheel is disabled! Enable it by doing</color> <color=#8DD9FF>.cw enable");
		}

		if (!has)
			ctx.Reply("No chat is bound to the wheel.");
	}


	[Command("clear", description: "Removes all bound messages", adminOnly: false)]
	public static void ClearCommand(ChatCommandContext ctx)
	{
		var platformId = ctx.Event.User.PlatformId.ToString();
		int count = 0;

		foreach (var kvp in Database.CHATWHEEL)
		{
			if (kvp.Key.StartsWith(platformId))
			{
				count++;
				Database.CHATWHEEL.Remove(kvp.Key);
			}
		}

		if (count > 0)
		{
			Database.SaveChatWheel();
			ctx.Reply($"Cleared {count} message(s).");
		}
		else
		{
			ctx.Reply("Nothing to clear.");
		}
	}

	[Command("enable", description: "Enables chatwheel", adminOnly: false)]
	public static void EnableCommand(ChatCommandContext ctx)
	{
		var platformId = ctx.Event.User.PlatformId.ToString();
		Database.EnableChatWheel(platformId);
		ctx.Reply($"ChatWheel enabled.");
	}

	[Command("disable", description: "Disables chatwheel", adminOnly: false)]
	public static void DisableCommand(ChatCommandContext ctx)
	{
		var platformId = ctx.Event.User.PlatformId.ToString();
		Database.DisableChatWheel(platformId);
		ctx.Reply($"ChatWheel disabled.");
	}


	[Command("reload", "r", description: "Reload the chunk map if file was manually updated", adminOnly: true)]
	public static void ReloadChunkMapCommand(ChatCommandContext ctx)
	{
		Database.ReloadChunkMap();
		ctx.Reply("Reloaded.");
	}


	[Command("renamechunk", "rc", description: "Updates the chunk map", adminOnly: true)]
	public static void UpdateChunkMapCommand(ChatCommandContext ctx, string name)

	{
		var zone = ctx.Event.SenderUserEntity.Read<CurrentMapZone>();
		string location = zone.TerrainChunk.ToString();

		var query = Core.EntityManager.CreateEntityQuery(new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<TerrainChunkMetadata>()
			}
		});
		var metadatas = query.ToEntityArray(Allocator.Temp);

		try
		{
			foreach (var metadata in metadatas)
			{
				if (metadata.TryRead<TerrainChunkMetadata>(out var chunkMetadata) && chunkMetadata.Coordinate.Equals(zone.TerrainChunk))
				{
					location = chunkMetadata.ChunkName.ToString();
					break;
				}
			}
		}
		finally
		{
			metadatas.Dispose();
		}

		Database.UpdateChunkMap(location, name);
		ctx.Reply($"Updated name for \"{location}\" to \"{name}\"");
	}
}
