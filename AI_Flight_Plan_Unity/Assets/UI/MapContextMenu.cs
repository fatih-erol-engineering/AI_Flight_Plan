using UnityEngine;
using UnityEngine.UIElements;

public class MapContextMenu : MonoBehaviour
{
    public UIDocument uiDocument;

    VisualElement root, ctxRoot, mainActions, aircraftPicker;
    Button btnAddAircraft, btnAddRestricted;
    DropdownField ddAircraft;

    void OnEnable()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        ctxRoot = root.Q<VisualElement>("ContextRoot");
        mainActions = root.Q<VisualElement>("MainActions");
        aircraftPicker = root.Q<VisualElement>("AircraftPicker");
        btnAddAircraft = root.Q<Button>("BtnAddAircraft");
        btnAddRestricted = root.Q<Button>("BtnAddRestricted");
        ddAircraft = root.Q<DropdownField>("DdAircraft");

        // Dropdown içeriði
        ddAircraft.choices = new System.Collections.Generic.List<string> { "Uçak 1", "Uçak 2" };
        ddAircraft.value = ddAircraft.choices[0];

        // Týklamalar
        btnAddAircraft.clicked += () =>
        {
            // Butonlarý gizle, dropdown göster
            mainActions.AddToClassList("hidden");
            aircraftPicker.RemoveFromClassList("hidden");
        };

        btnAddRestricted.clicked += () =>
        {
            Debug.Log("Add Restricted Area seçildi.");
            HideMenu();
        };

        ddAircraft.RegisterValueChangedCallback(evt =>
        {
            Debug.Log("Seçilen Aircraft: " + evt.newValue);
            HideMenu();
        });

        // Dýþarý týklayýnca kapat
        root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    void Update()
    {
        // Sað týkla aç
        if (Input.GetMouseButtonDown(1))
        {
            ShowAt(Input.mousePosition);
        }
        // ESC ile kapat
        if (Input.GetKeyDown(KeyCode.Escape)) HideMenu();
    }

    void ShowAt(Vector2 screenPos)
    {
        // UI Toolkit koordinatý: sol-üst (0,0), Screen ise sol-alt
        Vector2 panelPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

        // Menü içeriðini resetle
        aircraftPicker.AddToClassList("hidden");
        mainActions.RemoveFromClassList("hidden");

        ctxRoot.style.left = panelPos.x;
        ctxRoot.style.top = panelPos.y;
        ctxRoot.style.display = DisplayStyle.Flex;
    }

    void HideMenu()
    {
        ctxRoot.style.display = DisplayStyle.None;
    }

    void OnRootPointerDown(PointerDownEvent evt)
    {
        // Menü hariç bir yere sol týklanýrsa kapat
        if (ctxRoot.resolvedStyle.display == DisplayStyle.None) return;
        if (!ctxRoot.worldBound.Contains(evt.position)) HideMenu();
    }
}
