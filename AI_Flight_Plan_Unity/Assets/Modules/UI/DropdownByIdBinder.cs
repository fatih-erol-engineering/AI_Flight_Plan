using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DropdownByIdBinder : MonoBehaviour
{
    public UIDocument ui;    

    // UI ile id’yi baðlamak için yardýmcý listeler
    List<string> _labels;
    List<int> _ids;

    // Dýþ dünya için seçili id
    public int SelectedId { get; private set; }

    void OnEnable()
    {
        //var root = ui.rootVisualElement;
        //var dd = root.Q<DropdownField>("idDropdown");

        //// Kaynak veriyi hazýrla
        //_labels = new List<string>();
        //_ids = new List<int>();

        //foreach (var aircraft in aircrafts)
        //{
        //    _labels.Add(aircraft.displayName);
        //    _ids.Add(aircraft.ID);
        //}

        //// Dropdown’ý doldur
        //dd.choices = _labels;

        //// Baþlangýç (ilk öðe)
        //if (_ids.Count > 0)
        //{
        //    dd.index = 0;                      // UI seçimi
        //    SelectedId = _ids[0];              // id karþýlýðý
        //}

        //// Deðiþiklikte id’yi güncelle
        //dd.RegisterValueChangedCallback(evt =>
        //{
        //    int idx = dd.index;                // seçilen satýrýn index’i
        //    if (idx >= 0 && idx < _ids.Count)
        //        SelectedId = _ids[idx];

        //    // Örnek kullaným:
        //    Debug.Log($"Seçilen: label='{evt.newValue}', id={SelectedId}");
        //});
    }

    // Dýþarýdan id verip UI’yý o id’ye setlemek istersen:
    public void SetById(int id)
    {
        //int idx = _ids.IndexOf(id);
        //if (idx >= 0)
        //{
        //    var dd = ui.rootVisualElement.Q<DropdownField>("idDropdown");
        //    // Bildirim spam’ini önlemek için:
        //    dd.SetValueWithoutNotify(_labels[idx]);
        //    SelectedId = id;
        //}
    }
}
