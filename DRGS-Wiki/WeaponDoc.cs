using System.Collections;
using Assets.Scripts.Milestones;
using Assets.Scripts.SkillSystem;
using BepInEx.Unity.IL2CPP.Utils.Collections;
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
        public static void OnSceneLoaded(MenuAssetLoader __instance) {
            __instance.StartCoroutine(Instance.DocWeapons().WrapToIl2Cpp());
        }
    }

    public static int GetNrOfProjectiles(ProjectileWeaponSkillData weapon) {
        switch (weapon.FireMode) {
            case WeaponSkillData.EFireMode.SINGLE:
                return 1;
            case WeaponSkillData.EFireMode.BURST:
                return 1 + weapon.BurstShots;
            default:
                Plugin.Instance.Log.LogWarning($"Unknown fire mode {weapon.FireMode} for weapon {weapon.name}");
                break;
        }

        return 1;
    }

    public static float GetFireRate(ProjectileWeaponSkillData weapon) {
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

    public static float GetFireRate(GrenadeWeaponSkillData weapon) {
        return 1f / weapon.ReloadTime;
    }

    public static float GetDPS(ProjectileWeaponSkillData weapon) {
        float damage = weapon.BaseDamage;
        float fireRate = GetFireRate(weapon);
        float clipSize = weapon.BaseClipSize;
        float reloadTime = weapon.ReloadTime;

        return damage * fireRate * (clipSize / fireRate) / (clipSize / fireRate + reloadTime);
    }

    public static float GetDPS(GrenadeWeaponSkillData weapon) {
        float damage = weapon.BaseDamage;
        float reloadTime = weapon.ReloadTime;

        return damage / reloadTime;
    }

    public static Dictionary<string, MilestoneData> weaponMilestones = new Dictionary<string, MilestoneData>();
    public static Dictionary<string, string> defaultUnlockedWeapons = new Dictionary<string, string>();

    public IEnumerator DocWeapons() {
        // we have to wait a few frames before the localization is properly loaded
        for (int i = 0; i < 5; i++) {
            yield return null;
        }

        Il2CppArrayBase<MilestoneData>? milestones = Resources.FindObjectsOfTypeAll<MilestoneData>();
        weaponMilestones.Clear();
        defaultUnlockedWeapons.Clear();

        foreach (MilestoneData milestone in milestones) {
            if (milestone.WeaponReward != null) {
                weaponMilestones.TryAdd(milestone.WeaponReward.name, milestone);
                Plugin.Instance.Log.LogInfo($"Found milestone {milestone.name} for weapon {milestone.WeaponReward.name} {milestone.DescriptionText}");
            }
        }

        foreach (MilestoneData milestone in milestones) {
            if (milestone.ClassReward != null) {
                foreach (WeaponSkillData defaultUnlockedWeapon in milestone.ClassReward.DefaultUnlockedWeapons) {
                    weaponMilestones.TryAdd(defaultUnlockedWeapon.name, milestone);
                    defaultUnlockedWeapons.TryAdd(defaultUnlockedWeapon.name, milestone.ClassReward.DisplayName);
                    Plugin.Instance.Log.LogInfo($"Found milestone {milestone.name} for weapon {defaultUnlockedWeapon.name} {milestone.DescriptionText}");
                }
            }
        }

        Il2CppArrayBase<ProjectileWeaponSkillData>? projectileWeapons = Resources.FindObjectsOfTypeAll<ProjectileWeaponSkillData>();
        Plugin.Instance.Log.LogInfo($"Found {projectileWeapons.Length} projectile weapons");

        AddTable("Weapons",
            projectileWeapons.Where(w => !w.IsBoscoSkill && !w.name.StartsWith("Enemy")),
            new string[] { "Name", "Damage", "Fire Rate", "Clip Size", "Reload Time", "DPS" },
            w => new object[] {
                w.Title, w.BaseDamage, $"{GetFireRate(w):0.00}/s", w.BaseClipSize, $"{w.ReloadTime}s", $"{GetDPS(w):0.00}"
            }
        );

        List<WeaponSkillData> weapons = new List<WeaponSkillData>();
        weapons.AddRange(Resources.FindObjectsOfTypeAll<ArcProjectileWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<BoomerangWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<ProjectileWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<ShardDiffractorWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<BeamWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<DefenseDroneWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<SpawnWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<GrenadeWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<AuraWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<ExplosionWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<MeleeWeaponSkillData>());
        weapons.AddRange(Resources.FindObjectsOfTypeAll<CoilGunWeaponSkillData>());

        HashSet<SingleWeaponDoc> singleWeaponDocs = new HashSet<SingleWeaponDoc>();
        foreach (WeaponSkillData weapon in weapons.Where(w => !w.IsBoscoSkill && !w.name.StartsWith("Enemy"))) {
            if (singleWeaponDocs.Any(w => w.Weapon.name == weapon.name)) {
                continue;
            }

            try {
                string title = weapon.Title;
            } catch (Exception e) {
                Plugin.Instance.Log.LogWarning($"Failed to get title for weapon {weapon.name}");
                continue;
            }

            singleWeaponDocs.Add(new SingleWeaponDoc(weapon));
        }

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
