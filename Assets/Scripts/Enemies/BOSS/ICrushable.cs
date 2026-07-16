// Implement this on anything that should be destroyed by a falling/crushing
// object (see CrushKill.cs) but needs its OWN death sequence instead of the
// patrol-enemy-specific EnemyDeath component (dissolve shader, respawn point,
// EnemyPatrol dependency, etc). BossController implements this.
public interface ICrushable
{
    // impactForce is collision.relativeVelocity.magnitude from the crushing object.
    void OnCrushed(float impactForce);
}
