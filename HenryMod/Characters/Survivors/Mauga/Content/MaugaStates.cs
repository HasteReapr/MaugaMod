using MaugaMod.Survivors.Mauga.SkillStates;

namespace MaugaMod.Survivors.Mauga
{
    public static class MaugaStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(Shoot_ChaCha));

            Modules.Content.AddEntityState(typeof(Shoot_ChaCha_Bomb));

            Modules.Content.AddEntityState(typeof(Shoot_Gunny));

            Modules.Content.AddEntityState(typeof(Shoot_Gunny_Missile));

            Modules.Content.AddEntityState(typeof(Charge));

            Modules.Content.AddEntityState(typeof(Overdrive));
        }
    }
}
