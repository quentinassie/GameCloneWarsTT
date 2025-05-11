using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    [SyncVar]
    public bool weHaveLooser = false;

    [SyncVar]
    public bool begin = true;

    [ClientRpc]
    public void RpcDestroyButtonsAll()
    {
        foreach (var pm in FindObjectsOfType<PlayerManager>())
        {
            pm.DestroySkillsButtons(); // Ou appelle directement la méthode de destruction
        }
    }

}

