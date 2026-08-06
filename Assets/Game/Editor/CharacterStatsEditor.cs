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
    SerializedProperty _baseMaxMana;
    SerializedProperty _baseHp5;
    SerializedProperty _baseMp5;
    SerializedProperty _baseMoveSpeedBonusPct;
    SerializedProperty _baseCooldownReduction;
    SerializedProperty _baseHealBonusPct;
    SerializedProperty _primaryStatDamagePctPerPoint;
    SerializedProperty _strengthDamagePctPerPoint;
    SerializedProperty _maxHealthPerVitality;
    SerializedProperty _maxManaPerIntelligence;
    SerializedProperty _moveSpeedPctPerAgility;
    SerializedProperty _effectiveMaxHealthBonus;
    SerializedProperty _effectiveDamageMultiplier;
    SerializedProperty _effectiveCriticalStrikeChance;
    SerializedProperty _effectiveCriticalStrikeDamageMultiplier;
    SerializedProperty _effectiveDamageReduction;
    SerializedProperty _effectiveMaxMana;
    SerializedProperty _currentMana;
    SerializedProperty _effectiveHp5;
    SerializedProperty _effectiveMp5;
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
        _baseMaxMana = serializedObject.FindProperty("baseMaxMana");
        _baseHp5 = serializedObject.FindProperty("baseHp5");
        _baseMp5 = serializedObject.FindProperty("baseMp5");
        _baseMoveSpeedBonusPct = serializedObject.FindProperty("baseMoveSpeedBonusPct");
        _baseCooldownReduction = serializedObject.FindProperty("baseCooldownReduction");
        _baseHealBonusPct = serializedObject.FindProperty("baseHealBonusPct");
        _primaryStatDamagePctPerPoint = serializedObject.FindProperty("primaryStatDamagePctPerPoint");
        _strengthDamagePctPerPoint = serializedObject.FindProperty("strengthDamagePctPerPoint");
        _maxHealthPerVitality = serializedObject.FindProperty("maxHealthPerVitality");
        _maxManaPerIntelligence = serializedObject.FindProperty("maxManaPerIntelligence");
        _moveSpeedPctPerAgility = serializedObject.FindProperty("moveSpeedPctPerAgility");
        _effectiveMaxHealthBonus = serializedObject.FindProperty("effectiveMaxHealthBonus");
        _effectiveDamageMultiplier = serializedObject.FindProperty("effectiveDamageMultiplier");
        _effectiveCriticalStrikeChance = serializedObject.FindProperty("effectiveCriticalStrikeChance");
        _effectiveCriticalStrikeDamageMultiplier = serializedObject.FindProperty("effectiveCriticalStrikeDamageMultiplier");
        _effectiveDamageReduction = serializedObject.FindProperty("effectiveDamageReduction");
        _effectiveMaxMana = serializedObject.FindProperty("effectiveMaxMana");
        _currentMana = serializedObject.FindProperty("currentMana");
        _effectiveHp5 = serializedObject.FindProperty("effectiveHp5");
        _effectiveMp5 = serializedObject.FindProperty("effectiveMp5");
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
        EditorGUILayout.LabelField("Base Resource Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_baseMaxMana, new GUIContent("Max Mana"));
        EditorGUILayout.PropertyField(_baseHp5, new GUIContent("HP5"));
        EditorGUILayout.PropertyField(_baseMp5, new GUIContent("MP5"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Base Utility Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_baseMoveSpeedBonusPct, new GUIContent("Move Speed Bonus"));
        EditorGUILayout.PropertyField(_baseCooldownReduction, new GUIContent("Cooldown Reduction"));
        EditorGUILayout.PropertyField(_baseHealBonusPct, new GUIContent("Heal Bonus"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Level Progression Tuning", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_primaryStatDamagePctPerPoint, new GUIContent("Primary Stat Damage / Point"));
        EditorGUILayout.PropertyField(_strengthDamagePctPerPoint, new GUIContent("Strength Damage / Point"));
        EditorGUILayout.PropertyField(_maxHealthPerVitality, new GUIContent("Max Health / Vitality"));
        EditorGUILayout.PropertyField(_maxManaPerIntelligence, new GUIContent("Max Mana / Intelligence"));
        EditorGUILayout.PropertyField(_moveSpeedPctPerAgility, new GUIContent("Move Speed / Agility"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Runtime Readouts", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_effectiveMaxHealthBonus, new GUIContent("Max Health Bonus"));
        EditorGUILayout.PropertyField(_effectiveDamageMultiplier, new GUIContent("Damage Multiplier"));
        EditorGUILayout.PropertyField(_effectiveCriticalStrikeChance, new GUIContent("Critical Strike Chance"));
        EditorGUILayout.PropertyField(_effectiveCriticalStrikeDamageMultiplier, new GUIContent("Critical Damage Multiplier"));
        EditorGUILayout.PropertyField(_effectiveDamageReduction, new GUIContent("Damage Reduction"));
        EditorGUILayout.PropertyField(_effectiveMaxMana, new GUIContent("Max Mana"));
        EditorGUILayout.PropertyField(_currentMana, new GUIContent("Current Mana"));
        EditorGUILayout.PropertyField(_effectiveHp5, new GUIContent("HP5"));
        EditorGUILayout.PropertyField(_effectiveMp5, new GUIContent("MP5"));
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
