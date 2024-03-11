using System.Text.RegularExpressions;
using Assets.Scripts.Milestones;

namespace DRGS_Wiki;

public class SingleWeaponDoc : Doc {
    public WeaponSkillData Weapon { get; }

    private Dictionary<string, MilestoneData> WeaponMilestones;

    public SingleWeaponDoc(WeaponSkillData weapon, Dictionary<string, MilestoneData> weaponMilestones) : base($"weapons/{weapon.name}") {
        Plugin.Instance.Log.LogInfo($"Documenting weapon {weapon.name}");
        Weapon = weapon;
        WeaponMilestones = weaponMilestones;

        AddText("{{SurvivorWeaponsStats");
        AddText($" | name = {weapon.Title} <!--String (mandatory)-->");
        AddText($" | image = {ImageName()} <!--Text-->");
        AddText($" | unlockedBy = {UnlockedBy()} <!--Text-->".Replace("  ", " "));

        AddText($"<!--Tags-->");
        AddText($" | damageTag = {GetDamageTags(Weapon)} <!--List (^) of String (allowed values = {GetDamageTags()})-->");
        AddText($" | familyTag = {GetFamilyTags(Weapon)} <!--List (^) of String (allowed values = {GetFamilyTags()})-->");
        AddText($" | firingTag = {GetFiringTags(Weapon)} <!--List (^) of String (allowed values = {GetFiringTags()})-->");
        AddText($" | typeTag = {GetTypeTags(Weapon)} <!--List (^) of String (allowed values = {GetTypeTags()})-->");

        AddText($"<!--Base Stats-->");
        AddText($" | damage = {ConvertToString((int)Weapon.BaseDamage)} <!--Integer-->");

        if (weapon is ProjectileWeaponSkillData projectileWeapon) {
            AddText($" | roF = {ConvertToString(WeaponDoc.GetFireRate(projectileWeapon))} <!--Float-->");
            AddText($" | clipSize = {ConvertToString(projectileWeapon.BaseClipSize)} <!--Integer-->");
        }

        AddText($" | reloadTime = {ConvertToString(Weapon.ReloadTime)} <!--Float-->");
        AddText($" | weaponRange = {ConvertToString(Weapon.BaseRange)} <!--Float-->");

        AddText($"<!--Overclocks-->");
        AddText($" | balancedOC = {GetBalancedOverclocks()} <!--List (^) of String-->");
        AddText($" | unstableOC = {GetUnstableOverclocks()} <!--List (^) of String-->");

        BeamWeaponSkillData beamWeapon = weapon as BeamWeaponSkillData;
        DefenseDroneWeaponSkillData defenseDroneWeapon = weapon as DefenseDroneWeaponSkillData;
        SpawnWeaponSkillData spawnWeapon = weapon as SpawnWeaponSkillData;

        if (beamWeapon || defenseDroneWeapon || spawnWeapon) {
            AddText("<!--Lasting-->");
            AddText($" | duration = {ConvertToString(beamWeapon?.BaseLifeTime ?? defenseDroneWeapon?.BaseLifeTime ?? spawnWeapon?.SpawnLifeTime)} <!--Float-->");
        }

        if (spawnWeapon) {
            AddText("<!--Turret-->");
            AddText($" | carryCapacity = {ConvertToString(spawnWeapon.MaxCharges)} <!--Integer-->");
        }

        if (beamWeapon) {
            AddText("<!--Beam-->");
            AddText($" | beamCount = {ConvertToString(beamWeapon.BeamCount)} <!--Integer-->");
        }

        if (defenseDroneWeapon) {
            AddText("<!--Drone-->");
            AddText($" | droneCount = {ConvertToString(defenseDroneWeapon.DroneCount)} <!--Integer-->");
        }

        AddText("}}");
    }

    private string ImageName() {
        string title = Weapon.Title;

        Regex quotes = new Regex("\"[^\"]*\"");
        if (quotes.IsMatch(title)) {
            title = quotes.Match(title).Value;
            title = title.Substring(1, title.Length - 2);
        }

        return $"Survivor {title}.png";
    }

    private string UnlockedBy() {
        if (WeaponMilestones.TryGetValue(Weapon.name, out MilestoneData milestone)) {
            if (milestone.ClassRequirement != null) {
                return $"{milestone.ClassRequirement.DisplayName} Class Rank {milestone.Target}";
            }

            if (milestone.ClassReward != null) {
                return $"Player Rank {milestone.Target}";
            }

            return milestone.DescriptionText;
        }

        return "";
    }

    private static List<string> damageTags = new List<string> {
        "Kinetic",
        "Fire",
        "Cold",
        "Electric",
        "Acid",
        "Plasma"
    };

    private static List<string> familyTags = new List<string> {
        "Light",
        "Medium",
        "Heavy",
        "Throwable",
        "Construct"
    };

    private static List<string> firingTags = new List<string> {
        "Precise",
        "Spray",
        "Area",
        "Beam",
        "Lasting"
    };

    private static List<string> typeTags = new List<string> {
        "Projectile",
        "Explosive",
        "Drone",
        "Turret"
    };

    private string GetDamageTags() {
        return string.Join(", ", damageTags);
    }

    private string GetDamageTags(WeaponSkillData weapon) {
        return string.Join("^", weapon.Tags.ToArray().Where(t => damageTags.Contains(t.DisplayName)).Select(t => t.DisplayName));
    }

    private string GetFamilyTags() {
        return string.Join(", ", familyTags);
    }

    private string GetFamilyTags(WeaponSkillData weapon) {
        return string.Join("^", weapon.Tags.ToArray().Where(t => familyTags.Contains(t.DisplayName)).Select(t => t.DisplayName));
    }

    private string GetFiringTags() {
        return string.Join(", ", firingTags);
    }

    private string GetFiringTags(WeaponSkillData weapon) {
        return string.Join("^", weapon.Tags.ToArray().Where(t => firingTags.Contains(t.DisplayName)).Select(t => t.DisplayName));
    }

    private string GetTypeTags() {
        return string.Join(", ", typeTags);
    }

    private string GetTypeTags(WeaponSkillData weapon) {
        return string.Join("^", weapon.Tags.ToArray().Where(t => typeTags.Contains(t.DisplayName)).Select(t => t.DisplayName));
    }

    private string GetBalancedOverclocks() {
        return string.Join("^", Weapon.BalancedOverclocks.Select(o => o.Title));
    }

    private string GetUnstableOverclocks() {
        return string.Join("^", Weapon.UnstableOverclocks.Select(o => o.Title));
    }
}
