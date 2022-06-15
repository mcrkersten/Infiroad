using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SectorButton : MonoBehaviour
{
    public Sector sector;
    public Button deleteButton;
    public Color textSelectionColor;
    public Color textDeselectionColor;
    public TextMeshProUGUI sectorName;

    public delegate void OnSectorDeleted(Sector sector);
    public static event OnSectorDeleted sectorDeleted;
    public delegate void OnSectorSelected(Sector sector);
    public static event OnSectorSelected sectorSelected;
    public delegate void OnSectorDeselected();
    public static event OnSectorDeselected sectorDeselected;
    // Start is called before the first frame update
    void Start()
    {
        Buttons b = transform.parent.gameObject.GetComponent<Buttons>();
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry_0 = new EventTrigger.Entry();
        entry_0.eventID = EventTriggerType.Select;
        entry_0.callback.AddListener((data) => { b.OnHoverStart(this.gameObject); });
        trigger.triggers.Add(entry_0);

        EventTrigger.Entry entry_1 = new EventTrigger.Entry();
        entry_1.eventID = EventTriggerType.Deselect;
        entry_1.callback.AddListener((data) => { b.OnHoverEnd(this.gameObject); });
        trigger.triggers.Add(entry_1);

        GetComponent<Button>().Select();
    }

    public void OnSelection()
    {
        sector.roadChain.gameObject.SetActive(true);
        sectorSelected?.Invoke(sector);
    }

    public void OnDeselection()
    {
        sector.roadChain.gameObject.SetActive(false);
        sectorDeselected?.Invoke();
    }

    public void DeleteSector()
    {
        sectorDeleted?.Invoke(sector);
    }

    public void SetSelectionColor(TextMeshProUGUI text)
    {
        text.color = textSelectionColor;
    }

    public void SetDeselectionColor(TextMeshProUGUI text)
    {
        text.color = textDeselectionColor;
    }
}
