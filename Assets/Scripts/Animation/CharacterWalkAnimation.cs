using UnityEngine;

// 실제 이동 속도를 보고 걷기 애니메이션과 정지 프레임을 전환한다
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterWalkAnimation : MonoBehaviour
{
    private static readonly int IsWalkingParameter = Animator.StringToHash("IsWalking");

    [SerializeField] private float walkSpeedThreshold = 0.1f;

    private Animator animator;
    private Rigidbody2D characterRigidbody;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateWalkAnimation();
    }

    private void UpdateWalkAnimation()
    {
        bool isWalking = characterRigidbody.linearVelocity.sqrMagnitude > walkSpeedThreshold * walkSpeedThreshold;
        animator.SetBool(IsWalkingParameter, isWalking);
    }
}
