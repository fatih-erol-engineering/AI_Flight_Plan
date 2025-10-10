using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class CreateModeController : MonoBehaviour, IGameController
{    
    public UIManager uIManager { get; private set; }

    [SerializeField]
    private AircraftFactory aircraftFactory;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask pickMask = ~0;   // seçilebilir katmanlar

    private class StateHooks
    {
        public Action Enter;
        public Action Tick;
        public Action Apply;
        public Action Exit;
        public Action Undo;
        public Action Redo;
    }
    
    private Dictionary<CreateMode, StateHooks> _states;
    
    public CreateMode currentMode { get; private set; }
    private StateHooks currentStateHook;




    public void Starter()
    {
        if (!cam) cam = Camera.main;
        ConfigureStates();
        SetMode(CreateMode.CreateAircraft);
    }
    public bool Updater()
    {
        bool isExited = false;
        currentStateHook?.Tick?.Invoke();

        // 2) ESC ile interrupt
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentStateHook?.Exit?.Invoke();       
            SetMode(CreateMode.None);
            isExited = true; 
        }

        return isExited;
    }




    private void ConfigureStates()
    {
        _states = new()
        {
            [CreateMode.None] = new StateHooks
            {
                Enter   = () => { },
                Tick    = () => { },
                Apply   = () => { },
                Exit    = () => { },
                Undo    = () => { },
                Redo    = () => { },
            },
            [CreateMode.CreateAircraft] = new StateHooks
            {
                Enter = CreateAircraftStart,
                Tick = CreateAircraftTick,
                Apply = CreateAircraftApply,
                Exit = CreateAircraftExit,
                Undo = () => { },
                Redo = () => { },
            },
            [CreateMode.CreateWaypoints] = new StateHooks
            {
                Enter = () => { },
                Tick = () => { },
                Apply = () => { },
                Exit = () => { },
                Undo = () => { },
                Redo = () => { },
            }
        };
    }
    public void SetMode(CreateMode next)
    {
        if (next == currentMode) return;

        // Exit eski moda ait
        currentStateHook?.Apply?.Invoke();

        currentMode = next;
        currentStateHook = _states.TryGetValue(next, out var hooks) ? hooks : null;

        // Enter yeni moda ait
        currentStateHook?.Enter?.Invoke();
    }







    private bool GetMouseHitPosition(out Vector3 mouseHitPosition)
    {
        mouseHitPosition = default;
        bool isSuccessful = false;
        // UI üzerindeyse tıklamayı alma (opsiyonel)
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            isSuccessful = false;            
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float maxDist = cam ? cam.farClipPlane : 1000f;
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, pickMask))
        {
            mouseHitPosition = hit.point;
            isSuccessful = true;
        }
        return isSuccessful;
    }

    private void CreateAircraftStart() 
    {
        if (aircraftFactory.aircraftPreShow == null)
        {
            aircraftFactory.SpawnForPreShow();
        }
    }
    private void CreateAircraftTick()
    {
        Vector3 mouseHitPosition = Vector3.zero;
        if (GetMouseHitPosition(out mouseHitPosition))
        {
            Vector3 clearance = new Vector3(0f, 1f, 0f);
            aircraftFactory.aircraftPreShow.aircraftVisualObject.transform.position = mouseHitPosition + clearance;
        }

        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentStateHook?.Exit?.Invoke();
            SetMode(CreateMode.None);            
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
        {
            currentStateHook?.Apply?.Invoke();
            SetMode(CreateMode.None);            
        }
    }
    private void CreateAircraftApply()
    {
        aircraftFactory.DeleteAircraftPreShow();
        Vector3 mouseHitPosition = Vector3.zero;
        if (GetMouseHitPosition(out mouseHitPosition))
        {
            Vector3 clearance = new Vector3(0f,1f,0f);
            aircraftFactory.Spawn(aircraftFactory.aircraftSpecToSpawn.model,mouseHitPosition+ clearance, Quaternion.identity);            
        }
        else 
        {             
            aircraftFactory.Spawn();
        }
    }
    private void CreateAircraftExit() 
    {
        aircraftFactory.DeleteAircraftPreShow();
    }       




}
public enum CreateMode
{
    None,
    CreateAircraft,
    CreateWaypoints,        
}