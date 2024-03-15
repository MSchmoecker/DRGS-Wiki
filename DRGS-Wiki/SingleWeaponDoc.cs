using System.Text.RegularExpressions;
using Assets.Scripts.Milestones;
using Assets.Scripts.SkillSystem;

namespace DRGS_Wiki;

public class SingleWeaponDoc : Doc {
    public WeaponSkillData Weapon { get; }

    public SingleWeaponDoc(WeaponSkillData weapon) : base($"weapons/{weapon.Title.Replace("\"", "")}_{weapon.name}") {
        Plugin.Instance.Log.LogInfo($"Documenting weapon {weapon.name}");
        Weapon = weapon;

        ProjectileWeaponSkillData projectileWeapon = weapon as ProjectileWeaponSkillData;
        BeamWeaponSkillData beamWeapon = weapon as BeamWeaponSkillData;
        DefenseDroneWeaponSkillData defenseDroneWeapon = weapon as DefenseDroneWeaponSkillData;
        SpawnWeaponSkillData spawnWeapon = weapon as SpawnWeaponSkillData;
        GroundzoneGrenadeWeaponData grondZoneGrenadeWeapon = weapon as GroundzoneGrenadeWeaponData;
        GrenadeWeaponSkillData grenadeWeapon = weapon as GrenadeWeaponSkillData;
        CoilGunWeaponSkillData coilGunWeapon = weapon as CoilGunWeaponSkillData;

        AddText("{{SurvivorWeaponsStats");
        AddText($" | name = {weapon.Title} <!--String (mandatory)-->");
        AddText($" | image = Survivor {ImageName()}.png <!--Text-->");
        AddText($" | unlockedBy = {UnlockedBy(Weapon)} <!--Text-->".Replace("  ", " "));
        AddText($" | alwaysForClass = {AlwaysForClass()} <!--String (allowed values = Scout, Engineer, Gunner, Driller)-->".Replace("  ", " "));

        AddText($"<!--Tags-->");
        AddText($" | damageTag = {GetDamageTags(Weapon)} <!--List (^) of String (allowed values = {GetDamageTags()})-->");
        AddText($" | familyTag = {GetFamilyTags(Weapon)} <!--List (^) of String (allowed values = {GetFamilyTags()})-->");
        AddText($" | firingTag = {GetFiringTags(Weapon)} <!--List (^) of String (allowed values = {GetFiringTags()})-->");
        AddText($" | typeTag = {GetTypeTags(Weapon)} <!--List (^) of String (allowed values = {GetTypeTags()})-->".Replace("  ", " "));

        AddText($"<!--Base Stats-->");
        AddText($" | damage = {ConvertToString((int)Weapon.BaseDamage)} <!--Integer-->");

        if (projectileWeapon) {
            AddText($" | roF = {ConvertToString(WeaponDoc.GetFireRate(projectileWeapon))} <!--Float-->");
            AddText($" | clipSize = {ConvertToString(projectileWeapon.BaseClipSize)} <!--Integer-->");
        } else if (grenadeWeapon) {
            AddText($" | roF = {ConvertToString(WeaponDoc.GetFireRate(grenadeWeapon))} <!--Float-->");
            AddText($" | clipSize = 1 <!--Integer-->");
        } else {
            AddText($" | roF = <!--Float-->");
            AddText($" | clipSize = <!--Integer-->");
        }

        AddText($" | reloadTime = {ConvertToString(Weapon.ReloadTime)} <!--Float-->");
        AddText($" | weaponRange = {ConvertToString(Weapon.BaseRange)} <!--Float-->");
        AddText($" | hasKnockback = {ConvertToString(Weapon.KnockBack)} <!--String (allowed values = Yes, No)-->");
        AddText($" | minesTerrain = <!--String (allowed values = Yes, No)-->");

        AddText($"<!--Overclocks-->");
        AddText($" | balancedOC = {GetBalancedOverclocks()} <!--List (^) of String-->");
        AddText($" | unstableOC = {GetUnstableOverclocks()} <!--List (^) of String-->");

        AddText("<!--Projectile only-->");
        if (projectileWeapon) {
            AddText($" | nbrOfProjectiles = {ConvertToString(WeaponDoc.GetNrOfProjectiles(projectileWeapon))} <!--Integer-->");
            AddText($" | firingPattern = {FirePattern(projectileWeapon)} <!--String-->");
            AddText($" | burstSize = {ConvertToString(projectileWeapon.FireMode == WeaponSkillData.EFireMode.BURST ? projectileWeapon.BurstShots : 0)} <!--Integer-->");
            AddText($" | pierceNumber = {projectileWeapon.ProjectileSettings.HitsAllowed} <!--Integer-->");
        } else {
            AddText($" | nbrOfProjectiles = <!--Integer-->");
            AddText($" | firingPattern = <!--String-->");
            AddText($" | burstSize = <!--Integer-->");
            AddText($" | pierceNumber = <!--Integer-->");
        }

        AddText("<!--Grenade only-->");
        if (grenadeWeapon) {
            AddText($" | triggerType = {ConvertToString(grenadeWeapon.TriggerType)} <!--String-->");
            AddText($" | fuseTime = {ConvertToString(grenadeWeapon.BaseFuseTime)} <!--Float-->");
        } else {
            AddText($" | triggerType = <!--String-->");
            AddText($" | fuseTime = <!--Float-->");
        }

        AddText("<!--Status effect only-->");
        if (grondZoneGrenadeWeapon != null) {
            AddText($" | nbrOfStacks = {ConvertToString(grondZoneGrenadeWeapon.StatusEffectStacks)} <!--Integer-->");
            AddText($" | damagePerStack = {ConvertToString((int)(grondZoneGrenadeWeapon.BaseDamage * grondZoneGrenadeWeapon.GroundZoneParams.DamageModifier))} <!--Integer-->");
            AddText($" | leavesPoolsOnGround = {ConvertToString((bool)grondZoneGrenadeWeapon.GroundZonePrefab)} <!--String (allowed values = Yes, No)-->");
            AddText($" | puddleDuration = {ConvertToString(grondZoneGrenadeWeapon.GroundZoneParams.LifeTime)} <!--Float-->");
        } else if (beamWeapon != null) {
            AddText($" | nbrOfStacks = {ConvertToString(beamWeapon.StatusEffectStacks)} <!--Integer-->");
            AddText($" | damagePerStack = {ConvertToString((int)(beamWeapon.BaseDamage * beamWeapon.GroundzoneParams.DamageModifier))} <!--Integer-->");
            AddText($" | leavesPoolsOnGround = {ConvertToString(beamWeapon.DropGroundzones)} <!--String (allowed values = Yes, No)-->");
            AddText($" | puddleDuration = {ConvertToString(beamWeapon.GroundzoneParams.LifeTime)} <!--Float-->");
        } else if (defenseDroneWeapon != null) {
            AddText($" | nbrOfStacks = {ConvertToString(defenseDroneWeapon.StatusEffectStacks)} <!--Integer-->");
            AddText($" | damagePerStack = {ConvertToString((int)(defenseDroneWeapon.BaseDamage * defenseDroneWeapon.GroundZoneParams.DamageModifier))} <!--Integer-->");
            AddText($" | leavesPoolsOnGround = {ConvertToString(defenseDroneWeapon.DropsGroundZones)} <!--String (allowed values = Yes, No)-->");
            AddText($" | puddleDuration = {ConvertToString(defenseDroneWeapon.GroundZoneParams.LifeTime)} <!--Float-->");
        } else {
            AddText($" | nbrOfStacks = <!--Integer-->");
            AddText($" | damagePerStack = <!--Integer-->");
            AddText($" | leavesPoolsOnGround = <!--String (allowed values = Yes, No)-->");
            AddText($" | puddleDuration = <!--Float-->");
        }

        AddText("<!--Lasting only-->");
        if (beamWeapon) {
            AddText($" | lifeTime = {ConvertToString(beamWeapon.BaseLifeTime)} <!--Float-->");
        } else if (defenseDroneWeapon) {
            AddText($" | lifeTime = {ConvertToString(defenseDroneWeapon.BaseLifeTime)} <!--Float-->");
        } else if (spawnWeapon) {
            AddText($" | lifeTime = {ConvertToString(spawnWeapon.SpawnLifeTime)} <!--Float-->");
        } else if (coilGunWeapon) {
            AddText($" | lifeTime = {ConvertToString(coilGunWeapon.BaseLifeTime)} <!--Float-->");
        } else {
            AddText($" | lifeTime = <!--Float-->");
        }

        AddText("<!--Turret only-->");
        if (spawnWeapon) {
            AddText($" | carryCapacity = {ConvertToString(spawnWeapon.MaxCharges)} <!--Integer-->");
        } else {
            AddText($" | carryCapacity = <!--Integer-->");
        }

        AddText("<!--Beam only-->");
        if (beamWeapon) {
            AddText($" | beamCount = {ConvertToString(beamWeapon.BeamCount)} <!--Integer-->");
            AddText($" | tickInterval = {ConvertToString(beamWeapon.TickInterval)} <!--Float-->");
        } else if (coilGunWeapon) {
            AddText($" | beamCount = 1 <!--Integer-->");
            AddText($" | tickInterval = {ConvertToString(coilGunWeapon.TickInterval)} <!--Float-->");
        } else {
            AddText($" | beamCount = <!--Integer-->");
            AddText($" | tickInterval = <!--Float-->");
        }

        AddText("<!--Drone only-->");
        if (defenseDroneWeapon) {
            AddText($" | droneCount = {ConvertToString(defenseDroneWeapon.DroneCount)} <!--Integer-->");
        } else {
            AddText($" | droneCount = <!--Integer-->");
        }

        AddText("}}");
    }

    private string ImageName() {
        string title = Weapon.Title;

        if (title.Contains("Thunderhead")) return "AutoCannon";
        if (title.Contains("BRT7")) return "BRT7";
        if (title.Contains("M1000")) return "M1000";
        if (title.Contains("LOK")) return "LOK1";
        if (title.Contains("Neurotoxin Grenade")) return "Acid Grenade";
        if (title.Contains("Arc-Tek")) return "Cryo Drone";
        if (title.Contains("ArmsKore")) return "Coilgun";
        if (title.Contains("Wave Cooker")) return "Wave Cooker";

        Regex quotes = new Regex("\"[^\"]*\"");
        if (quotes.IsMatch(title)) {
            title = quotes.Match(title).Value;
            title = title.Substring(1, title.Length - 2);
        }

        return title;
    }

    public static string UnlockedBy(WeaponSkillData weapon) {
        if (WeaponDoc.weaponMilestones.TryGetValue(weapon.name, out MilestoneData milestone)) {
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

    private string AlwaysForClass() {
        if (WeaponDoc.defaultUnlockedWeapons.TryGetValue(Weapon.name, out string className)) {
            return className;
        }

        return "";
    }

    private string FirePattern(ProjectileWeaponSkillData weapon) {
        if (weapon.name.Contains("Warthog")) {
            return "Shotgun";
        }

        return ConvertToString(weapon.FireMode);
    }

    private static List<string> damageTags = new List<string> {
        "Kinetic",
        "Fire",
        "Cold",
        "Electrical",
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
        return string.Join(", ", damageTags
            .Select(t => t.Replace("Electrical", "Electric")));
    }

    private string GetDamageTags(WeaponSkillData weapon) {
        return string.Join("^", weapon.Tags.ToArray()
            .Where(t => damageTags.Contains(t.DisplayName))
            .Select(t => t.DisplayName.Replace("Electrical", "Electric")));
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
        return string.Join("^", Weapon.BalancedOverclocks.Select(o => o.Title
            .Replace("A little more oomph!", "A Little More Oomph!")
            .Replace("Tape some nails to it", "Tape Some Nails to It")
            .Replace("Behaviour Chip : Aggro", "Behaviour Chip: Aggro")
            .Trim()
        ));
    }

    private string GetUnstableOverclocks() {
        return string.Join("^", Weapon.UnstableOverclocks.Select(o => o
            .Title.Replace("MK 11", "MK II")));
    }
}
