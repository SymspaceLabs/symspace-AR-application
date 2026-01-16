using UnityEngine;
using UnityEngine.UI;

public class ModelVariant : MonoBehaviour
{
    public Image img;
    public Button btn;
    public int index = 0;

    public void OnButtonClick()
    {
        UIManagerAR.instance.spawner.SelectObjectIndex(index);
    }
}
