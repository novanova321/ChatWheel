using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace ChatWheel;

public static class ECSExtensions
{
	public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
	{
		// Get the ComponentType for T
		var ct = new ComponentType(Il2CppType.Of<T>());

		// Marshal the component data to a byte array
		byte[] byteArray = StructureToByteArray(componentData);

		// Get the size of T
		int size = Marshal.SizeOf<T>();

		// Create a pointer to the byte array
		fixed (byte* p = byteArray)
		{
			// Set the component data
			Core.EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
		}
	}
	// Helper function to marshal a struct to a byte array
	public static byte[] StructureToByteArray<T>(T structure) where T : struct
	{
		int size = Marshal.SizeOf(structure);
		byte[] byteArray = new byte[size];
		IntPtr ptr = Marshal.AllocHGlobal(size);

		Marshal.StructureToPtr(structure, ptr, true);
		Marshal.Copy(ptr, byteArray, 0, size);
		Marshal.FreeHGlobal(ptr);

		return byteArray;
	}
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
