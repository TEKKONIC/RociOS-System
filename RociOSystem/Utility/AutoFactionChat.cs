using HarmonyLib;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.Game.Gui;
using System.Reflection;
using NLog.Fluent;
using RociOS;

namespace RociOS.utilities
{
    [HarmonyPatch]
    internal static class MyChatSystemPatch
    {
        private static bool messageShown = false;

        private static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(MyChatSystem));
        }

        private static void Postfix(ref ChatChannel ___m_currentChannel)
        {
            if (!messageShown)
            {
                ___m_currentChannel = ChatChannel.Faction;
                Log.Info("Switched to Faction Channel");
                MyAPIGateway.Utilities.ShowMessage("RociOS", "Switched to Faction Channel.");
                Log.Info("Message shown successfully.");
                messageShown = true;
            }
        }
    }
}