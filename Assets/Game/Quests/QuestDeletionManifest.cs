using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quest Forge deletion tombstones. Dedicated servers use this resource to
/// remove deleted authored quests and their persisted player state.
/// </summary>
public sealed class QuestDeletionManifest : ScriptableObject
{
    public List<string> questIds = new();
}
