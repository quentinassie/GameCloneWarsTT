using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;


public class OnClickButton1 : NetworkBehaviour
{

    public PlayerManager playerManager;


    // Start is called before the first frame update
    void Start()
    {


    }


    public void OnClick()
    {

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();


        playerManager.CmdDealsCards();

        this.GetComponent<Button>().gameObject.SetActive(false);




    }
}
