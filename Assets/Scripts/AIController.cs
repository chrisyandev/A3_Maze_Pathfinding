using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public event Action OnDie;
    public event Action OnBallHit;

    private readonly int IdleAnimationName = Animator.StringToHash("C_096_AIdle_F");
    private readonly int RunAnimationName = Animator.StringToHash("C_096_ARun_F - Forward");
    private readonly int DeathAnimationName = Animator.StringToHash("C_096_Dstd_F");
    private const float DissolveSpeed = 0.5f;

    [SerializeField]
    private Material dissolveMaterial;
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;
    [SerializeField]
    private AudioSource deathSound;
    [SerializeField]
    private AudioSource respawnSound;

    private NavMeshAgent navMeshAgent;
    private CharacterController characterController;
    private Animator animator;
    private Material defaultMaterial;
    private int health;
    private bool isDead;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        defaultMaterial = meshRenderer.material;

        health = 3;
        dissolveMaterial.SetFloat("_DissolveAmount", 0.0f);

        navMeshAgent.updateRotation = false;
    }

    private void Update()
    {
        if (!isDead)
        {
            Vector3 movementDirection = navMeshAgent.desiredVelocity.normalized;

            if (movementDirection != Vector3.zero)
            {
                transform.forward = movementDirection;
            }

            if (navMeshAgent.velocity == Vector3.zero)
            {
                animator.Play(IdleAnimationName);
            }
            else
            {
                animator.Play(RunAnimationName);
            }
        }
        else
        {
            float dissolveAmount = dissolveMaterial.GetFloat("_DissolveAmount");

            if (dissolveAmount < 0.9f)
            {
                dissolveMaterial.SetFloat("_DissolveAmount", dissolveAmount + DissolveSpeed * Time.deltaTime);
            }
            else
            {
                meshRenderer.enabled = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out ThrowableBall ball))
        {
            Destroy(ball.gameObject);
            OnBallHit?.Invoke();

            if (!isDead)
            {
                health--;

                if (health <= 0)
                {
                    Die();
                }
            }
        }
    }

    public void Respawn()
    {
        isDead = false;
        meshRenderer.enabled = true;
        meshRenderer.material = defaultMaterial;
        navMeshAgent.enabled = true;
        characterController.enabled = true;
        health = 3;
        dissolveMaterial.SetFloat("_DissolveAmount", 0.0f);
        respawnSound.Play();
    }

    private void Die()
    {
        isDead = true;
        meshRenderer.material = dissolveMaterial;
        navMeshAgent.enabled = false;
        characterController.enabled = false;
        animator.Play(DeathAnimationName);
        deathSound.Play();
        OnDie?.Invoke();
    }
}
