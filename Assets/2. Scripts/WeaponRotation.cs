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

    [Header("Recoil Settings")]
    public float recoilDistance = 0.2f; // 반동 거리
    public float recoilDuration = 0.05f; // 반동 효과 지속 시간
    private Vector3 originalPosition; // 총의 원래 위치
    private Coroutine recoilCoroutine; // 현재 실행 중인 반동 코루틴

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.localPosition; // 초기 위치 저장
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
                pv.RPC("SortingOrderControl", RpcTarget.AllBuffered, 6);
            }

            // 발사 키 입력 시 반동 효과
            if (Input.GetMouseButtonDown(0))
            {
                TriggerRecoil();
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

    /// <summary>
    /// 총기 반동 효과를 트리거
    /// </summary>
    private void TriggerRecoil()
    {
        // 이미 실행 중인 반동 코루틴이 있다면 중단
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        // 새로운 반동 효과 시작
        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    /// <summary>
    /// 반동 효과 코루틴
    /// </summary>
    private IEnumerator RecoilRoutine()
    {
        Vector3 recoilPosition = originalPosition - transform.right * recoilDistance;

        // 반동 위치로 이동
        float elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 원래 위치로 복귀
        elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 반동 효과 종료
        transform.localPosition = originalPosition;
        recoilCoroutine = null;
    }
}
