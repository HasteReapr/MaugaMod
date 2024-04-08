using EntityStates;
using MaugaMod.Characters.Survivors.Mauga.Components;
using MaugaMod.Survivors.Mauga;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

namespace MaugaMod.Survivors.Mauga.SkillStates
{
    public class Shoot_Gunny : BaseSkillState
    {
        public static float damageCoefficient = MaugaStaticValues.gunnyDamageCoefficient;
        public static float procCoefficient = 0.2f;
        public static float baseDuration = 0.035f; // 28 shots per second
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 5f;
        public static float recoil = 0;
        public static float range = 2048f;
        public static GameObject tracerEffectPrefab = MaugaAssets.chaingunGunnyTracer;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        private MaugaChaingunComponent chaingunComp;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            chaingunComp = base.GetComponent<MaugaChaingunComponent>();

            chaingunComp.firingGunny = true;

            PlayAnimation("RightArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            chaingunComp.firingGunny = false;
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime)
            {
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(chaingunComp.firingChaCha ? 6.5f : 0.1f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();
                    //AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                    BulletAttack GunnyBullet =  new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = range,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 1,
                        maxSpread = 2,
                        isCrit = RollCrit(),
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = procCoefficient,
                        radius = 0.05f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = chaingunComp.firingChaCha ? 1 : 0.1f,
                        spreadYawScale = chaingunComp.firingChaCha ? 1 : 0.1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    };

                    GunnyBullet.AddModdedDamageType(MaugaAssets.dmgGunny);

                    GunnyBullet.Fire();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}