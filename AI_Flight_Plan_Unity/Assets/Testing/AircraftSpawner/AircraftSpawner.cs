// using System.Collections.Generic;
// using UnityEngine;

// // [ExecuteAlways]
// public class AircraftSpawner : Spawner
// {
//     [SerializeField] private GameObject[] aircraftPrefabList;
//     [SerializeField] private GameObject selectedAircraftToSpawn;
//     [HideInInspector] public Aircraft spawnedAircraft;
//     [SerializeField] private List<Aircraft> spawnedAircraftList;
//     void OnValidate()
//     {
//         Clear();
//     }

//     // public new void Spawn()
//     // {
//     //     base.Spawn();
//     //     spawnedAircraft = spawnedObject.GetComponent<Aircraft>();
//     //     spawnedAircraftList.Add(spawnedAircraft);
//     // }

//     public new void CancelSpawning()
//     {
//         base.Cancel();
//         Clear();
//     }
//     public void Clear()
//     {
//         foreach (Aircraft aircraft in spawnedAircraftList)
//         {
//             Destroy(aircraft.gameObject);
//         }
//         spawnedAircraftList.Clear();
//     }

//     public override void SetActivePreviewMode(bool isPreview)
//     {
//         if (isPreview)
//         {
//             spawnedAircraft.SetHighlightEdgeColor(ThemeManager.Instance.theme.Preview);
//             spawnedAircraft.SetHighlightEdgeWidth(1f);
//             spawnedAircraft.SetHighlightMeshRendererEnabled(true);
//         }
//         else
//         {
//             spawnedAircraft.SetHighlightMeshRendererEnabled(false);
//         }
//         Debug.Log("SetObjectPreview in AircraftSpawner base class called.");
//     }
//     public void SelectAircraftToSpawn(GameObject aircraftPrefab)
//     {
//         selectedAircraftToSpawn = aircraftPrefab;
//         // spawned = selectedAircraftToSpawn;
//     }

// }
