using EntityStates;
using MaugaMod.Survivors.Mauga;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace MaugaMod.Survivors.Mauga.SkillStates
{
    public class Overdrive : BaseSkillState
    {
        public static float BaseDuration = 4f;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(MaugaBuffs.OverDriveBuff, BaseDuration);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= BaseDuration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}