using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModelVariant : MonoBehaviour
{
    public Image img;
    public Button btn;
    public TextMeshProUGUI colorName;
    public Image colorImg;
    public GameObject selectedImg;
    public int index = 0;

    public void OnButtonClick()
    {
        if (UIManagerAR.instance.spawner != null)
            UIManagerAR.instance.spawner.ChangeTextureByIndex(index);
        else if (BodyTrackingWithMars.Instance != null)
            BodyTrackingWithMars.Instance.ChangeModelTexture(index);
    }

    public void OnColorClicked()
    {
        if (SceneManager.GetActiveScene().name == "AR Scene")
        {
            UIManagerAR.instance.spawner.ChangeTextureByIndex(index);
            UIManagerAR.instance.ChangeModelVariant();
        }
        else if (SceneManager.GetActiveScene().name == "Blogs")
        {
            CategoriesUI.Instance.ChangeModelTexture(index);
            CategoriesUI.Instance.ChangeModelVariant();
        }
    }
}
