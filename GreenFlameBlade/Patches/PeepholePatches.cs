using HarmonyLib;

namespace GreenFlameBlade.Patches
{
    [HarmonyPatch(typeof(Peephole))]
    public static class PeepholePatches
    {
        [HarmonyPostfix, HarmonyPatch(nameof(Peephole.Update))]
        public static void Update(Peephole __instance)
        {
            if (__instance._peeping && !PlayerState.InDreamWorld())
            {
                __instance._peeping = false;
                __instance._exitClosingEyes = true;
                __instance._exitTransitioningOut = false;
                __instance._fadeTimer = 0f;
            }
        }
    }
}
