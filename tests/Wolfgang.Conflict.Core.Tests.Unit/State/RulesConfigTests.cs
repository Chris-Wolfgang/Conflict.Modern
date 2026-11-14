using Wolfgang.Conflict.Core.State;

namespace Wolfgang.Conflict.Core.Tests.Unit.State;

public class RulesConfigTests
{
    [Fact]
    public void Default_is_classic_igougo_with_explored_fog_and_shared_identity_revealing_vision()
    {
        var rules = RulesConfig.Default;

        Assert.Equal(TurnOrderMode.Igougo, rules.TurnOrder);
        Assert.Equal(UnitActionPolicy.Classic, rules.UnitActionPolicy);
        Assert.Equal(FogMode.Explored, rules.FogMode);
        Assert.True(rules.DetectionRevealsIdentity);
        Assert.True(rules.SharedVisionPerFaction);
    }


    [Fact]
    public void Init_keeps_every_selected_rule()
    {
        var rules = new RulesConfig
        {
            TurnOrder = TurnOrderMode.AlternatingActivation,
            UnitActionPolicy = UnitActionPolicy.FreeFlow,
            FogMode = FogMode.Strict,
            DetectionRevealsIdentity = false,
            SharedVisionPerFaction = false
        };

        Assert.Equal(TurnOrderMode.AlternatingActivation, rules.TurnOrder);
        Assert.Equal(UnitActionPolicy.FreeFlow, rules.UnitActionPolicy);
        Assert.Equal(FogMode.Strict, rules.FogMode);
        Assert.False(rules.DetectionRevealsIdentity);
        Assert.False(rules.SharedVisionPerFaction);
    }
}
