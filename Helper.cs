using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using System.Runtime.InteropServices;
using System;

namespace ChatWheel;

// This is an anti-pattern, move stuff away from Helper not into it
internal static partial class Helper
{
	public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
			.AddAll(new(Il2CppType.Of<T1>(), Unity.Entities.ComponentType.AccessMode.ReadWrite))
			.WithOptions(options);

		var query = Core.EntityManager.CreateEntityQuery(ref entityQueryBuilder);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
			.AddAll(new(Il2CppType.Of<T1>(), Unity.Entities.ComponentType.AccessMode.ReadWrite))
			.AddAll(new(Il2CppType.Of<T2>(), Unity.Entities.ComponentType.AccessMode.ReadWrite))
			.WithOptions(options);

		var query = Core.EntityManager.CreateEntityQuery(ref entityQueryBuilder);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static void SendSystemMessageToClient(User user, string message)
	{
		var msg = new FixedString512Bytes(message);
		ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref msg);
	}

	// alternative for EntityManager.SetComponentData
	public unsafe static void SetComponentData<T>(Entity entity, T componentData) where T : struct
	{
		var size = Marshal.SizeOf(componentData);
		//byte[] byteArray = new byte[size];
		var byteArray = StructureToByteArray(componentData);
		fixed (byte* data = byteArray)
		{
			//UnsafeUtility.CopyStructureToPtr(ref componentData, data);
			Core.EntityManager.SetComponentDataRaw(entity, ComponentTypeIndex<T>(), data, size);
		}
	}

	private static ComponentType ComponentType<T>()
	{
		return new ComponentType(Il2CppType.Of<T>());
	}
	private static int ComponentTypeIndex<T>()
	{
		return ComponentType<T>().TypeIndex;
	}

	private static byte[] StructureToByteArray<T>(T structure) where T : struct
	{
		int size = Marshal.SizeOf(structure);
		byte[] byteArray = new byte[size];
		System.IntPtr ptr = Marshal.AllocHGlobal(size);
		try
		{
			Marshal.StructureToPtr(structure, ptr, true);
			Marshal.Copy(ptr, byteArray, 0, size);
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}
		return byteArray;
	}

	public static bool AddComponent<T>(Entity entity) where T : struct
	{
		return Core.EntityManager.AddComponent(entity, ComponentType<T>());
	}
	public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
	{
		AddComponent<T>(entity);
		SetComponentData(entity, componentData);
	}

	public static void SendMessageFromPlayerWithType(FromCharacter fromCharacter, string message, ChatMessageType _chatMessageType, ServerChatMessageType _serverChatMessageType)
	{
		FixedString512Bytes msg = new FixedString512Bytes(message);
		Entity charEntity = fromCharacter.Character;
		Entity userEntity = fromCharacter.User;
		NetworkId userNetworkId = userEntity.Read<NetworkId>();
		NetworkId characterNetworkId = charEntity.Read<NetworkId>();
		ConnectedUser connectedUser = userEntity.Read<ConnectedUser>();

		int userIndex = connectedUser.UserIndex;

		ServerChatUtils.SendChatMessage(Core.Server.EntityManager, ref userIndex, ref msg, ref userNetworkId, ref characterNetworkId, _serverChatMessageType, DateTime.UtcNow.Ticks);

		Entity messageEntity = Core.EntityManager.CreateEntity(
			Unity.Entities.ComponentType.ReadWrite<FromCharacter>(),
			Unity.Entities.ComponentType.ReadWrite<ChatMessageEvent>()
		);

		messageEntity.Write(fromCharacter);
		messageEntity.Write(new ChatMessageEvent
		{
			MessageType = _chatMessageType,
			MessageText = new FixedString512Bytes(message),
		});
	}
}
