﻿using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JailMod
{
    public class JailModModSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private JailData jailData;
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            //Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("jailmod:hello"));



        }

        //public override void StartClientSide(ICoreClientAPI api)
        //{
        //    Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("jailmod:hello"));
        //}

    }
}
