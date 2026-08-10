using UnityEngine;

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
    [Tooltip("Reserves both hands while this main-hand item is equipped.")]
    public bool twoHanded;
}
