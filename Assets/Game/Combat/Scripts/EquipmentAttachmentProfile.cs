using System;
using UnityEngine;

[Serializable]
public sealed class EquipmentAttachmentClassOverride
{
    [Range(0, 4)] public int classIndex;
    public string attachmentBoneName;
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public Vector3 localScale = Vector3.one;
}

/// <summary>Reusable skeleton attachment defaults shared by a family of equipment models.</summary>
[CreateAssetMenu(fileName = "EquipmentAttachmentProfile", menuName = "BCE/Equipment Attachment Profile")]
public sealed class EquipmentAttachmentProfile : ScriptableObject
{
    public string profileName;
    [Tooltip("Default skeleton transform used by this equipment family.")]
    public string attachmentBoneName;
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public Vector3 localScale = Vector3.one;
    [Tooltip("Optional class-specific transforms. Missing classes use the shared defaults above.")]
    public EquipmentAttachmentClassOverride[] classOverrides = Array.Empty<EquipmentAttachmentClassOverride>();
    [Tooltip("Reserves both hands while this main-hand item is equipped.")]
    public bool twoHanded;

    public EquipmentAttachmentClassOverride FindClassOverride(int classIndex)
    {
        if (classOverrides == null) return null;
        foreach (EquipmentAttachmentClassOverride value in classOverrides)
            if (value != null && value.classIndex == classIndex) return value;
        return null;
    }

    public EquipmentAttachmentClassOverride GetOrCreateClassOverride(int classIndex)
    {
        EquipmentAttachmentClassOverride existing = FindClassOverride(classIndex);
        if (existing != null) return existing;
        var created = new EquipmentAttachmentClassOverride
        {
            classIndex = classIndex,
            attachmentBoneName = attachmentBoneName,
            localPosition = localPosition,
            localEulerAngles = localEulerAngles,
            localScale = localScale.sqrMagnitude > 0.0001f ? localScale : Vector3.one
        };
        int length = classOverrides?.Length ?? 0;
        Array.Resize(ref classOverrides, length + 1);
        classOverrides[length] = created;
        return created;
    }
}
