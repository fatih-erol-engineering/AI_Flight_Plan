using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;


public class AircraftPreShow: Aircraft
{
    public Material preShowMaterial;    
    public override void UpdateColor()
    {
        aircraftMeshRenderer.material = preShowMaterial;
    }


}

