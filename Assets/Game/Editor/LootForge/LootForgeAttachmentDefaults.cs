#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.LootForge
{
    /// <summary>Creates and resolves the shared attachment profiles used by ordinary equipment.</summary>
    static class LootForgeAttachmentDefaults
    {
        const string Folder = "Assets/Game/Resources/LootForge/Attachment Profiles/Defaults";

        public static EquipmentAttachmentProfile GetOrCreate(
            LootEquipmentSlot slot, bool twoHanded)
        {
            if (slot == LootEquipmentSlot.None || slot == LootEquipmentSlot.Ring ||
                slot == LootEquipmentSlot.Trinket)
                return null;

            string profileName = slot == LootEquipmentSlot.MainHand
                ? (twoHanded ? "Two-Handed Weapon" : "One-Handed Weapon")
                : slot == LootEquipmentSlot.OffHand ? "Offhand"
                : slot + " Armor";
            string path = $"{Folder}/{profileName}.asset";
            EquipmentAttachmentProfile profile =
                AssetDatabase.LoadAssetAtPath<EquipmentAttachmentProfile>(path);
            if (profile != null) return profile;

            EnsureFolder(Folder);
            profile = ScriptableObject.CreateInstance<EquipmentAttachmentProfile>();
            profile.profileName = profileName;
            profile.attachmentBoneName = SocketName(slot, twoHanded);
            profile.localPosition = Vector3.zero;
            profile.localEulerAngles = Vector3.zero;
            profile.localScale = Vector3.one;
            profile.twoHanded = slot == LootEquipmentSlot.MainHand && twoHanded;
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            return profile;
        }

        public static bool IsDefault(EquipmentAttachmentProfile profile)
        {
            string path = profile != null ? AssetDatabase.GetAssetPath(profile) : "";
            return path.StartsWith(Folder + "/");
        }

        static string SocketName(LootEquipmentSlot slot, bool twoHanded) => slot switch
        {
            LootEquipmentSlot.MainHand => twoHanded ? "TwoHandSocket" : "RightHandSocket",
            LootEquipmentSlot.OffHand => "LeftHandSocket",
            LootEquipmentSlot.Head => "HeadSocket",
            LootEquipmentSlot.Chest => "ChestSocket",
            LootEquipmentSlot.Hands => "HandsSocket",
            LootEquipmentSlot.Legs => "LegsSocket",
            LootEquipmentSlot.Feet => "FeetSocket",
            _ => ""
        };

        static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
