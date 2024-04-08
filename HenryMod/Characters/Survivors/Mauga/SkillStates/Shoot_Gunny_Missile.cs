using EntityStates;
using MaugaMod.Characters.Survivors.Mauga.Components;
using MaugaMod.Survivors.Mauga;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static R2API.DamageAPI;

namespace MaugaMod.Survivors.Mauga.SkillStates
{
    public class Shoot_Gunny_Missile : BaseSkillState
    {
        public static float damageCoefficient = MaugaStaticValues.chaChaDamageCoefficient;
        public static float procCoefficient = 0.1f;
        public static float baseDuration = 0.083f; // 12 shots per second
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 5f;
        public static float recoil = 0;
        public static float range = 2048f;
        public static GameObject tracerEffectPrefab = MaugaAssets.chaingunChaChaTracer;

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

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
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

                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();
                    //AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
                    float randomSpread = chaingunComp.firingChaCha ? 2 : 0.5f;

                    FireProjectileInfo info = new FireProjectileInfo()
                    {
                        owner = gameObject,
                        damage = MaugaStaticValues.gunnyMissileDmgCoeff * characterBody.damage,
                        force = 5,
                        crit = characterBody.RollCrit(),
                        position = aimRay.origin,//FindModelChild(handString).position,
                        rotation = Util.QuaternionSafeLookRotation(aimRay.direction) * Quaternion.Euler(Random.RandomRange(-randomSpread, randomSpread), Random.RandomRange(-randomSpread, randomSpread), Random.RandomRange(-randomSpread, randomSpread)),
                        projectilePrefab = MaugaAssets.maugaMissileProjPrefab,
                        speedOverride = 128,
                    };

                    ProjectileManager.instance.FireProjectile(info);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}