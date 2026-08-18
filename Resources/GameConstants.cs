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
    public const int DefaultUnitHp = 30;
    public const int DefaultWeaponRange = 3;
    public const int DefaultTensionCost = 2;
    public const int DefaultDamage = 10;
    public const int DefaultMoveRange = 4;

    // Damage-vs-armor multipliers (tech doc section 5.3).
    public const float NeutralMultiplier = 1.0f;
    public const float StrongMultiplier = 1.5f;
    public const float SlightAdvantageMultiplier = 1.25f;
    public const float SlightPenaltyMultiplier = 0.75f;
    public const float WeakMultiplier = 0.5f;
}
