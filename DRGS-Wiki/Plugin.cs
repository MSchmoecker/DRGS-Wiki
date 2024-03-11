using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace DRGS_Wiki;

[BepInPlugin(ModGuid, ModName, ModVersion)]
public class Plugin : BasePlugin {
    public const string ModName = "DRGS Wiki";
    public const string ModGuid = "com.maxsch.DRGS.DRGSWiki";
    public const string ModVersion = "0.0.1";

    public static Plugin Instance { get; private set; }

    public static Harmony Harmony { get; private set; }

    public override void Load() {
        Instance = this;
        Harmony = new Harmony(ModGuid);

        Doc.BaseDir = Paths.PluginPath + Path.DirectorySeparatorChar + "DRGS-Wiki";

        List<Doc> docs = new List<Doc> {
            new WeaponDoc(),
            new EnemyDoc(),
        };
    }
}
