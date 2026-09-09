using Flexalon;
using UnityEngine;

[RequireComponent(typeof(FlexalonGridLayout))]
[DisallowMultipleComponent]
public class FlexalonGridAutoRows : MonoBehaviour
{
    private FlexalonGridLayout grid;

    void Awake()
    {
        grid = GetComponent<FlexalonGridLayout>();
    }

    void OnEnable()
    {
        UpdateRows();
    }

    void Update()
    {
        UpdateRows();
    }

    void UpdateRows()
    {
        if (grid == null)
            grid = GetComponent<FlexalonGridLayout>();
        if (grid == null) return;

        int activeChildren = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                activeChildren++;
        }

        int rows = Mathf.Max(1, Mathf.CeilToInt((float)activeChildren / grid.Columns));
        if (grid.Rows != rows)
            grid.Rows = (uint)rows;
    }
}
