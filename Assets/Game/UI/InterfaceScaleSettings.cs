using System;
using UnityEngine;

/// <summary>
/// Shared client-side scale for the primary HUD and player windows.
/// Kept separate from CanvasScaler so anchors, resolution handling, and dragging
/// continue to operate in their original coordinate space.
/// </summary>
public static class InterfaceScaleSettings
{
    public const string PreferenceKey = "InterfaceScale";
    public const float Minimum = 0.60f;
    public const float Maximum = 1.00f;
    public const float Default = 1.00f;

    public static event Action<float> Changed;

    public static float Scale => Mathf.Clamp(
        PlayerPrefs.GetFloat(PreferenceKey, Default), Minimum, Maximum);

    public static void SetScale(float value)
    {
        value = Mathf.Clamp(value, Minimum, Maximum);
        PlayerPrefs.SetFloat(PreferenceKey, value);
        PlayerPrefs.Save();
        Changed?.Invoke(value);
    }
}
