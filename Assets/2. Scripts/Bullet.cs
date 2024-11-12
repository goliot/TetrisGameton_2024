using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    Vector3 dir = Vector3.zero;
    [SerializeField]
    private float speed = 50f;

    //private void Start() => Destroy(gameObject, 10f);

    private void Update()
    {
        if (dir == Vector3.zero) return;
        transform.Translate(speed * Time.deltaTime * dir);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);

        // �´� �� ���忡�� ����
        // �Ѿ��� ������ �ƴϰ�, �� �Ѿ��� �÷��̾ �������, �� �÷��̾ ���� �� ȣ���ϴ°�
        if(!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine) // �����ʿ� ���� Hit ����
        {
            collision.GetComponent<PlayerController>().Hit();
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DirRPC(Vector3 dir) => this.dir = dir.normalized;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
