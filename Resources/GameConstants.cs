// Single source of truth for names and tunable values.
// When the real character names are decided (open item in the story doc),
// change them HERE and nowhere else. No string literals in game logic.
public static class CharacterNames
{
    // Placeholders — story doc section 9: proper names still undecided.
    public const string Protagonist = "PROTAGONIST_PLACEHOLDER";
    public const string ClassTopStudent = "TOP_STUDENT_PLACEHOLDER";   // "la mejor de la clase"
    public const string EliteBrother = "ELITE_BROTHER_PLACEHOLDER";    // hermano menor de uno de los Siete
    public const string Technician = "TECHNICIAN_PLACEHOLDER";         // la técnica sin pilotaje
    public const string Father = "FATHER_PLACEHOLDER";
    public const string RebellionLeader = "REBELLION_LEADER_PLACEHOLDER";

    // Fixed, non-story opponent used by technical prototypes/vertical slices.
    // Deliberately separate from TheSeven so a real boss slot isn't spent on a test scene.
    public const string TrainingDummy = "TRAINING_DUMMY_PLACEHOLDER";

    // The Seven — placeholder array, one slot per boss.
    public static readonly string[] TheSeven =
    {
        "SEVEN_1_PLACEHOLDER",
        "SEVEN_2_PLACEHOLDER",
        "SEVEN_3_PLACEHOLDER",
        "SEVEN_4_PLACEHOLDER",
        "SEVEN_5_PLACEHOLDER",
        "SEVEN_6_PLACEHOLDER",
        "SEVEN_7_PLACEHOLDER",
    };
}

public static class CombatConstants
{
    // Default stats — placeholder balance values, tune freely.
    public const int DefaultUnitHp = 60;

    // Training Dummy is a disposable test opponent (CharacterNames.TrainingDummy),
    // calibrated separately from DefaultUnitHp: the heat curve needs at least
    // 6 exchanges to be traversed end to end (tech doc 5.3), so its HP is set
    // higher than a real playable unit's just to make the test mission last
    // long enough for Overload to become reachable.
    public const int TrainingDummyHp = 90;
    public const int DefaultWeaponRange = 3;
    public const int DefaultTensionCost = 3;
    public const int DefaultDamage = 10;
    public const int DefaultMoveRange = 4;

    // Heat costs and dissipation (tech doc 5.3).
    public const int MoveHeatCost = 1;
    public const int HeatDissipationPerTurn = 1;
    public const int NoActionHeatDissipation = 4;

    // Heat band thresholds and cap (tech doc 5.3).
    public const int OptimalHeatThreshold = 4;
    public const int CriticalHeatThreshold = 8;
    public const int MaxTension = 10;

    // Heat band damage multipliers (tech doc 5.3).
    public const float HeatDamageBonusMultiplier = 1.3f;
    public const float HeatDefensePenaltyMultiplier = 1.5f;

    // Damage-vs-armor multipliers (tech doc section 5.3).
    public const float NeutralMultiplier = 1.0f;
    public const float StrongMultiplier = 1.5f;
    public const float SlightAdvantageMultiplier = 1.25f;
    public const float SlightPenaltyMultiplier = 0.75f;
    public const float WeakMultiplier = 0.5f;
}
