using CG.Client.UserData;
using CG.Ship.Modules;
using CG.Ship.Modules.Shield;
using Gameplay.CompositeWeapons;
using Gameplay.Damage;
using Gameplay.Loot;
using ResourceAssets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VC.Common.CoreData;
using VoidManager.Utilities;
using static DebugTools.Common;

namespace DebugTools
{
    internal static class WikiTools
    {
        internal static bool LogGuns = false;

        public static void AllReadouts(bool showDebugObjects = false)
        {
            DamageTablesReadout();
            WeaponModulesReadout(showDebugObjects);
            CraftablesReadout(showDebugObjects);
            UnlockablesReadout(showDebugObjects);
            CarryablesReadout(showDebugObjects);
            QuestAssetReadout();
            ClonestarObjectReadout();
            ScriptableObjectReadout();
            EndlessQuestDropTablesReadout();
            SurvivorQuestDropTablesReadout();
            ShieldsReadout();
            KPDsReadout();
            MissilesReadout();
            ModulesReadout();
        }

        public static void DamageTablesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Damage Tables Readout");
            List<string> lines = new();
            foreach (var thing in DamageTypesTable.Instance.DamageTypes)
            {
                string values = string.Empty;
                foreach (var thingy in thing.DamageMultipliers)
                {
                    values += $"\n{thingy.Key.Name} - {thingy.Value}";
                }
                lines.Add($"{thing.name}:{values}");
            }
            WriteReadoutFile("DamageTables.csv", lines.ToArray());
        }

        public static void WeaponModulesReadout(bool showDebugObjects = false)
        {
            BepinPlugin.Log.LogInfo("Starting CompositeWeaponDataDef Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Shop Category,Accuracy,Damage,Firerate,Range,Projectile Speed,Damage Type,Spread range,Spread Decrease Speed,Spread Increase Factor,Spread Decrease Factor");
            foreach (CompositeWeaponDataDef compositeWeaponDataDef in CompositeWeaponDataContainer.Instance.RuntimeDescriptions)
            {
                if (!showDebugObjects && IsItemLocked(compositeWeaponDataDef.AssetGuid)) { continue; }

                CompositeWeapon weapon = compositeWeaponDataDef.Asset.Data;
                WeaponFeederBase feeder = weapon.FeederElement.Asset;
                BarrelStatDescriptor barrel = (BarrelStatDescriptor)weapon.BarrelStats.Asset;
                ProjectileStatDescriptor projectileStats = (ProjectileStatDescriptor)weapon.ProjectileStats.Asset;
                FeederStatDescriptor feederStats = (FeederStatDescriptor)weapon.FeederStats.Asset;
                ForwardWeaponProjectile projectileElement = (ForwardWeaponProjectile)weapon.ProjectileElement.Asset;

                try
                {
                    lines.Add($"{compositeWeaponDataDef.AssetGuid},{compositeWeaponDataDef.Ref.Filename},{compositeWeaponDataDef.Asset.DisplayName},{compositeWeaponDataDef.Category},{barrel.Accuracy.MinValue}-{barrel.Accuracy.MaxValue},{projectileStats.Damage.MinValue}-{projectileStats.Damage.MaxValue},{feederStats.FireRate.MinValue}-{feederStats.FireRate.MaxValue},{projectileStats.Range.BaseValue},{projectileStats.ProjectileSpeed.BaseValue},{projectileElement.DamageType.name},{feeder.SpreadBase}-{feeder.SpreadMax},{feeder.SpreadDecreaseSpeed},{feeder.SpreadIncreasePerSecond},{feeder.spreadIncreaseFactor},{feeder.spreadDecreaseFactor}");
                }
                catch(Exception ex)
                {
                    BepinPlugin.Log.LogWarning("Caught error while processing Weapon Modules readout\n" + ex.Message);
                }
            }
            WriteReadoutFile("WeaponModules.csv", lines.ToArray());
        }

        public static void CraftablesReadout(bool showDebugObjects = false)
        {
            BepinPlugin.Log.LogInfo("Starting CraftableItemDef Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Crafting Method,Craft Recipe,Recyclable");
            foreach (CraftableItemDef craftableItemDef in CraftingDataContainer.Instance.RuntimeDescriptions)
            {
                if (!showDebugObjects && IsItemLocked(craftableItemDef.AssetGuid)) { continue; }
                lines.Add($"{craftableItemDef.AssetGuid},{craftableItemDef.Ref.Filename},{craftableItemDef.ContextInfo.HeaderText},{craftableItemDef.crafting.CraftingMethod},{(craftableItemDef.crafting.CanBePrinted ? $"{craftableItemDef.crafting.Creation.Resource}-{craftableItemDef.crafting.Creation.Amount}" : craftableItemDef.crafting.CanBeInscribed ? $"MK{craftableItemDef.crafting.ItemExchangeInfo.MinimumRequiredTier} BlankBox" : "No Recipe")},{craftableItemDef.crafting.CanBeRecycled}:{craftableItemDef.crafting.Recycle.Resource}-{craftableItemDef.crafting.Recycle.Amount}");
            }
            WriteReadoutFile("Craftables.csv", lines.ToArray());
        }

        public static void UnlockablesReadout(bool showDebugObjects = false)
        {
            BepinPlugin.Log.LogInfo("Starting UnlockItemDef Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Name,Unlock Criteria,Rank Requirement,Rarity");
            foreach (UnlockItemDef unlockItemDef in UnlockContainer.Instance.RuntimeDescriptions)
            {
                if (!showDebugObjects && unlockItemDef.UnlockOptions.UnlockCriteria == UnlockCriteriaType.Never) { continue; }
                lines.Add($"{unlockItemDef.AssetGuid},{unlockItemDef.Ref.Filename},{unlockItemDef.Asset.name},{unlockItemDef.UnlockOptions.UnlockCriteria},{unlockItemDef.UnlockOptions.RankRequirement},{unlockItemDef.rarity}");
            }
            WriteReadoutFile("Unlockables.csv", lines.ToArray());
        }

        public static void CarryablesReadout(bool showDebugObjects = false)
        {
            BepinPlugin.Log.LogInfo("Starting CarryableDef Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Shop Category");
            foreach (CarryableDef carryableDef in CarryableContainer.Instance.RuntimeDescriptions)
            {
                if (!showDebugObjects && IsItemLocked(carryableDef.AssetGuid)) { continue; }
                lines.Add($"{carryableDef.AssetGuid},{carryableDef.Ref.Filename},{carryableDef.ContextInfo.HeaderText},{carryableDef.Category}");
            }
            WriteReadoutFile("Carryables.csv", lines.ToArray());
        }

        public static void QuestAssetReadout()
        {
            BepinPlugin.Log.LogInfo("Starting QuestAssetDef Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Quest Type");
            foreach (QuestAssetDef questAssetDef in QuestAssetContainer.Instance.RuntimeDescriptions)
            {
                lines.Add($"{questAssetDef.AssetGuid},{questAssetDef.Ref.Filename},{questAssetDef.ContextInfo.HeaderText},{questAssetDef.Asset.QuestType}");
            }
            WriteReadoutFile("Quests.csv", lines.ToArray());
        }

        public static void ClonestarObjectReadout()
        {
            BepinPlugin.Log.LogInfo("Starting CloneStarObjectDef Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name");
            foreach (CloneStarObjectDef cloneStarObjectDef in CloneStarObjectContainer.Instance.RuntimeDescriptions)
            {
                lines.Add($"{cloneStarObjectDef.AssetGuid},{cloneStarObjectDef.Ref.Filename},{cloneStarObjectDef.ContextInfo.HeaderText}");
            }
            WriteReadoutFile("Clonestar Objects.csv", lines.ToArray());
        }

        public static void ScriptableObjectReadout()
        {
            BepinPlugin.Log.LogInfo("Starting ScriptableObjectDef Readout");
            List<string> lines = new();
            lines.Add("GUID,Path,Type");
            foreach (ScriptableObjectDef thing in ScriptableObjectContainer.Instance.RuntimeDescriptions)
            {
                lines.Add($"{thing.AssetGuid},{thing.Path},{thing.GetType()}");
            }
            WriteReadoutFile("ScriptableObjects.csv", lines.ToArray());
        }

        public static void ShieldsReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Shields Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Absorption,Recharge Delay,Recharge Speed,Hit Points");
            foreach (ModuleDef moduleDef in ModuleContainer.Instance.RuntimeDescriptions)
            {
                if (moduleDef.Asset is ShieldModule shieldModule)
                    lines.Add($"{moduleDef.AssetGuid},{moduleDef.Ref.Filename},{shieldModule.DisplayName},{shieldModule.shieldConfig.absorption},{shieldModule.shieldConfig.rechargeDelay},{shieldModule.shieldConfig.rechargeSpeed},{shieldModule.shieldConfig.hitPoints}");
            }
            WriteReadoutFile("ShieldGenerators.csv", lines.ToArray());
        }

        public static void KPDsReadout()
        {
            BepinPlugin.Log.LogInfo("Starting KPD Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Tracking Range,Burst Cooldown,Magazine Consumption Efficiency,Ammo Per Burst,Burst Duration,Target Loss Cooldown,Turret Tracking Angular Speed,Tracking Time Before Burst,Feeder Travel Duration");
            foreach (ModuleDef moduleDef in ModuleContainer.Instance.RuntimeDescriptions)
            {
                if (moduleDef.Asset is KineticPointDefenseModule KPDModule)
                    lines.Add($"{moduleDef.AssetGuid},{moduleDef.Ref.Filename},{KPDModule.DisplayName},{KPDModule.TrackingRange.Value},{KPDModule.CooldownAfterBurst.Value},{KPDModule.MagazineConsumptionEfficiency.Value},{KPDModule.AmmoUsedPerBurst},{KPDModule.BurstDuration},{KPDModule.CooldownAfterLossOfTarget},{KPDModule.TurretTrackingAngularSpeed},{KPDModule.TrackingTimeBeforeBurst},{KPDModule.FeederTravelDuration}");
            }
            WriteReadoutFile("KineticPointDefenceModules.csv", lines.ToArray());
        }

        public static void MissilesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Missile Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Projectile Count,Missile Range,Damage Type,Damage,Turning Arc Distance,Speed,Shoot Delay,Seek Delay,Angular Multiplier,Inner Radius, Outer Radius");
            foreach (CarryableDef moduleDef in CarryableContainer.Instance.RuntimeDescriptions)
            {
                if (moduleDef.Asset is not Payload payload || !payload.TryGetComponent<PayloadMissileGameplayEffect>(out var PMGE)) return;

                Missile missile = PMGE.ReplacementObjects[0].Asset as Missile;

                if (missile is not AOEMissile aoeMissile)
                {
                    lines.Add($"{moduleDef.AssetGuid},{moduleDef.Ref.Filename},{PMGE.Payload.DisplayName},{PMGE.ReplacementObjects.Count},{missile.Range.Value},{missile.DamageType.name},{missile.Damage.Value},{missile.MovementArcLength},{missile.Speed.Value},{missile.ShootDelaySeconds},{missile.SeekDelaySeconds},{missile.angularMultiplier}");
                }
                else
                {
                    lines.Add($"{moduleDef.AssetGuid},{moduleDef.Ref.Filename},{PMGE.Payload.DisplayName},{PMGE.ReplacementObjects.Count},{missile.Range.Value},{missile.DamageType.name},{missile.Damage.Value},{missile.MovementArcLength},{missile.Speed.Value},{missile.ShootDelaySeconds},{missile.SeekDelaySeconds},{missile.angularMultiplier},{aoeMissile.InnerExplosionRadius},{aoeMissile.OuterExplosionRadius}");
                }
            }
            WriteReadoutFile("Missiles.csv", lines.ToArray());
        }

        public static void ModulesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Module Readout");
            List<string> lines = new();
            lines.Add("GUID,File Name,Display Name,Category");
            foreach (ModuleDef moduleDef in ModuleContainer.Instance.RuntimeDescriptions)
            {
                lines.Add($"{moduleDef.AssetGuid},{moduleDef.Ref.Filename},{moduleDef.ContextInfo.HeaderText},{moduleDef.Category}");
            }
            WriteReadoutFile("Modules.csv", lines.ToArray());
        }

        public static void EndlessQuestDropTablesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Endless Drop Tables Readout");

            DropTableReadout("EndlessPilgrimage.csv", Game.EndlessQuestAsset.LootTable);
        }

        private static GUIDUnion SurvivorQuestGUID = new GUIDUnion("c3dcaf364807cae40b836a6ef6ebe748");

        public static void SurvivorQuestDropTablesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Endless Drop Tables Readout");

            DropTableReadout("SurvivorChallenge.csv", Game.GetQuestAsset(SurvivorQuestGUID).LootTable);
        }

        /*private class DTEComparer : IComparer<LootTableEntry>
        {
            public int Compare(LootTableEntry x, LootTableEntry y)
            {
                return x.LootRarity - y.LootRarity;
            }
        }*/

        public static void DropTableReadout(string OutputName, LootTable lootTable)
        {
            List<string> lines = new();

            if (lootTable.useSectorDropAmountLimiters)
            {
                lines.Add("Drop Chances Per Sector");
                lines.Add("Rarity, Base Drop Limit, Drop Limit Positive Jitter");
                foreach (var DropChances in lootTable.MaxDropsPerSectorPerRarity)
                {
                    lines.Add($"{DropChances.Key}, {DropChances.Value.MaxDropAmountBase}, {DropChances.Value.MaxDropAmountPositiveJitter}");
                }
            }
            else
            {
                lines.Add("Not using drop amount limits");
            }

            // CSV Table
            lines.Add("Table Entries");
            List<string> CSV = new();
            CSV.Add("GUID,FileName,DisplayName,SpawnLocations,SpawnLimiters,Rarity,Amount");

            List<LootTableEntry> sorted = lootTable.Loot.ToList();
            sorted.Sort((x, y) => x.LootRarity - y.LootRarity);

            foreach (LootTableEntry LTEntry in sorted)
            {
                CSV.Add($"{LTEntry.ItemRef.AssetGuid},{LTEntry.ItemRef.Filename},{CraftingDataContainer.Instance.GetAssetDefById(LTEntry.ItemRef.AssetGuid).ContextInfo.HeaderText},{ListEnums(LTEntry.PossibleSpawnLocationTypes)},{LTEntry.LootSpawnLimiters.Length},{LTEntry.LootRarity},{LTEntry.Amount}");
            }

            // Dev Drop Chances Calc
            lines.Add(Game.GetQuestAsset(SurvivorQuestGUID).LootTable.DropChancesText());

            WriteReadoutFile(OutputName + "LootData.txt", lines.ToArray());
            WriteReadoutFile(OutputName + "LootTable.csv", CSV.ToArray());
        }

        public static string ListEnums(LootSpawnLocationType LootLocations)
        {
            string setFlags = string.Empty;
            foreach (LootSpawnLocationType flag in Enum.GetValues(typeof(LootSpawnLocationType)))
            {
                if ((LootLocations & flag) == flag)
                {
                    setFlags += $" {flag}";
                }
            }
            return setFlags;
        }

        // Getter since static string defaulted to Void Crew parent folder.
        public static string WikiReadoutDirectory = Environment.CurrentDirectory + "\\WikiReadout";

        /// <summary>
        /// Writes a file to the readout directory.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lines"></param>
        public static void WriteReadoutFile(string fileName, string[] lines)
        {
            Directory.CreateDirectory(WikiReadoutDirectory);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(WikiReadoutDirectory, fileName)))
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
        }
    }

    /*
    [HarmonyPatch(typeof(CompositeWeaponModule), "OnPhotonInstantiate")]
    internal class WeaponStatsPatch
    {
        static void Postfix(CompositeWeaponModule __instance)
        {
            if (WikiTools.LogGuns)
            {
                WeaponFeederBase feeder = __instance.TopElementsCollection.Feeder;
                BepinPlugin.Log.LogInfo($"{__instance.DisplayName}: Accuracy: {__instance.Accuracy.MinValue}-{__instance.Accuracy.MaxValue}, Damage: {__instance.Damage.MinValue}-{__instance.Damage.MaxValue}, FireRate: {__instance.FireRate.MinValue}-{__instance.FireRate.MaxValue}, Range: {__instance.Range.BaseValue}, Projectile Speed: {__instance.ProjectileSpeed.BaseValue}, Damage Type: {feeder.Projectile.Asset.DamageType.name}, SpreadRange: {feeder.SpreadBase}-{feeder.SpreadMax}; DecreaseSpeed - {feeder.SpreadDecreaseSpeed}; IncreaseSpeed - {feeder.SpreadIncreasePerSecond}; IncreaseFactor - {feeder.spreadIncreaseFactor}; DecreaseFactor - {feeder.spreadDecreaseFactor}");
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ShieldModule), "ToggleShield")]
    class ShieldStatsPatch
    {
        static void Postfix(ShieldModule __instance)
        {
            if (WikiTools.LogGuns)
            {
                BepinPlugin.Log.LogInfo($"{__instance.DisplayName}: Absorption: {__instance.shieldConfig.absorption}, Recharge Delay: {__instance.shieldConfig.rechargeDelay}, Recharge Speed: {__instance.shieldConfig.rechargeSpeed}, Hit Points: {__instance.shieldConfig.hitPoints}");
            }
        }
    }*/

    /*[HarmonyPatch(typeof(KineticPointDefenseModule), "EnterStateOn")]
    class KPDStatsPatch
    {
        static void Postfix(KineticPointDefenseModule __instance)
        {
            if (WikiTools.LogGuns)
            {
                BepinPlugin.Log.LogInfo($"{__instance.DisplayName}: TrackingRange: {__instance.TrackingRange.Value}, CooldownAfterBurst: {__instance.CooldownAfterBurst.Value}, MagazineConsumptionEfficiency: {__instance.MagazineConsumptionEfficiency.Value}, AmmoUsedPerBurst: {__instance.AmmoUsedPerBurst}, BurstDuration: {__instance.BurstDuration}, CooldownAfterLossOfTarget: {__instance.CooldownAfterLossOfTarget}, TurretTrackingAngularSpeed: {__instance.TurretTrackingAngularSpeed}, TrackingTimeBeforeBurst: {__instance.TrackingTimeBeforeBurst}, FeederTravelDuration: {__instance.FeederTravelDuration}");
            }
        }
    }*/

    /*
    [HarmonyPatch(typeof(PayloadMissileGameplayEffect), "OnAwake")]
    class MissilePatch
    {
        static void Postfix(PayloadMissileGameplayEffect __instance)
        {
            if (WikiTools.LogGuns)
            {
                Missile missile = __instance.ReplacementObjects[0].Asset as Missile;
                AOEMissile aoeMissile = missile as AOEMissile;

                int projectileCount = __instance.ReplacementObjects.Count;

                if (aoeMissile == null)
                {
                    BepinPlugin.Log.LogInfo($"{__instance.Payload.DisplayName}: {(projectileCount > 1 ? $"ProjectileCount: {projectileCount}," : string.Empty)} Range: {missile.Range.Value}, DamageType: {missile.DamageType.name}, Damage: {missile.Damage.Value}, TurningArcDistance: {missile.MovementArcLength}, Speed: {missile.Speed.Value}, ShootDelay: {missile.ShootDelaySeconds} SeekDelay: {missile.SeekDelaySeconds}, AngularMultiplier: {missile.angularMultiplier}");
                }
                else
                {
                    BepinPlugin.Log.LogInfo($"{__instance.Payload.DisplayName}: Range: {missile.Range.Value}, DamageType: {missile.DamageType.name}, Damage: {missile.Damage.Value}, InnerRadius: {aoeMissile.InnerExplosionRadius}, OuterRadius: {aoeMissile.OuterExplosionRadius}, TurningArcDistance: {missile.MovementArcLength}, Speed: {missile.Speed.Value}, ShootDelay: {missile.ShootDelaySeconds} SeekDelay: {missile.SeekDelaySeconds}, AngularMultiplier: {missile.angularMultiplier}");
                }
            }
        }
    }*/
}
