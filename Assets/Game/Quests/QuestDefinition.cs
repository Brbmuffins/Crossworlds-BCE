using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestObjectiveType { KillEnemy, CollectItem, InteractWithObject, EnterArea }

[Serializable]
public sealed class QuestObjectiveDefinition
{
    public QuestObjectiveType type;
    [Tooltip("Enemy template ID, inventory item ID, object ID, or area ID.")]
    public string targetId;
    public string description;
    [Min(1)] public int requiredAmount = 1;
    [Tooltip("Optional authoring reference. Runtime matching always uses Target ID.")]
    public GameObject targetPrefab;
}

[CreateAssetMenu(menuName = "Crossworlds/Quests/Quest Definition", fileName = "Quest_New")]
public sealed class QuestDefinition : ScriptableObject
{
    [Header("Identity")]
    public string questId = "quest_new";
    public string title = "New Quest";
    [TextArea(2, 5)] public string description;
    public bool objectivesMustBeCompletedInOrder;

    [Header("Dialogue")]
    [TextArea(2, 5)] public string offerText = "I have a task for you.";
    [TextArea(2, 5)] public string activeText = "How goes the task?";
    [TextArea(2, 5)] public string completionText = "Well done.";

    [Header("World References")]
    public GameObject questGiverPrefab;
    public GameObject turnInPrefab;

    [Header("Objectives")]
    public List<QuestObjectiveDefinition> objectives = new();

    [Header("Rewards")]
    [Min(0)] public int goldReward;
    [Min(0)] public int experienceReward;
    public string itemRewardId;
    [Min(0)] public int itemRewardQuantity;
}
