using BepInEx.Configuration;
using MaugaMod.Characters.Survivors.Mauga.Components;
using MaugaMod.Modules;
using MaugaMod.Modules.Characters;
using MaugaMod.Survivors.Mauga.Components;
using MaugaMod.Survivors.Mauga.SkillStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaugaMod.Survivors.Mauga
{
    public class MaugaSurvivor : SurvivorBase<MaugaSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "ror2mauga"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "MaugaBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "MaugaMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlMauga";
        public override string displayPrefabName => "MaugaDisplay";

        public const string MAUGA_PREFIX = MaugaPlugin.DEVELOPER_PREFIX + "_MAUGA_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => MAUGA_PREFIX;
        
        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = MAUGA_PREFIX + "NAME",
            subtitleNameToken = MAUGA_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texMaugaIcon"),
            bodyColor = Color.white,
            sortPosition = 100,

            crosshair = Assets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 175f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = assetBundle.LoadMaterial("matHenry"),
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                },
                new CustomRendererInfo
                {
                    childName = "Model",
                }
        };

        public override UnlockableDef characterUnlockableDef => MaugaUnlockables.characterUnlockableDef;

        //public override ItemDisplaysBase itemDisplays => new MaugaItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize()
        {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();

            bodyPrefab.AddComponent<MaugaChaingunComponent>();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            MaugaUnlockables.Init();

            base.InitializeCharacter();

            MaugaConfig.Init();
            MaugaStates.Init();
            MaugaTokens.Init();

            MaugaAssets.Init(assetBundle);
            MaugaBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<HenryWeaponComponent>();
            //bodyPrefab.AddComponent<HuntressTrackerComopnent>();
            //anything else here
        }

        public void AddHitboxes()
        {
            ChildLocator childLocator = characterModelObject.GetComponent<ChildLocator>();

            //example of how to create a hitbox
            Transform hitBoxTransform = childLocator.FindChild("SwordHitbox");
            Transform chargeBoxTransform = childLocator.FindChild("ChargeHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", hitBoxTransform);
            Prefabs.SetupHitBoxGroup(characterModelObject, "ChargeHitbox", chargeBoxTransform);
        }

        public override void InitializeEntityStateMachines() 
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            SkillLocator sk = bodyPrefab.GetComponent<SkillLocator>();

            //the main "body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
                //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon"); // Cha-Cha goes here
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2"); // Gunny goes in this one
            Prefabs.AddEntityStateMachine(bodyPrefab, "CardiacSelfheal"); // Overdrive takes up this state machine, so it can be active and not turn off while the buff is active
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            MaugaStaticValues.Shoot_ChaCha = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "Mauga_ChaCha",
                    MAUGA_PREFIX + "PRIMARY_GUN_NAME",
                    MAUGA_PREFIX + "PRIMARY_GUN_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot_ChaCha)),
                    "Weapon",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            //primarySkillDef1.stepCount = 2;
            //primarySkillDef1.stepGraceDuration = 0.5f;

            Skills.AddPrimarySkills(bodyPrefab, MaugaStaticValues.Shoot_ChaCha);

            SteppedSkillDef shootBombs = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "Mauga_ChaCha_Bomb",
                    MAUGA_PREFIX + "PRIMARY_BOMB_NAME",
                    MAUGA_PREFIX + "PRIMARY_BOMB_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot_ChaCha_Bomb)),
                    "Weapon",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            //primarySkillDef1.stepCount = 2;
            //primarySkillDef1.stepGraceDuration = 0.5f;

            Skills.AddPrimarySkills(bodyPrefab, shootBombs);
            //Skills.AddSkillToFamily(bodyPrefab.GetComponent<SkillLocator>().primary.skillFamily, shootBombs);
        }

        private void AddSecondarySkills()
        {
            MaugaStaticValues.Shoot_Gunny = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "Mauga_Gunny",
                    MAUGA_PREFIX + "SECONDARY_GUN_NAME",
                    MAUGA_PREFIX + "SECONDARY_GUN_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot_Gunny)),
                    "Weapon2",
                    true
                ));

            Skills.AddSecondarySkills(bodyPrefab, MaugaStaticValues.Shoot_Gunny);

            SteppedSkillDef shootMissiles = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "Mauga_Gunny",
                    MAUGA_PREFIX + "SECONDARY_BOMB_NAME",
                    MAUGA_PREFIX + "SECONDARY_BOMB_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot_Gunny_Missile)),
                    "Weapon2",
                    true
                ));

            Skills.AddSecondarySkills(bodyPrefab, shootMissiles);
        }

        private void AddUtiitySkills()
        {
            //here's a skilldef of a typical movement skill.
            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "MaugaCharge",
                skillNameToken = MAUGA_PREFIX + "UTILITY_CHARGE_NAME",
                skillDescriptionToken = MAUGA_PREFIX + "UTILITY_CHARGE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Charge)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 8f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1);
        }

        private void AddSpecialSkills()
        {
            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "MaugaSelfheal",
                skillNameToken = MAUGA_PREFIX + "SPECIAL_SELFHEAL_NAME",
                skillDescriptionToken = MAUGA_PREFIX + "SPECIAL_SELFHEAL_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Overdrive)),
                activationStateMachineName = "CardiacSelfheal", 
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 10f,

                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                mustKeyPress = false,
            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1);
        }
        #endregion skills
        
        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
                //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
                //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin
            
            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);
            
            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            MaugaAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {

            if (sender.HasBuff(MaugaBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }
        }
    }
}