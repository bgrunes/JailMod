using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using JailMod.config;
using System.Configuration;

namespace JailMod.commands
{
    internal class JailCommands
    {
        public static void RegisterCommands(ICoreServerAPI sapi)
        {
            // Main command for Jail Mod
            sapi.ChatCommands.Create("jail")
                .WithDescription("Provides the server jail commands")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(args => HandleJailCommand(args, sapi))
                // Subcommand to set the jail location
                .BeginSubCommand("set")
                .WithDescription("Set the jail location")
                .RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .HandleWith(args => HandleJailSetCommand(args, sapi))
                .EndSubCommand()
                // Subcommand to jail a player
                .BeginSubCommand("jail")
                .WithDescription("Jail a player")
                .RequiresPrivilege(Privilege.controlserver)
                .WithArgs(new WordArgParser("player", true), new DoubleArgParser("duration", 1.0, true))
                .HandleWith(args => HandleJailPlayerCommand(args, sapi))
                .EndSubCommand()
                // Subcommand to release a player from jail
                .BeginSubCommand("release")
                .WithDescription("Release a player from jail")
                .RequiresPrivilege(Privilege.controlserver)
                .WithArgs(new WordArgParser("player", true))
                .HandleWith(args => HandleJailReleaseCommand(args, sapi))
                .EndSubCommand()
                // Subcommand to list players in jail
                .BeginSubCommand("list")
                .WithDescription("List players in jail")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(args => HandleJailListCommand(args, sapi))
                .EndSubCommand();

        }

        private static TextCommandResult HandleJailCommand(TextCommandCallingArgs args, ICoreServerAPI sapi)
        {
            return TextCommandResult.Success("Usage: /jail set|jail|release|list");
        }

        private static TextCommandResult HandleJailSetCommand(TextCommandCallingArgs args, ICoreServerAPI sapi)
        {
            try
            {
                // Get the mod system for access to data
                var modSystem = sapi.ModLoader.GetModSystem<JailModModSystem>();
                var pos = args.Caller.Player.Entity.Pos.XYZ;

                // Get the jail data, assign the jail position, save the data
                var jailData = modSystem.GetJailData();
                jailData.JailPosition = pos;
                modSystem.SaveJailDataExternal(jailData);
                return TextCommandResult.Success("Jail position set to your current location.");
            }
            catch (Exception e)
            {
                return TextCommandResult.Error("Error setting jail position: " + e.Message);
            }
        }

        private static TextCommandResult HandleJailPlayerCommand(TextCommandCallingArgs args, ICoreServerAPI sapi)
        {
            try
            {
                var modSystem = sapi.ModLoader.GetModSystem<JailModModSystem>();

                // Check if the jail position is set
                if (modSystem.GetJailData().JailPosition == null)
                {
                    throw new Exception("Jail position not set. Use '/jail set' to set the jail position.");
                }

                // Get player name and duration through args parsers
                string playerName = args.Parsers[0].GetValue() as String;
                double duration = (double)args.Parsers[1].GetValue();

                IServerPlayer player = sapi
                    .Server
                    .Players
                    .FirstOrDefault(
                        p => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase)
                    ) as IServerPlayer;
                if (player == null)
                {
                    throw new Exception("Player not found or not online.");
                }

                modSystem.JailPlayer(player, duration);
                return TextCommandResult.Success($"{player.PlayerName} has been jailed for {duration} hours.");
            }
            catch (Exception e)
            {
                return TextCommandResult.Error("Error jailing player: " + e.Message);
            }
        }

        private static TextCommandResult HandleJailReleaseCommand(TextCommandCallingArgs args, ICoreServerAPI sapi)
        {
            try
            {
                var modSystem = sapi.ModLoader.GetModSystem<JailModModSystem>();

                // Find player
                string playerName = args.Parsers[0].GetValue() as String;
                IServerPlayer player = sapi
                    .Server
                    .Players
                    .FirstOrDefault(
                        p => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase)
                    ) as IServerPlayer;

                if (player == null)
                {
                    throw new Exception("Player not found or not online.");
                }

                if (!modSystem.GetJailData().JailedPlayers.ContainsKey(player.PlayerUID))
                {
                    throw new Exception("Player is not in jail.");
                }

                // Release player
                modSystem.ReleasePlayerExternal(player);
                sapi.Logger.Notification($"{player.PlayerName} has been released from jail.");
                return TextCommandResult.Success($"{player.PlayerName} has been released from jail.");
            }
            catch (Exception e)
            {
                return TextCommandResult.Error("Error releasing player: " + e.Message);
            }
        }

        private static TextCommandResult HandleJailListCommand(TextCommandCallingArgs args, ICoreServerAPI sapi)
        {
            try
            {
                var modSystem = sapi.ModLoader.GetModSystem<JailModModSystem>();
                var jailData = modSystem.GetJailData().JailedPlayers;

                if (jailData.Count == 0)
                {
                    return TextCommandResult.Success("No players are currently jailed.");
                }

                string list = "CURRENTLY JAILED PLAYERS:\n-----------------------------------------\n";
                double currentTime = sapi.World.Calendar.TotalHours;
                foreach (var entry in jailData)
                {
                    double timeLeft = (entry.Value.ReleaseTime - currentTime) * 60.0;
                    list += $"{entry.Key} (Time left: {timeLeft:F1} minutes)\n";
                }
                list += "-----------------------------------------\n";

                return TextCommandResult.Success(list);
            }
            catch (Exception e)
            {
                return TextCommandResult.Error("Error printing list: " + e.Message);
            }
        }
    }
}
