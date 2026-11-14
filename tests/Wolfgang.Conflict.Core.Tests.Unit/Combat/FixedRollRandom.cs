namespace Wolfgang.Conflict.Core.Tests.Unit.Combat;

/// <summary>
/// A <see cref="Random"/> whose ranged <see cref="Random.Next(int, int)"/> always returns
/// the same roll, so a test can force a hit (roll 1) or a miss (roll 100) without
/// searching seeds.
/// </summary>
internal sealed class FixedRollRandom : Random
{
    private readonly int _roll;


    public FixedRollRandom(int roll)
    {
        _roll = roll;
    }


    public override int Next(int minValue, int maxValue) => _roll;
}
