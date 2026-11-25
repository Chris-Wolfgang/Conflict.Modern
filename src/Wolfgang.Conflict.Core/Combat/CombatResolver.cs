namespace Wolfgang.Conflict.Core.Combat;

/// <summary>
/// Pure stateless combat math. One shot in, one <see cref="CombatResult"/>
/// out. The engine assembles each <see cref="CombatAttempt"/> from current
/// state, calls this resolver, applies damage, and (if mutual LOS, range,
/// and ammo permit) repeats with the roles swapped for the counter-attack.
/// </summary>
/// <remarks>
/// <para>
/// To-hit math is D&amp;D-flavored but probability-based, not d20:
/// <c>HitChance% = clamp(weapon.Accuracy − 5×(defender.Dex + defender.Luck) − terrainCover, 5, 95)</c>.
/// </para>
/// <para>
/// Damage on a hit is <c>max(0, weapon.DamageByArmor[defender.ArmorClass] − defender.Armor)</c>,
/// where the damage table is pre-baked per armor class so weapons can model
/// real asymmetries (e.g. small arms vs MBT does nothing; sabot vs infantry
/// is overkill but unlikely to land).
/// </para>
/// </remarks>
public static class CombatResolver
{
    /// <summary>Lower clamp on hit chance — every shot can theoretically land.</summary>
    public const int MinHitChancePercent = 5;

    /// <summary>Upper clamp on hit chance — every shot can theoretically miss.</summary>
    public const int MaxHitChancePercent = 95;


    /// <summary>
    /// Compute the final to-hit percentage for an attempt without consuming
    /// RNG. Used by the UI to preview attack odds and by AI heuristics.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="attempt"/> is <see langword="null"/>.</exception>
    public static int EstimateHitChance(CombatAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var evasion = 5 * (attempt.DefenderDexterity + attempt.DefenderLuck);
        var cover = TerrainCover.CoverFor(attempt.DefenderTerrain);
        var raw = attempt.Weapon.Accuracy - evasion - cover;

        return Math.Clamp(raw, MinHitChancePercent, MaxHitChancePercent);
    }


    /// <summary>
    /// Roll a single shot and produce its outcome. Consumes one RNG draw.
    /// </summary>
    /// <param name="attempt">The shot inputs.</param>
    /// <param name="rng">Seeded RNG; the engine threads <c>GameState.RngSeed/Step</c> through this.</param>
    /// <returns>Hit chance, roll, hit/miss, damage, and the number of RNG draws consumed.</returns>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The attempt is structurally illegal (no LOS or out of weapon range).
    /// The engine should pre-validate; this is defense in depth.
    /// </exception>
    public static CombatResult Resolve(CombatAttempt attempt, Random rng)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(rng);
        ValidateAttempt(attempt);

        var hitChance = EstimateHitChance(attempt);
        var roll = rng.Next(1, 101); // [1, 100]
        var didHit = roll <= hitChance;
        var damage = didHit ? ComputeDamage(attempt) : 0;

        return new CombatResult
        (
            HitChancePercent: hitChance,
            HitRoll: roll,
            DidHit: didHit,
            Damage: damage,
            RngStepsConsumed: 1
        );
    }


    private static int ComputeDamage(CombatAttempt attempt)
    {
        var rawDamage = attempt.Weapon.DamageByArmor.TryGetValue(attempt.DefenderArmorClass, out var d) ? d : 0;
        return Math.Max(0, rawDamage - attempt.DefenderArmor);
    }


    private static void ValidateAttempt(CombatAttempt attempt)
    {
        if (attempt.Weapon is null)
        {
            throw new InvalidOperationException("CombatAttempt.Weapon must not be null.");
        }

        if (attempt.Weapon.DamageByArmor is null)
        {
            throw new InvalidOperationException("CombatAttempt.Weapon.DamageByArmor must not be null.");
        }

        if (!attempt.HasLineOfSight)
        {
            throw new InvalidOperationException("Attacker has no line of sight to defender.");
        }

        if (attempt.Distance < attempt.Weapon.MinRange || attempt.Distance > attempt.Weapon.MaxRange)
        {
            throw new InvalidOperationException
            (
                $"Distance {attempt.Distance} is outside weapon range [{attempt.Weapon.MinRange}, {attempt.Weapon.MaxRange}]."
            );
        }
    }
}
