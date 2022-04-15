using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaythroughData : MonoBehaviour
{
    static public PlaythroughData instance;

    public float timeSinceGameStart;

    void Awake() {
        if (instance) {
            GameObject.Destroy(gameObject);
        } else {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}
