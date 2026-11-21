using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Combat;

namespace Wolfgang.Conflict.Core.Tests.Unit.Combat;

public class CombatResolverEstimateTests
{
    [Fact]
    public void EstimateHitChance_subtracts_5_per_dexterity_point()
    {
        // Accuracy 70, Dex 3, Luck 0, Plain terrain → 70 − 5*(3+0) − 0 = 55
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 3,
            defenderLuck: 0,
            defenderTerrain: Terrain.Plain
        );

        Assert.Equal(55, CombatResolver.EstimateHitChance(attempt));
    }


    [Fact]
    public void EstimateHitChance_subtracts_5_per_luck_point()
    {
        // Accuracy 70, Dex 0, Luck 2, Plain → 70 − 10 − 0 = 60
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 0,
            defenderLuck: 2,
            defenderTerrain: Terrain.Plain
        );

        Assert.Equal(60, CombatResolver.EstimateHitChance(attempt));
    }


    [Fact]
    public void EstimateHitChance_subtracts_terrain_cover_in_addition_to_evasion()
    {
        // Accuracy 70, Dex 3, Luck 1, Forest (15) → 70 − 20 − 15 = 35
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 3,
            defenderLuck: 1,
            defenderTerrain: Terrain.Forest
        );

        Assert.Equal(35, CombatResolver.EstimateHitChance(attempt));
    }


    [Fact]
    public void EstimateHitChance_clamps_to_minimum_when_evasion_is_overwhelming()
    {
        // Accuracy 70, Dex 50 → would be way negative; clamped to MinHitChancePercent.
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderDexterity: 50,
            defenderLuck: 0,
            defenderTerrain: Terrain.Plain
        );

        Assert.Equal(CombatResolver.MinHitChancePercent, CombatResolver.EstimateHitChance(attempt));
    }


    [Fact]
    public void EstimateHitChance_clamps_to_maximum_when_defender_is_a_sitting_duck()
    {
        // Accuracy 100 weapon vs zero-evasion target on plain → would be 100; clamped to Max.
        var weapon = CombatTestFixture.SmallArms() with { Accuracy = 100 };
        var attempt = CombatTestFixture.Attempt
        (
            weapon,
            defenderDexterity: 0,
            defenderLuck: 0,
            defenderTerrain: Terrain.Plain
        );

        Assert.Equal(CombatResolver.MaxHitChancePercent, CombatResolver.EstimateHitChance(attempt));
    }


    [Fact]
    public void EstimateHitChance_for_infantry_vs_tank_at_close_range_is_high()
    {
        // Plan example: infantry has *good* odds of hitting a tank up close
        // (big slow target). M4 vs heavy armor target with Dex=1, Luck=1, Plain.
        // 70 − 5*2 − 0 = 60
        var attempt = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderArmorClass: ArmorClass.HeavyArmor,
            defenderArmor: 6,
            defenderDexterity: 1,
            defenderLuck: 1,
            defenderTerrain: Terrain.Plain
        );

        var hitChance = CombatResolver.EstimateHitChance(attempt);
        Assert.True(hitChance >= 50, $"Expected >= 50, got {hitChance}");
    }


    [Fact]
    public void EstimateHitChance_for_tank_vs_infantry_at_close_range_is_lower_than_infantry_vs_tank()
    {
        // Plan example: tank has *low* odds of hitting infantry (small dispersed target).
        // Sabot has Accuracy=60; defender infantry Dex=3, Luck=1, Plain → 60 − 20 = 40.
        var infantryDefenderForTank = CombatTestFixture.Attempt
        (
            CombatTestFixture.TankSabot(),
            defenderArmorClass: ArmorClass.Infantry,
            defenderArmor: 1,
            defenderDexterity: 3,
            defenderLuck: 1
        );
        var tankDefenderForInfantry = CombatTestFixture.Attempt
        (
            CombatTestFixture.SmallArms(),
            defenderArmorClass: ArmorClass.HeavyArmor,
            defenderArmor: 6,
            defenderDexterity: 1,
            defenderLuck: 1
        );

        var hitTankToInfantry = CombatResolver.EstimateHitChance(infantryDefenderForTank);
        var hitInfantryToTank = CombatResolver.EstimateHitChance(tankDefenderForInfantry);

        Assert.True
        (
            hitTankToInfantry < hitInfantryToTank,
            $"Tank vs infantry ({hitTankToInfantry}) should be lower than infantry vs tank ({hitInfantryToTank})."
        );
    }


    [Fact]
    public void EstimateHitChance_throws_when_attempt_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => CombatResolver.EstimateHitChance(null!));
    }
}
