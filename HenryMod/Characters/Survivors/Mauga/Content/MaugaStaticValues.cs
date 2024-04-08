using System;
using RoR2.Skills;

namespace MaugaMod.Survivors.Mauga
{
    public static class MaugaStaticValues
    {
        public const float swordDamageCoefficient = 2.8f;

        public const float chaChaDamageCoefficient = 0.3f; // Cha Cha is the incendiary chaingun, which crits when shooting on fire targets

        public const float chaChaBombDmgCoeff = 0.9f; // Cha Cha is the incendiary chaingun, which crits when shooting on fire targets

        public const float gunnyDamageCoefficient = 0.2f; // Gunny is the healing chaingun

        public const float gunnyMissileDmgCoeff = 1f; // Gunny is the healing chaingun

        public const float maugaOverrunDamageCoef = 2.5f;

        public const float maugaSlamDamageCoef = 15f;

        internal static SteppedSkillDef Shoot_ChaCha;
        internal static SteppedSkillDef Shoot_Gunny;
    }
}