using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Combat;

namespace Wolfgang.Conflict.Core.Tests.Unit.Combat;

public class CombatResolverResolveTests
{
    [Fact]
    public void Resolve_consumes_exactly_one_RNG_step()
    {
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms());

        var result = CombatResolver.Resolve(attempt, new Random(1));

        Assert.Equal(1, result.RngStepsConsumed);
    }


    [Fact]
    public void Resolve_returns_the_estimated_hit_chance_unchanged()
    {
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms());

        var estimate = CombatResolver.EstimateHitChance(attempt);
        var result = CombatResolver.Resolve(attempt, new Random(1));

        Assert.Equal(estimate, result.HitChancePercent);
    }


    [Fact]
    public void Resolve_records_the_roll_in_the_inclusive_one_to_one_hundred_range()
    {
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms());

        var result = CombatResolver.Resolve(attempt, new Random(12345));

        Assert.InRange(result.HitRoll, 1, 100);
    }


    [Fact]
    public void Resolve_marks_DidHit_true_when_roll_is_within_hit_chance()
    {
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms());

        var result = CombatResolver.Resolve(attempt, new Random(1));

        Assert.Equal(result.HitRoll <= result.HitChancePercent, result.DidHit);
    }


    [Fact]
    public void Resolve_returns_zero_damage_on_a_miss()
    {
        // Force a near-impossible hit by giving the defender huge evasion.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 50
        );

        // Try several seeds; the 5% floor means *some* will hit. Pick one that misses.
        for (var seed = 0; seed < 20; seed++)
        {
            var result = CombatResolver.Resolve(attempt, new Random(seed));
            if (!result.DidHit)
            {
                Assert.Equal(0, result.Damage);
                return;
            }
        }

        Assert.Fail("Expected at least one miss within 20 seeds.");
    }


    [Fact]
    public void Resolve_deals_raw_damage_minus_armor_on_a_hit_against_a_known_armor_class()
    {
        // Sabot vs HeavyArmor: raw 16, defender armor 6 → 10 damage on hit.
        // Defender Dex=1, Luck=1 → hit chance 60 − 10 = 50; pick a seed that hits.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.TankSabot(),
            defenderArmorClass: ArmorClass.HeavyArmor,
            defenderArmor: 6,
            defenderDexterity: 1,
            defenderLuck: 1
        );

        for (var seed = 0; seed < 20; seed++)
        {
            var result = CombatResolver.Resolve(attempt, new Random(seed));
            if (result.DidHit)
            {
                Assert.Equal(16 - 6, result.Damage);
                return;
            }
        }

        Assert.Fail("Expected at least one hit within 20 seeds.");
    }


    [Fact]
    public void Resolve_deals_zero_damage_when_weapon_has_no_entry_for_defender_armor_class()
    {
        // Small arms vs HeavyArmor: no entry → raw 0 → final 0 even on a hit.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderArmorClass: ArmorClass.HeavyArmor,
            defenderArmor: 6,
            defenderDexterity: 1,
            defenderLuck: 1
        );

        for (var seed = 0; seed < 20; seed++)
        {
            var result = CombatResolver.Resolve(attempt, new Random(seed));
            if (result.DidHit)
            {
                Assert.Equal(0, result.Damage);
                return;
            }
        }

        Assert.Fail("Expected at least one hit within 20 seeds.");
    }


    [Fact]
    public void Resolve_deals_zero_damage_when_armor_exceeds_raw_damage()
    {
        // Small arms vs LightArmor: raw 2, defender armor 5 → max(0, -3) = 0.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderArmorClass: ArmorClass.LightArmor,
            defenderArmor: 5,
            defenderDexterity: 1,
            defenderLuck: 1
        );

        for (var seed = 0; seed < 20; seed++)
        {
            var result = CombatResolver.Resolve(attempt, new Random(seed));
            if (result.DidHit)
            {
                Assert.Equal(0, result.Damage);
                return;
            }
        }

        Assert.Fail("Expected at least one hit within 20 seeds.");
    }


    [Fact]
    public void Resolve_throws_when_attempt_is_out_of_weapon_range()
    {
        // Small arms range is [1,1]; distance 3 is out of range.
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms(), distance: 3);

        Assert.Throws<InvalidOperationException>
        (
            () => CombatResolver.Resolve(attempt, new Random(1))
        );
    }


    [Fact]
    public void Resolve_throws_when_attacker_has_no_line_of_sight()
    {
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms(), hasLineOfSight: false);

        Assert.Throws<InvalidOperationException>
        (
            () => CombatResolver.Resolve(attempt, new Random(1))
        );
    }


    [Fact]
    public void Resolve_throws_when_attempt_is_null()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => CombatResolver.Resolve(null!, new Random(1))
        );
    }


    [Fact]
    public void Resolve_throws_when_rng_is_null()
    {
        var attempt = CombatTestFixture.Attempt(CombatTestFixture.SmallArms());

        Assert.Throws<ArgumentNullException>
        (
            () => CombatResolver.Resolve(attempt, null!)
        );
    }


    [Fact]
    public void Resolve_for_air_to_air_engagement_at_long_range_is_legal()
    {
        // AMRAAM range [2,6]; distance 4 is legal.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.AirToAirMissile(),
            defenderArmorClass: ArmorClass.Aircraft,
            defenderArmor: 1,
            defenderDexterity: 5,
            defenderLuck: 2,
            distance: 4
        );

        var result = CombatResolver.Resolve(attempt, new Random(1));

        Assert.Equal(1, result.RngStepsConsumed);
    }
}
