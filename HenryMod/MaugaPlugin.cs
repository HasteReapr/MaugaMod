using BepInEx;
using MaugaMod.Survivors.Mauga;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace MaugaMod
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class MaugaPlugin : BaseUnityPlugin
    {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.hastereapr.MaugaMod";
        public const string MODNAME = "MaugaMod";
        public const string MODVERSION = "1.0.0";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "HASTEREAPR";

        public static MaugaPlugin instance;
        //public static HUD hud = null;

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new MaugaSurvivor().Initialize();

            Hook();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }

        private void Hook()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;

            On.RoR2.CharacterBody.OnTakeDamageServer += CharacterBody_OnTakeDamageServer;

            //On.RoR2.UI.HUD.Awake += HUD_Awake;
        }

        /*private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            hud = self;
        }*/

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            // Check if the victim is on fire, and if our damage type is ChaCha's. If both are true then we can 100% crit.
            if (self.body.HasBuff(RoR2Content.Buffs.OnFire) && DamageAPI.HasModdedDamageType(damageInfo, MaugaAssets.dmgChaCha))
            {
                damageInfo.crit = true;
            }

            // Check if the victim has overdrive buff, if so reduce damage by 30%.
            if (self.body.HasBuff(MaugaBuffs.OverDriveBuff))
            {
                damageInfo.damage *= 0.7f;
            }

            orig(self, damageInfo);
        }

        private void CharacterBody_OnTakeDamageServer(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);

            // Check if the damage type is ChaCha's type, and then set on fire if the 10% chance hits.
            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, MaugaAssets.dmgChaCha))
            {
                if(RoR2.Util.CheckRoll(33f, damageReport.attackerMaster))
                    self.AddTimedBuff(RoR2Content.Buffs.OnFire, 1f);

                /*if (self.HasBuff(RoR2Content.Buffs.OnFire) && RoR2.Util.CheckRoll(100f, damageReport.attackerMaster))
                {
                    BlastAttack explosiveRound = new BlastAttack();
                    explosiveRound.radius = 2;
                    explosiveRound.procCoefficient = 1f;
                    explosiveRound.position = self.corePosition;
                    explosiveRound.attacker = gameObject;
                    explosiveRound.inflictor = gameObject;
                    explosiveRound.crit = true;
                    explosiveRound.baseDamage = damageReport.damageDealt;
                    explosiveRound.damageType = DamageType.Stun1s;
                    explosiveRound.baseForce = 50;
                    explosiveRound.teamIndex = damageReport.attackerBody.teamComponent.teamIndex;
                    explosiveRound.attackerFiltering = AttackerFiltering.AlwaysHit;
                    explosiveRound.canRejectForce = true;
                    explosiveRound.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    explosiveRound.Fire();
                }*/
            }

            // Check if the damage type is Gunny's type, and then give barrier to the player if it crits.
            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, MaugaAssets.dmgGunny))
            {
                if (damageReport.damageInfo.crit)
                {
                    damageReport.attackerBody.healthComponent.barrier += damageReport.damageDealt;
                }
            }

            // Check if the attacker has the overdrive buff, if they do heal back 25% of the damage dealt.
            if (damageReport.attackerBody.HasBuff(MaugaBuffs.OverDriveBuff))
            {
                damageReport.attackerBody.healthComponent.Heal(damageReport.damageDealt * 0.25f, default);
            }
        }
    }
}
