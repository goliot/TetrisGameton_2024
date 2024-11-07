using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotation : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView pv;

    private SpriteRenderer spriteRenderer;
    private Quaternion localTargetRotation; // ���� �÷��̾��� ȸ�� ��ǥ
    private Quaternion networkTargetRotation; // ��Ʈ��ũ �÷��̾��� ȸ�� ��ǥ

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            // ���� �÷��̾��� ��� ���콺 ��ġ ��� ȸ�� ó��
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector2 dir = mousePosition - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            localTargetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = localTargetRotation;

            // ���콺 ��ġ�� ���� flipX �� gunTip�� ��ġ ����
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
            // ��Ʈ��ũ �÷��̾��� ��� �ε巯�� ȸ�� ����
            transform.rotation = Quaternion.Lerp(transform.rotation, networkTargetRotation, Time.deltaTime * 10f);
        }
    }

    [PunRPC]
    void FlipYRPC(bool flipFlag) => spriteRenderer.flipY = flipFlag;

    [PunRPC]
    void SortingOrderControl(int x) => spriteRenderer.sortingOrder = x;

    // OnPhotonSerializeView�� ȸ�� �� ����ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ���� �÷��̾�: ȸ�� �� ����
            stream.SendNext(localTargetRotation);
        }
        else
        {
            // ��Ʈ��ũ �÷��̾�: ȸ�� �� ����
            networkTargetRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}