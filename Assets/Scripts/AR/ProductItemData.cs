using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductItemData : MonoBehaviour
{
    public Image productImage;
    public TextMeshProUGUI name;
    public TextMeshProUGUI type;
    public TextMeshProUGUI price;
    public TextMeshProUGUI salePrice;
    public GameObject loadingIcon;
    public GameObject loadingBar;
    public Image loadingBarImage;
    public TextMeshProUGUI percentageTxt;

    public void ProductProgress(float progress)
    {
        loadingBar.SetActive(true);
        loadingBarImage.fillAmount = 1 - progress;
        percentageTxt.text = Mathf.RoundToInt(progress * 100f) + "%";

        if(loadingBarImage.fillAmount == 0)
            loadingBar.SetActive(false);
    }

    public void DownloadFailed()
    {
        loadingBar.SetActive(false);
    }
}
