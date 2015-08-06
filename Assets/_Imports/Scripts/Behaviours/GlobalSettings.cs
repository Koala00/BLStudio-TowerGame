// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

// A container of settings that doesn't need to be in the scene at all, but should be 
// attached to an (otherwise) empty gameobject in the project's Resources asset folder.
// The actual settings can be retrieved from anywhere in code by using GlobalSettings.Instance. 
public class GlobalSettings : MonoBehaviour
{
    public BallisticsSettings ballisticsSettings;

    public static GlobalSettings Instance
    {
        get {
            if (_instance == null)
            {
                // Try loading the 'Resources/GlobalSettings' game object prefab
                GameObject globalsObject = (GameObject)Resources.Load("GlobalSettings", typeof(GameObject));
                if (globalsObject == null) 
                    Debug.LogError("Failed to load the gameobject 'Resources/GlobalSettings'");

                // Try using the prefab's GlobalSettings script component as the instance
                _instance = globalsObject.GetComponent<GlobalSettings>();
                if (_instance == null) 
                    Debug.LogError("Failed to get the GlobalSettings component from the gameobject 'Resources/GlobalSettings'");
            }
            return _instance;
        }
    }

    public void Awake()
    {
        Debug.LogError("Tried to instantiate a gameobject with the 'GlobalSettings' MonoBehaviour in '" +
                        gameObject.name + "', which is not allowed.\n" +
                        "Instead, use GlobalSettings.Instance to access the GlobalSettings' members.");
    }

    private static GlobalSettings _instance = null;


}
