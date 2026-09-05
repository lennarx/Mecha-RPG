// Pure C# — no Godot dependency. Testable outside the engine.
public enum DamageType
{
    Energy,
    Kinetic,
    Explosive
}

public enum ArmorType
{
    Light,
    Medium,
    Heavy
}

// Heat bands (tech doc 5.3). Cold [0-3], Optimal [4-7], Critical [8-9],
// Overload [== MaxTension]. Drives both offensive and defensive multipliers.
public enum HeatBand
{
    Cold,
    Optimal,
    Critical,
    Overload
}
