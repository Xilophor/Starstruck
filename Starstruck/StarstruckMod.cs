using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Starstruck
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class StarstruckMod : BaseUnityPlugin
    {
        public static StarstruckMod Instance;
        
        internal static new ManualLogSource Logger;

        private Harmony _harmony;
        
        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();
            
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void Patch()
        {
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);


        }

        private void Unpatch()
        {
            
        }
    }
}