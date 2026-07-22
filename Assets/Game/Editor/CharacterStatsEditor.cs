#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterStats))]
public class CharacterStatsEditor : Editor
{
    SerializedProperty _baseMaxHealthBonus;
    SerializedProperty _baseDamageBonusPct;
    SerializedProperty _baseCriticalStrikeChance;
    SerializedProperty _baseCriticalStrikeDamageMultiplier;
    SerializedProperty _baseDamageReduction;
    SerializedProperty _baseMoveSpeedBonusPct;
    SerializedProperty _baseCooldownReduction;
    SerializedProperty _baseHealBonusPct;
    SerializedProperty _effectiveMaxHealthBonus;
    SerializedProperty _effectiveDamageMultiplier;
    SerializedProperty _effectiveCriticalStrikeChance;
    SerializedProperty _effectiveCriticalStrikeDamageMultiplier;
    SerializedProperty _effectiveDamageReduction;
    SerializedProperty _effectiveMoveSpeedMultiplier;
    SerializedProperty _effectiveCooldownReduction;
    SerializedProperty _effectiveHealMultiplier;

    void OnEnable()
    {
        _baseMaxHealthBonus = serializedObject.FindProperty("baseMaxHealthBonus");
        _baseDamageBonusPct = serializedObject.FindProperty("baseDamageBonusPct");
        _baseCriticalStrikeChance = serializedObject.FindProperty("baseCriticalStrikeChance");
        _baseCriticalStrikeDamageMultiplier = serializedObject.FindProperty("baseCriticalStrikeDamageMultiplier");
        _baseDamageReduction = serializedObject.FindProperty("baseDamageReduction");
        _baseMoveSpeedBonusPct = serializedObject.FindProperty("baseMoveSpeedBonusPct");
        _baseCooldownReduction = serializedObject.FindProperty("baseCooldownReduction");
        _baseHealBonusPct = serializedObject.FindProperty("baseHealBonusPct");
        _effectiveMaxHealthBonus = serializedObject.FindProperty("effectiveMaxHealthBonus");
        _effectiveDamageMultiplier = serializedObject.FindProperty("effectiveDamageMultiplier");
        _effectiveCriticalStrikeChance = serializedObject.FindProperty("effectiveCriticalStrikeChance");
        _effectiveCriticalStrikeDamageMultiplier = serializedObject.FindProperty("effectiveCriticalStrikeDamageMultiplier");
        _effectiveDamageReduction = serializedObject.FindProperty("effectiveDamageReduction");
        _effectiveMoveSpeedMultiplier = serializedObject.FindProperty("effectiveMoveSpeedMultiplier");
        _effectiveCooldownReduction = serializedObject.FindProperty("effectiveCooldownReduction");
        _effectiveHealMultiplier = serializedObject.FindProperty("effectiveHealMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Percent fields use decimal values: 0.10 = 10%.", MessageType.Info);

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Base Combat Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_baseMaxHealthBonus, new GUIContent("Max Health Bonus"));
        EditorGUILayout.PropertyField(_baseDamageBonusPct, new GUIContent("Damage Bonus"));
        EditorGUILayout.PropertyField(_baseCriticalStrikeChance, new GUIContent("Critical Strike Chance"));
        EditorGUILayout.PropertyField(_baseCriticalStrikeDamageMultiplier, new GUIContent("Critical Damage Multiplier"));
        EditorGUILayout.PropertyField(_baseDamageReduction, new GUIContent("Damage Reduction"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Base Utility Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_baseMoveSpeedBonusPct, new GUIContent("Move Speed Bonus"));
        EditorGUILayout.PropertyField(_baseCooldownReduction, new GUIContent("Cooldown Reduction"));
        EditorGUILayout.PropertyField(_baseHealBonusPct, new GUIContent("Heal Bonus"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Runtime Readouts", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_effectiveMaxHealthBonus, new GUIContent("Max Health Bonus"));
        EditorGUILayout.PropertyField(_effectiveDamageMultiplier, new GUIContent("Damage Multiplier"));
        EditorGUILayout.PropertyField(_effectiveCriticalStrikeChance, new GUIContent("Critical Strike Chance"));
        EditorGUILayout.PropertyField(_effectiveCriticalStrikeDamageMultiplier, new GUIContent("Critical Damage Multiplier"));
        EditorGUILayout.PropertyField(_effectiveDamageReduction, new GUIContent("Damage Reduction"));
        EditorGUILayout.PropertyField(_effectiveMoveSpeedMultiplier, new GUIContent("Move Speed Multiplier"));
        EditorGUILayout.PropertyField(_effectiveCooldownReduction, new GUIContent("Cooldown Reduction"));
        EditorGUILayout.PropertyField(_effectiveHealMultiplier, new GUIContent("Heal Multiplier"));
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying && GUILayout.Button("Recalculate Now"))
        {
            foreach (UnityEngine.Object selected in targets)
                ((CharacterStats)selected).Recalculate();
        }
    }
}
#endif
