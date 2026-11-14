using Wolfgang.Conflict.Core.Combat;

namespace Wolfgang.Conflict.Core.Tests.Unit.Combat;

/// <summary>
/// Empirical tests: run the resolver many times against a fixed attempt and
/// verify the observed hit rate falls within a tight band of the estimated
/// hit chance. This catches regressions in the underlying RNG distribution
/// or off-by-one bugs in the &lt;= comparison without locking us to a specific
/// seed's output.
/// </summary>
public class CombatResolverProbabilityBandTests
{
    private const int Trials = 10_000;
    private const int ToleranceBasisPoints = 200; // 2.0 percentage points


    [Fact]
    public void Hit_rate_for_a_standard_attempt_is_within_two_percent_of_the_estimate()
    {
        // SmallArms vs typical infantry defender → estimated 60%
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 1,
            defenderLuck: 1
        );

        AssertHitRateMatchesEstimate(attempt);
    }


    [Fact]
    public void Hit_rate_at_the_minimum_clamp_is_about_five_percent()
    {
        // Pile on evasion to trigger the lower clamp.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 50
        );

        AssertHitRateMatchesEstimate(attempt);
    }


    [Fact]
    public void Hit_rate_at_the_maximum_clamp_is_about_ninety_five_percent()
    {
        var weapon = CombatTestFixture.SmallArms() with { Accuracy = 100 };
        var attempt = CombatTestFixture.Attempt
        (
            weapon,
            defenderDexterity: 0,
            defenderLuck: 0
        );

        AssertHitRateMatchesEstimate(attempt);
    }


    [Fact]
    public void Hit_rate_drops_when_the_defender_takes_cover()
    {
        var openAttempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 1,
            defenderLuck: 1,
            defenderTerrain: Wolfgang.Conflict.Core.Catalog.Terrain.Plain
        );
        var coverAttempt = openAttempt with { DefenderTerrain = Wolfgang.Conflict.Core.Catalog.Terrain.Forest };

        var openRate = MeasureHitRate(openAttempt);
        var coverRate = MeasureHitRate(coverAttempt);

        Assert.True
        (
            coverRate < openRate,
            $"Cover hit rate ({coverRate}/{Trials}) should be less than open hit rate ({openRate}/{Trials})."
        );
    }


    private static void AssertHitRateMatchesEstimate(CombatAttempt attempt)
    {
        var estimate = CombatResolver.EstimateHitChance(attempt);
        var hits = MeasureHitRate(attempt);

        var observedBp = hits * 10000 / Trials; // observed × 100% in basis points
        var estimateBp = estimate * 100;
        var diff = Math.Abs(observedBp - estimateBp);

        Assert.True
        (
            diff <= ToleranceBasisPoints,
            $"Observed hit rate {hits}/{Trials} ({observedBp / 100.0:F2}%) deviates from estimate {estimate}% by more than {ToleranceBasisPoints / 100.0:F2}%."
        );
    }


    private static int MeasureHitRate(CombatAttempt attempt)
    {
        var hits = 0;
        for (var seed = 0; seed < Trials; seed++)
        {
            var result = CombatResolver.Resolve(attempt, new Random(seed));
            if (result.DidHit)
            {
                hits++;
            }
        }
        return hits;
    }
}
