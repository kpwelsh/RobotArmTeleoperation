using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasSystemManager : MonoBehaviour
{
    private static SystemManager _systemManager;
    public SystemManager SystemManager {
        get {
            if (_systemManager == null) {
                _systemManager = FindObjectOfType<SystemManager>() as SystemManager;
            }
            return _systemManager;
        }
    }
}
