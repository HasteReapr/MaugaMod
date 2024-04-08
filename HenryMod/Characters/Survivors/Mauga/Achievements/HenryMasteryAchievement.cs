using RoR2;
using MaugaMod.Modules.Achievements;

namespace MaugaMod.Survivors.Mauga.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class HenryMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = MaugaSurvivor.MAUGA_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = MaugaSurvivor.MAUGA_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => MaugaSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}