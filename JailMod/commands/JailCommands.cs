using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace JailMod.commands
{
    internal class JailCommands
    {
        public static void RegisterCommands(ICoreServerAPI sapi)
        {
            sapi.ChatCommands.Create("setjail")
                .WithDescription("Set the jail location")
                .RequiresPrivilege(Privilege.controlserver)
                .WithAlias("sj")
                .HandleWith(args => SetJailCommand(args, sapi));
        }

        private static TextCommandResult SetJailCommand(TextCommandCallingArgs args, ICoreServerAPI sapi)
        {
            try
            {

            }
            catch (Exception e)
            {
                sapi.Logger.Error("Error setting jail location: " + e.Message);
                return TextCommandResult.Error("Error setting jail location: " + e.Message);
            }
            
            return TextCommandResult.Success("Jail location set to: " + args.Caller.Pos.ToString());
        }
    }
}
