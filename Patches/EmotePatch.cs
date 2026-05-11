using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using System;
using ChatWheel.Commands;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using ChatWheel.Models;
using Stunlock.Core;
using ProjectM.Terrain;
using ProjectM.CastleBuilding;
using ProjectM.Scripting;

namespace ChatWheel.Patches;

[HarmonyPatch(typeof(EmoteSystem), nameof(EmoteSystem.OnUpdate))]
public static class EmoteSystemPatch
{
	public static readonly PrefabGUID Buff_InCombat_PvPVampire = new PrefabGUID(697095869);
	public static readonly PrefabGUID Buff_General_VampirePvPDeathDebuff = new PrefabGUID(1591132469);

	[HarmonyPrefix]
	static void OnUpdatePrefix(EmoteSystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);

		try
		{
			foreach (var entity in entities)
			{
				if (!entity.Exists()) continue;

				var useEmoteEvent = entity.Read<UseEmoteEvent>();
				var fromCharacter = entity.Read<FromCharacter>();
				var emoteGuid = useEmoteEvent.Action;
				var charEntity = fromCharacter.Character;
				var userEntity = fromCharacter.User;

				if (!userEntity.Exists())
					continue;

				var user = userEntity.Read<User>();
				var platformId = user.PlatformId.ToString();
				var key = $"{platformId}|{emoteGuid.GuidHash}";

				if (ChatWheelCommands.TO_BIND.TryGetValue(platformId, out var toBind))
				{
					Database.CHATWHEEL[key] = toBind;
					Database.SaveChatWheel();
					ChatWheelCommands.TO_BIND.Remove(platformId);
					Core.EntityManager.DestroyEntity(entity);
					Helper.SendSystemMessageToClient(user, $"ChatWheel: Message is now bound to {Core.ChatWheelService.GetEmoteName(emoteGuid)}");
				}
				else if (ChatWheelCommands.TO_UNBIND.Contains(platformId))
				{
					Database.CHATWHEEL.Remove(key);
					Database.SaveChatWheel();
					ChatWheelCommands.TO_UNBIND.Remove(platformId);
					Core.EntityManager.DestroyEntity(entity);
					Helper.SendSystemMessageToClient(user, $"ChatWheel: Emote {Core.ChatWheelService.GetEmoteName(emoteGuid)} is now back to normal");
				}
				else if (Database.CHATWHEEL.TryGetValue(key, out var cw) && Database.IsChatWheelEnabled(platformId))
				{
					var networkId = userEntity.Read<NetworkId>();
					var clanEntity = user.ClanEntity.GetEntityOnServer();
					var networkId2 = charEntity.Read<NetworkId>();

					bool hasCombatTimer = cw.message.Contains("$combatTimer", StringComparison.OrdinalIgnoreCase);
					bool hasBaneTimer = cw.message.Contains("$baneTimer", StringComparison.OrdinalIgnoreCase);
					bool hasDeathTimer = cw.message.Contains("$deathTimer", StringComparison.OrdinalIgnoreCase);
					bool hasLocation = cw.message.Contains("$location", StringComparison.OrdinalIgnoreCase);
					bool hasUltCd = cw.message.Contains("$ultCd", StringComparison.OrdinalIgnoreCase);

					string combatTimer = "0s";
					string baneTimer = "0s";
					string deathTimer = "30s";
					string location = "";
					string ultCd = "N/A";

					if (hasCombatTimer || hasBaneTimer)
					{
						var buffEntities = Helper.GetEntitiesByComponentTypes<Buff, PrefabGUID>();

						foreach (var buffEntity in buffEntities)
						{
							if (buffEntity.Read<EntityOwner>().Owner == charEntity)
							{
								var buffGuidHash = buffEntity.Read<PrefabGUID>().GuidHash;
								if (hasCombatTimer && buffGuidHash.Equals(Buff_InCombat_PvPVampire.GuidHash))
								{
									var age = buffEntity.Read<Age>();
									var lifeTime = buffEntity.Read<LifeTime>();
									combatTimer = Convert.ToInt32(lifeTime.Duration - age.Value) + "s";
								}
								else if ((hasBaneTimer || hasDeathTimer) && buffGuidHash.Equals(Buff_General_VampirePvPDeathDebuff.GuidHash))
								{
									if (hasBaneTimer)
									{
										var age = buffEntity.Read<Age>();
										var lifeTime = buffEntity.Read<LifeTime>();
										int totalSeconds = Convert.ToInt32(lifeTime.Duration - age.Value);
										int minutes = totalSeconds / 60;
										int seconds = totalSeconds % 60;
										baneTimer = "";
										if (minutes > 0)
											baneTimer += $"{minutes}m";
										if (seconds > 0)
											baneTimer += $"{seconds}s";
									}
									if (hasDeathTimer)
									{
										var buff = buffEntity.Read<Buff>();
										deathTimer = Math.Min(150, 30 + (buff.Stacks * 60)) + "s";
									}
								}
							}
						}
					}

					if (hasLocation)
					{
						// check if inside someone's castle
						CastleTerritory fetchedTerritoryComponent;
						TilePosition tilePos = charEntity.Read<TilePosition>();
						var castleHeartEntities = Helper.GetEntitiesByComponentType<CastleHeart>();
						foreach (var castleHeart in castleHeartEntities)
						{
							var actualTerritory = castleHeart.Read<CastleHeart>().CastleTerritoryEntity;
							bool isInTerritory = CastleTerritoryExtensions.IsTileInTerritory(Core.EntityManager, tilePos.Tile, ref actualTerritory, out fetchedTerritoryComponent);
							if (isInTerritory && castleHeart.TryRead<UserOwner>(out var userOwner))
							{
								var owner = userOwner.Owner.GetEntityOnServer();
								if (!owner.Equals(Entity.Null) && owner.Exists() && owner.TryRead<User>(out var castleUserOwner))
									location = $"{castleUserOwner.CharacterName.ToString()}'s castle in ";
								break;
							}
						}

						// sdfa
						var zone = userEntity.Read<CurrentMapZone>();
						string terrainChunkString = zone.TerrainChunk.ToString();

						// handle special case where chunk coordinate is different but name is the same
						if (terrainChunkString == "8,8" || terrainChunkString == "14,8")
						{
							location += Database.GetChunkCommonName(terrainChunkString);
						}
						else
						{
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
										location += Database.GetChunkCommonName(chunkMetadata.ChunkName.ToString());
										break;
									}
								}
							}
							finally
							{
								metadatas.Dispose();
							}
						}
					}

					if (hasUltCd)
					{
						var time = Core.Server.GetExistingSystemManaged<ServerScriptMapper>().GetServerGameManager().ServerTime;
						var abilityBuffer = Core.EntityManager.GetBuffer<AbilityGroupSlotBuffer>(charEntity);
						foreach (var ability in abilityBuffer)
						{
							var abilitySlot = ability.GroupSlotEntity._Entity;
							var activeAbility = abilitySlot.Read<AbilityGroupSlot>();
							if (activeAbility.SlotId != 7) continue;

							var activeAbility_Entity = activeAbility.StateEntity._Entity;
							if (!activeAbility.CopyCooldown)
							{
								activeAbility_Entity = activeAbility.PreviousStateEntity._Entity;
							}
							if (activeAbility_Entity.Equals(Entity.Null) || !activeAbility_Entity.Exists()) break;

							var abilityStateBuffer = Core.EntityManager.GetBuffer<AbilityStateBuffer>(activeAbility_Entity);
							foreach (var state in abilityStateBuffer)
							{
								var abilityState = state.StateEntity._Entity;
								var abilityCooldownState = abilityState.Read<AbilityCooldownState>();
								ultCd = Math.Ceiling(Math.Max(abilityCooldownState.CooldownEndTime - time, 0)) + "s";
								break;
							}
							break;
						}
					}

					var msg = new FixedString512Bytes(
						cw.message.Replace("$combatTimer", combatTimer, StringComparison.OrdinalIgnoreCase)
						.Replace("$baneTimer", baneTimer, StringComparison.OrdinalIgnoreCase)
						.Replace("$deathTimer", deathTimer, StringComparison.OrdinalIgnoreCase)
						.Replace("$location", location, StringComparison.OrdinalIgnoreCase)
						.Replace("$ultCd", ultCd, StringComparison.OrdinalIgnoreCase)
					);
					Core.EntityManager.DestroyEntity(entity);

					if (cw.message[0] == '.')
					{
						var messageEvent = new ChatMessageEvent()
						{
							MessageType = ChatMessageType.Whisper,
							MessageText = cw.message,
						};

						var tempEnt = Core.EntityManager.CreateEntity();
						Core.EntityManager.AddComponentData(tempEnt, fromCharacter);
						Helper.AddComponentData(tempEnt, messageEvent);
						continue;
					}

					var scope = cw.scope == "global" ? ServerChatMessageType.Global : (cw.scope == "clan" ? ServerChatMessageType.Team : ServerChatMessageType.Local);

					// for clan chat
					if (scope == ServerChatMessageType.Team)
					{
						if (!clanEntity.Exists() || clanEntity.Equals(Entity.Null))
						{
							Helper.SendSystemMessageToClient(user, $"ChatWheel: You don't have a clan.");
							continue;
						}

						var userBuffer = Core.EntityManager.GetBuffer<SyncToUserBuffer>(clanEntity);

						for (var i = 0; i < userBuffer.Length; ++i)
						{
							var userBufferEntry = userBuffer[i];
							var memberUserEntity = userBufferEntry.UserEntity;

							if (memberUserEntity.TryRead<ConnectedUser>(out var connectedUser))
							{
								int idx = connectedUser.UserIndex;
								ServerChatUtils.SendChatMessage(Core.EntityManager, ref idx, ref msg, ref networkId, ref networkId2, scope, DateTime.UtcNow.Ticks);
							}
						}
					}

					// for local and global chats
					else
					{
						var playerEntities = Helper.GetEntitiesByComponentType<PlayerCharacter>();
						foreach (var playerEntity in playerEntities)
						{
							var userEnt = playerEntity.Read<PlayerCharacter>().UserEntity;

							if (userEnt.TryRead<ConnectedUser>(out var connectedUser))
							{
								if (scope == ServerChatMessageType.Local)
								{
									var senderPos = charEntity.Read<LocalToWorld>().Position;
									var pos = playerEntity.Read<LocalToWorld>().Position;
									if (Vector3.Distance(senderPos, pos) > 40) continue;
								}

								int idx = connectedUser.UserIndex;
								ServerChatUtils.SendChatMessage(Core.EntityManager, ref idx, ref msg, ref networkId, ref networkId2, scope, DateTime.UtcNow.Ticks);
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Core.Log.LogError($"Error in EmoteSystemPatch: {ex.Message}");
		}
		finally
		{
			entities.Dispose();
		}
	}
}
