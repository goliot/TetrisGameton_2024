using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public SpriteRenderer SR;

    void Update()
    {
        if (PV.IsMine) // Control Local View°¡ True¶ó¸é
        {
            float axis = Input.GetAxisRaw("Horizontal");
            transform.Translate(new Vector3(axis * Time.deltaTime * 7, 0, 0));

            if (axis != 0) PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        }
    }

    [PunRPC]
    void FlipXRPC(float axis)
    {
        SR.flipX = axis == -1;
    }
}