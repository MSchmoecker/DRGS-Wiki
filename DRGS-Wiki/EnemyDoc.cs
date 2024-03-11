using Assets.Scripts.EnemyBehaviours;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace DRGS_Wiki;

public class EnemyDoc : Doc {
    private static EnemyDoc Instance { get; set; }

    public EnemyDoc() : base("enemies") {
        Instance = this;
        Plugin.Harmony.PatchAll(typeof(Patches));
    }

    public class Patches {
        // [HarmonyPatch(typeof(EnemySpawner), nameof(EnemySpawner.Start))]
        [HarmonyPatch(typeof(MenuAssetLoader), nameof(MenuAssetLoader.Awake))]
        [HarmonyPostfix]
        public static void OnSceneLoaded(EnemySpawner __instance) {
            Instance.DocEnemies(__instance);
        }
    }

    public void DocEnemies(EnemySpawner spawner) {
        List<Enemy> enemies = Resources.FindObjectsOfTypeAll<Enemy>().Where(e => !e.name.EndsWith("(Clone)")).ToList();
        Plugin.Instance.Log.LogInfo($"Found {enemies.Count} enemies");

        AddTable("Enemies",
            enemies,
            new string[] { "Name", "Health", "Damage", "Movement Speed" },
            e => new object[] { e.name, e._baseMaxHp, e._baseDamage, e._baseMoveSpeed }
        );
    }
}
