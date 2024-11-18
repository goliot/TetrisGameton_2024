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

    private void Start() => Invoke("Destroy", 10f);

    private void Update()
    {
        if (dir == Vector3.zero) return;

        // �̵�
        transform.Translate(speed * Time.deltaTime * Vector3.right);

        // ȸ��
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);

        // �´� �� ���忡�� ����
        // �Ѿ��� ������ �ƴϰ�, �� �Ѿ��� �÷��̾ �������, �� �÷��̾ ���� �� ȣ���ϴ°�
        if(collision.tag == "Player") // �����ʿ� ���� Hit ����
        {
            if (!pv.IsMine && collision.GetComponent<PhotonView>().IsMine)
            {
                collision.GetComponent<PlayerController>().Hit();
                pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
            }
            if(pv.IsMine && !collision.GetComponent<PhotonView>().IsMine) // �� �� ����
            {
                pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
            }
        }
    }

    private void Destroy()
    {
        pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void DirRPC(Vector3 dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
