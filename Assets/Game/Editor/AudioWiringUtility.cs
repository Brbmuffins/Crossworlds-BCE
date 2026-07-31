#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Crossworlds.EditorTools
{
    public static class AudioWiringUtility
    {
        private const string SpellSfxRoot = "Assets/Magic Spell Sound Effects Pack Vol 1";
        private const string OrcMiscRoot = "Assets/Orc/Misc";

        [MenuItem("BCE/Setup/12 ▶ Wire Spell and Enemy Audio")]
        public static void WireAllAudio()
        {
            Debug.Log("[AudioWiringUtility] Beginning automated audio matching and wiring...");

            // 1. Gather all spell clips by subfolders
            Dictionary<string, List<AudioClip>> spellCategoryClips = LoadSpellClips();
            if (spellCategoryClips.Count == 0)
            {
                Debug.LogError("[AudioWiringUtility] No spell clips found in '" + SpellSfxRoot + "'! Please verify files.");
                return;
            }

            // 2. Gather Orc voice clips
            Dictionary<string, List<AudioClip>> orcClips = LoadOrcClips();

            // 3. Process Player/Class prefabs with spellbooks
            string[] classPaths = {
                "Assets/Game/Game_Prefabs/Arcanist.prefab",
                "Assets/Game/Game_Prefabs/Cleric.prefab",
                "Assets/Game/Game_Prefabs/Ironclad.prefab",
                "Assets/Game/Game_Prefabs/Marauder.prefab",
                "Assets/Game/Game_Prefabs/Shadowblade.prefab",
                "Assets/Game/Game_Prefabs/Muffin Junk/Warden_Legacy.prefab"
            };

            int spellsWiredCount = 0;
            foreach (var path in classPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning("[AudioWiringUtility] Class prefab not found: " + path);
                    continue;
                }

                AbilityCaster caster = prefab.GetComponentInChildren<AbilityCaster>();
                if (caster == null)
                {
                    Debug.LogWarning("[AudioWiringUtility] No AbilityCaster found on " + path);
                    continue;
                }

                SerializedObject serializedCaster = new SerializedObject(caster);
                SerializedProperty spellbookProp = serializedCaster.FindProperty("spellbook");

                if (spellbookProp == null || !spellbookProp.isArray)
                {
                    Debug.LogWarning("[AudioWiringUtility] Spellbook property not found on " + caster.name);
                    continue;
                }

                Debug.Log("[AudioWiringUtility] Processing spellbook for: " + path + " (Spells: " + spellbookProp.arraySize + ")");

                for (int i = 0; i < spellbookProp.arraySize; i++)
                {
                    SerializedProperty abilityProp = spellbookProp.GetArrayElementAtIndex(i);
                    SerializedProperty nameProp = abilityProp.FindPropertyRelative("abilityName");
                    SerializedProperty castSfxProp = abilityProp.FindPropertyRelative("castSFX");
                    SerializedProperty hitSfxProp = abilityProp.FindPropertyRelative("hitSFX");
                    SerializedProperty sfxVolumeProp = abilityProp.FindPropertyRelative("sfxVolume");

                    string abilityName = nameProp != null ? nameProp.stringValue : "Ability";
                    if (string.IsNullOrEmpty(abilityName)) abilityName = "Ability";

                    // Determine the best category for this ability
                    string category = DetermineCategory(abilityName);
                    if (spellCategoryClips.TryGetValue(category, out List<AudioClip> clips) && clips.Count > 0)
                    {
                        // Use deterministic hash code of ability name to select clips
                        int hash = Mathf.Abs(abilityName.GetHashCode());
                        AudioClip castClip = clips[hash % clips.Count];
                        AudioClip hitClip = clips[(hash + 1) % clips.Count]; // Offset by 1 for different hit clip

                        if (castSfxProp != null) castSfxProp.objectReferenceValue = castClip;
                        if (hitSfxProp != null) hitSfxProp.objectReferenceValue = hitClip;
                        if (sfxVolumeProp != null) sfxVolumeProp.floatValue = 0.75f;

                        spellsWiredCount++;
                    }
                    else
                    {
                        // Fallback to General category
                        if (spellCategoryClips.TryGetValue("General", out List<AudioClip> fallbackClips) && fallbackClips.Count > 0)
                        {
                            int hash = Mathf.Abs(abilityName.GetHashCode());
                            AudioClip castClip = fallbackClips[hash % fallbackClips.Count];
                            AudioClip hitClip = fallbackClips[(hash + 3) % fallbackClips.Count];

                            if (castSfxProp != null) castSfxProp.objectReferenceValue = castClip;
                            if (hitSfxProp != null) hitSfxProp.objectReferenceValue = hitClip;
                            if (sfxVolumeProp != null) sfxVolumeProp.floatValue = 0.75f;

                            spellsWiredCount++;
                        }
                    }

                    // Process ability variants (cursor distance selectable zones)
                    SerializedProperty variantsProp = abilityProp.FindPropertyRelative("variants");
                    if (variantsProp != null && variantsProp.isArray)
                    {
                        for (int j = 0; j < variantsProp.arraySize; j++)
                        {
                            SerializedProperty variantProp = variantsProp.GetArrayElementAtIndex(j);
                            SerializedProperty vNameProp = variantProp.FindPropertyRelative("variantName");
                            SerializedProperty vCastSfxProp = variantProp.FindPropertyRelative("castSFX");
                            SerializedProperty vHitSfxProp = variantProp.FindPropertyRelative("hitSFX");

                            string vName = vNameProp != null ? vNameProp.stringValue : "Variant";
                            string vCategory = DetermineCategory(vName + " " + abilityName);

                            if (spellCategoryClips.TryGetValue(vCategory, out List<AudioClip> vClips) && clips.Count > 0)
                            {
                                int hash = Mathf.Abs((vName + abilityName).GetHashCode());
                                if (vCastSfxProp != null) vCastSfxProp.objectReferenceValue = vClips[hash % vClips.Count];
                                if (vHitSfxProp != null) vHitSfxProp.objectReferenceValue = vClips[(hash + 1) % vClips.Count];
                            }
                        }
                    }
                }

                serializedCaster.ApplyModifiedProperties();
                EditorUtility.SetDirty(caster);
                PrefabUtility.RecordPrefabInstancePropertyModifications(caster);
            }

            // 4. Process Enemy prefabs with Orc Voice
            string[] enemyPaths = {
                "Assets/Game/3D Models/Enemies/Ogres/O'gar Brute/Prefab_OgreBrute.prefab",
                "Assets/Game/Game_Prefabs/Muffin Junk/Enemy_Elite.prefab",
                "Assets/Game/Game_Prefabs/Muffin Junk/Enemy_Grunt.prefab",
                "Assets/Game/Game_Prefabs/Muffin Junk/Enemy_Ranged.prefab",
                "Assets/Game/3D Models/Ashen Wasteland/aw_enemies/model_aw_pummeler/Prefab/forged_aw_pummeler_3D_rigged (1) 1_Enemy.prefab",
                "Assets/Game/3D Models/Ashen Wasteland/aw_enemies/model_aw_chaos_weaver/Prefab/forged_aw_chaos_weaver_Enemy.prefab",
                "Assets/Game/3D Models/Ashen Wasteland/aw_enemies/model_aw_instigator/Prefab/forged_aw_instigator_3D_rigged_Enemy.prefab",
                "Assets/Game/3D Models/Ashen Wasteland/aw_enemies/model_aw_devastator/Prefab/forged_devastator_3D_rigged 1_Enemy.prefab"
            };

            int enemiesWiredCount = 0;
            foreach (var path in enemyPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning("[AudioWiringUtility] Enemy prefab not found: " + path);
                    continue;
                }

                // Make sure EnemySfxProfile is present
                EnemySfxProfile sfxProfile = prefab.GetComponent<EnemySfxProfile>();
                if (sfxProfile == null)
                {
                    sfxProfile = prefab.AddComponent<EnemySfxProfile>();
                    Debug.Log("[AudioWiringUtility] Added missing EnemySfxProfile to " + path);
                }

                SerializedObject serializedSfx = new SerializedObject(sfxProfile);
                SerializedProperty aggroProp = serializedSfx.FindProperty("aggro");
                SerializedProperty attack1Prop = serializedSfx.FindProperty("attack1");
                SerializedProperty attack2Prop = serializedSfx.FindProperty("attack2");
                SerializedProperty attack3Prop = serializedSfx.FindProperty("attack3");
                SerializedProperty attack4Prop = serializedSfx.FindProperty("attack4");
                SerializedProperty attackImpactProp = serializedSfx.FindProperty("attackImpact");
                SerializedProperty getHitProp = serializedSfx.FindProperty("getHit");
                SerializedProperty deathProp = serializedSfx.FindProperty("death");
                SerializedProperty volumeProp = serializedSfx.FindProperty("volume");
                SerializedProperty pitchVarProp = serializedSfx.FindProperty("pitchVariation");

                // Blending Growl Hard (heavy combat effort) and Vocalfry (raspy/creepier breaths & pain)
                AssignOrcClip(aggroProp, orcClips, "Growl", "Growl_Hard", 0);       // Aggro: loud growl
                AssignOrcClip(attack1Prop, orcClips, "Attack", "Growl_Hard", 0);     // Attack 1: heavy hard growl
                AssignOrcClip(attack2Prop, orcClips, "Attack", "Vocalfry", 1);       // Attack 2: vocalfry effort
                AssignOrcClip(attack3Prop, orcClips, "Attack", "Growl_Hard", 2);     // Attack 3: heavy hard growl
                AssignOrcClip(attack4Prop, orcClips, "Attack", "Vocalfry", 3);       // Attack 4: vocalfry effort
                AssignOrcClip(attackImpactProp, orcClips, "Grunt", "Growl_Hard", 0); // Impact: hard grunt
                AssignOrcClip(getHitProp, orcClips, "Hurt", "Vocalfry", 0);          // Hit: raspy vocal pain grunt
                AssignOrcClip(deathProp, orcClips, "Death", "Growl_Hard", 0);        // Death: intense growl death

                if (volumeProp != null) volumeProp.floatValue = 0.85f;
                if (pitchVarProp != null) pitchVarProp.floatValue = 0.06f;

                serializedSfx.ApplyModifiedProperties();
                EditorUtility.SetDirty(sfxProfile);
                PrefabUtility.RecordPrefabInstancePropertyModifications(sfxProfile);

                enemiesWiredCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.LogFormat("[AudioWiringUtility] Success! Wired {0} abilities across classes and {1} enemy voice profiles.", spellsWiredCount, enemiesWiredCount);
        }

        private static Dictionary<string, List<AudioClip>> LoadSpellClips()
        {
            var results = new Dictionary<string, List<AudioClip>>();
            string[] subfolders = { "Frost", "Fire", "Lightning", "Dark", "Nature", "Arcane", "General Spells and Effects" };

            foreach (var sf in subfolders)
            {
                string dirPath = Path.Combine(SpellSfxRoot, sf);
                if (!Directory.Exists(dirPath)) continue;

                var list = new List<AudioClip>();
                string[] files = Directory.GetFiles(dirPath, "*.wav", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(f);
                    if (clip != null) list.Add(clip);
                }

                // Map logical categories to our loaded list
                if (sf == "General Spells and Effects")
                    results["General"] = list;
                else
                    results[sf] = list;
            }

            return results;
        }

        private static Dictionary<string, List<AudioClip>> LoadOrcClips()
        {
            var results = new Dictionary<string, List<AudioClip>>();
            if (!Directory.Exists(OrcMiscRoot)) return results;

            string[] files = Directory.GetFiles(OrcMiscRoot, "*.wav");
            foreach (var f in files)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(f);
                if (clip == null) continue;

                string filename = Path.GetFileNameWithoutExtension(f); // e.g. "Orc_Attack1_Growl_Hard"
                // Split by underscore to find event type and voice style
                string[] parts = filename.Split('_');
                if (parts.Length < 3) continue;

                // parts[1] contains event (Attack1, Death2, Hurt1, Growl1, Grunt1)
                // parts[2] + parts[3] (or parts[2]) contains Growl_Hard or Vocalfry
                string eventType = parts[1].ToLower();
                string style = filename.Contains("Vocalfry") ? "Vocalfry" : "Growl_Hard";

                // Group logically into broad categories
                string category = "General";
                if (eventType.Contains("attack")) category = "Attack";
                else if (eventType.Contains("death")) category = "Death";
                else if (eventType.Contains("hurt")) category = "Hurt";
                else if (eventType.Contains("growl")) category = "Growl";
                else if (eventType.Contains("grunt")) category = "Grunt";

                string key = category + "_" + style;
                if (!results.ContainsKey(key))
                {
                    results[key] = new List<AudioClip>();
                }
                results[key].Add(clip);
            }

            return results;
        }

        private static void AssignOrcClip(SerializedProperty prop, Dictionary<string, List<AudioClip>> orcClips, string category, string style, int fallbackIdx)
        {
            if (prop == null) return;
            string key = category + "_" + style;
            if (orcClips.TryGetValue(key, out List<AudioClip> clips) && clips.Count > 0)
            {
                prop.objectReferenceValue = clips[fallbackIdx % clips.Count];
            }
            else
            {
                // General fallback
                string altKey = category + "_" + (style == "Vocalfry" ? "Growl_Hard" : "Vocalfry");
                if (orcClips.TryGetValue(altKey, out List<AudioClip> altClips) && altClips.Count > 0)
                {
                    prop.objectReferenceValue = altClips[fallbackIdx % altClips.Count];
                }
            }
        }

        private static string DetermineCategory(string spellName)
        {
            string lower = spellName.ToLower();
            if (lower.Contains("ice") || lower.Contains("frost") || lower.Contains("glacial") || lower.Contains("freeze") || lower.Contains("chill") || lower.Contains("blizzard") || lower.Contains("rend"))
                return "Frost";
            if (lower.Contains("fire") || lower.Contains("ember") || lower.Contains("burn") || lower.Contains("scorch") || lower.Contains("conflagration") || lower.Contains("heat") || lower.Contains("meteor") || lower.Contains("conflag"))
                return "Fire";
            if (lower.Contains("lightning") || lower.Contains("storm") || lower.Contains("thunder") || lower.Contains("lash") || lower.Contains("electric") || lower.Contains("shock") || lower.Contains("spark") || lower.Contains("volt"))
                return "Lightning";
            if (lower.Contains("dark") || lower.Contains("void") || lower.Contains("maw") || lower.Contains("shadow") || lower.Contains("collapse") || lower.Contains("soul") || lower.Contains("mind") || lower.Contains("spike") || lower.Contains("silence"))
                return "Dark";
            if (lower.Contains("thorn") || lower.Contains("vine") || lower.Contains("earth") || lower.Contains("grasp") || lower.Contains("nature") || lower.Contains("forest") || lower.Contains("spirit") || lower.Contains("wisp"))
                return "Nature";
            if (lower.Contains("heal") || lower.Contains("mend") || lower.Contains("grace") || lower.Contains("divine") || lower.Contains("aegis") || lower.Contains("sacred") || lower.Contains("runic") || lower.Contains("rune") || lower.Contains("ward") || lower.Contains("holy") || lower.Contains("mending") || lower.Contains("shield") || lower.Contains("circle"))
                return "General";

            return "Arcane";
        }
    }
}
#endif
