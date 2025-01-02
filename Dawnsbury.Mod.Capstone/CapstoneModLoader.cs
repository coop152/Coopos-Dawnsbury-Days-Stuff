using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Animations;
using Dawnsbury.Audio;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;

namespace Dawnsbury.Mods.Capstone;


public static class CapstoneModLoader
{
    static readonly Trait CapstoneTrait = ModManager.RegisterTrait("Capstone");

    static SpellId CataclysmId;

    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
#if DEBUG || DEBUG_V2
        //Debugger.Launch();
#endif
#if DAWNSBURY_V2
        ModManager.AssertV2();
#else
        ModManager.AssertV3();
#endif

        AddFeats(GetFeats());

        QEffectId storageEffect = ModManager.RegisterEnumMember<QEffectId>("CataclysmGettingTargetsWithThisReallyLongEnumMemberNameThatWillNotPossiblyBeRepeatedAnywhereElse");

        CataclysmId = ModManager.RegisterNewSpell("Cataclysm",
            10,
            (SpellId id, Creature? caster, int level, bool inCombat, SpellInformation spellInfo) =>
            {
                return Spells.CreateModern(IllustrationName.ExtractElements,
                    "Cataclysm",
                    [Trait.Acid, Trait.Air, Trait.Cold, Trait.Concentrate, Trait.Earth, Trait.Electricity, Trait.Fire, Trait.Manipulate, Trait.Water, Trait.Arcane],
                    "You call upon the unimaginable power of world-ending cataclysms, ripping a small piece of each cataclysm and combining them together into one horrifically powerful attack.",
                    "The following effects come down upon all creatures in a 60-foot burst. Treat the resistances of creatures in the area as if they were 10 lower for the purpose of determining the cataclysm's damage. Each creature attempts one basic Reflex save that applies to all five types of damage.\n\n• Flesh-dissolving acid rain deals 3d10 acid damage.\n• A roaring earthquake shakes and bludgeons creatures on the ground, dealing 3d10 bludgeoning damage.\n• A blast of freezing wind deals 3d10 cold damage.\n• Incredible lightning lashes the area, dealing 3d10 electricity damage.\n• An instant tsunami sweeps over creatures in the area, dealing 3d10 bludgeoning damage.\n• A massive wildfire burns in a sudden inferno, dealing 3d10 fire damage.",
                    Target.Burst(1000/5, 60/5),
                    level,
                    SpellSavingThrow.Basic(Defense.Reflex)
                    ).WithEffectOnEachTarget(async (CombatAction spell, Creature caster, Creature target, CheckResult checkResult) =>
                    {
                        // store targets and check results for WithEffectOnChosenTargets
                        QEffect? storage = caster.FindQEffect(storageEffect);
                        List<(Creature, CheckResult)> targets = [];
                        if (storage == null)
                        {
                            caster.AddQEffect(new QEffect()
                            {
                                Id = storageEffect,
                                Tag = targets
                            });
                        } else
                        {
                            // the null thing is probably unnecessary but it kept complaining about nullability, so
                            targets = (storage.Tag as List<(Creature, CheckResult)>) ?? targets;
                        }
                        targets.Add((target, checkResult));
                    }).WithEffectOnChosenTargets(async (CombatAction spell, Creature caster, ChosenTargets chosenTargets) =>
                    {
                        var storage = caster.FindQEffect(storageEffect);
                        if (storage == null || storage.Tag == null) { return; }
                        List<(Creature, CheckResult)> targets = (storage.Tag as List<(Creature, CheckResult)>) ?? [];
                        Point origin = chosenTargets.ChosenPointOfOrigin;
                        List<Tile> particleTargets = chosenTargets.ChosenTiles;

                        List<(IllustrationName, DamageKind, SfxName)> damageTypes = [
                            (IllustrationName.AcidSplash, DamageKind.Acid, SfxName.DeepNecromancy),
                            (IllustrationName.Rock, DamageKind.Bludgeoning, SfxName.Tremor),
                            (IllustrationName.RayOfFrost, DamageKind.Cold, SfxName.RayOfFrost),
                            (IllustrationName.LightningBolt, DamageKind.Electricity, SfxName.ElectricBlast),
                            (IllustrationName.Water, DamageKind.Bludgeoning, SfxName.ElementalBlastWater),
                            (IllustrationName.PersistentFire, DamageKind.Fire, SfxName.Fireball),
                        ];

                        var sfxRef = Sfxs.Play(SfxName.PureEnergyRelease);
                        await CommonAnimations.CreateConeAnimation(caster.Battle, caster.Occupies.ToCenterVector(), [new Tile(origin.X, origin.Y, caster.Battle)], 100, ProjectileKind.Arrow, IllustrationName.None);
                        await Task.Delay(500);
                        foreach ((IllustrationName illustration, DamageKind damageKind, SfxName sfx) in damageTypes)
                        {
                            var rand = new Random();
                            //var randomPoint = origin + new Point( rand.Next(-1,1) , rand.Next(-1, 1));
                            var randomPoint = origin.ToVector2() + new Vector2(rand.NextSingle() - 0.5f, rand.NextSingle() - 0.5f);
                            Sfxs.Play(sfx, 1.2f);
                            await CommonAnimations.CreateConeAnimation(caster.Battle, randomPoint, particleTargets, 25, ProjectileKind.Cone, illustration);
                            await Task.Delay(100);
                            await Task.WhenAll(
                                targets.Select(
                                    ((Creature, CheckResult) target) => {
                                        target.Item1.WeaknessAndResistance.GetAndApplyResistance(spell, damageKind, 10); // use up 10 points of resistance
                                        return CommonSpellEffects.DealBasicDamage(spell, caster, target.Item1, target.Item2, "3d10", damageKind);
                                    }
                                    )
                                );
                        }
                    }).WithProjectileCone(IllustrationName.None, 0, ProjectileKind.None);
            });
    }

    static void AddFeats(IEnumerable<Feat> feats)
    {
        foreach (Feat f in feats)
        {
            ModManager.AddFeat(f);
        }
    }

    static IEnumerable<Feat> GetFeats()
    {
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Capstone"),
            1,
            "You don't mind cheating a little to get the upper hand on your enemies.",
            "You gain a 20th-level class feat.",
            [CapstoneTrait, Trait.Wizard]
            ).WithOnSheet((sheet) =>
            {
                
                sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption("capstoneFeat", "Capstone Feat", 1, (chosen) => chosen.HasTrait(CapstoneTrait)));
            });
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Archwizard's Spellcraft"),
            -1,
            "You command the most potent arcane magic and can cast a spell of truly incredible power.",
            "You gain a single 10th-rank spell slot, with which you can cast the spell 'Cataclysm'.",
            [CapstoneTrait]
            );
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Archwizard"),
            -1,
            "You command the most potent arcane magic and can cast spells of truly incredible power.",
            "Your proficiency rank for spellcasting increases to Legendary. You gain 3 spell slots of each spell level, regardless of your character level. You can cast one 10th level spell per day.",
            [CapstoneTrait]
            ).WithOnSheet((sheet) =>
            {
                sheet.Sheet.MaximumLevel = 20;
                sheet.CurrentLevel = 20;
                sheet.SetProficiency(Trait.Spell, Proficiency.Legendary);

                int slotsOfLevel(int spellRank) => sheet.PreparedSpells[Trait.Wizard].Slots.Where(slot => slot.SpellLevel == spellRank).Count();
                for (int level = 1; level <= 20; level++)
                {
                    int spellRank = (int)Math.Ceiling((double)level / 2);
                    if ((level == 19 || level == 20) && slotsOfLevel(10) != 1)
                    {
                        sheet.PreparedSpells[Trait.Wizard].Slots.Add(new FreePreparedSpellSlot(10, "Wizard:Spell10-1"));
                    }
                    else if (level % 2 == 1 && slotsOfLevel(spellRank) == 0)
                    {
                        sheet.PreparedSpells[Trait.Wizard].Slots.Add(new FreePreparedSpellSlot(spellRank, $"Wizard:Spell{spellRank}-1"));
                        sheet.PreparedSpells[Trait.Wizard].Slots.Add(new FreePreparedSpellSlot(spellRank, $"Wizard:Spell{spellRank}-2"));
                    }
                    else if (level % 2 == 0 && slotsOfLevel(spellRank) == 2)
                    {
                        sheet.PreparedSpells[Trait.Wizard].Slots.Add(new FreePreparedSpellSlot(spellRank, $"Wizard:Spell{spellRank}-3"));
                    }
                }                
            }).WithOnCreature((Creature cr) =>
            {
                // TODO: remove this
                cr.AddQEffect(new QEffect()
                {
                    BonusToInitiative = (QEffect self) => new Bonus(20, BonusType.Untyped, "this fucker goes first")
                });
            });
    }
}
