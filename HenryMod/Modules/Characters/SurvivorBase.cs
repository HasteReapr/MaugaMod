﻿using BepInEx.Configuration;
using RoR2;
using HenryMod.Modules.Characters;
using UnityEngine;
using System.Collections.Generic;
using RoR2.Skills;

//todo windows change namespace
namespace HenryMod.Modules.Survivors
{
    internal abstract class SurvivorBase<T> : CharacterBase<T> where T: SurvivorBase<T>, new()
    {
        public abstract string displayPrefabName { get; }

        public abstract string survivorTokenPrefix { get; }
        
        public abstract UnlockableDef characterUnlockableDef { get; }

        public virtual GameObject displayPrefab { get; set; }

        public override void InitializeCharacter()
        {
            base.InitializeCharacter();

            InitializeDisplayPrefab();

            InitializeSurvivor();
        }

        protected virtual void InitializeDisplayPrefab()
        {
            displayPrefab = Modules.Prefabs.CreateDisplayPrefab(assetBundle, displayPrefabName, bodyPrefab);
        }

        protected virtual void InitializeSurvivor() {      
            RegisterNewSurvivor(bodyPrefab, displayPrefab, bodyInfo.bodyColor, survivorTokenPrefix, characterUnlockableDef, bodyInfo.sortPosition);
        }

        //todo funny? also why does this have overloads if it's one of the ones we don't want the user to have to see?
        //survivorinfo?
        public static void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string tokenPrefix, UnlockableDef unlockableDef, float sortPosition)
        {
            Modules.Content.CreateSurvivor(bodyPrefab, displayPrefab, charColor, tokenPrefix, unlockableDef, sortPosition);
        }

        #region CharacterSelectSurvivorPreviewDisplayController
        protected virtual void AddCssPreviewSkill(int indexFromEditor, SkillFamily skillFamily, SkillDef skillDef)
        {
            CharacterSelectSurvivorPreviewDisplayController CSSPreviewDisplayConroller = displayPrefab.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
            if (!CSSPreviewDisplayConroller)
            {
                Log.Error("trying to add skillChangeResponse to null CharacterSelectSurvivorPreviewDisplayController.\nMake sure you created one on your Display prefab in editor");
                return;
            }

            CSSPreviewDisplayConroller.skillChangeResponses[indexFromEditor].triggerSkillFamily = skillFamily;
            CSSPreviewDisplayConroller.skillChangeResponses[indexFromEditor].triggerSkill = skillDef;
        }

        protected virtual void AddCssPreviewSkin(int indexFromEditor, SkinDef skinDef)
        {
            CharacterSelectSurvivorPreviewDisplayController CSSPreviewDisplayConroller = displayPrefab.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
            if (!CSSPreviewDisplayConroller)
            {
                Log.Error("trying to add skinChangeResponse to null CharacterSelectSurvivorPreviewDisplayController.\nMake sure you created one on your Display prefab in editor");
                return;
            }

            CSSPreviewDisplayConroller.skinChangeResponses[indexFromEditor].triggerSkin = skinDef;
        }

        protected virtual void FinalizeCSSPreviewDisplayController()
        {
            if (!displayPrefab)
                return;

            CharacterSelectSurvivorPreviewDisplayController CSSPreviewDisplayConroller = displayPrefab.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
            if (!CSSPreviewDisplayConroller)
                return;

            //set body prefab
            CSSPreviewDisplayConroller.bodyPrefab = bodyPrefab;

            //clear list of null entries
            List<CharacterSelectSurvivorPreviewDisplayController.SkillChangeResponse> newlist = new List<CharacterSelectSurvivorPreviewDisplayController.SkillChangeResponse>();

            for (int i = 0; i < CSSPreviewDisplayConroller.skillChangeResponses.Length; i++)
            {
                if (CSSPreviewDisplayConroller.skillChangeResponses[i].triggerSkillFamily != null)
                {
                    newlist.Add(CSSPreviewDisplayConroller.skillChangeResponses[i]);
                }
            }

            CSSPreviewDisplayConroller.skillChangeResponses = newlist.ToArray();
        }
        #endregion
    }
}
