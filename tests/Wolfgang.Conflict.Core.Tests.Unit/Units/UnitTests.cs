using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Tests.Unit.Engine;

namespace Wolfgang.Conflict.Core.Tests.Unit.Units;

public class UnitTests
{
    [Fact]
    public void Constructor_keeps_hit_points_and_fuel()
    {
        var unit = GameEngineTestFixture.Infantry(1, GameEngineTestFixture.BlueFactionId, new HexCoord(0, 0)) with
        {
            CurrentHp = 7,
            CurrentFuel = 3
        };

        Assert.Equal(7, unit.CurrentHp);
        Assert.Equal(3, unit.CurrentFuel);
    }
}
