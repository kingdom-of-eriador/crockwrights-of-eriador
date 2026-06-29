using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Crockwrights
{
    public class CrockwrightsModSystem : ModSystem
    {
        private Harmony harmony;

        // Called on server and client
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            string harmonyId = Mod.Info.ModID;
            if (!Harmony.HasAnyPatches(harmonyId))
            {
                var harmony = new Harmony(harmonyId);
                harmony.PatchAll();
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
        }

        public override void Dispose()
        {
            string harmonyId = Mod.Info.ModID;

            if (Harmony.HasAnyPatches(harmonyId))
            {
                new Harmony(harmonyId).UnpatchAll(harmonyId);
            }

            base.Dispose();
        }

    }

    // --- Harmony Prefix Patch ---
    [HarmonyPatch]

    public static class PotInFirePitShapeSupplierPatch
    {
        public static string OverrideBasePath(string defaultPath, Block blockToRender) => blockToRender?.Attributes?["rendererBasePath"].AsString(defaultPath) ?? defaultPath;

        [HarmonyPatch(typeof(PotInFirepitRenderer), MethodType.Constructor, typeof(ICoreClientAPI), typeof(ItemStack), typeof(BlockPos), typeof(bool))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> tRANSPILER(IEnumerable<CodeInstruction> instructions, ILGenerator generator) => new CodeMatcher(instructions, generator)
            .MatchStartForward(CodeMatch.LoadsConstant("shapes/block/clay/pot-"))
            .InsertAfter(
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PotInFirePitShapeSupplierPatch), nameof(OverrideBasePath)))
            ).InstructionEnumeration();
    }

}