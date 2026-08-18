#if UNITY_EDITOR || !UNITY_SERVER
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class VendorHoverUI : MonoBehaviour
{
    TextMeshProUGUI _label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("[VendorHoverUI]");
        DontDestroyOnLoad(go);
        go.AddComponent<VendorHoverUI>();
    }

    void Awake()
    {
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(transform, false);
        var canvas=canvasGo.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=116;
        var scaler=canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080);
        var bg=new GameObject("VendorName",typeof(RectTransform),typeof(Image)); bg.transform.SetParent(canvasGo.transform,false);
        var r=bg.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(.5f,.82f); r.sizeDelta=new Vector2(360,42); bg.GetComponent<Image>().color=new Color(0,0,0,.62f);
        var labelGo=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI)); labelGo.transform.SetParent(bg.transform,false);
        var lr=labelGo.GetComponent<RectTransform>(); lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one; lr.offsetMin=lr.offsetMax=Vector2.zero;
        _label=labelGo.GetComponent<TextMeshProUGUI>(); _label.fontSize=20; _label.fontStyle=FontStyles.Bold; _label.color=new Color(1f,.82f,.3f); _label.alignment=TextAlignmentOptions.Center; _label.raycastTarget=false;
        bg.SetActive(false);
    }

    void Update()
    {
        GameObject panel=_label.transform.parent.gameObject;
        if (Camera.main==null || NetworkClient.localPlayer==null || UnityEngine.InputSystem.Mouse.current==null || (EventSystem.current!=null && EventSystem.current.IsPointerOverGameObject())) { panel.SetActive(false); return; }
        Ray ray=Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        if (ZonePhysics.Raycast(NetworkClient.localPlayer.gameObject, ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Collide))
        {
            NetworkVendor vendor=hit.collider.GetComponentInParent<NetworkVendor>();
            if (vendor!=null && vendor.isClient) { _label.text=vendor.DisplayName; panel.SetActive(true); return; }
        }
        panel.SetActive(false);
    }
}
#endif
