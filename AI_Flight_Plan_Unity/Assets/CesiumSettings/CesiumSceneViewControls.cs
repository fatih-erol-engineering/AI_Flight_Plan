// using UnityEngine;
// using UnityEngine.EventSystems;
// using CesiumForUnity;
// using Unity.Mathematics;

// [RequireComponent(typeof(Camera))]
// public class CesiumSceneViewControls : MonoBehaviour
// {
//     [Header("Bindings")]
//     public CesiumCameraController controller;   // Dynamic Camera�daki
//     public CesiumGlobeAnchor globeAnchor;       // Dynamic Camera�daki

//     [Header("Mouse Mappings")]
//     public KeyCode panMouse = KeyCode.Mouse2; // MMB
//     public KeyCode orbitMouse = KeyCode.Mouse1; // RMB

//     [Header("Pan")]
//     public float panSensitivity = 1.0f;   // 1 = do�al; daha h�zl�/yava� i�in ayarla
//     public bool invertPan = false;        // harita hissi i�in genelde false
//     public bool clampLatLon = true;       // kutup/meridyen sapmalar�n� engelle

//     [Header("Zoom")]
//     public bool zoomEnabled = true;
//     public float zoomSpeed = 1.2f;        // >1 log �l�ek (1.1�1.4 iyi)
//     public double minHeight = 5.0;        // metre
//     public double maxHeight = 2_000_000;  // metre (2.000 km)

//     Camera _cam;
//     bool _panning;
//     Vector2 _lastMouse;
//     double3 _lonLatH;                     // (lon, lat, height)

//     const double EarthRadius = 6378137.0; // WGS84 (yakla��k)

//     void Awake()
//     {
//         _cam = GetComponent<Camera>();
//         if (!controller) controller = GetComponent<CesiumCameraController>();
//         if (!globeAnchor) globeAnchor = GetComponent<CesiumGlobeAnchor>();
//         _lonLatH = globeAnchor ? globeAnchor.longitudeLatitudeHeight : new double3(29.0, 41.0, 100.0); // �stanbul civar� default
//     }

//     void Update()
//     {
//         if (!globeAnchor) return;

//         // UI �zerinde iken kontrolleri kapat
//         if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//         {
//             SetOrbit(false);
//             _panning = false;
//             return;
//         }

//         HandleOrbit();
//         HandlePan();
//         if (zoomEnabled) HandleZoom();
//     }

//     void HandleOrbit()
//     {
//         if (Input.GetKeyDown(orbitMouse)) SetOrbit(true);
//         else if (Input.GetKeyUp(orbitMouse)) SetOrbit(false);
//     }

//     void SetOrbit(bool on)
//     {
//         if (controller) controller.enableRotation = on;
//     }

//     void HandlePan()
//     {
//         if (Input.GetKeyDown(panMouse))
//         {
//             _panning = true;
//             _lastMouse = Input.mousePosition;
//             _lonLatH = globeAnchor.longitudeLatitudeHeight;
//             SetOrbit(false); // pan s�ras�nda orbit kapans�n
//         }
//         else if (Input.GetKeyUp(panMouse))
//         {
//             _panning = false;
//         }

//         if (!_panning) return;

//         Vector2 now = Input.mousePosition;
//         Vector2 dPix = now - _lastMouse;
//         _lastMouse = now;

//         if (dPix.sqrMagnitude < 0.0001f) return;

//         double h = math.max(1.0, globeAnchor.longitudeLatitudeHeight.z);
//         double metersPerPixel = (2.0 * h * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad)) / Screen.height;

//         double sx = dPix.x * metersPerPixel * panSensitivity;
//         double sy = dPix.y * metersPerPixel * panSensitivity;

//         if (!invertPan) { sx = -sx; sy = -sy; }

//         double latRad = _lonLatH.y * Mathf.Deg2Rad;
//         double metersPerDegLat = (Mathf.PI / 180.0) * EarthRadius;
//         double metersPerDegLon = metersPerDegLat * Mathf.Cos((float)latRad);

//         if (metersPerDegLon < 1e-6) metersPerDegLon = 1e-6; // kutuplarda patlamas�n

//         double dLatDeg = sy / metersPerDegLat;
//         double dLonDeg = sx / metersPerDegLon;

//         _lonLatH.x += dLonDeg; // lon
//         _lonLatH.y += dLatDeg; // lat

//         if (clampLatLon)
//         {
//             // Lon�u [-180,180) aral���na getir
//             _lonLatH.x = WrapLon(_lonLatH.x);
//             // Lat�� [-85,85] gibi makul bir banda k�s
//             _lonLatH.y = math.clamp(_lonLatH.y, -85.0, 85.0);
//         }

//         globeAnchor.longitudeLatitudeHeight = _lonLatH;
//     }

//     void HandleZoom()
//     {
//         float scroll = Input.mouseScrollDelta.y;
//         if (Mathf.Abs(scroll) < 0.0001f) return;

//         var llh = globeAnchor.longitudeLatitudeHeight;
//         double h = math.clamp(llh.z, minHeight, maxHeight);

//         // Logaritmik �l�ek: scroll>0 yak�nla�, <0 uzakla�
//         double factor = System.Math.Pow(zoomSpeed, scroll);
//         double newH = math.clamp(h / factor, minHeight, maxHeight);

//         llh.z = newH;
//         globeAnchor.longitudeLatitudeHeight = llh;
//     }

//     static double WrapLon(double lon)
//     {
//         // [-180,180) band�na sar
//         lon = System.Math.IEEERemainder(lon, 360.0);
//         if (lon < -180.0) lon += 360.0;
//         if (lon >= 180.0) lon -= 360.0;
//         return lon;
//     }
// }
