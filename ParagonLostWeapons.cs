using MelonLoader;
using BTD_Mod_Helper;
using ParagonLostWeapons;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;

using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib.Tools;
using Il2CppSystem.Collections.Generic;

using System;
using Assets.Scripts.Models.Effects;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Towers.TowerFilters;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Models.Towers.Weapons.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Bloons.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Models.Behaviors;
using UnhollowerBaseLib;
using Assets.Scripts.Models.Towers.Mutators;
using Assets.Scripts.Models.GeraldoItems;
using HarmonyLib;

[assembly: MelonInfo(typeof(ParagonLostWeapons.ParagonLostWeapons), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace ParagonLostWeapons
{
    public class ParagonLostWeapons : BloonsTD6Mod
    {
        public override void OnNewGameModel(GameModel gameModel, List<ModModel> mods)
        {


            //[Getting Paragons]
            var ApexPlasmaMaster = gameModel.GetTowersWithBaseId(TowerType.DartMonkey).First(dMonkey => dMonkey.isParagon == true);
            var GlaiveDominus = gameModel.GetTowersWithBaseId(TowerType.BoomerangMonkey).First(bMonkey => bMonkey.isParagon == true);
            var AscendedShadow = gameModel.GetTowersWithBaseId(TowerType.NinjaMonkey).First(nMonkey => nMonkey.isParagon == true);
            var NavarchOfTheSeas = gameModel.GetTowersWithBaseId(TowerType.MonkeyBuccaneer).First(bcMonkey => bcMonkey.isParagon == true);
            var MasterBuilder = gameModel.GetTowersWithBaseId(TowerType.EngineerMonkey).First(eMonkey => eMonkey.isParagon == true);


            //[GATHERING/INSTANTIATING LOST WEAPONS]

            //--Dart Monkey--
            //Support model buff dart monkeys (middle path)

            //Support Filters
            var dartMonkeyFilters = new TowerFilterModel[2];
            dartMonkeyFilters[0] = new FilterInBaseTowerIdModel("onlyDartMonkeys", new String[] { TowerType.DartMonkey });
            dartMonkeyFilters[1] = new FilterInTowerTiersModel("onlyDartWeapons", 0, 2, 0, 3, 0, 2);

            var newDartMonkeyPierceSupport = new PierceSupportModel("ApexPlasmaMasterPierceSupport", true, 3, "noMutatorId", dartMonkeyFilters, false, "nobuffLocsName", "noBuffIcon");
            var newDartMonkeySpeedSupport = new RateSupportModel("ApexPlasmaMasterSpeedSupport", 0.5f, true, "noMutatorId", false, 1, dartMonkeyFilters, "noBuffLocsName", "noBuffIcon");

            //--Ninja Monkey--
            //Getting Bloon Saboteour
            var sabotageAttack = gameModel.GetTower(TowerType.NinjaMonkey, 0, 5, 0).GetDescendants<AttackModel>().ToList().First(aModel => aModel.GetDescendant<DamagePercentOfMaxModel>() != null).Duplicate();
            //sabotageAttack.GetDescendant<DamagePercentOfMaxModel>().percent = 0.5f;

            //Getting Caltrops            
            var caltropAttack = gameModel.GetTower(TowerType.NinjaMonkey, 0, 0, 2).GetDescendants<AttackModel>().ToList().First(nMonkey => nMonkey.GetDescendant<AgeModel>() != null).Duplicate();
            var caltropProjectile = caltropAttack.GetDescendant<ProjectileModel>();
            caltropAttack.weapons[0].Rate *= 10; //decreasing equals increasing, remember
            caltropProjectile.GetDamageModel().damage = 500;
            caltropProjectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;
            caltropProjectile.pierce = 35;
            caltropProjectile.scale *= 2;
            caltropProjectile.radius *= 2;
            caltropProjectile.GetDescendant<AgeModel>().Lifespan *= 2;
            caltropProjectile.GetDescendant<AgeModel>().rounds = 2;
            caltropProjectile.UpdateCollisionPassList();

            int EXTRA_CALTROPS = AscendedShadow.GetDescendant<ArcEmissionModel>().Count - 1;
            var caltropWeapon = caltropAttack.weapons[0].Duplicate();
            for (int i = 0; i < EXTRA_CALTROPS; i++)
            {
                caltropAttack.AddWeapon(caltropWeapon.Duplicate());
            }

            //--Monkey Buccaneer--
            //Creating Extra Income
            var extraMoney = new PerRoundCashBonusTowerModel("Navarch Extra Money", 2000, 1, 3, new Assets.Scripts.Utils.PrefabReference(), false);

            //--Master Builder--
            //Slowing Cleansing Foam
            var slowingFoamAttack = gameModel.GetTower(TowerType.EngineerMonkey, 0, 3, 2).GetDescendants<AttackModel>().ToList().First(aModel => aModel.GetDescendant<AgeModel>() != null && aModel.GetDescendant<SlowModel>() != null).Duplicate();

            foreach (var projectileModel in slowingFoamAttack.GetDescendants<ProjectileModel>().ToList())
            {
                projectileModel.filters = null;
                projectileModel.pierce *= 5;
                projectileModel.scale *= 1.5f;
                projectileModel.radius *= 1.5f;
                var weakeningFoam = new AddBonusDamagePerHitToBloonModel("FoamPlusTwo", "noMutationId", 5, 3, 10, true, true, false);
                projectileModel.AddBehavior(weakeningFoam);
            }

            slowingFoamAttack.RemoveBehaviors<TargetSupplierModel>();
            slowingFoamAttack.targetProvider = caltropAttack.targetProvider.Duplicate();
            slowingFoamAttack.AddBehavior(caltropAttack.targetProvider.Duplicate());
            
            
                
            //slowingFoamAttack.GetDescendant<TargetSelectedPointModel>().isSelectable = false;
            foreach (var slowModifier in slowingFoamAttack.GetDescendants<SlowModifierForTagModel>().ToList())
            {
                slowModifier.tag = "Bad";
            }

            slowingFoamAttack.GetDescendant<ProjectileModel>().UpdateCollisionPassList();
            //MelonLogger.Msg(slowingFoamAttack.GetDescendant<RandomPositionModel>() != null);


            //Bloon Trap
            var bloonTrapAttack = gameModel.GetTower(TowerType.EngineerMonkey, 0, 0, 5).GetDescendants<AttackModel>().ToList().First(aModel => aModel.GetDescendant<EatBloonModel>() != null).Duplicate();
            MelonLogger.Msg(bloonTrapAttack._name);
            foreach(var weaponModel in bloonTrapAttack.GetDescendants<WeaponModel>().ToList())
            {
                weaponModel.Rate *= 3;
            }
            var bloonTrapProjectile = bloonTrapAttack.GetDescendants<ProjectileModel>().ToList().Last(pModel => pModel.GetDescendant<EatBloonModel>() != null);
            var bloonTrapModel = bloonTrapProjectile.GetDescendant<EatBloonModel>();
            bloonTrapModel.rbeCapacity = 75000;
            //bloonTrapProjectile
            bloonTrapProjectile.scale *= 1.5f;
            bloonTrapProjectile.radius *= 1.5f;
            //bloonTrapProjectile.GetDescendant<ProjectileFilterModel>().filters = new FilterModel[] { new FilterWithTagsModel("NoFortifiedBads", new string[] { "BadFortified" }, true) };

            //[Adding Weapons]
            //--Apex Plasma Master--
            ApexPlasmaMaster.AddBehavior(newDartMonkeyPierceSupport);
            ApexPlasmaMaster.AddBehavior(newDartMonkeySpeedSupport);
            

            //--Ascended Shadow--
            AscendedShadow.AddBehavior(caltropAttack);
            AscendedShadow.AddBehavior(sabotageAttack);

            //--Navarch Of The Seas--
            NavarchOfTheSeas.AddBehavior(extraMoney);

            //Master Builder
            MasterBuilder.AddBehavior(slowingFoamAttack);
            MasterBuilder.AddBehavior(bloonTrapAttack);
            
            /*
            foreach(var bloon in gameModel.bloons)
            {
                MelonLogger.Msg(bloon.id);
            }//BadFortified
            */ 
        }
    }
}

