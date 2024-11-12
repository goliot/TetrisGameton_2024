using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("# Components")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public PhotonView pv;
    public Text nicknameText;
    public Image healthImage;
    public GameObject fireEffect;
    public CapsuleCollider2D capsuleCollider;
    public Transform gunTip;

    [Header("# Stats")]
    public float atkSpeed;

    [SerializeField]
    private GameObject aimLine;

    private float atkTime;
    private bool isGround;
    private Vector3 curPos;
    private Coroutine CoShootEffect;

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>(); // CapsuleCollider2D 컴포넌트 가져오기
        nicknameText.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nicknameText.color = pv.IsMine ? Color.green : Color.red;

        if (pv.IsMine)
        {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.Follow = transform;
            cm.LookAt = transform;

            aimLine.SetActive(true);
        }
    }

    private void Update()
    {
        atkTime += Time.deltaTime;

        if (pv.IsMine)
        {
            // 이동
            float axis = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * axis, rb.velocity.y);

            if (axis != 0)
            {
                anim.SetBool("walk", true);
                pv.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
            }
            else
            {
                anim.SetBool("walk", false);
            }

            // 점프, 바닥 체크
            float groundCheckRadius = capsuleCollider.size.x / 2; // CapsuleCollider의 반경을 기준으로 설정
            Vector2 groundCheckOffset = new Vector2(0, capsuleCollider.offset.y - capsuleCollider.size.y / 2);

            isGround = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, 1 << LayerMask.NameToLayer("Ground"));
            anim.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.W) && isGround)
                pv.RPC("JumpRPC", RpcTarget.All);

            // 공격
            if (Input.GetMouseButtonDown(0) && atkTime > atkSpeed)
            {
                Shoot();
                atkTime = 0f;
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    [PunRPC]
    void FlipXRPC(float axis) => spriteRenderer.flipX = axis == -1;

    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 700);
    }

    void Shoot()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 direction = (mousePosition - gunTip.transform.position).normalized;

        Vector3 bulletSpawnPosition = gunTip.transform.position;

        PhotonNetwork.Instantiate("Bullet", bulletSpawnPosition, Quaternion.identity)
            .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, direction);

        if (CoShootEffect == null)
            CoShootEffect = StartCoroutine(ShootEffect());
    }

    IEnumerator ShootEffect()
    {
        fireEffect.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        fireEffect.SetActive(false);

        CoShootEffect = null;
    }

    public void Hit()
    {
        healthImage.fillAmount -= 0.1f;
        if (healthImage.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    private void OnDrawGizmos()
    {
        if (capsuleCollider != null)
        {
            Vector2 checkPosition = (Vector2)transform.position + new Vector2(0, capsuleCollider.offset.y - capsuleCollider.size.y / 2);
            float groundCheckRadius = capsuleCollider.size.x / 2;

            Gizmos.color = isGround ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }
    }
}
