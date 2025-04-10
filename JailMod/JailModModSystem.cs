using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using JailMod.commands;
using JailMod.config;
using System.Linq;
using System;

namespace JailMod
{
    public class JailModModSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private JailModConfig config = new JailModConfig();
        private JailData jailData;
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Jail Mod" + api.Side + " " + Lang.Get("jailmod:hello"));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            //Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("jailmod:hello"));
            sapi = api;
            
            // Register all commands for the Jail Mod
            JailCommands.RegisterCommands(sapi);

            // Load jail data from a file
           jailData = config.LoadJailData(sapi);

            sapi.Event.Timer(CheckJailTimes, 1 / 60.0);
        }

        private void CheckJailTimes()
        {
            if (jailData == null)
            {
                sapi.Logger.Warning("Jail not initialized, cannot check jail times. Attempting to recover...");
                jailData = config.LoadJailData(sapi);
                if (jailData == null)
                {
                    sapi.Logger.Error("Failed to recover jail data. Please check the configuration.");
                    return;
                }
                
                sapi.Logger.Notification("Recovered jail data.");
            }

            try
            {
                double currentTime = sapi.World.Calendar.TotalHours;
                List<string> playersToRelease = new List<string>();

                foreach (var entry in jailData.JailedPlayers)
                {
                    if (entry.Value.ReleaseTime <= currentTime)
                    {
                        playersToRelease.Add(entry.Key);
                    }
                }

                foreach (string uid in playersToRelease)
                {
                    IServerPlayer player = sapi.Server.Players.FirstOrDefault(p => p.PlayerUID == uid);
                    if (player != null)
                    {
                        ReleasePlayer(player);
                        sapi.SendMessageToGroup(GlobalConstants.GeneralChatGroup,
                            $"{player.PlayerName} has been released from jail!",
                            EnumChatType.Notification);
                    }
                }
            }
            catch (Exception e)
            {
                sapi.Logger.Error("Error checking jail times: " + e.Message);
            }
        }

        private void ReleasePlayer(IServerPlayer player)
        {
            if (jailData.JailedPlayers.ContainsKey(player.PlayerUID))
            {
                // Get the original spawn point and position of the player before jailing
                Vec3d ogPos = jailData.JailedPlayers[player.PlayerUID].OriginalPosition;
                PlayerSpawnPos ogSpawn = new PlayerSpawnPos
                {
                    x = (int)jailData.JailedPlayers[player.PlayerUID].OriginalSpawn.X,
                    y = (int)jailData.JailedPlayers[player.PlayerUID].OriginalSpawn.Y,
                    z = (int)jailData.JailedPlayers[player.PlayerUID].OriginalSpawn.Z,
                    yaw = player.Entity.Pos.Yaw,
                    pitch = player.Entity.Pos.Pitch
                };
                player.Entity.TeleportTo(ogPos);
                player.SetSpawnPosition(ogSpawn);

                // Remove the player from the jail data
                jailData.JailedPlayers.Remove(player.PlayerUID);
                // Save the updated jail data
                config.SaveJailData(sapi, jailData);
            }
            else
            {
                sapi.Logger.Error($"Player {player.PlayerName} is not in jail.");
            }
        }

        

        // Public methods for access to data
        public JailData GetJailData() => jailData;
        public void SaveJailDataExternal(JailData jailData)
        {
            this.jailData = jailData;
            config.SaveJailData(sapi, jailData);
        }
        public void JailPlayer(IServerPlayer player, double duration)
        {
            Vec3d ogPos = player.Entity.Pos.XYZ;
            Vec3d ogSpawn = player.GetSpawnPosition(false)?.XYZ;

            player.Entity.TeleportTo(jailData.JailPosition);

            // Create a PlayerSpawnPos object from the player's current position
            PlayerSpawnPos spawnPos = new PlayerSpawnPos
            {
                x = (int)player.Entity.Pos.X,
                y = (int)player.Entity.Pos.Y,
                z = (int)player.Entity.Pos.Z,
                yaw = player.Entity.Pos.Yaw,
                pitch = player.Entity.Pos.Pitch
            };

            player.SetSpawnPosition(spawnPos);

            // Add player to list of jailed players
            jailData.JailedPlayers.Add(player.PlayerName, new JailEntry
            {
                OriginalPosition = ogPos,
                OriginalSpawn = ogSpawn,
                ReleaseTime = sapi.World.Calendar.TotalHours + duration
            });

            config.SaveJailData(sapi, jailData);
        }
        public void ReleasePlayerExternal(IServerPlayer player) => ReleasePlayer(player);
    }
}
