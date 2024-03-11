using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace DRGS_Wiki;

public class WeaponDoc : Doc {
    private static WeaponDoc Instance { get; set; }

    public WeaponDoc() : base("weapons") {
        Instance = this;
        Plugin.Harmony.PatchAll(typeof(Patches));
    }

    public class Patches {
        [HarmonyPatch(typeof(MenuAssetLoader), nameof(MenuAssetLoader.Awake))]
        [HarmonyPostfix]
        public static void OnSceneLoaded() {
            Instance.DocWeapons();
        }
    }

    private static float GetFireRate(ProjectileWeaponSkillData weapon) {
        switch (weapon.FireMode) {
            case WeaponSkillData.EFireMode.SINGLE:
                return 1f / weapon.ShotInterval;
            case WeaponSkillData.EFireMode.BURST:
                return 1f / weapon.ShotInterval * weapon.BurstShots;
            default:
                Plugin.Instance.Log.LogWarning($"Unknown fire mode {weapon.FireMode} for weapon {weapon.name}");
                break;
        }

        return 0f;
    }

    private static float GetDPS(ProjectileWeaponSkillData weapon) {
        float damage = weapon.BaseDamage;
        float fireRate = GetFireRate(weapon);
        float clipSize = weapon.BaseClipSize;
        float reloadTime = weapon.ReloadTime;

        return damage * fireRate * (clipSize / fireRate) / (clipSize / fireRate + reloadTime);
    }

    private static float GetDPS(GrenadeWeaponSkillData weapon) {
        float damage = weapon.BaseDamage;
        float reloadTime = weapon.ReloadTime;

        return damage / reloadTime;
    }

    public void DocWeapons() {
        Il2CppArrayBase<ProjectileWeaponSkillData>? projectileWeapons = Resources.FindObjectsOfTypeAll<ProjectileWeaponSkillData>();
        Plugin.Instance.Log.LogInfo($"Found {projectileWeapons.Length} projectile weapons");

        AddTable("Weapons",
            projectileWeapons.Where(w => !w.IsBoscoSkill && !w.name.StartsWith("Enemy")),
            new string[] { "Name", "Damage", "Fire Rate", "Clip Size", "Reload Time", "DPS" },
            w => new object[] {
                w.name, w.BaseDamage, $"{GetFireRate(w):0.00}/s", w.BaseClipSize, $"{w.ReloadTime}s", $"{GetDPS(w):0.00}"
            }
        );

        Il2CppArrayBase<GrenadeWeaponSkillData>? grenadeWeapons = Resources.FindObjectsOfTypeAll<GrenadeWeaponSkillData>();
        Plugin.Instance.Log.LogInfo($"Found {grenadeWeapons.Length} grenade weapons");

        AddTable("Grenades",
            grenadeWeapons.Where(w => !w.IsBoscoSkill && !w.name.StartsWith("Enemy")),
            new string[] { "Name", "Damage", "Explosion Radius", "Reload Time", "DPS" },
            w => new object[] {
                w.name, w.BaseDamage, w.BaseExplosionRadius, $"{w.ReloadTime}s", $"{GetDPS(w):0.00}"
            }
        );
    }
}
