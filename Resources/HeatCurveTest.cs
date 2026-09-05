using Godot;
using System.Collections.Generic;

// Headless smoke test for the heat curve (tech doc 5.3): attach to a
// Node2D in its own scene and run with F6, same pattern as AttackTest.cs.
// Operates only on UnitState, Combat and WeaponData -- no grid, no
// Mission.cs, no mission scene. Every threshold/cost is read from
// CombatConstants so the test stays valid if balance is recalibrated.
public partial class HeatCurveTest : Node2D
{
    public override void _Ready()
    {
        var weapon = BuildTestWeapon();

        CheckBandProgressionAndClamp(weapon);
        CheckOverloadLockAndDissipation(weapon);
        CheckHeatDamageMultiplier();
    }

    // Built in code (not a .tres) so the test needs zero Inspector setup
    // and its cost/damage always match CombatConstants exactly.
    private static WeaponData BuildTestWeapon()
    {
        var weapon = new WeaponData
        {
            DisplayName = "Heat Curve Test Weapon",
            TensionCost = CombatConstants.DefaultTensionCost,
        };
        weapon.Effects.Add(new DamageEffect
        {
            Amount = CombatConstants.DefaultDamage,
            Type = DamageType.Kinetic,
        });
        return weapon;
    }

    // Check 1 & 2: repeated firing walks the heat bands in order
    // (Cold -> Optimal -> Critical -> Overload) and heat never exceeds
    // MaxTension, even when fired past the cap.
    private static void CheckBandProgressionAndClamp(WeaponData weapon)
    {
        GD.Print("--- Heat band progression & clamping ---");

        var attacker = new UnitState { Name = "HeatTestAttacker" };
        var target = new UnitState { Name = "HeatTestTarget", Hp = int.MaxValue / 2, Armor = ArmorType.Medium };

        int lastBandValue = (int)Combat.GetHeatBand(attacker.Tension);
        bool orderIsIncreasing = true;
        bool everExceededMax = false;

        // Safety bound only, not a balance threshold: guarantees the loop
        // ends even if TensionCost were ever recalibrated to something tiny.
        int safetyLimit = CombatConstants.MaxTension + 5;
        int shotsFired = 0;

        while (attacker.Tension < CombatConstants.MaxTension && shotsFired < safetyLimit)
        {
            weapon.ResolveAttack(attacker, target, new List<string>());
            shotsFired++;

            if (attacker.Tension > CombatConstants.MaxTension)
                everExceededMax = true;

            var band = Combat.GetHeatBand(attacker.Tension);
            int bandValue = (int)band;
            if (bandValue != lastBandValue)
            {
                GD.Print($"  shot {shotsFired}: heat {attacker.Tension} -> {band}");
                if (bandValue < lastBandValue)
                    orderIsIncreasing = false;
                lastBandValue = bandValue;
            }
        }

        bool reachedOverload = attacker.IsOverloaded;
        Check("Repeated firing walks bands in order and reaches Overload",
            orderIsIncreasing && reachedOverload,
            "bands only increase, ending in Overload",
            $"orderIsIncreasing={orderIsIncreasing}, finalBand={Combat.GetHeatBand(attacker.Tension)}");

        // Fire a couple more shots past the cap to prove the clamp holds
        // once already at max, not just that we never happened to overshoot.
        weapon.ResolveAttack(attacker, target, new List<string>());
        weapon.ResolveAttack(attacker, target, new List<string>());

        Check("Heat is clamped at MaxTension and never exceeds it",
            !everExceededMax && attacker.Tension == CombatConstants.MaxTension,
            $"{CombatConstants.MaxTension}",
            $"{attacker.Tension} (everExceededMax={everExceededMax})");
    }

    // Check 3 & 4: a unit that reaches max tension holds there instead of
    // dissipating the turn it got there; the following turn locks it out,
    // and dissipating that blocked turn applies the larger,
    // no-actions-taken rate.
    private static void CheckOverloadLockAndDissipation(WeaponData weapon)
    {
        GD.Print("--- Overload lock & recovery dissipation ---");

        var unit = new UnitState { Name = "HeatTestLockUnit" };
        var target = new UnitState { Name = "HeatTestLockTarget", Hp = int.MaxValue / 2, Armor = ArmorType.Medium };

        unit.BeginTurn();
        // Fast-forward heat to one shot away from the cap, then fire that
        // shot so overload is reached mid-turn, as it would in a real match.
        unit.Tension = CombatConstants.MaxTension - weapon.TensionCost;
        weapon.ResolveAttack(unit, target, new List<string>());

        Check("Firing the shot that fills tension reaches Overload",
            unit.IsOverloaded,
            "IsOverloaded=true",
            $"IsOverloaded={unit.IsOverloaded}, heat={unit.Tension}");

        unit.DissipateHeat(); // end of the turn it just overloaded on
        Check("A unit that just overloaded does not dissipate this turn",
            unit.Tension == CombatConstants.MaxTension,
            $"{CombatConstants.MaxTension}",
            $"{unit.Tension}");

        unit.BeginTurn(); // next turn: should now be locked out
        bool lockedOut = !unit.CanMove && !unit.CanAttack;
        Check("The turn after overload locks move and attack",
            lockedOut,
            "CanMove=false, CanAttack=false",
            $"CanMove={unit.CanMove}, CanAttack={unit.CanAttack}");

        unit.DissipateHeat(); // end of the blocked turn: no actions were possible
        int expectedHeat = System.Math.Max(0, CombatConstants.MaxTension - CombatConstants.NoActionHeatDissipation);
        Check("The blocked turn dissipates at the no-action rate",
            unit.Tension == expectedHeat,
            $"{expectedHeat} (Max {CombatConstants.MaxTension} - NoActionHeatDissipation {CombatConstants.NoActionHeatDissipation})",
            $"{unit.Tension}");
    }

    // Check 5: the same attack deals more damage when the attacker is in a
    // hotter band. Uses Energy vs Medium armor, which has no entry in
    // Combat's armor-multiplier table (Neutral, 1.0), so the only variable
    // between the two calculations is the offensive heat multiplier.
    private static void CheckHeatDamageMultiplier()
    {
        GD.Print("--- Heat band damage multiplier ---");

        const DamageType isolatingType = DamageType.Energy;
        const ArmorType isolatingArmor = ArmorType.Medium;
        int baseDamage = CombatConstants.DefaultDamage;

        int coldDamage = Combat.CalculateDamage(baseDamage, isolatingType, isolatingArmor, attackerTension: 0, targetTension: 0);
        int optimalDamage = Combat.CalculateDamage(baseDamage, isolatingType, isolatingArmor, attackerTension: CombatConstants.OptimalHeatThreshold, targetTension: 0);

        Check("Attacking from Optimal deals more damage than from Cold",
            optimalDamage > coldDamage,
            "optimalDamage > coldDamage",
            $"cold={coldDamage}, optimal={optimalDamage}");
    }

    private static void Check(string description, bool condition, string expected, string actual)
    {
        string status = condition ? "PASS" : "FAIL";
        GD.Print($"[{status}] {description} (expected: {expected} | actual: {actual})");
    }
}
