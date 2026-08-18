using Godot;

// Base class for all effects. A weapon does not "deal damage":
// a weapon HAS a list of effects, and one of them deals damage.
// Tomorrow: KnockbackEffect, OverheatEffect, RepairEffect... without
// touching WeaponData and without writing any switch.
// This is the "composable effect" from the tech doc (5.1).
[GlobalClass]
public partial class EffectData : Resource
{
    public virtual void Apply(AttackContext ctx)
    {
        // Base does nothing. Each concrete effect overrides this.
    }
}
