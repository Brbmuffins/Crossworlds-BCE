using System;
using UnityEngine;

[Serializable]
public sealed class VendorStockEntry
{
    public LootItemDefinition item;
    [Min(1)] public int buyPrice = 1;
}

[CreateAssetMenu(fileName = "VendorProfile", menuName = "BCE/Vendor Profile")]
public sealed class VendorProfile : ScriptableObject
{
    public string vendorId = "general_goods";
    public string displayName = "Merchant";
    public string subtitle = "";
    public bool buysItems = true;
    [Range(0f, 20f)] public float interactionRadius = 3f;
    public VendorStockEntry[] stock = Array.Empty<VendorStockEntry>();
}
