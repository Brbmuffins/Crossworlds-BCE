#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.EnemyForge
{
    [Serializable]
    internal sealed class EnemyForgeContractSnapshot
    {
        public string fingerprint;
        public List<string> fields = new List<string>();
    }

    internal readonly struct EnemyForgeSourceState
    {
        public readonly string fingerprint;
        public readonly string summary;
        public readonly string[] safeChanges;
        public readonly string[] blockingChanges;
        public readonly bool requiresAcknowledgement;
        public bool AuditPassed => blockingChanges == null || blockingChanges.Length == 0;
        public bool HasMissingSources => blockingChanges != null && blockingChanges.Any(change =>
            change.StartsWith("Required source is missing:", StringComparison.Ordinal));
        public bool CanAcknowledge => !HasMissingSources;

        public EnemyForgeSourceState(string fingerprint, string summary, string[] safeChanges,
            string[] blockingChanges, bool requiresAcknowledgement)
        {
            this.fingerprint = fingerprint;
            this.summary = summary;
            this.safeChanges = safeChanges;
            this.blockingChanges = blockingChanges;
            this.requiresAcknowledgement = requiresAcknowledgement;
        }
    }

    internal static class EnemyForgeCombatSourceMonitor
    {
        const string AcceptedSnapshotKey = "Crossworlds.EnemyForge.AcceptedCombatContract.v2";

        static readonly string[] RequiredSources =
        {
            "Assets/Game/Combat/Scripts/EnemyController.cs",
            "Assets/Game/Combat/Scripts/Health.cs",
            "Assets/Game/Combat/Scripts/EnemyHeavyAttack.cs",
            "Assets/Game/Combat/Scripts/DropTable.cs",
            "Assets/Game/Combat/Scripts/EnemyProjectile.cs",
            "Assets/Game/Networking/RodNetworkManager.cs",
        };

        static readonly Type[] ContractTypes =
        {
            typeof(EnemyController), typeof(Health), typeof(EnemyHeavyAttack),
            typeof(DropTable), typeof(EnemyProjectile), typeof(RodNetworkManager),
        };

        public static EnemyForgeSourceState Check()
        {
            var missing = new List<string>();
            string fingerprint = BuildFingerprint(missing);
            var current = CaptureSnapshot(fingerprint);
            string savedJson = EditorPrefs.GetString(AcceptedSnapshotKey, string.Empty);
            if (string.IsNullOrEmpty(savedJson))
            {
                Save(current);
                return ClearState(fingerprint);
            }

            var accepted = JsonUtility.FromJson<EnemyForgeContractSnapshot>(savedJson);
            if (accepted == null || accepted.fields == null)
            {
                Save(current);
                return ClearState(fingerprint);
            }
            if (accepted.fingerprint == fingerprint) return ClearState(fingerprint);

            var oldMap = ToFieldMap(accepted.fields);
            var newMap = ToFieldMap(current.fields);
            var safe = new List<string>();
            var blocking = new List<string>();

            foreach (var pair in newMap)
            {
                if (!oldMap.TryGetValue(pair.Key, out string oldType))
                    safe.Add($"Added {pair.Key} ({pair.Value}); component default will be preserved.");
                else if (oldType != pair.Value)
                    blocking.Add($"Changed {pair.Key}: {oldType} → {pair.Value}.");
            }
            foreach (var pair in oldMap)
                if (!newMap.ContainsKey(pair.Key)) blocking.Add($"Removed {pair.Key} ({pair.Value}).");
            foreach (string path in missing) blocking.Add($"Required source is missing: {path}.");

            if (safe.Count == 0 && blocking.Count == 0)
                safe.Add("Combat implementation changed without altering its serialized authoring contract.");

            string summary = blocking.Count > 0
                ? "Combat requirements changed and the compatibility audit found changes that require an Enemy Forge adapter update. Authoring remains locked."
                : "Combat requirements changed. The compatibility audit passed; review the safe updates and acknowledge them to continue.";
            return new EnemyForgeSourceState(fingerprint, summary, safe.ToArray(), blocking.ToArray(), true);
        }

        public static void Acknowledge(string fingerprint)
        {
            Save(CaptureSnapshot(fingerprint));
        }

        static EnemyForgeSourceState ClearState(string fingerprint) =>
            new EnemyForgeSourceState(fingerprint, string.Empty, Array.Empty<string>(), Array.Empty<string>(), false);

        static EnemyForgeContractSnapshot CaptureSnapshot(string fingerprint)
        {
            var snapshot = new EnemyForgeContractSnapshot { fingerprint = fingerprint };
            foreach (Type type in ContractTypes)
            {
                foreach (FieldInfo field in GetSerializedFields(type))
                    snapshot.fields.Add($"{type.FullName}|{field.Name}|{FriendlyTypeName(field.FieldType)}");
            }
            snapshot.fields.Sort(StringComparer.Ordinal);
            return snapshot;
        }

        static IEnumerable<FieldInfo> GetSerializedFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetFields(flags).Where(field =>
                !field.IsStatic && !field.IsNotSerialized &&
                (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null));
        }

        static Dictionary<string, string> ToFieldMap(IEnumerable<string> entries)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string entry in entries)
            {
                string[] parts = entry.Split('|');
                if (parts.Length == 3) result[parts[0] + "." + parts[1]] = parts[2];
            }
            return result;
        }

        static string BuildFingerprint(List<string> missing)
        {
            var signature = new StringBuilder();
            foreach (string path in RequiredSources)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                string hash = string.IsNullOrEmpty(guid) ? "missing" : AssetDatabase.GetAssetDependencyHash(path).ToString();
                signature.Append(path).Append('|').Append(guid).Append('|').Append(hash).Append('\n');
                if (string.IsNullOrEmpty(guid)) missing.Add(path);
            }
            using (var algorithm = SHA256.Create())
            {
                byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(signature.ToString()));
                var result = new StringBuilder(bytes.Length * 2);
                foreach (byte item in bytes) result.Append(item.ToString("x2"));
                return result.ToString();
            }
        }

        static string FriendlyTypeName(Type type)
        {
            if (type.IsArray) return FriendlyTypeName(type.GetElementType()) + "[]";
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition().FullName + "<" +
                    string.Join(",", type.GetGenericArguments().Select(FriendlyTypeName)) + ">";
            return type.FullName ?? type.Name;
        }

        static void Save(EnemyForgeContractSnapshot snapshot)
        {
            EditorPrefs.SetString(AcceptedSnapshotKey, JsonUtility.ToJson(snapshot));
        }
    }
}
#endif
