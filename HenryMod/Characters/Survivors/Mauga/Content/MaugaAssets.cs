using RoR2;
using UnityEngine;
using MaugaMod.Modules;
using System;
using R2API;
using RoR2.Projectile;
using static R2API.DamageAPI;

namespace MaugaMod.Survivors.Mauga
{
    public static class MaugaAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        // bullet tracers

        public static GameObject chaingunChaChaTracer;
        public static GameObject chaingunGunnyTracer;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject maugaBombProjPrefab;
        public static GameObject maugaMissileProjPrefab;

        //damage types
        public static ModdedDamageType dmgChaCha = ReserveDamageType();
        public static ModdedDamageType dmgGunny = ReserveDamageType();
        public static ModdedDamageType dmgBombChaCha = ReserveDamageType();
        public static ModdedDamageType dmgMissileGunny = ReserveDamageType();

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("HenrySwordHit");

            CreateEffects();

            CreateTracers();

            CreateProjectiles();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
        }

        private static void CreateTracers()
        {
            chaingunChaChaTracer = Assets.CloneTracer("TracerGoldGat", "TracerChaCha");
            chaingunChaChaTracer.GetComponent<Tracer>().speed = 150f;
            chaingunChaChaTracer.GetComponent<Tracer>().length = 10f;
            chaingunChaChaTracer.GetComponent<LineRenderer>().startColor = new Color(1, 0.1059f, 0, 1);
            chaingunChaChaTracer.GetComponent<LineRenderer>().endColor = new Color(1, 0.3216f, 0, 1);

            chaingunGunnyTracer = Assets.CloneTracer("TracerGoldGat", "TracerGunny");
            chaingunGunnyTracer.GetComponent<Tracer>().speed = 250f;
            chaingunGunnyTracer.GetComponent<Tracer>().length = 6f;
            chaingunGunnyTracer.GetComponent<LineRenderer>().startColor = new Color(1, 0.83f, 0, 1);
            chaingunGunnyTracer.GetComponent<LineRenderer>().endColor = new Color(1, 1, 1, 1);
        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateChaChaBomb();
            CreateGunnyMissile();
        }

        private static void CreateGunnyMissile()
        {
            maugaMissileProjPrefab = Assets.CloneProjectilePrefab("CommandoGrenadeProjectile", "MaugaMissile");

            maugaMissileProjPrefab.AddComponent<ModdedDamageTypeHolderComponent>().Add(dmgMissileGunny);

            Rigidbody bombRigidBody = maugaMissileProjPrefab.GetComponent<Rigidbody>();
            if (!bombRigidBody)
            {
                bombRigidBody = maugaMissileProjPrefab.AddComponent<Rigidbody>();
            }

            ProjectileImpactExplosion maugaBombExplosion = maugaMissileProjPrefab.GetComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(maugaBombExplosion);

            //EffectComponent effectComponent = Assets.poisonExplosionEffect.GetComponent<EffectComponent>();
            //effectComponent.soundName = "assassinBottleBreak";

            maugaBombExplosion.blastRadius = 3f;
            maugaBombExplosion.destroyOnEnemy = true;
            maugaBombExplosion.destroyOnWorld = true;
            //maugaBombExplosion.impactEffect = Assets.poisonExplosionEffect;
            maugaBombExplosion.lifetime = 24f;
            maugaBombExplosion.timerAfterImpact = true;
            maugaBombExplosion.lifetimeAfterImpact = 0.5f;

            ProjectileController bombController = maugaMissileProjPrefab.GetComponent<ProjectileController>();
            //if (Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("mdlPoison") != null) poisonController.ghostPrefab = CreateGhostPrefab("mdlPoison");

            //poisonController.ghostPrefab.transform.Find("poison_trail").GetComponent<ParticleSystemRenderer>().SetMaterial(Assets.smokeTrailMat);

            /*var poisonTrailDupe = Assets.poisonTrail;
            poisonTrailDupe.transform.parent = poisonController.ghostPrefab.transform;*/

            bombController.rigidbody = bombRigidBody;
            bombController.rigidbody.useGravity = false;
            bombController.procCoefficient = 0.5f;
        }

        private static void CreateChaChaBomb()
        {
            maugaBombProjPrefab = Assets.CloneProjectilePrefab("CommandoGrenadeProjectile", "MaugaBomb");

            maugaBombProjPrefab.AddComponent<ModdedDamageTypeHolderComponent>().Add(dmgBombChaCha);

            Rigidbody bombRigidBody = maugaBombProjPrefab.GetComponent<Rigidbody>();
            if (!bombRigidBody)
            {
                bombRigidBody = maugaBombProjPrefab.AddComponent<Rigidbody>();
            }

            ProjectileImpactExplosion maugaBombExplosion = maugaBombProjPrefab.GetComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(maugaBombExplosion);

            //EffectComponent effectComponent = Assets.poisonExplosionEffect.GetComponent<EffectComponent>();
            //effectComponent.soundName = "assassinBottleBreak";

            maugaBombExplosion.blastRadius = 8f;
            maugaBombExplosion.destroyOnEnemy = true;
            maugaBombExplosion.destroyOnWorld = true;
            //maugaBombExplosion.impactEffect = Assets.poisonExplosionEffect;
            maugaBombExplosion.lifetime = 16f;
            maugaBombExplosion.timerAfterImpact = true;
            maugaBombExplosion.lifetimeAfterImpact = 0.5f;

            ProjectileController bombController = maugaBombProjPrefab.GetComponent<ProjectileController>();
            //if (Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("mdlPoison") != null) poisonController.ghostPrefab = CreateGhostPrefab("mdlPoison");

            //poisonController.ghostPrefab.transform.Find("poison_trail").GetComponent<ParticleSystemRenderer>().SetMaterial(Assets.smokeTrailMat);

            /*var poisonTrailDupe = Assets.poisonTrail;
            poisonTrailDupe.transform.parent = poisonController.ghostPrefab.transform;*/

            bombController.rigidbody = bombRigidBody;
            bombController.rigidbody.useGravity = true;
            bombController.procCoefficient = 0.5f;
        }

        private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
        {
            projectileImpactExplosion.blastDamageCoefficient = 1f;
            projectileImpactExplosion.blastProcCoefficient = 1f;
            projectileImpactExplosion.blastRadius = 1f;
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.childrenDamageCoefficient = 0f;
            projectileImpactExplosion.childrenProjectilePrefab = null;
            projectileImpactExplosion.destroyOnEnemy = false;
            projectileImpactExplosion.destroyOnWorld = false;
            projectileImpactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.impactEffect = null;
            projectileImpactExplosion.lifetime = 0f;
            projectileImpactExplosion.lifetimeAfterImpact = 0f;
            projectileImpactExplosion.lifetimeRandomOffset = 0f;
            projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
            projectileImpactExplosion.timerAfterImpact = false;

            projectileImpactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
        }
        #endregion projectiles
    }
}
