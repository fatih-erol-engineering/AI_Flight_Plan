using System.Reflection;
using UnityEngine;
using CesiumForUnity; // paket ismine göre import et


public class PrintCesiumCachePath : MonoBehaviour
{
    void Awake()
    {
        // CesiumForUnity sürümleri arasında alan adı değişiklikleri olabilir.
        // Reflection ile güvenli şekilde uygun static property/field aranır.
        string cachePath = TryGetStaticStringMember(typeof(CesiumRuntimeSettings), "IonAssetCachePath");
        if (string.IsNullOrEmpty(cachePath))
            cachePath = TryGetStaticStringMember(typeof(CesiumRuntimeSettings), "defaultIonAssetCachePath");

        // Eğer hâlâ bulunamadıysa, aramada daha gevşek bir kurala geç
        if (string.IsNullOrEmpty(cachePath))
        {
            cachePath = TryFindStaticStringMemberByKeywords(typeof(CesiumRuntimeSettings), new[] { "ion", "cache", "path" });
        }

        if (!string.IsNullOrEmpty(cachePath))
        {
            Debug.Log("Cesium cache path: " + cachePath);
        }
        else
        {
            Debug.LogWarning("Could not find Cesium cache path property/field on CesiumRuntimeSettings. Make sure the API name matches or update this helper.");
        }
    }

    private static string TryGetStaticStringMember(System.Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;

        var prop = type.GetProperty(name, flags);
        if (prop != null && prop.PropertyType == typeof(string))
        {
            var val = prop.GetValue(null) as string;
            return val;
        }

        var field = type.GetField(name, flags);
        if (field != null && field.FieldType == typeof(string))
        {
            var val = field.GetValue(null) as string;
            return val;
        }

        return null;
    }

    private static string TryFindStaticStringMemberByKeywords(System.Type type, string[] keywords)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;

        // search properties
        foreach (var prop in type.GetProperties(flags))
        {
            if (prop.PropertyType != typeof(string)) continue;
            var name = prop.Name.ToLowerInvariant();
            bool ok = true;
            foreach (var k in keywords)
            {
                if (!name.Contains(k)) { ok = false; break; }
            }
            if (ok)
            {
                var val = prop.GetValue(null) as string;
                if (!string.IsNullOrEmpty(val)) return val;
            }
        }

        // search fields
        foreach (var field in type.GetFields(flags))
        {
            if (field.FieldType != typeof(string)) continue;
            var name = field.Name.ToLowerInvariant();
            bool ok = true;
            foreach (var k in keywords)
            {
                if (!name.Contains(k)) { ok = false; break; }
            }
            if (ok)
            {
                var val = field.GetValue(null) as string;
                if (!string.IsNullOrEmpty(val)) return val;
            }
        }

        return null;
    }
}
