using Wolfgang.Conflict.Core.Catalog;
using Wolfgang.Conflict.Core.Engine;
using Wolfgang.Conflict.Core.Hex;
using Wolfgang.Conflict.Core.Map;
using Wolfgang.Conflict.Core.State;
using Wolfgang.Conflict.Core.Units;
using GameUnit = Wolfgang.Conflict.Core.Units.Unit;

namespace Wolfgang.Conflict.Core.Tests.Unit.Engine;

/// <summary>
/// Builds a minimal two-faction game state for engine tests so each test
/// stays focused on the rule it exercises, not on setup boilerplate.
/// </summary>
internal static class GameEngineTestFixture
{
    public const string BlueFactionId = "blue";
    public const string RedFactionId = "red";
    public const string InfantryUnitTypeId = "test-infantry";


    public static UnitTypeDefinition InfantryDefinition() => new
    (
        Id: InfantryUnitTypeId,
        DisplayName: "Test Infantry",
        Archetype: UnitArchetype.Infantry,
        MaxHp: 10,
        FuelCapacity: 0,
        SightRange: 2,
        MovementPoints: 3,
        ArmorClass: ArmorClass.Infantry,
        Armor: 1,
        Dexterity: 3,
        Luck: 1,
        CanCapture: true,
        CanFly: false,
        WeaponSystemIds: [],
        TerrainMovementCosts: new Dictionary<Terrain, int>
        {
            [Terrain.Plain] = 1,
            [Terrain.Hills] = 2,
            [Terrain.Forest] = 2,
            [Terrain.Urban] = 1
        }
    );


    public static FactionDefinition Faction(string id) => new
    (
        Id: id,
        DisplayName: id,
        Color: "#000000",
        WeaponSystems: new Dictionary<string, WeaponSystemDefinition>(),
        UnitTypes: new Dictionary<string, UnitTypeDefinition>
        {
            [InfantryUnitTypeId] = InfantryDefinition()
        }
    );


    public static GameEngine CreateEngine() =>
        new
        (
            new Dictionary<string, FactionDefinition>
            {
                [BlueFactionId] = Faction(BlueFactionId),
                [RedFactionId] = Faction(RedFactionId)
            }
        );


    /// <summary>
    /// 6×6 plain-terrain map, blue side first, optional pre-placed units.
    /// </summary>
    public static GameState CreateState(params GameUnit[] units) => new
    (
        Map: HexMap.OfRectangle(6, 6, Terrain.Plain),
        Units: units.ToDictionary(u => u.Id),
        SideOrder: [BlueFactionId, RedFactionId],
        CurrentSideIndex: 0,
        TurnNumber: 1,
        Rules: RulesConfig.Default,
        RngSeed: 42,
        RngStep: 0
    );


    public static GameUnit Infantry(int id, string factionId, HexCoord position) => new
    (
        Id: id,
        UnitTypeId: InfantryUnitTypeId,
        OwnerFactionId: factionId,
        Position: position,
        CurrentHp: 10,
        CurrentFuel: 0,
        HasMoved: false,
        HasAttacked: false,
        Status: UnitStatus.Active
    );
}
