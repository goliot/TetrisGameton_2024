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

        // 이동
        transform.Translate(speed * Time.deltaTime * Vector3.right);

        // 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);

        // 맞는 쪽 입장에서 생각
        // 총알이 내꺼가 아니고, 그 총알이 플레이어에 닿았으며, 그 플레이어가 나일 때 호출하는것
        if(collision.tag == "Player") // 느린쪽에 맞춰 Hit 판정
        {
            if (!pv.IsMine && collision.GetComponent<PhotonView>().IsMine)
            {
                collision.GetComponent<PlayerController>().Hit();
                pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
            }
            if(pv.IsMine && !collision.GetComponent<PhotonView>().IsMine) // 쏜 쪽 입장
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
