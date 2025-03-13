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
using Microsoft.Xna.Framework;

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
        static bool Prefix(Player __instance, PhysicsObject ___physicsObject, float ___jumpTime, out float __state, float dt)
        {
            __state = ___jumpTime;
            PatchState.playerInput = __instance.PlayerInput;
            float substepGravity = PhysicsManager.GRAVITY.Y * dt;
            PatchState.preJumpYVelocity = ___physicsObject.Velocity.Y; //ignoring collision and ground
            return true;
        }
        static void Postfix(PhysicsObject ___physicsObject, float ___OFF_LEDGE_JUMP_TIME, float ___JUMP_LAUNCH_VELOCITY, float ___MAX_JUMP_TIME, float ___JUMP_CONTROL_POWER, float ___jumpTime, float __state, float dt)
        {
            float substepGravity = PhysicsManager.GRAVITY.Y * dt;
            float actualVelocity = ___physicsObject.Velocity.Y + substepGravity; //ignoring collision and ground
            actualVelocity = Vector2.Clamp(new Vector2(0f, actualVelocity), -PatchState.MAX_VELOCITY, PatchState.MAX_VELOCITY).Y;
            if (___jumpTime != __state || PatchState.playerInput.Jump || !___physicsObject.IsOnGround)
            {
                if (___physicsObject.IsOnGround)
                {
                    PatchState.startingY = ___physicsObject.AABB.Center.Y;
                    PatchState.prevFrameYVelocity = 0f;
                }
                _logger.LogMessage($"new: {___jumpTime/ PatchState.timeScale:R}, old + delta: {__state + dt / PatchState.timeScale:R}, OffGroundTime: {___physicsObject.OffGroundTime:R}");
                _logger.LogMessage($"CanJump: {___physicsObject.OffGroundTime < ___OFF_LEDGE_JUMP_TIME}, IsAtCeiling: {___physicsObject.IsAtCeiling}");
                _logger.LogMessage($"Y delta: {___physicsObject.AABB.Center.Y - PatchState.startingY:R}, Y velocity: {actualVelocity:R}");
                float jumpVelocity = ___JUMP_LAUNCH_VELOCITY * (1f - (float)Math.Pow(___jumpTime / ___MAX_JUMP_TIME, ___JUMP_CONTROL_POWER));
                if (___jumpTime == 0f)
                {
                    jumpVelocity = 0f;
                }
                _logger.LogMessage($"jump Y velocity      : {jumpVelocity:R}");
                _logger.LogMessage($"jump Y velocity delta: {jumpVelocity - PatchState.preJumpYVelocity:R}, substep gravity: {substepGravity:R}");
                _logger.LogMessage($"player Y velocity delta: {actualVelocity - PatchState.prevFrameYVelocity:R}");
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
            PatchState.prevFrameYVelocity = actualVelocity;
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

    [HarmonyPatch(typeof(PhysicsManager))]
    [HarmonyPatch(nameof(PhysicsManager.Step))]
    public class PhysicsManagerPatch
    {
        static bool Prefix(Vector2 ___MAX_VELOCITY)
        {
            PatchState.MAX_VELOCITY = ___MAX_VELOCITY;
            return true;
        }
    }

    public class PatchState
    {
        public static float timeScale = 0f;
        public static PlayerInput playerInput = PlayerInput.Empty;
        public static float startingY = 0f;
        public static float preJumpYVelocity = 0f;
        public static float prevFrameYVelocity = 0f;
        public static Vector2 MAX_VELOCITY = Vector2.One;
    }
}
