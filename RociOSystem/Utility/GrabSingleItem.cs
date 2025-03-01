using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using VRage.Input;
using RociOS;
using NLog;

namespace RociOS.utilities
{
    [HarmonyPatch]
    internal static class MyTerminalInventoryControllerPatch
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [HarmonyPatch("Sandbox.Game.Gui.MyTerminalInventoryController", "grid_ItemClicked")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Debug("Transpiler method called for grid_ItemClicked.");
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Or)
                {
                    Log.Debug("Opcode 'Or' found, inserting CheckForAlt call.");
                    yield return CodeInstruction.Call(typeof(MyTerminalInventoryControllerPatch), nameof(CheckForAlt));
                }
            }
        }

        private static bool CheckForAlt(bool original)
        {
            bool isAltPressed = MyInput.Static.IsAnyAltKeyPressed();
            Log.Debug($"CheckForAlt called. Original: {original}, IsAnyAltKeyPressed: {isAltPressed}");
            return original || isAltPressed;
        }

        [HarmonyPatch("Sandbox.Game.Gui.MyTerminalInventoryController", "grid_ItemDragged")]
        [HarmonyPrefix]
        private static bool DragPrefix()
        {
            bool isAltPressed = MyInput.Static.IsAnyAltKeyPressed();
            Log.Debug($"DragPrefix called. IsAnyAltKeyPressed: {isAltPressed}");
            return !isAltPressed;
        }

        [HarmonyPatch("Sandbox.Game.Gui.MyTerminalInventoryController", "grid_ItemDoubleClicked")]
        [HarmonyPrefix]
        private static bool DoubleClickPrefix()
        {
            bool isAltPressed = MyInput.Static.IsAnyAltKeyPressed();
            Log.Debug($"DoubleClickPrefix called. IsAnyAltKeyPressed: {isAltPressed}");
            return !isAltPressed;
        }
    }
}