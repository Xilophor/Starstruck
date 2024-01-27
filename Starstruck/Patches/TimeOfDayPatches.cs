using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;

namespace Starstruck.Patches;

[HarmonyPatch(typeof(StartOfRound))]
[HarmonyPriority(Priority.HigherThanNormal)]
[HarmonyWrapSafe]
internal class TimeOfDayPatches
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPrefix]
    private static void AwakePrefix(StartOfRound __instance)
    {
    }
}