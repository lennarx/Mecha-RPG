using Godot;

// First concrete effect: typed damage against armor type.
// [Export] makes these fields editable in the Godot Inspector.
[GlobalClass]
public partial class DamageEffect : EffectData
{
    [Export] public int Amount { get; set; } = CombatConstants.DefaultDamage;
    [Export] public DamageType Type { get; set; } = DamageType.Kinetic;

    public override void Apply(AttackContext ctx)
    {
        int finalDamage = Combat.CalculateDamage(Amount, Type, ctx.Target.Armor, ctx.Attacker.Tension, ctx.Target.Tension);
        ctx.Target.Hp -= finalDamage;
        ctx.Log.Add($"{ctx.Target.Name} takes {finalDamage} damage ({Type} vs {ctx.Target.Armor})");
    }
}
