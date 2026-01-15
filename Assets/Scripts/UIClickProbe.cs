using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClickProbe : MonoBehaviour
{
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    void Awake()
    {
        if (!eventSystem) eventSystem = EventSystem.current;
        if (!raycaster) raycaster = Object.FindFirstObjectByType<GraphicRaycaster>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var data = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            raycaster.Raycast(data, results);

            if (results.Count == 0)
            {
                Debug.Log("Clique: nenhum elemento de UI foi atingido.");
                return;
            }

            Debug.Log("Clique UI (topo): " + results[0].gameObject.name);

            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log($"{i}: {results[i].gameObject.name} | depth:{results[i].depth} | sorting:{results[i].sortingOrder}");
            }
        }
    }
}
