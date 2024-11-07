using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotation : MonoBehaviourPunCallbacks
{
    public PhotonView pv;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector2 dir = mousePosition - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // 마우스 위치에 따라 flipX 및 gunTip의 위치 반전
        if (mousePosition.x < transform.position.x)
        {
            pv.RPC("FlipYRPC", RpcTarget.AllBuffered, true);
            //spriteRenderer.flipY = true;
            pv.RPC("SortingOrderControl", RpcTarget.AllBuffered, 4);
            spriteRenderer.sortingOrder = 4;
        }
        else
        {
            pv.RPC("FlipYRPC", RpcTarget.AllBuffered, false);
            //spriteRenderer.flipY = false;
            pv.RPC("SortingOrderControl", RpcTarget.AllBuffered, 6);
            spriteRenderer.sortingOrder = 6;
        }
    }

    [PunRPC]
    void FlipYRPC(bool flipFlag) => spriteRenderer.flipY = flipFlag;

    [PunRPC]
    void SortingOrderControl(int x) => spriteRenderer.sortingOrder = x;
}
