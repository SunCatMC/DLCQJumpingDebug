using BepInEx;
using BepInEx.Logging;
using BepInEx.NET.Common;
using HarmonyLib;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Logger = KaitoKid.ArchipelagoUtilities.Net.Client.Logger;
using DLCLib;
using DLCLib.Input;
using DLCLib.Physics;
using System;
using System.Drawing.Printing;

namespace DLCQJumpingDebug
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("DLC.exe")]
    public class Plugin : BasePlugin
    {
        protected Harmony harmony;
        protected ILogger logger;
        public override void Load()
        {
            // Plugin startup logic
            Log.LogInfo($"Loading {PluginInfo.PLUGIN_NAME}...");

            logger = new LogHandler(Log);
            try
            {
                harmony = new Harmony(PluginInfo.PLUGIN_NAME);
                harmony.PatchAll();
            }
            catch (FileNotFoundException fnfe)
            {
                if (fnfe.FileName.Contains("Microsoft.Xna.Framework"))
                    Log.LogError($"Cannot load {PluginInfo.PLUGIN_NAME}: Microsoft XNA Framework 4.0 is not installed. Please run DLC Quest from Steam, then try again.");
                throw;
            }
            JumpPatch.Initialize(logger);

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }

    public class LogHandler(ManualLogSource logger) : Logger
    {
        private readonly ManualLogSource _logger = logger;

        public override void LogError(string message)
        {
            _logger.LogError(message);
        }
        public override void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }
        public override void LogMessage(string message)
        {
            _logger.LogMessage(message);
        }
        public override void LogInfo(string message)
        {
            _logger.LogInfo(message);
        }
        public override void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.PrePhysicsUpdate))]
    public class JumpPatch 
    {
        private static ILogger _logger;
        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }
        static bool Prefix(Player __instance, float ___jumpTime, out float __state, float dt)
        {
            __state = ___jumpTime;
            PatchState.playerInput = __instance.PlayerInput;
            return true;
        }

        static void Postfix(PhysicsObject ___physicsObject, float ___OFF_LEDGE_JUMP_TIME, float ___jumpTime, float __state, float dt)
        {
            if (___jumpTime != __state || PatchState.playerInput.Jump || ___physicsObject.OffGroundTime * 1000 > 17)
            {
                _logger.LogMessage($"new: {(___jumpTime * 1000)/ PatchState.timeScale}, old + delta: {((__state + dt) * 1000) / PatchState.timeScale}, OffGroundTime: {___physicsObject.OffGroundTime * 1000}");
                _logger.LogMessage($"CanJump: {___physicsObject.OffGroundTime < ___OFF_LEDGE_JUMP_TIME}, IsAtCeiling: {___physicsObject.IsAtCeiling}");
                if (PatchState.playerInput.Jump)
                {
                    _logger.LogMessage($"Player is jumping");
                }
                else
                {
                    _logger.LogMessage("");
                }
                _logger.LogMessage($"---");
            }
            if (PatchState.playerInput.Attack || PatchState.playerInput.Activate)
            {
                _logger.LogMessage($"---");
            }
        }
    }

    [HarmonyPatch(typeof(Scene))]
    [HarmonyPatch(nameof(Scene.Update))]
    public class ScenePatch
    {
        static bool Prefix(Scene __instance)
        {
            PatchState.timeScale = __instance.TimeScale;
            return true;
        }
    }

    public class PatchState
    {
        public static float timeScale = 0f;
        public static PlayerInput playerInput = PlayerInput.Empty;
    }
}
