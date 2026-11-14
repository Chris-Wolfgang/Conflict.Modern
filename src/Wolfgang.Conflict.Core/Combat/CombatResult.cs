namespace Wolfgang.Conflict.Core.Combat;

/// <summary>
/// Outcome of a single resolved shot.
/// </summary>
/// <param name="HitChancePercent">
/// Final hit chance the resolver computed, clamped to [5, 95], exposed for UI preview
/// and for log/replay reconstruction.
/// </param>
/// <param name="HitRoll">
/// The 1–100 roll the resolver consumed from the RNG to decide the hit.
/// </param>
/// <param name="DidHit">
/// <see langword="true"/> if <see cref="HitRoll"/> was at most <see cref="HitChancePercent"/>.
/// </param>
/// <param name="Damage">
/// Damage actually dealt (0 on a miss; otherwise raw weapon damage vs the
/// defender's armor class minus the defender's flat armor, floored at zero).
/// </param>
/// <param name="RngStepsConsumed">
/// Number of RNG draws the resolver made — the engine uses this to advance
/// <c>GameState.RngStep</c>.
/// </param>
public sealed record CombatResult
(
    int HitChancePercent,
    int HitRoll,
    bool DidHit,
    int Damage,
    int RngStepsConsumed
);
