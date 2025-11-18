using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class UnitController : MonoBehaviour
{
    [Header("Data")]
    public SealData data;
    public int currentHP;
    public bool isAlive => currentHP > 0;

    [Header("Physics & Movement")]
    private Rigidbody rb;
    public float baseLaunchForce = 1f;
    public float minLaunchForce = 4f;
    public float maxLaunchForce = 18f;
    public float linearDragWhileMoving = 0.15f;
    public float angularDragWhileMoving = 0.5f;

    [Header("Combat")]
    public float collisionKnockback = 3f;

    [Header("Rotation / Aiming")]
    private float targetYaw;
    private bool hasTargetYaw = false;
    public float yawLerpSpeed = 8f;

    [Header("Detection")]
    public float stopVelocityThreshold = 0.2f;
    public float stopTimeRequired = 0.8f;

    [Header("Round State")]
    [Tooltip("True if this unit has been used in the current round")]
    public bool hasBeenUsed = false;

    [HideInInspector] public bool isSelected = false;

    private bool isLaunched = false;
    public bool IsLaunched => isLaunched;

    private float defaultDrag;
    private float defaultAngularDrag;

    public Action<UnitController> OnLaunchComplete;

    [Header("Effects")]
    [Tooltip("Particle prefab instantiated on collision")]
    public GameObject collisionVFXPrefab;
    [Tooltip("Sound clip played on collision")]
    public AudioClip collisionSFXClip;
    [Range(0f, 1f)]
    public float collisionSFXVolume = 0.7f;

    private AudioSource audioSource;

    // Initializes Rigidbody, AudioSource, and default HP.
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 50f;

        if (data != null) currentHP = data.maxHP;
        else currentHP = Mathf.Max(1, currentHP);

        defaultDrag = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        hasBeenUsed = false;
    }

    // Applies Y-axis rotation lerping if set by Host.
    void FixedUpdate()
    {
        if (hasTargetYaw && rb != null)
        {
            Quaternion current = rb.rotation;
            Quaternion desired = Quaternion.Euler(0f, targetYaw, 0f);
            Quaternion next = Quaternion.Slerp(current, desired, Mathf.Clamp01(yawLerpSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(next);
        }
    }

    // Launches the unit. Called by Host.
    public void Launch(Vector3 direction, float strengthNormalized)
    {
        if (!isAlive || isLaunched || rb == null) return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Vector3 dir = direction.normalized;

        rb.isKinematic = false;
        rb.WakeUp();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float curve = Mathf.Pow(Mathf.Clamp01(strengthNormalized), 1.05f);
        float rawForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, curve);
        float forceFinal = rawForce * baseLaunchForce;

        rb.AddForce(dir * forceFinal, ForceMode.Impulse);
        rb.linearDamping = linearDragWhileMoving;
        rb.angularDamping = angularDragWhileMoving;

        isLaunched = true;
        StartCoroutine(WaitUntilStoppedCoroutine());
    }

    // Coroutine (on Host) that waits for physics to settle, then resets state.
    private IEnumerator WaitUntilStoppedCoroutine()
    {
        float umbralSqr = stopVelocityThreshold * stopVelocityThreshold;
        float timeStill = 0f;

        while (true)
        {
            Vector3 velXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float velSqr = velXZ.sqrMagnitude;

            if (velSqr < umbralSqr)
            {
                timeStill += Time.deltaTime;
                if (timeStill >= stopTimeRequired)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.linearDamping = defaultDrag;
                    rb.angularDamping = defaultAngularDrag;

                    isLaunched = false;
                    
                    OnLaunchComplete?.Invoke(this);
                    yield break;
                }
            }
            else
            {
                timeStill = 0f;
            }
            yield return null;
        }
    }

    // Sets the target Y-axis rotation (called by Host).
    public void SetTargetYaw(float angleDegrees)
    {
        targetYaw = angleDegrees;
        hasTargetYaw = true;
    }
    
    // This is the local-only collision entry point for cosmetic effects (VFX/SFX).
    private void OnCollisionEnter(Collision collision)
    {
        if (!isAlive) return;

        UnitController other = collision.collider.GetComponentInParent<UnitController>();

        if (other != null && other.isAlive && other != this)
        {
            if(collision.contacts.Length > 0)
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 impactPoint = contact.point;
                Vector3 impactNormal = contact.normal; 

                SpawnCollisionEffect(impactPoint, impactNormal);
            }
        }
    }

    // Called by NetworkSealCombat on clients when NetHP changes.
    public void SetHPLocal(int hp)
    {
        currentHP = hp;
        if (currentHP <= 0)
        {
            DieLocal();
        }
    }

    // Local-only death. Disables the object and fixes turn manager bugs.
    public void DieLocal()
    {
        currentHP = Mathf.Max(0, currentHP);
        
        if (isLaunched)
        {
            StopAllCoroutines(); 
            isLaunched = false;
        }

        gameObject.SetActive(false);
    }

    // Checks if the unit is alive and has not been used this round.
    public bool IsAvailable => isAlive && !hasBeenUsed && gameObject.activeInHierarchy;
    
    // Marks the unit as used for this round.
    public void MarkUsed() => hasBeenUsed = true;
    
    // Resets the unit's "used" flag for a new round.
    public void ResetForNewRound() => hasBeenUsed = false;

    // Resets the unit's launch state and physics.
    public void ResetLaunchState()
    {
        try { StopAllCoroutines(); } catch { }
        isLaunched = false;
        
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.linearDamping = defaultDrag;
            rb.angularDamping = defaultAngularDrag;
        }
    }

    // Instantiates VFX and plays SFX at the collision point.
    public void SpawnCollisionEffect(Vector3 position, Vector3 normal)
    {
        if (collisionVFXPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            GameObject vfxInstance = Instantiate(collisionVFXPrefab, position, rotation);
            
            Destroy(vfxInstance, 1.0f);
        }

        if (audioSource != null && collisionSFXClip != null)
        {
            audioSource.PlayOneShot(collisionSFXClip, collisionSFXVolume);
        }
    }
}