using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    //Composants de base
    public GameObject PlayerArea;
    public GameObject EnemyArea;
    public GameObject Canvas;
    public GameObject backCardPrefab;
    public GameObject ButtonLeaveClient;

    public List<string> cardPrefabNames = new List<string>
{
    "MaceWindu",
    "Ziro",
    "Rancor",
    "MagnaGuard",
    "Yoda",
    "NuteGunray"
    // etc. → exactement les noms des prefabs dans Resources/Cards/
};


    //Buttons skills
    public List<GameObject> buttonsSkillsPrefabs;
    private List<GameObject> buttonsSkillsPrefabInstances;
    private static Dictionary<uint, Dictionary<string, int>> skillValues = new();

    //Deck
    public List<GameObject> instantiatedCard = new List<GameObject>();//deck cards instanciées
    private static List<List<int>> indices = new List<List<int>>();
    private List<int> deckIndex;

    //Autre utiles
    public bool myTurn = false;
    static private int starter; //0 ou 1
    public static Dictionary<uint, PlayerManager> playersByNetId = new();

    //UI
    private FadeBackgroundOnTurn fadeBGTurn;


    GameObject LoadCardPrefabByIndex(int index)
    {
        string path = $"Cards/{cardPrefabNames[index]}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"[LoadCardPrefabByIndex] Prefab not found at: {path}");
        }

        return prefab;
    }



    public (List<int>, List<int>) GetShuffledLists(int n)
    {
        List<int> result = Enumerable.Range(0, n).ToList();
        for (int i = 0; i < result.Count; i++)
        {
            int randomIndex = Random.Range(i, result.Count);
            (result[i], result[randomIndex]) = (result[randomIndex], result[i]);
        }

        List<int> listPlayer1 = result.Take(result.Count / 2).ToList();
        List<int> listPlayer2 = result.Skip(result.Count / 2).ToList();
        return (listPlayer1, listPlayer2);
    }

    public void ClearSkillButtons()
    {
        if (buttonsSkillsPrefabInstances != null)
        {
            foreach (var btn in buttonsSkillsPrefabInstances)
            {
                if (btn != null)
                {
                    Button buttonComp = btn.GetComponent<Button>();
                    if (buttonComp != null)
                    {
                        buttonComp.onClick.RemoveAllListeners();
                    }
                    Destroy(btn);
                }
            }
            buttonsSkillsPrefabInstances.Clear();
        }
        else
        {
            buttonsSkillsPrefabInstances = new List<GameObject>();
        }
    }



    public void InitializeButtons()
    {
        if (!isLocalPlayer) return;

        ClearSkillButtons();

        foreach (GameObject prefab in buttonsSkillsPrefabs)
        {
            if (prefab == null) continue;

            GameObject btnObj = Instantiate(prefab, Canvas.transform);
            Button btn = btnObj.GetComponent<Button>();

            string skillName = prefab.name.Replace("Button", "").Replace("(Clone)", "");
            btn.onClick.AddListener(() => CmdPlayTurn(skillName));

            btnObj.SetActive(false);
            buttonsSkillsPrefabInstances.Add(btnObj);
        }
    }



    public void AddSkillValue(uint netId, string skill, int value)
    {
        if (!skillValues.ContainsKey(netId))
            skillValues[netId] = new Dictionary<string, int>();

        skillValues[netId][skill] = value;
    }

    public void AddValuesToDictionnary()
    {
        GameObject card = instantiatedCard[0];

        AddSkillValue(netId, "Courage", card.GetComponent<Card>().GetCourage());
        AddSkillValue(netId, "Ruse", card.GetComponent<Card>().GetRuse());
        AddSkillValue(netId, "Autorite", card.GetComponent<Card>().GetAutorite());
        AddSkillValue(netId, "AptitudeAuCombat", card.GetComponent<Card>().GetAptitudeAuCombat());
        AddSkillValue(netId, "TechniquesDeCombat", card.GetComponent<Card>().GetTechniquesDeCombat());
        AddSkillValue(netId, "PouvoirJedi", card.GetComponent<Card>().GetPouvoirJedi());
        Debug.Log(string.Join(" | ", skillValues.Select(kvp => $"netId: {kvp.Key}, {string.Join(", ", kvp.Value.Select(sv => $"{sv.Key}:{sv.Value}"))}")));
    }

    public void ShowBack()
    {
        GameObject back = Instantiate(backCardPrefab, new Vector2(0, 0), Quaternion.identity);
        back.tag = "Back";
        back.transform.SetParent(EnemyArea.transform, false);
        Debug.Log($"[ShowBack] isServer: {isServer} | isClient: {isClient} | isLocalPlayer: {isLocalPlayer}");

    }

    public void SendDeck()
    {
        if (indices.Count < 1)
        {
            Debug.LogError("[SendDeck] Indices not initialized. Aborting.");
            return;
        }

        if (netId == 6)
        {
            deckIndex = new List<int>(indices[0]);
        }

        if (netId == 7)
        {
            deckIndex = new List<int>(indices[1]);
        }
        Debug.Log($"[SendDeck] netId: {netId} assigned deck index: {string.Join(",", deckIndex)}");
    }


    [Command]
    public void CmdShutdownServer()
    {
        Debug.Log("Arrêt du serveur initié depuis un client.");

        GameManager.instance.weHaveLooser = true;
        GameManager.instance.begin = true;
        GameManager.instance.RpcDestroyButtonsAll();
        RpcDestroyInstantiedCard();
        RpcCleanBack();
        RpcResetBG();
        RpcResetMyTurn();
        RpcCleanLocalPlayer();

        StartCoroutine(DelayedStopHost());
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        PlayerArea = GameObject.FindWithTag("PlayerArea");
        EnemyArea = GameObject.FindWithTag("EnemyArea");
        Canvas = GameObject.FindWithTag("MainCanvas");
        ButtonLeaveClient = GameObject.FindWithTag("ButtonLeaveClient");
        playersByNetId[netId] = this;
        fadeBGTurn = FindObjectOfType<FadeBackgroundOnTurn>();
        fadeBGTurn.ResetBG();
        SendDeck();
    }

    public override void OnStopClient()
    {
        if (buttonsSkillsPrefabInstances != null)
        {
            foreach (var btn in buttonsSkillsPrefabInstances)
                if (btn != null) Destroy(btn);
            buttonsSkillsPrefabInstances.Clear();
        }
    }

    public override void OnStartLocalPlayer()
    {
        InitializeButtons();
    }


    [Server]
    void ResetStatics()
    {
        starter = -1;
        playersByNetId.Clear();
        indices.Clear();
        skillValues.Clear();
    }

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();

        Debug.Log($"begin value : {GameManager.instance.begin}");

        if (GameManager.instance.begin)
        {
            ResetStatics();
            GameManager.instance.begin = false;
            GameManager.instance.weHaveLooser = false;
            starter = Random.Range(0, 2);
            Debug.Log($"[OnStartServer] starter choisi = {starter}");
            (var l1, var l2) = GetShuffledLists(cardPrefabNames.Count);
            indices.Add(l1);
            indices.Add(l2);
        }
    }

    public void CleanupLocalPlayer()
    {
        instantiatedCard.Clear();
        myTurn = false;
        DestroySkillsButtons();
        Resources.UnloadUnusedAssets();

    }


    [ClientRpc]
    void RpcShowCard(GameObject card, string type)
    {
        if (type == "Dealt")
        {
            if (card.GetComponent<NetworkIdentity>().isOwned)
            {
                card.transform.SetParent(PlayerArea.transform, false);
            }
            else
            {
                //card.transform.SetParent(EnemyArea.transform, false);
                var backs = GameObject.FindGameObjectsWithTag("Back");
                if (backs.Length == 0)
                {
                    ShowBack();
                }
            }
        }
        else if (type == "Played")
        {
            // À implémenter
        }
    }


    [TargetRpc]
    void TargetSetMyTurn(NetworkConnection target, bool isMyTurn)
    {
        myTurn = isMyTurn;
        Debug.Log($"[TargetSetMyTurn] Client received turn status: {myTurn}");
    }

    [TargetRpc]
    void TargetActivateButton(NetworkConnection target)
    {
        foreach (GameObject prefab in buttonsSkillsPrefabInstances)
            prefab.SetActive(myTurn);

        Debug.Log($"[TargetActivateButton] netId: {netId}, myTurn: {myTurn}, isOwned: {GetComponent<NetworkIdentity>().isOwned}");

        fadeBGTurn.SetTurn(myTurn);
    }

    [Command]
    public void CmdDealsCards()
    {
        Debug.Log($"[cmdDealsCard] executing cmddealscards{netId},{deckIndex}");
        GameObject prefab = LoadCardPrefabByIndex(deckIndex[deckIndex.Count - 1]);
        GameObject card = Instantiate(prefab, new Vector2(0, 0), Quaternion.identity);
        NetworkServer.Spawn(card, connectionToClient);
        instantiatedCard.Add(card);
        card.GetComponent<Card>().SetParameters();

        RpcShowCard(card, "Dealt");

        var ordered = playersByNetId.Values.OrderBy(p => p.connectionToClient.connectionId).ToList();
        int myIndex = ordered.IndexOf(this);
        bool isMyTurn = (starter == myIndex);
        //myTurn = isMyTurn;
        TargetSetMyTurn(connectionToClient, isMyTurn);
        TargetActivateButton(connectionToClient);

        AddValuesToDictionnary();
    }


    [Command]
    public void CmdPlayTurn(string skill)
    {

        var maxKey = skillValues.Aggregate((x, y) => x.Value[skill] > y.Value[skill] ? x : y).Key;
        var loserKey = playersByNetId.First(kvp => kvp.Key != maxKey).Key;
        int indexLostCard = playersByNetId[loserKey].deckIndex[playersByNetId[loserKey].deckIndex.Count - 1];

        playersByNetId[maxKey].TargetResultOfTurn(playersByNetId[maxKey].connectionToClient, maxKey, indexLostCard);
        playersByNetId[loserKey].TargetResultOfTurn(playersByNetId[loserKey].connectionToClient, maxKey, 0);

        skillValues.Clear();

    }


    [TargetRpc]
    void TargetResultOfTurn(NetworkConnection target, uint netid, int indexlostcard)
    {
        if (netId == netid)
        {
            myTurn = true;
            CmdUpdateAfterWin(indexlostcard);

        }
        else
        {
            myTurn = false;
            CmdDestroyLastCard();

        }
    }


    public void DestroySkillsButtons()
    {
        if (buttonsSkillsPrefabInstances == null) return;
        foreach (var prefab in buttonsSkillsPrefabInstances)
            Destroy(prefab);
        buttonsSkillsPrefabInstances.Clear();
    }

    [ClientRpc]
    void RpcCleanBack()
    {
        var backs = GameObject.FindGameObjectsWithTag("Back");
        Debug.Log($"[TargetCleanBack] Found {backs.Length} 'Back' objects on client");
        foreach (var back in backs)
            Destroy(back);
    }

    [ClientRpc]
    void RpcWeHaveLooser()
    {
        Debug.Log("Partie perdue !");
    }

    [ClientRpc]
    void RpcResetBG()
    {
        fadeBGTurn.ResetBG();
    }


    [ClientRpc]
    void RpcResetMyTurn()
    {
        myTurn = false;
    }

    [ClientRpc]
    void RpcCleanLocalPlayer()
    {
        CleanupLocalPlayer();
    }

    [ClientRpc]
    void RpcDestroyInstantiedCard()
    {
        if (instantiatedCard.Count > 0)
        {
            GameObject card = instantiatedCard[0];
            instantiatedCard.RemoveAt(0);
            NetworkServer.Destroy(card);
        }
    }


    [Command]
    public void CmdDestroyLastCard()
    {
        GameObject card = instantiatedCard[0];
        instantiatedCard.RemoveAt(0);
        NetworkServer.Destroy(card);
        deckIndex.RemoveAt(deckIndex.Count - 1);
        System.Text.StringBuilder sb = new();
        foreach (int i in deckIndex)
        {
            if (i >= 0 && i < cardPrefabNames.Count)
                sb.Append(cardPrefabNames[i]).Append(" | ");
            else
                sb.Append("INVALID_INDEX").Append(" | ");
        }
        Debug.Log("Contenu du deckIndex après destruction : " + sb.ToString());

        if (deckIndex.Count == 0)
        {
            GameManager.instance.weHaveLooser = true;
            GameManager.instance.begin = true;
            GameManager.instance.RpcDestroyButtonsAll();
            RpcCleanBack();
            RpcResetBG();
            RpcResetMyTurn();
            CleanupLocalPlayer();
            return;
        }
        SpawnAndShowCardSafe(netId);

    }

    [Command]
    public void CmdUpdateAfterWin(int index)
    {
        uint looserNetId = playersByNetId.First(kvp => kvp.Key != netId).Key;
        var looser = playersByNetId[looserNetId];

        GameObject card = instantiatedCard[0];
        instantiatedCard.RemoveAt(0);
        NetworkServer.Destroy(card);

        int last = deckIndex[deckIndex.Count - 1];
        deckIndex.RemoveAt(deckIndex.Count - 1);
        deckIndex.Insert(0, last);
        deckIndex.Insert(0, index);
        //Debug.Log("Contenu du deckIndex après le win : " +
        //string.Join(" | ", deckIndex.Select(i => cardPrefabs[i].name)));
        if (deckIndex.Count == cardPrefabNames.Count)
        {
            GameManager.instance.weHaveLooser = true;
            CleanupLocalPlayer();
            return;
        }
        SpawnAndShowCardSafe(netId);
    }

    [Server]
    void SpawnAndShowCardSafe(uint netid)
    {

        if (GameManager.instance != null && GameManager.instance.weHaveLooser)
        {
            Debug.Log("[PlayerManager] Un perdant détecté !");
            return;
        }

        PlayerManager player = playersByNetId[netid];
        int lastCardIndex = player.deckIndex[player.deckIndex.Count - 1];
        GameObject prefab = LoadCardPrefabByIndex(lastCardIndex);
        GameObject card = Instantiate(prefab, new Vector2(0, 0), Quaternion.identity);
        NetworkServer.Spawn(card, player.connectionToClient);
        player.instantiatedCard.Add(card);
        card.GetComponent<Card>().SetParameters();
        StartCoroutine(DelayedRpcShowCard(player, card));
    }

    private IEnumerator DelayedRpcShowCard(PlayerManager player, GameObject card)
    {
        yield return null;
        player.RpcShowCard(card, "Dealt");
        player.TargetActivateButton(player.connectionToClient);
        player.AddValuesToDictionnary();
    }

    private IEnumerator DelayedStopHost()
    {
        yield return new WaitForSeconds(0.5f);
        NetworkManager.singleton.StopHost();
    }

}