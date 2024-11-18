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

    [Header("Recoil Settings")]
    public float recoilDistance = 0.2f; // �ݵ� �Ÿ�
    public float recoilDuration = 0.05f; // �ݵ� ȿ�� ���� �ð�
    private Vector3 originalPosition; // ���� ���� ��ġ
    private Coroutine recoilCoroutine; // ���� ���� ���� �ݵ� �ڷ�ƾ

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.localPosition; // �ʱ� ��ġ ����
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
                pv.RPC("SortingOrderControl", RpcTarget.AllBuffered, 6);
            }

            // �߻� Ű �Է� �� �ݵ� ȿ��
            if (Input.GetMouseButtonDown(0))
            {
                TriggerRecoil();
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

    /// <summary>
    /// �ѱ� �ݵ� ȿ���� Ʈ����
    /// </summary>
    private void TriggerRecoil()
    {
        // �̹� ���� ���� �ݵ� �ڷ�ƾ�� �ִٸ� �ߴ�
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        // ���ο� �ݵ� ȿ�� ����
        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    /// <summary>
    /// �ݵ� ȿ�� �ڷ�ƾ
    /// </summary>
    private IEnumerator RecoilRoutine()
    {
        Vector3 recoilPosition = originalPosition - transform.right * recoilDistance;

        // �ݵ� ��ġ�� �̵�
        float elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���� ��ġ�� ����
        elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �ݵ� ȿ�� ����
        transform.localPosition = originalPosition;
        recoilCoroutine = null;
    }
}
