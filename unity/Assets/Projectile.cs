using UnityEngine;

/// <summary>
/// Simple projectile behaviour for battle effects.  Attach this script to a
/// prefab with a SpriteRenderer (e.g. the foxy bead or sword aura).
/// When instantiated, call <see cref="Initialize"/> to set its target
/// position and optionally override the movement speed.
/// The projectile will move towards the target and destroy itself on arrival.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// Speed at which the projectile moves (units per second).  Can be
    /// overridden on the prefab or at runtime via <see cref="Initialize"/>.
    /// </summary>
    [Tooltip("Movement speed in units per second.")]
    public float speed = 10f;

    // internal target position to move towards
    private Vector3 _targetPosition;
    private bool _initialized;

    /// <summary>
    /// Initialize the projectile with a target position and optional speed.
    /// This method must be called right after instantiating the projectile.
    /// </summary>
    /// <param name="target">World-space position the projectile should travel to.</param>
    /// <param name="moveSpeed">Optional override for the movement speed.</param>
    public void Initialize(Vector3 target, float moveSpeed = -1f)
    {
        _targetPosition = target;
        if (moveSpeed > 0f)
            speed = moveSpeed;
        _initialized = true;
    }

    private void Update()
    {
        // Don't do anything until initialized
        if (!_initialized) return;

        // Move towards the target position
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);

        // If we've reached (or overshot) the target, destroy the projectile
        if (Vector3.Distance(transform.position, _targetPosition) <= 0.01f)
        {
            Destroy(gameObject);
        }
    }
}