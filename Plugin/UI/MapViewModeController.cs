using BepInEx.Configuration;
using DynamicMaps.Config;
using DynamicMaps.Utils;
using UnityEngine;

namespace DynamicMaps.UI;

internal class MapViewModeController : MonoBehaviour
{
    public ModdedMapScreen MapScreen { get; set; }
    public RectTransform MapScreenTrueParent { get; set; }
    public RectTransform RectTransform { get; private set; }
    public KeyboardShortcut PeekShortcut { get; set; }
    public KeyboardShortcut HideMinimapShortcut { get; set; }
    public bool HoldForPeek { get; set; }  // opposite is peek toggle
    
    private static bool IsMiniMapEnabled => Settings.MiniMapEnabled.Value;
    
    private bool _isPeeking;

    public EMapViewMode PreviousMapViewMode { get; private set; } = EMapViewMode.None;
    public EMapViewMode CurrentMapViewMode { get; private set; } = EMapViewMode.None;
    
    private void Awake()
    {
        RectTransform = gameObject.GetRectTransform();
    }

    private void Update()
    {
        if (!GameUtils.ShouldShowMapInRaid())
        {
            if (CurrentMapViewMode == EMapViewMode.MiniMap)
            {
                EndMiniMap();
            }

            if (CurrentMapViewMode == EMapViewMode.Peek)
            {
                EndPeek();
            }

            return;
        }

        if (!GameUtils.IsInRaid()) return;
        
        HandleMinimapState();
        HandlePeekState();
    }

    private void HandleMinimapState()
    {
        if (!IsMiniMapEnabled)
        {
            if (CurrentMapViewMode == EMapViewMode.MiniMap)
            {
                EndMiniMap();
            }
            
            return;
        }
        
        if (HideMinimapShortcut.BetterIsDown())
        {
            if (CurrentMapViewMode != EMapViewMode.MiniMap && 
                CurrentMapViewMode != EMapViewMode.Peek && 
                CurrentMapViewMode != EMapViewMode.MapScreen)
            {
                BeginMiniMap(false);
            }
            else
            {
                EndMiniMap();
            }

            return;
        }
        
        if (CurrentMapViewMode != EMapViewMode.Peek && CurrentMapViewMode != EMapViewMode.MapScreen)
        {
            BeginMiniMap();
        }
        else
        {
            EndMiniMap();
        }
    }

    private void HandlePeekState()
    {
        if (HoldForPeek && PeekShortcut.BetterIsPressed() != _isPeeking)
        {
            // hold for peek
            if (PeekShortcut.BetterIsPressed())
            {
                EndMiniMap();
                BeginPeek(PreviousMapViewMode == EMapViewMode.MiniMap);
            }
            else
            {
                EndPeek();
            }
        }
        else if (!HoldForPeek && PeekShortcut.BetterIsDown())
        {
            // toggle peek
            if (CurrentMapViewMode != EMapViewMode.Peek)
            {
                EndMiniMap();
                BeginPeek(PreviousMapViewMode == EMapViewMode.MiniMap);
            }
            else
            {
                EndPeek();
            }
        }
    }
    
    internal void ShowMapScreen()
    {
       if (CurrentMapViewMode == EMapViewMode.MapScreen) return;

       if (MapScreen.RememberMapPosition)
       { 
           MapScreen.MapView.SetMapPos(MapScreen.MapView.MainMapPos, 0f);
       }
       
       DynamicMapsPlugin.Log.LogWarning("Showing Map Screen");
       
       transform.parent.Find("MapBlock").gameObject.SetActive(false);
       transform.parent.Find("EmptyBlock").gameObject.SetActive(false);
       transform.parent.gameObject.SetActive(true);

       CurrentMapViewMode = EMapViewMode.MapScreen;
       MapScreen.Show(false);
    }
    
    internal void EndMapScreen()
    {
        if (CurrentMapViewMode != EMapViewMode.MapScreen) return;
        
        MapScreen.Hide();
        CurrentMapViewMode = EMapViewMode.None;
        
        if (PreviousMapViewMode == EMapViewMode.MiniMap)
        {
            BeginMiniMap();
        }

        PreviousMapViewMode = EMapViewMode.MapScreen;
    }
    
    private void BeginPeek(bool playAnimation = true)
    {
        if (CurrentMapViewMode == EMapViewMode.Peek) return;
        
        // just in case something else is attached and tries to be in front
        transform.SetAsLastSibling();
        
        _isPeeking = true;
        CurrentMapViewMode = EMapViewMode.Peek;
        
        // attach map screen to peek mask
        MapScreen.transform.SetParent(RectTransform);
        MapScreen.Show(playAnimation);
    }

    internal void EndPeek()
    {
        if (CurrentMapViewMode != EMapViewMode.Peek) return;
        
        // un-attach map screen and re-attach to true parent
        MapScreen.Hide();
        MapScreen.transform.SetParent(MapScreenTrueParent);

        CurrentMapViewMode = EMapViewMode.None;
        _isPeeking = false;
        
        if (PreviousMapViewMode == EMapViewMode.MiniMap)
        {
            BeginMiniMap();
        }
        
        PreviousMapViewMode = EMapViewMode.Peek;
    }

    private void BeginMiniMap(bool playAnimation = true)
    {
        if (CurrentMapViewMode == EMapViewMode.MiniMap) return;
        
        // just in case something else is attached and tries to be in front
        transform.SetAsLastSibling();
        
        CurrentMapViewMode = EMapViewMode.MiniMap;
        
        MapScreen.transform.SetParent(RectTransform);
        MapScreen.Show(playAnimation);
    }

    internal void EndMiniMap()
    {
        if (CurrentMapViewMode != EMapViewMode.MiniMap) return;
        
        CurrentMapViewMode = EMapViewMode.None;
        
        MapScreen.Hide();
        MapScreen.transform.SetParent(MapScreenTrueParent);
        
        PreviousMapViewMode = EMapViewMode.MiniMap;
    }
}
