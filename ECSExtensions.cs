using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace ChatWheel;

public static class ECSExtensions
{
	public unsafe static T Read<T>(this Entity entity) where T : struct
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		void* rawPointer = Core.EntityManager.GetComponentDataRawRO(entity, ct.TypeIndex);
		T componentData = Marshal.PtrToStructure<T>(new IntPtr(rawPointer));
		return componentData;
	}

	public static bool TryRead<T>(this Entity entity, out T componentData) where T : struct
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		componentData = default;
		return Core.EntityManager.HasComponent(entity, ct) && Core.EntityManager.TryGetComponentData<T>(entity, out componentData);
	}
	public static bool Exists(this Entity entity)
	{
		return Core.EntityManager.Exists(entity);
	}

	public static string LookupName(this PrefabGUID prefabGuid)
	{
		var prefabCollectionSystem = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		return prefabCollectionSystem._PrefabLookupMap.GuidToEntityMap.ContainsKey(prefabGuid)
			? prefabCollectionSystem._PrefabLookupMap.GetName(prefabGuid) + " PrefabGuid(" + prefabGuid.GuidHash + ")" : "GUID Not Found";
	}
}
