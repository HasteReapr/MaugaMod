using EntityStates;
using MaugaMod.Characters.Survivors.Mauga.Components;
using MaugaMod.Survivors.Mauga;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

namespace MaugaMod.Survivors.Mauga.SkillStates
{
    public class Shoot_ChaCha : BaseSkillState
    {
        public static float damageCoefficient = MaugaStaticValues.chaChaDamageCoefficient;
        public static float procCoefficient = 0.1f;
        public static float baseDuration = 0.06f; // 15 shots per second
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

            chaingunComp.firingChaCha = true;

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            chaingunComp.firingChaCha = false;
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

                characterBody.AddSpreadBloom(chaingunComp.firingGunny ? 4.5f : 0.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();
                    //AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                    BulletAttack ChaChaBullet = new BulletAttack
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
                        minSpread = 2,
                        maxSpread = 3,
                        isCrit = false,
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
                        spreadPitchScale = chaingunComp.firingGunny ? 1f : 0.3f,
                        spreadYawScale = chaingunComp.firingGunny ? 1 : 0.3f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    };

                    ChaChaBullet.AddModdedDamageType(MaugaAssets.dmgChaCha);

                    ChaChaBullet.Fire();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}