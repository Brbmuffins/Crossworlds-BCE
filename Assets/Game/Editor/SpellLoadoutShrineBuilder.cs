#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Crossworlds.EditorTools
{
    public static class SpellLoadoutShrineBuilder
    {
        [MenuItem(
            "BCE/Build/Spell Loadout Shrine",
            priority = 44)]
        public static void CreateShrine()
        {
            var root = new GameObject("Spell Loadout Shrine");
            Undo.RegisterCreatedObjectUndo(
                root,
                "Create Spell Loadout Shrine");

            Vector3 position = Vector3.zero;
            if (Selection.activeTransform != null)
                position = Selection.activeTransform.position;
            else if (SceneView.lastActiveSceneView != null)
                position = SceneView.lastActiveSceneView.pivot;
            root.transform.position = position;

            SpellLoadoutShrine shrine =
                Undo.AddComponent<SpellLoadoutShrine>(root);
            shrine.interactionRadius = 3f;
            shrine.shrineName = "Spell Shrine";

            SphereCollider trigger =
                Undo.AddComponent<SphereCollider>(root);
            trigger.isTrigger = true;
            trigger.radius = shrine.interactionRadius;

            CreatePrimitive(
                root.transform,
                "Base",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.15f, 0f),
                new Vector3(1.4f, 0.15f, 1.4f));
            CreatePrimitive(
                root.transform,
                "Pedestal",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.85f, 0f),
                new Vector3(0.55f, 0.7f, 0.55f));
            GameObject focus = CreatePrimitive(
                root.transform,
                "Arcane Focus",
                PrimitiveType.Sphere,
                new Vector3(0f, 1.75f, 0f),
                Vector3.one * 0.65f);

            Light glow = Undo.AddComponent<Light>(focus);
            glow.type = LightType.Point;
            glow.color = new Color(0.55f, 0.20f, 1f);
            glow.intensity = 2.5f;
            glow.range = 4f;

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            EditorSceneManager.MarkSceneDirty(root.scene);
        }

        static GameObject CreatePrimitive(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject child =
                GameObject.CreatePrimitive(primitiveType);
            child.name = name;
            Undo.RegisterCreatedObjectUndo(
                child,
                "Create Spell Loadout Shrine");
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = localScale;

            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
                Undo.DestroyObjectImmediate(collider);

            return child;
        }
    }
}
#endif
