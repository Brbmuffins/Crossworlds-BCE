#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

/// <summary>Warms client visuals while the loading overlay is opaque.</summary>
public static class ZoneVisualPreloader
{
    public static IEnumerator PrewarmLoadedZone()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        foreach (ShaderVariantCollection collection in
                 Resources.LoadAll<ShaderVariantCollection>("ZoneWarmup"))
            if (collection != null && !collection.isWarmedUp)
                collection.WarmUp();

        Camera sourceCamera = Camera.main;
        GameObject cameraObject = new GameObject("[Zone Warmup Camera]");
        Camera warmupCamera = cameraObject.AddComponent<Camera>();
        if (sourceCamera != null) warmupCamera.CopyFrom(sourceCamera);
        warmupCamera.enabled = false;

        var texture = new RenderTexture(64, 64, 16)
        {
            name = "Zone Warmup Target"
        };
        texture.Create();
        warmupCamera.targetTexture = texture;

        var instances = new List<GameObject>();
        ElementalLightningVFXProfile lightning =
            Resources.Load<ElementalLightningVFXProfile>(
                "EnemyAbilities/ElementalLightning");
        if (lightning != null)
        {
            AddInstance(lightning.castEffect, warmupCamera, instances);
            AddInstance(lightning.handEffect, warmupCamera, instances);
            AddInstance(lightning.spellEffect, warmupCamera, instances);
            AddInstance(lightning.hitEffect, warmupCamera, instances);
        }

        warmupCamera.Render();
        SimulateEffects(instances);
        warmupCamera.Render();
        yield return new WaitForEndOfFrame();

        foreach (GameObject instance in instances)
            if (instance != null) Object.Destroy(instance);
        warmupCamera.targetTexture = null;
        texture.Release();
        Object.Destroy(texture);
        Object.Destroy(cameraObject);

        yield return null;
        yield return new WaitForEndOfFrame();
    }

    static void AddInstance(
        GameObject prefab, Camera camera, List<GameObject> instances)
    {
        if (prefab == null || camera == null) return;
        GameObject instance = Object.Instantiate(prefab);
        instance.name = prefab.name + "_ZoneWarmup";
        instance.transform.position =
            camera.transform.position + camera.transform.forward * 3f;
        instance.transform.rotation = camera.transform.rotation;
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;
        foreach (AudioSource audio in instance.GetComponentsInChildren<AudioSource>(true))
            audio.enabled = false;
        instances.Add(instance);
    }

    static void SimulateEffects(List<GameObject> instances)
    {
        foreach (GameObject instance in instances)
        {
            if (instance == null) continue;
            foreach (ParticleSystem particles in
                     instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                particles.Stop(false,
                    ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Simulate(0.1f, false, true, true);
            }
            foreach (VisualEffect effect in
                     instance.GetComponentsInChildren<VisualEffect>(true))
            {
                effect.Reinit();
                effect.Simulate(0.1f);
            }
        }
    }
}
#endif
