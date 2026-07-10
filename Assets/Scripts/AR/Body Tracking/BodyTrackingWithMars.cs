using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.MARS;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityGLTF;
using static CategoryManager;

public class BodyTrackingWithMars : MonoBehaviour
{
    public static BodyTrackingWithMars Instance;

    public GameObject targetCharacter;
    public Transform bodyProxyRoot;

    public GameObject tutorialPages;

    int currentPage = 0;

    public GameObject plusBtnCanvas;

    public Transform chestPosition;

    public ModelViewer mv;

    public ProductDetails productSelected;

    private void Awake()
    {
        // Original behavior: only set the singleton instance
        Instance = this;

        if (PlayerPrefs.GetInt("Mars Tutorial", 0) == 0)
        {
            PlayerPrefs.SetInt("Mars Tutorial", 1);
            tutorialPages.SetActive(true);
        }

        PlayerPrefs.SetInt("Restart", 1);
    }

    public void BodyModelSelected(Products p, string url, string bodySlotName, ProductItemData pid = null)
    {

        //BodySlot slot;
        //if (!System.Enum.TryParse(bodySlotName, out slot))
        //{
        //    if(CategoryManager.Instance.isDebugMode)Debug.LogError("Invalid body slot name!");
        //    return;
        //}

        BodySlot targetSlot = BodySlot.Accessory;
        bool isRigged = false;

        string lowerSlot = bodySlotName.ToLower();

        // 🔹 Define which items are Rigged vs Static
        if (lowerSlot.Contains("shirt") || lowerSlot.Contains("hoodies") || lowerSlot.Contains("tops"))
        {
            targetSlot = BodySlot.Top;
            isRigged = true;
        }
        else if (lowerSlot.Contains("pant") || lowerSlot.Contains("skirt") || lowerSlot.Contains("bottom") || lowerSlot.Contains("jeans"))
        {
            targetSlot = BodySlot.Bottom;
            isRigged = true;
        }
        else if (lowerSlot.Contains("hat") || lowerSlot.Contains("head"))
        {
            targetSlot = BodySlot.Head;
            isRigged = false; // Static
        }
        else if (lowerSlot.Contains("watch"))
        {
            targetSlot = BodySlot.Wrist;
            isRigged = false; // Static
        }
        else if (lowerSlot.Contains("shoe"))
        {
            targetSlot = BodySlot.Footwear;
            isRigged = false; // Static
        }
        if(CategoryManager.Instance.isDebugMode)Debug.Log("body slot Name : " + bodySlotName);
        StartCoroutine(DownloadAndAssignBodyModel(p, url, targetSlot, isRigged, pid ? pid.ProductProgress : null, pid ? pid.DownloadFailed : null));
    }

    public BodySlot GetBodySlot(string slotName)
    {
        switch (slotName.ToLower())
        {
            case "top":
            case "hoodies":
                return BodySlot.Top;
            case "bottom":
            case "pant":
            case "jeans":
                return BodySlot.Bottom;
            case "head":
                return BodySlot.Head;
            case "wrist":
                return BodySlot.Wrist;
            case "footwear":
                return BodySlot.Footwear;
            case "accessory":
                return BodySlot.Accessory;
            default:
                if(CategoryManager.Instance.isDebugMode)Debug.LogError("Invalid body slot name! " + slotName);
                return BodySlot.Accessory;
        }
    }

    public Dictionary<BodySlot, ModelTracker> activeBodyModels = new Dictionary<BodySlot, ModelTracker>();

    public class ModelTracker
    {
        public GameObject RootObject;
        public List<GameObject> ReparentedBones = new List<GameObject>();
    }

    IEnumerator DownloadAndAssignBodyModel(Products p, string url, BodySlot slot, bool isRigged, Action<float> onProgress = null, Action onFailed = null)
    {
        //using (UnityWebRequest www = UnityWebRequest.Get(url))
        //{
        //    string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));
        //    www.downloadHandler = new DownloadHandlerFile(localPath);

        //    www.SendWebRequest();

        //    while (!www.isDone)
        //    {
        //        float progress = www.downloadProgress;
        //        onProgress?.Invoke(progress);
        //        yield return null;
        //    }

        //    if (www.result != UnityWebRequest.Result.Success)
        //    {
        //        if(CategoryManager.Instance.isDebugMode)Debug.LogError("Download failed: " + www.error);
        //        onFailed?.Invoke();
        //        yield break;
        //    }

        //    onProgress?.Invoke(1f);

        //    using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        //    {
        //        var importOptions = new ImportOptions();
        //        var importer = new GLTFSceneImporter(stream, importOptions);

        //        yield return importer.LoadSceneAsync();

        //        GameObject loadedGLB = importer.LastLoadedScene;

        //        if (loadedGLB == null)
        //        {
        //            if(CategoryManager.Instance.isDebugMode)Debug.LogError("Failed to load GLB model!");
        //            yield break;
        //        }

        GameObject loadedGLB = null;
        yield return StartCoroutine(ModelLoaderService.DownloadAndLoad(url, (model) => { loadedGLB = model; }, onProgress, onFailed));

        if (loadedGLB == null)
            yield break;

        // 🔹 CLEANUP
        if (activeBodyModels.ContainsKey(slot))
        {
            foreach (var bone in activeBodyModels[slot].ReparentedBones)
            {
                if (bone != null) Destroy(bone);
            }
            Destroy(activeBodyModels[slot].RootObject);
            activeBodyModels.Remove(slot);
        }

        ModelTracker newTracker = new ModelTracker();
        newTracker.RootObject = loadedGLB;

        if (isRigged)
        {
            EquipClothing(loadedGLB);

            // 🔹 RIGGED LOGIC (Tops/Bottoms)
            //Transform[] downloadedTransforms = loadedGLB.GetComponentsInChildren<Transform>(true);
            //foreach (Transform modelBone in downloadedTransforms)
            //{
            //    Transform matchingProxyBone = FindDeepChild(bodyProxyRoot, modelBone.name);
            //    if (matchingProxyBone != null && modelBone != loadedGLB.transform)
            //    {
            //        newTracker.ReparentedBones.Add(modelBone.gameObject);
            //        modelBone.SetParent(matchingProxyBone);
            //        modelBone.localPosition = Vector3.zero;
            //        modelBone.localRotation = Quaternion.identity;
            //    }
            //}

            //// Parent the main container to root
            //loadedGLB.transform.SetParent(bodyProxyRoot);
        }
        else
        {
            // 🔹 STATIC LOGIC (Hats, Watches, Shoes)
            // Find the single specific bone this item should stick to
            Transform targetBone = GetStaticTargetBone(slot);
            if (targetBone != null)
            {
                loadedGLB.transform.SetParent(targetBone);
            }
            else
            {
                loadedGLB.transform.SetParent(bodyProxyRoot);
            }
        }

        ProductDetails pd = loadedGLB.AddComponent<ProductDetails>();

        pd.imagesUrl.Clear();
        foreach (var img in p.images)
            pd.imagesUrl.Add(img.url);

        pd.texturesUrl.Clear();
        foreach (var model in p.threeDModels)
            pd.texturesUrl.Add(model.texture);

        pd.colors.Clear();
        for (int i = 0; i < p.colors.Count; i++)
        {
            UnityEngine.Color newColor1;
            ColorUtility.TryParseHtmlString(p.colors[i].code, out newColor1);
            pd.colors.Add(newColor1);
        }

        pd.product = p;
        StartCoroutine(CategoryManager.Instance.SetProductImages(pd));


        GameObject modelToView = Instantiate(UIManagerAR.instance.modelPrefab);
        modelToView.transform.parent = UIManagerAR.instance.UI_3D_Models_Parent.transform;
        modelToView.transform.localPosition = Vector3.zero;
        UIManagerAR.instance.UI_3D_Models.Add(modelToView);


        ProductDetails mtv_pd = modelToView.GetComponent<ProductDetails>();
        mtv_pd.imagesUrl.Clear();
        foreach (var img in p.images)
            mtv_pd.imagesUrl.Add(img.url);

        mtv_pd.texturesUrl.Clear();
        foreach (var model in p.threeDModels)
            mtv_pd.texturesUrl.Add(model.texture);

        StartCoroutine(CategoryManager.Instance.SetProductImages(mtv_pd));

        mtv_pd.product = p;

        mtv_pd.colors.Clear();
        for (int i = 0; i < p.colors.Count; i++)
        {
            UnityEngine.Color newColor1;
            ColorUtility.TryParseHtmlString(p.colors[i].code, out newColor1);
            mtv_pd.colors.Add(newColor1);
        }

        loadedGLB.transform.localPosition = Vector3.zero;
        loadedGLB.transform.localRotation = Quaternion.identity;
        activeBodyModels.Add(slot, newTracker);

        modelToView.transform.Find("Visual").GetComponent<MeshFilter>().mesh = loadedGLB.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
        modelToView.transform.Find("Visual").GetComponent<MeshRenderer>().materials = loadedGLB.GetComponentInChildren<SkinnedMeshRenderer>().materials;

        Material mat = loadedGLB.GetComponentInChildren<SkinnedMeshRenderer>().material;

        if (pd.textures.Count > 0)
            mat.mainTexture = pd.textures[0];

        loadedGLB.GetComponentInChildren<SkinnedMeshRenderer>().material = mat;

        loadedGLB.name = CategoryManager.Instance.GetUniqueName(p.name, UIManagerAR.instance.UI_3D_Models);

        modelToView.name = loadedGLB.name;


        SpawnCanvas(loadedGLB, slot);

        ProductDetails pd2 = loadedGLB.GetComponent<ProductDetails>();
        if (pd2 != null && pd2.product != null)
            UIManagerAR.instance.SelectModel(pd2);

        mv.FrameObject(modelToView);

        if(CategoryManager.Instance.isDebugMode)Debug.Log($"✅ {slot} attached as {(isRigged ? "Rigged" : "Static")}.");

        CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();

        //float sizeValue;
        //if (float.TryParse(p.variants[0].size.size, out sizeValue))
        //{
        ClothingFitController.ApplySizeFromLabel(p.variants[0].size.size, loadedGLB.GetComponentInChildren<SkinnedMeshRenderer>());
        //}

        //    }
        //}
    }

    void SpawnCanvas(GameObject newObject, BodySlot slot)
    {
        WorldCanvasFaceCamera[] allWorldCanvases =
                    FindObjectsByType<WorldCanvasFaceCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        bool spawnCanvas = true;

        var newPd = newObject.GetComponent<ProductDetails>();

        foreach (var c in allWorldCanvases)
        {
            if (c.pd == newPd)
            {
                spawnCanvas = false;
                break;
            }
        }

        if (spawnCanvas)
        {
            WorldCanvasFaceCamera btnCanvas =
                Instantiate(plusBtnCanvas).GetComponent<WorldCanvasFaceCamera>();

            if (!btnCanvas.GetComponent<ObjectDetail>())
                btnCanvas.gameObject.AddComponent<ObjectDetail>();

            switch (slot)
            {
                case BodySlot.Top:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Right;
                    break;

                case BodySlot.Bottom:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Right;
                    break;

                case BodySlot.Head:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.TopRight;
                    break;

                default:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.BottomLeft;
                    break;
            }

            btnCanvas.targetModel = newObject.GetComponentInChildren<SkinnedMeshRenderer>()?.transform;
            newPd.plusCanvas = btnCanvas.gameObject;
            btnCanvas.pd = newPd;
            btnCanvas.objDetail = newObject.GetComponent<ObjectDetail>();
        }
    }

    public void EquipClothing(GameObject clothingModel)
    {
        // 1. Instantiate the clothing
        //GameObject newCloth = Instantiate(clothingPrefab, targetCharacter.transform);

        //clothingModel.transform.SetParent(targetCharacter.transform, false);

        // 2. Get the SkinnedMeshRenderer of the clothing
        SkinnedMeshRenderer clothRenderer = clothingModel.GetComponentInChildren<SkinnedMeshRenderer>();

        // 3. Get all the bones of the main character
        Transform[] characterBones = targetCharacter.GetComponentInChildren<SkinnedMeshRenderer>().bones;

        // 4. Prepare an array for the new bones
        Transform[] newBones = new Transform[clothRenderer.bones.Length];

        // 5. Match bones by name
        for (int i = 0; i < clothRenderer.bones.Length; i++)
        {
            string boneName = clothRenderer.bones[i].name;
            bool found = false;

            foreach (Transform charBone in characterBones)
            {
                if (charBone.name == boneName)
                {
                    newBones[i] = charBone;
                    found = true;
                    break;
                }
            }

            if (!found) if(CategoryManager.Instance.isDebugMode)Debug.LogWarning("Could not find bone: " + boneName);
        }

        // 6. Assign the character's bones to the clothing renderer
        clothRenderer.bones = newBones;
        clothRenderer.rootBone = targetCharacter.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;
    }

    public GameObject GetModel(BodySlot slot)
    {
        if (activeBodyModels.ContainsKey(slot))
            return activeBodyModels[slot].RootObject;
        return null;
    }


    // 🔹 Map the Static Slot to a specific bone name in your Proxy
    Transform GetStaticTargetBone(BodySlot slot)
    {
        string boneName = "";
        switch (slot)
        {
            case BodySlot.Head: boneName = "Head"; break;
            case BodySlot.Wrist: boneName = "LeftHand"; break; // or RightHand
            case BodySlot.Footwear: boneName = "LeftFoot"; break; // Shoes usually handled as a pair or per foot
            case BodySlot.Neck: boneName = "Neck"; break;
        }
        return FindDeepChild(bodyProxyRoot, boneName);
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(name)) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    public void ChangeModelTexture(int index)
    {
        foreach (Transform child in UIManagerAR.instance.colorParent_LD)
        {
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }

        foreach (Transform child in UIManagerAR.instance.colorParent_SD)
        {
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }

        foreach (Transform child in CategoryManager.Instance.modelVariantParent)
        {
            child.gameObject.SetActive(true);
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }

        if (index >= UIManagerAR.instance.selectedModelDetails.textures.Count)
            return;

        UIManagerAR.instance.selectedModelDetails.selectedColorIndex = index;
        Material mat = UIManagerAR.instance.selectedModelDetails.GetComponentInChildren<SkinnedMeshRenderer>().material;

        //Color newColor1;
        //ColorUtility.TryParseHtmlString(selectedProduct.product.colors[index].code, out newColor1);
        //mat.color = newColor1;

        mat.mainTexture = UIManagerAR.instance.selectedModelDetails.textures[index];

        productSelected.GetComponentInChildren<SkinnedMeshRenderer>().material = mat;
    }

    public void DeleteSpawnedModels()
    {
        foreach (var kvp in activeBodyModels)
        {
            foreach (var bone in kvp.Value.ReparentedBones)
            {
                if (bone != null) Destroy(bone);
            }
            Destroy(kvp.Value.RootObject);
        }
        activeBodyModels.Clear();

        UIManagerAR.instance.DeleteAllSpawnedObjects();
    }

    public void DeleteSelectedItem(GameObject objToDelete)
    {
        foreach (var kvp in activeBodyModels)
        {
            if (kvp.Value.RootObject == objToDelete)
            {
                foreach (var bone in kvp.Value.ReparentedBones)
                {
                    if (bone != null) Destroy(bone);
                }
                
                var productDetails = kvp.Value.RootObject.GetComponent<ProductDetails>();
                if (productDetails != null && productDetails.plusCanvas != null)
                {
                    Destroy(productDetails.plusCanvas);
                }

                Destroy(kvp.Value.RootObject);
                activeBodyModels.Remove(kvp.Key);
                break;
            }
        }
    }

    public enum BodySlot
    {
        Head,
        Top,
        Bottom,
        Footwear,
        Wrist,
        Neck,
        Accessory
    }
}