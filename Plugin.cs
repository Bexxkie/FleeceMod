﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Configuration;

namespace FleeceMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string LEGACY_GUID = "com.lily.goldenFleeceFix";
        public const string GUID = "goldenFleeceFix";
        public const string NAME = "Golden Fleece Fix";
        public const string VERSION = "1.0";

        public static ManualLogSource _log;

        public static ConfigEntry<float> IncrementValue;
        // INIT...
        private void Awake()
        {
            _log = new ManualLogSource("fleeceMod-Log");
            BepInEx.Logging.Logger.Sources.Add(_log);

            IncrementValue = Config.Bind("General", "IncDamage", 0.1f, "Set damage increase per kill");
            // Apply all the patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            _log.LogInfo($"Fleecemod loaded");

        }
    }
    // --Patches--
    //  Golden fleece patch
    [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.IncrementDamageModifier))]
    public class PlayerFleeceManagerPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin._log.LogInfo($"Patching Golden fleece");
            float incDam = Plugin.IncrementValue.Value;
            return new CodeMatcher(instructions)
                .Advance(offset: 4)                 // skip over 'DataManager.Instance.PlayerFleece == 1'
                .RemoveInstructionsInRange(4, 6)    // remove 'damageMultiplier < 2f'
                .Advance(offset: 2)                 // skip down to the next float declaration 
                .Set(OpCodes.Ldc_R4, incDam)        // replace the +0.1f with our custom value
                .InstructionEnumeration();        
        }
    }
    //  Next fleece patch goes here

    // Find some way to incentivize the other fleece without nerfing anything.
    // 
    // Tarot        - gain 4 tarot at the start, but cannot draw new ones 
    //       - prop - pos - chance to draw 2 cards | increase chance of high quality tarot   |  higher chance of card drops from chests
    //       - prop - neg - cannot heal past 1/2 HP
    //
    // Rotten_heart - on gaining a tarot +1 rotten heart
    //       - prop - pos - rotten hearts deal x2 damage | chance for enemy to drop rotten heart | 
    //       - prop - neg - can only pick up rotten hearts (no blue, red)
    //
    // Golden       - on kill +damage, take double damage
    //       - prop - pos - increase weapon damage on kill (does not affect curses
    //       - prop - neg - player will die in one hit and lose all resources, cannot gain HP or tempHP 
    //
    // Blue_heart   - 1.5x HP, but all are tempHP
    //       - prop - pos - 2x HP as blue hearts
    //       - prop - neg - can only gain blue hearts, health pickups half as effective
    //
    // Curse_dmg    - 2x curse damage 1/2 curse cost 1/2 weapon damage 1/2 HP
    //       - prop - pos - 
    //       - prop - neg - 


}

