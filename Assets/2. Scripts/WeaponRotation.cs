using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotation : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView pv;

    private SpriteRenderer spriteRenderer;
    private Quaternion localTargetRotation; // 로컬 플레이어의 회전 목표
    private Quaternion networkTargetRotation; // 네트워크 플레이어의 회전 목표

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            // 로컬 플레이어의 경우 마우스 위치 기반 회전 처리
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector2 dir = mousePosition - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            localTargetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = localTargetRotation;

            // 마우스 위치에 따라 flipX 및 gunTip의 위치 반전
            if (mousePosition.x < transform.position.x)
            {
                pv.RPC("FlipYRPC", RpcTarget.AllBuffered, true);
                pv.RPC("SortingOrderControl", RpcTarget.AllBuffered, 4);
            }
            else
            {
                pv.RPC("FlipYRPC", RpcTarget.AllBuffered, false);
                //spriteRenderer.flipY = false;
                pv.RPC("SortingOrderControl", RpcTarget.AllBuffered, 6);
            }
        }
        else
        {
            // 네트워크 플레이어의 경우 부드러운 회전 적용
            transform.rotation = Quaternion.Lerp(transform.rotation, networkTargetRotation, Time.deltaTime * 10f);
        }
    }

    [PunRPC]
    void FlipYRPC(bool flipFlag) => spriteRenderer.flipY = flipFlag;

    [PunRPC]
    void SortingOrderControl(int x) => spriteRenderer.sortingOrder = x;

    // OnPhotonSerializeView로 회전 값 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어: 회전 값 전송
            stream.SendNext(localTargetRotation);
        }
        else
        {
            // 네트워크 플레이어: 회전 값 수신
            networkTargetRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}