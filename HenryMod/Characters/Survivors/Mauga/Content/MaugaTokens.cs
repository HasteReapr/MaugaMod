using System;
using MaugaMod.Modules;
using MaugaMod.Survivors.Mauga.Achievements;

namespace MaugaMod.Survivors.Mauga
{
    public static class MaugaTokens
    {
        public static void Init()
        {
            AddHenryTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Henry.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddHenryTokens()
        {
            string prefix = MaugaSurvivor.MAUGA_PREFIX;

            string desc = "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so he left, searching for a new identity.";
            string outroFailure = "..and so he vanished, forever a blank slate.";

            Language.Add(prefix + "NAME", "Mauga");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Berserker Minigun Guy");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Berserker");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Something better than default mauga passive");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_GUN_NAME", "Incendiary Chaingun");
            Language.Add(prefix + "PRIMARY_GUN_DESCRIPTION", Tokens.agilePrefix + $"Fire incendiary rounds from your left chaingun with a wide spread. <style=cIsUtility>Deals critical damage to burning targets.</style>");
            #endregion

            #region AltPrimary
            Language.Add(prefix + "PRIMARY_BOMB_NAME", "Incendiary Chaingun");
            Language.Add(prefix + "PRIMARY_BOMB_DESCRIPTION", Tokens.agilePrefix + $"Fire incendiary rounds from your left chaingun with a wide spread. <style=cIsUtility>Deals critical damage to burning targets.</style>");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_GUN_NAME", "Volatile Chaingun");
            Language.Add(prefix + "SECONDARY_GUN_DESCRIPTION", Tokens.agilePrefix + $"Fire volatile rounds from your right chaingun with a tight spread. <style=cIsHealing>50% of critical damage dealt is added in barriers.</style>");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_CHARGE_NAME", "Overrun");
            Language.Add(prefix + "UTILITY_CHARGE_DESCRIPTION", "Charge forward at a rapid pace. You cannot be stunned while charging. Pressing primary or secondary cancels the charge into a stomp. Pressing utility cancels the charge.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_SELFHEAL_NAME", "Overdrive");
            Language.Add(prefix + "SPECIAL_SELFHEAL_DESCRIPTION", $"Activate self healing, healing yourself for <style=cIsHealing>25% damage dealt.</style> Taking <style=cIsUtility>30% less damage</style> while active.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(HenryMasteryAchievement.identifier), "Henry: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(HenryMasteryAchievement.identifier), "As Henry, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
