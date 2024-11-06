using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NicknameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    /// <summary>
    /// ConnectUsingSettings 성공시 호출
    /// </summary>
    public override void OnConnectedToMaster()
    {
        RoomOptions roomOption = new RoomOptions();
        roomOption.MaxPlayers = 6;
        //roomOption.CustomRoomProperties = new HashTable() { { "키1", "문자열" }, { "키2", "문자열" } };

        PhotonNetwork.LocalPlayer.NickName = NicknameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", roomOption, null);
    }

    /// <summary>
    /// JoinOrCreateRoom 성공시 호출
    /// </summary>
    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine(DestroyBullet());
        Spawn();
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            go.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
        }
    }

    public void Spawn()
    {
        Debug.Log("Spawn");
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-6f, 19f), 4, 0), Quaternion.identity);
        RespawnPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// 접속 해제시 호출
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }
}
