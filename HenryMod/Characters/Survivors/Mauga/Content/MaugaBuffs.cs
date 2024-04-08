using RoR2;
using UnityEngine;

namespace MaugaMod.Survivors.Mauga
{
    public static class MaugaBuffs
    {
        // armor buff gained during roll
        public static BuffDef armorBuff;
        public static BuffDef OverDriveBuff;

        public static void Init(AssetBundle assetBundle)
        {
            armorBuff = Modules.Content.CreateAndAddBuff("HenryArmorBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.white,
                false,
                false);

            OverDriveBuff = Modules.Content.CreateAndAddBuff("MaugaOverDriveBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite, // replace this with a real icon at some point, need photoshop
                Color.white,
                false,
                false);
        }
    }
}
