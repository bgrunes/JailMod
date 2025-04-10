using System;
using System.Collections.Generic;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace JailMod.config
{
    public class JailData
    {
        public Vec3d JailPosition { get; set; }
        public Dictionary<string, JailEntry> JailedPlayers { get; set; } = new Dictionary<string, JailEntry>();

        public JailData()
        {
            JailPosition = null;
            JailedPlayers = new Dictionary<string, JailEntry>();
        }
    }

    public struct JailEntry
    {
        public Vec3d OriginalPosition { get; set; }
        public Vec3d? OriginalSpawn { get; set; }
        public double ReleaseTime { get; set; }
    }

    internal class JailModConfig 
    {
        private readonly string jailDataFilePath = "jailDataconfig.json";
        public JailData LoadJailData(ICoreServerAPI sapi)
        {
            // Load jail data from a ModConfig
            try
            {
                JailData data = sapi.LoadModConfig<JailData>(jailDataFilePath);
                if (data == null)
                {
                    sapi.Logger.Notification("No jail data found, creating new data file.");
                    data = new JailData();
                    SaveJailData(sapi, data); // Save the new data to create the file

                    sapi.Logger.Notification($"Data: {data.JailedPlayers.Keys.ToString()}");
                    return data;
                }
                else
                {
                    sapi.Logger.Notification("Loaded jail data.");
                    return data;
                }
            }
            catch(Exception e)
            {
                sapi.Logger.Error("Error loading jail data: " + e.Message);
                var defaultData = new JailData();
                SaveJailData(sapi, defaultData);
                return defaultData;
            }
        }

        public void SaveJailData(ICoreServerAPI sapi, JailData jailData) 
        {
            // Save jail data to a ModConfig
            try
            {
                sapi.StoreModConfig<JailData>(jailData, jailDataFilePath);
                sapi.Logger.Notification("Saved jail data.");
            }
            catch (Exception e)
            {
                sapi.Logger.Error("Error saving jail data: " + e.Message);
            }
        }
    }

}

