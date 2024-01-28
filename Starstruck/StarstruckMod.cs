using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;

namespace Starstruck;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("evaisa.lethallib")]
[BepInDependency("LethalNetworkAPI")]
public class StarstruckMod : BaseUnityPlugin
{
    public static StarstruckMod Instance;
        
    internal new static ManualLogSource Logger;
    internal static GameObject effectObject;
    internal static GameObject effectPermanentObject;
    
    private static Harmony _harmony;
        
    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        LoadAssetBundle("starstruckweatherassets");
        Patch();
        
        effectObject = Instantiate(Assets["StarstruckMeteorContainer"] as GameObject, Vector3.zero, Quaternion.identity);
        effectObject.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(effectObject);
        effectPermanentObject = Instantiate(Assets["StarstruckWeather"] as GameObject, Vector3.zero, Quaternion.identity);
        effectPermanentObject.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(effectPermanentObject);
        
        var starstruckWeather = new WeatherEffect()
        {
            name = "Starstruck",
            effectObject = effectObject,
            effectPermanentObject = effectPermanentObject,
            lerpPosition = false,
            sunAnimatorBool = "",
            transitioning = false
        };
            
        Weathers.RegisterWeather("Starstruck", starstruckWeather, Levels.LevelTypes.All, 0, 0);
            
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private void LoadAssetBundle(string assetBundleName)
    {
        AssetBundle
            .LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location)!, assetBundleName))
            .LoadAllAssets()
            .Do(asset => Assets[asset.name] = asset);

#if DEBUG
        Assets.Do(assetPair => Logger.LogDebug(assetPair.Key));
#endif
    }

    private static void Patch()
    {
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        _harmony.PatchAll();
    }

    private static void Unpatch()
    {
        _harmony.UnpatchSelf();
    }

    internal static readonly Dictionary<string, Object> Assets = [];
}