using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class CenteredGrid : MonoBehaviour
{
    public int columns = 2; // number of columns in the grid

    private GridLayoutGroup grid;

    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        BalanceRows();
    }

    public void BalanceRows()
    {
        RectTransform rt = grid.GetComponent<RectTransform>();
        float totalWidth = rt.rect.width;

        int childCount = transform.childCount;

        for (int row = 0; row < Mathf.CeilToInt((float)childCount / columns); row++)
        {
            int startIndex = row * columns;
            int endIndex = Mathf.Min(startIndex + columns, childCount);
            int itemsInRow = endIndex - startIndex;

            if (itemsInRow == columns) continue; // full row → no need to shift

            // Calculate used width
            float rowWidth = itemsInRow * grid.cellSize.x + (itemsInRow - 1) * grid.spacing.x;
            float extraSpace = totalWidth - grid.padding.left - grid.padding.right - rowWidth;

            float offsetX = extraSpace / 2; // shift right by half

            for (int i = startIndex; i < endIndex; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;
                child.anchoredPosition += new Vector2(offsetX, 0);
            }
        }
    }
}