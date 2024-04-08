using EntityStates;
using MaugaMod.Survivors.Mauga;
using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MaugaMod.Survivors.Mauga.SkillStates
{
    public class Charge : BaseSkillState
    {
        public static float duration = 2.6f; // 
        public static float minChargeDuration = 0.32f; // if you instantly cancel
        public static float maxChargeDuration = 2f; // full duration of the charge
        public static float initialSpeedCoefficient = 3f;
        public static float finalSpeedCoefficient = 2.5f;

        public static string dodgeSoundString = "HenryRoll";
        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;

        //timing and movement stuffs
        private float rollSpeed;
        private bool lockedDir;
        private bool hasJumped;
        private bool hasKaboom;
        private bool endedEarly;

        private float jumpTime;
        private Vector3 desiredDirection;
        private Animator animator;
        private Vector3 previousPosition;

        //hitbox and damage stuff
        private OverlapAttack chargeAttack;
        private BlastAttack chargeSlam;
        private List<HurtBox> victimsHit = new List<HurtBox>();

        //UI and other stuffs
        //private GameObject stompText;
        //private GameObject cancelText;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            if (isAuthority && characterDirection)
            {
                desiredDirection = inputBank.aimDirection;
                desiredDirection.y = 0;

                characterDirection.forward = desiredDirection;
            }

            lockedDir = false;
            hasJumped = false;
            hasKaboom = false;
            endedEarly = false;

            RecalculateRollSpeed();

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", duration);
            Util.PlaySound(dodgeSoundString, gameObject);

            // make a UI element that says you can primary or secondary input to stomp, and utility input to cancel
            

            if (NetworkServer.active)
            {
                //buffs go here
            }

            SetupOverrunAttack();
            SetupChargeSlam();
        }

        private void SetupOverrunAttack()
        {
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "ChargeHitbox");
            }

            chargeAttack = new OverlapAttack();
            chargeAttack.attacker = base.gameObject;
            chargeAttack.inflictor = base.gameObject;
            chargeAttack.teamIndex = base.GetTeam();
            chargeAttack.damage = MaugaStaticValues.maugaOverrunDamageCoef * this.damageStat;
            chargeAttack.procCoefficient = 1f;
            chargeAttack.forceVector = Vector3.up * 100;
            chargeAttack.pushAwayForce = 5000;
            chargeAttack.hitBoxGroup = hitBoxGroup;
            chargeAttack.isCrit = RollCrit();
        }

        private void SetupChargeSlam()
        {
            chargeSlam = new BlastAttack();
            chargeSlam.radius = 12;
            chargeSlam.procCoefficient = 1f;
            chargeSlam.position = transform.position;
            chargeSlam.attacker = gameObject;
            chargeSlam.inflictor = gameObject;
            chargeSlam.crit = RollCrit();
            chargeSlam.baseDamage = MaugaStaticValues.maugaSlamDamageCoef * this.damageStat;
            chargeSlam.damageType = DamageType.Stun1s;
            chargeSlam.baseForce = 1000;
            chargeSlam.teamIndex = GetTeam();
            chargeSlam.attackerFiltering = AttackerFiltering.NeverHitSelf;
            chargeSlam.canRejectForce = false;
            chargeSlam.falloffModel = BlastAttack.FalloffModel.SweetSpot;
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        private void SteerCharge()
        {
            if (characterDirection && inputBank)
            {
                //strip the Y value
                Vector2 moveVector = Util.Vector3XZToVector2XY(inputBank.moveVector);
                Vector2 lookVector = Util.Vector3XZToVector2XY(inputBank.aimDirection);

                Vector2 lerpedVector = (moveVector + lookVector)/2;

                // if the input bank isnt 0, set out desired direction to the move vector
                if (lerpedVector != Vector2.zero)
                {
                    lerpedVector.Normalize();
                    desiredDirection = new Vector3(lerpedVector.x, 0f, lerpedVector.y).normalized;
                }
            }
        }

        private Vector3 calculateVelocity()
        {
            // Multiply the direction we're facing by our movespeed and the speed of the charge.
            return desiredDirection * characterBody.moveSpeed * rollSpeed;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateRollSpeed();
            SteerCharge();

            if (characterDirection && !lockedDir)
            {
                //here we set the velocity to the desired direction
                characterDirection.moveVector = desiredDirection;
                if (characterMotor && !characterMotor.disableAirControlUntilCollision)
                {
                    characterMotor.rootMotion += calculateVelocity() * Time.fixedDeltaTime;
                }

                //here we override the camera, and force it to look in the direction we're moving
                if (cameraTargetParams)
                {
                    //do somethign here, i dont actually know what yet.
                }
            }

            if (isAuthority)
            {
                // if we press primary or secondary, or if we're at the end of the charge duration, start the jump
                if ((inputBank.skill1.justPressed || inputBank.skill2.justPressed || fixedAge > maxChargeDuration) && characterMotor)
                {
                    //do the stomp thingy, slow down velocity to 0, jump and land.
                    //lock our direction so we stop doing the steering stuff
                    lockedDir = true;
                    //if we havent jumped yet, do the jump stuff
                    if (!hasJumped)
                    {
                        hasJumped = true;
                        characterMotor.Jump(1f, 1f); //without this here, the Y velocity gets zeroed out
                        characterMotor.velocity = new Vector3(calculateVelocity().x, 14, calculateVelocity().z); //Jump() needs to be before this, otherwise X and Z are zeroed out.
                        jumpTime = fixedAge;
                    }

                    // If we have jumped, and have landed, and haven't fired, and are at least 0.1 seconds after we started the jump we go kaboom.
                    // The timer is here because without it, the slam gets fired as we jump, instead of as we land.
                    if (hasJumped && characterMotor.isGrounded && !hasKaboom && fixedAge > jumpTime + 0.05f)
                    {
                        hasKaboom = true;
                        chargeSlam.position = transform.position;
                        chargeSlam.Fire();
                    }

                    if(hasJumped && hasKaboom)
                    {
                        endedEarly = true;
                    }
                }

                // if we press our utility button again, cancel the move.
                if (inputBank.skill3.justPressed)
                {
                    endedEarly = true;
                    outer.SetNextStateToMain();
                    return;
                }

                if (!lockedDir)
                {
                    //set the force vector to our momentum direction, so enemies get flung back
                    chargeAttack.forceVector = (desiredDirection/2) + new Vector3(0, 50, 0);
                    chargeAttack.Fire();
                }
            }

            if (isAuthority && endedEarly || (fixedAge >= duration && hasKaboom))
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();

            characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(desiredDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            desiredDirection = reader.ReadVector3();
        }
    }
}