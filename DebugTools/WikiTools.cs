using CG;
using CG.Client.UserData;
using CG.Objects;
using CG.Ship.Modules;
using CG.Ship.Modules.Shield;
using Gameplay.CompositeWeapons;
using Gameplay.Damage;
using Gameplay.Loot;
using Gameplay.Quests;
using HarmonyLib;
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
        }

        public static void DamageTablesReadout()
        {
            List<string> lines = new();

            //Damage Tables
            foreach (var thing in DamageTypesTable.Instance.DamageTypes)
            {
                string values = string.Empty;
                foreach (var thingy in thing.DamageMultipliers)
                {
                    values += $"\n{thingy.Key.Name} - {thingy.Value}";
                }
                lines.Add($"{thing.name}:{values}");
            }
            WriteReadoutFile("DamageTables", lines.ToArray());
        }

        public static void WeaponModulesReadout(bool showDebugObjects = false)
        {
            BepinPlugin.Log.LogInfo("Starting CompositeWeaponDataDef Readout");
            List<string> lines = new();
            foreach (CompositeWeaponDataDef compositeWeaponDataDef in ResourceAssetContainer<CompositeWeaponDataContainer, CompositeWeaponData, CompositeWeaponDataDef>.Instance.AssetDescriptions)
            {
                CompositeWeapon weapon = compositeWeaponDataDef.Asset.Data;
                CompositeWeaponModule weaponModule = weapon.CoreElement.Asset;
                WeaponFeederBase feeder = weapon.FeederElement.Asset;


                if (!showDebugObjects && IsItemLocked(compositeWeaponDataDef.AssetGuid)) { continue; }
                lines.Add($"{compositeWeaponDataDef.AssetGuid} {compositeWeaponDataDef.ContextInfo.HeaderText} {compositeWeaponDataDef.Ref.Filename} {compositeWeaponDataDef.Category}");
                lines.Add($"{weaponModule.DisplayName}: Accuracy: {weaponModule.Accuracy.MinValue}-{weaponModule.Accuracy.MaxValue}, Damage: {weaponModule.Damage.MinValue}-{weaponModule.Damage.MaxValue}, FireRate: {weaponModule.FireRate.MinValue}-{weaponModule.FireRate.MaxValue}, Range: {weaponModule.Range.BaseValue}, Projectile Speed: {weaponModule.ProjectileSpeed.BaseValue}, Damage Type: {feeder.Projectile.Asset.DamageType.name}, SpreadRange: {feeder.SpreadBase}-{feeder.SpreadMax}; DecreaseSpeed - {feeder.SpreadDecreaseSpeed}; IncreaseSpeed - {feeder.SpreadIncreasePerSecond}; IncreaseFactor - {feeder.spreadIncreaseFactor}; DecreaseFactor - {feeder.spreadDecreaseFactor}");
            }
            WriteReadoutFile("WeaponModules", lines.ToArray());
        }

        public static void CraftablesReadout(bool showDebugObjects = false)
        {
            //Craftables
            BepinPlugin.Log.LogInfo("Starting CraftableItemDef Readout");
            List<string> lines = new();
            foreach (CraftableItemDef craftableItemDef in ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.AssetDescriptions)
            {
                if (!showDebugObjects && IsItemLocked(craftableItemDef.AssetGuid)) { continue; }
                lines.Add($"{craftableItemDef.AssetGuid} {craftableItemDef.ContextInfo.HeaderText} {craftableItemDef.Ref.Filename} {craftableItemDef.Category} Crafting: {craftableItemDef.crafting.CraftingMethod} {(craftableItemDef.crafting.CanBePrinted ? $"{craftableItemDef.crafting.Creation.Resource}-{craftableItemDef.crafting.Creation.Amount}" : craftableItemDef.crafting.CanBeInscribed ? $"MK{craftableItemDef.crafting.ItemExchangeInfo.MinimumRequiredTier} BlankBox" : string.Empty)}, Recyclable: {craftableItemDef.crafting.CanBeRecycled}:{craftableItemDef.crafting.Recycle.Resource}-{craftableItemDef.crafting.Recycle.Amount}");
            }
            WriteReadoutFile("Craftables", lines.ToArray());
        }

        public static void UnlockablesReadout(bool showDebugObjects = false)
        {
            //Unlockables
            BepinPlugin.Log.LogInfo("Starting UnlockItemDef Readout");
            List<string> lines = new();
            foreach (UnlockItemDef unlockItemDef in ResourceAssetContainer<UnlockContainer, UnityEngine.Object, UnlockItemDef>.Instance.AssetDescriptions)
            {
                if (!showDebugObjects && unlockItemDef.UnlockOptions.UnlockCriteria == UnlockCriteriaType.Never) { continue; }
                lines.Add(unlockItemDef.AssetGuid + " " + unlockItemDef.Asset.name + " " + unlockItemDef.Ref.Filename + " " + unlockItemDef.UnlockOptions.UnlockCriteria.ToString() + " RankReq: " + unlockItemDef.UnlockOptions.RankRequirement.ToString() + " " + unlockItemDef.rarity.ToString());
            }
            WriteReadoutFile("Unlockables", lines.ToArray());
        }

        public static void CarryablesReadout(bool showDebugObjects = false)
        {
            //Carryables
            BepinPlugin.Log.LogInfo("Starting CarryableDef Readout");
            List<string> lines = new();
            foreach (CarryableDef carryableDef in ResourceAssetContainer<CarryableContainer, CarryableObject, CarryableDef>.Instance.AssetDescriptions)
            {
                if (!showDebugObjects && IsItemLocked(carryableDef.AssetGuid)) { continue; }
                lines.Add(carryableDef.AssetGuid + " " + carryableDef.ContextInfo.HeaderText + " " + carryableDef.Ref.Filename + " " + carryableDef.Category.ToString());
            }
            WriteReadoutFile("Carryables", lines.ToArray());
        }

        public static void QuestAssetReadout()
        {
            //QuestAssets
            BepinPlugin.Log.LogInfo("Starting QuestAssetDef Readout");
            List<string> lines = new();
            foreach (QuestAssetDef questAssetDef in ResourceAssetContainer<QuestAssetContainer, QuestAsset, QuestAssetDef>.Instance.AssetDescriptions)
            {
                lines.Add(questAssetDef.AssetGuid + " " + questAssetDef.ContextInfo.HeaderText + " " + questAssetDef.Ref.Filename + " " + questAssetDef.Asset.QuestType);
            }
            WriteReadoutFile("Quests", lines.ToArray());
        }

        public static void ClonestarObjectReadout()
        {
            //CloneStar Objects.
            BepinPlugin.Log.LogInfo("Starting CloneStarObjectDef Readout");
            List<string> lines = new();
            foreach (CloneStarObjectDef cloneStarObjectDef in ResourceAssetContainer<CloneStarObjectContainer, AbstractCloneStarObject, CloneStarObjectDef>.Instance.AssetDescriptions)
            {
                lines.Add(cloneStarObjectDef.AssetGuid + " " + cloneStarObjectDef.ContextInfo.HeaderText + " " + cloneStarObjectDef.Ref.Filename);
            }
            WriteReadoutFile("Clonestar Objects", lines.ToArray());
        }

        public static void ScriptableObjectReadout()
        {
            BepinPlugin.Log.LogInfo("Starting ScriptableObjectDef Readout");
            List<string> lines = new();
            foreach (ScriptableObjectDef thing in ScriptableObjectContainer.Instance.AssetDescriptions)
            {
                lines.Add($"{thing.AssetGuid} {thing.GetType()} {thing.Path}");
            }
            WriteReadoutFile("ScriptableObjects", lines.ToArray());
        }

        public static void EndlessQuestDropTablesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Endless Drop Tables Readout");

            DropTableReadout("EndlessPilgrimageLootTable", Game.EndlessQuestAsset.LootTable);
        }

        private static GUIDUnion SurvivorQuestGUID = new GUIDUnion("c3dcaf364807cae40b836a6ef6ebe748");

        public static void SurvivorQuestDropTablesReadout()
        {
            BepinPlugin.Log.LogInfo("Starting Endless Drop Tables Readout");

            DropTableReadout("SurvivorChallengLootTable", Game.GetQuestAsset(SurvivorQuestGUID).LootTable);
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

            lines.Add("Drop Chances Per Sector");
            lines.Add("Rarity, Base Drop Limit, Drop Limit Positive Jitter");
            foreach (var DropChances in lootTable.MaxDropsPerSectorPerRarity)
            {
                lines.Add($"{DropChances.Key}, {DropChances.Value.MaxDropAmountBase}, {DropChances.Value.MaxDropAmountPositiveJitter}");
            }

            lines.Add(string.Empty);
            lines.Add("Table Entries");
            List<LootTableEntry> sorted = lootTable.Loot.ToList();
            sorted.Sort((x, y) => x.LootRarity - y.LootRarity);
            foreach (LootTableEntry LTEntry in sorted)
            {
                lines.Add($"{LTEntry.ItemRef.AssetGuid} {LTEntry.ItemRef.Filename} {CraftingDataContainer.Instance.GetAssetDefById(LTEntry.ItemRef.AssetGuid).ContextInfo.HeaderText} SpawnLocations:{ListEnums(LTEntry.PossibleSpawnLocationTypes)} SpawnLimiters: {LTEntry.LootSpawnLimiters.Length} Rarity: {LTEntry.LootRarity} Amount: {LTEntry.Amount}");
            }

            lines.Add(string.Empty);
            lines.Add(Game.GetQuestAsset(SurvivorQuestGUID).LootTable.DropChancesText());

            WriteReadoutFile(OutputName, lines.ToArray());
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
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(WikiReadoutDirectory, $"{fileName}.txt")))
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

    [HarmonyPatch(typeof(ShieldModule), "ToggleShield")]
    class ShieldStatsPatch
    {
        static void Postfix(ShieldModule __instance)
        {
            if (WikiTools.LogGuns)
            {
                BepinPlugin.Log.LogInfo($"{__instance.DisplayName}: Absorption: {__instance.shieldConfig.absorption}, Recharge Delay: {__instance.shieldConfig.rechargeDelay}, Recharge Speed: {__instance.shieldConfig.rechargeSpeed}, Hit Points: {__instance.shieldConfig.hitPoints}");
            }
        }
    }

    [HarmonyPatch(typeof(KineticPointDefenseModule), "EnterStateOn")]
    class KPDStatsPatch
    {
        static void Postfix(KineticPointDefenseModule __instance)
        {
            if (WikiTools.LogGuns)
            {
                BepinPlugin.Log.LogInfo($"{__instance.DisplayName}: TrackingRange: {__instance.TrackingRange.Value}, CooldownAfterBurst: {__instance.CooldownAfterBurst.Value}, MagazineConsumptionEfficiency: {__instance.MagazineConsumptionEfficiency.Value}, AmmoUsedPerBurst: {__instance.AmmoUsedPerBurst}, BurstDuration: {__instance.BurstDuration}, CooldownAfterLossOfTarget: {__instance.CooldownAfterLossOfTarget}, TurretTrackingAngularSpeed: {__instance.TurretTrackingAngularSpeed}, TrackingTimeBeforeBurst: {__instance.TrackingTimeBeforeBurst}, FeederTravelDuration: {__instance.FeederTravelDuration}");
            }
        }
    }

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
    }
}
