/*
using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class KartPlayerAnimator : MonoBehaviour
{
    public Animator PlayerAnimator;
    public ArcadeKart Kart;
    public string SteeringParam = "Steering";
    public string GroundedParam = "Grounded";

    private int steerHash, groundHash;
    private float steeringSmoother;

    private NetworkedKartAnimState animState;

    private void Awake()
    {
        steerHash = Animator.StringToHash(SteeringParam);
        groundHash = Animator.StringToHash(GroundedParam);

        if (Kart == null) Kart = GetComponentInParent<ArcadeKart>();
        animState = GetComponentInParent<NetworkedKartAnimState>();
    }

    private void Update()
    {
        if (PlayerAnimator == null || PlayerAnimator.runtimeAnimatorController == null) return;

        float steer = animState != null ? animState.Steering.Value :
                      Kart != null ? Kart.Input.TurnInput : 0f;

        bool grounded = animState != null ? animState.Grounded.Value :
                        Kart != null && Kart.GroundPercent >= 0.5f;

        steeringSmoother = Mathf.Lerp(steeringSmoother, steer, Time.deltaTime * 5f);
        PlayerAnimator.SetFloat(steerHash, steeringSmoother);
        PlayerAnimator.SetBool(groundHash, grounded);
    }
}
*/