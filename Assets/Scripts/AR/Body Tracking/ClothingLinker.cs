using UnityEngine;
using System.Collections.Generic;

public class ClothingLinker : MonoBehaviour
{
    public GameObject targetCharacter; // The main character with the full skeleton
    public GameObject clothingPrefab;
    public Transform[] newBones;

    [ContextMenu("Equip Item")]
    public void EquipClothing()
    {
        // 1. Instantiate the clothing
        //GameObject newCloth = Instantiate(clothingPrefab, targetCharacter.transform);

        clothingPrefab.transform.SetParent(targetCharacter.transform, false);

        // 2. Get the SkinnedMeshRenderer of the clothing
        SkinnedMeshRenderer clothRenderer = clothingPrefab.GetComponentInChildren<SkinnedMeshRenderer>();

        // 3. Get all the bones of the main character
        Transform[] characterBones = targetCharacter.GetComponentInChildren<SkinnedMeshRenderer>().bones;

        // 4. Prepare an array for the new bones
        newBones = new Transform[clothRenderer.bones.Length];

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

            if (!found) Debug.LogWarning("Could not find bone: " + boneName);
        }

        // 6. Assign the character's bones to the clothing renderer
        clothRenderer.bones = newBones;
        clothRenderer.rootBone = targetCharacter.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;
    }
}