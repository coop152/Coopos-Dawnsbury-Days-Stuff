using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;

using Dawnsbury.Mods.BattleHarbinger.RegisteredValues;
using Dawnsbury.Core.Animations.AuraAnimations;

namespace Dawnsbury.Mods.BattleHarbinger
{
    internal class ModSpells
    {
        static readonly Illustration BenedictionArt = new ModdedIllustration("BattleHarbingerAssets/benediction.png");
        static readonly Illustration MaledictionArt = new ModdedIllustration("BattleHarbingerAssets/malediction.png");
        static readonly ModdedIllustration BenedictionCircleArt = new ModdedIllustration("BattleHarbingerAssets/benedictionCircle.png");
        static readonly ModdedIllustration MaledictionCircleArt = new ModdedIllustration("BattleHarbingerAssets/maledictionCircle.png");

        public static void RegisterSpells()
        {
            ModSpellId.BattleBenediction = ModManager.RegisterNewSpell("BattleHarbinger:Benediction", 1, (SpellId spellId, Creature? caster, int level, bool inCombat, SpellInformation spellInfo) =>
            {
                return BattleBenediction(level, caster);
            });

            ModSpellId.BattleMalediction = ModManager.RegisterNewSpell("BattleHarbinger:Malediction", 1, (SpellId spellId, Creature? caster, int level, bool inCombat, SpellInformation spellInfo) =>
            {
                return BattleMalediction(level, caster);
            });
            ModSpellId.BattleBless = ModManager.RegisterNewSpell("BattleHarbinger:Bless", 1, (SpellId spellId, Creature? caster, int level, bool inCombat, SpellInformation spellInfo) =>
            {
                return BattleBless(level, caster);
            });

            ModSpellId.BattleBane = ModManager.RegisterNewSpell("BattleHarbinger:Bane", 1, (SpellId spellId, Creature? caster, int level, bool inCombat, SpellInformation spellInfo) =>
            {
                return BattleBane(level, caster);
            });
        }

        public static CombatAction BattleBenediction(int level, Creature? caster)
        {
            List<Trait> traits = [Trait.Enchantment, Trait.Mental, ModTrait.BattleAura];
            return Spells.CreateModern(BenedictionArt,
                "Battle Benediction",
                [..traits],
                "Divine protection helps protect your companions.",
                "For the rest of the encounter, you and your allies gain a +1 status bonus to AC while within a 15-foot-radius emanation around you.\n\nOnce per turn, starting the turn after you cast benediction, you can Sustain the spell to increase the emanation's radius by 10 feet.",
                Target.Self(),
                level,
                null).WithSoundEffect(SfxName.Bless)
                .WithEffectOnSelf(async (CombatAction action, Creature cr) =>
                {
                    BeneMaledictionEffectOnSelf(action, cr, BenedictionArt, true, 3, 2);
                });
        }

        public static CombatAction BattleMalediction(int level, Creature? caster)
        {
            List<Trait> traits = [Trait.Enchantment, Trait.Mental, ModTrait.BattleAura];
            return Spells.CreateModern(MaledictionArt,
                "Battle Malediction",
                [..traits],
                "You incite distress in the minds of your enemies, making it more difficult for them to defend themselves.",
                "You create a 10-foot-radius emanation around you. Enemies within it must succeed at a Will save or take a –1 status penalty to AC as long as they are in the area.\n\nAn enemy who failed and then leaves the area and reenters later is automatically affected again. An enemy who enters the area for the first time rolls a Will save as they enter.\n\nOnce per turn, starting the turn after you cast malediction, you can Sustain the spell to increase the emanation's radius by 10 feet and force enemies in the area that weren't yet affected to attempt another saving throw.",
                Target.Self(),
                level,
                null).WithSoundEffect(SfxName.Fear)
                .WithEffectOnSelf(async (CombatAction action, Creature cr) =>
                {
                    BeneMaledictionEffectOnSelf(action, cr, MaledictionArt, false, 2, 2);
                });
        }

        public static CombatAction BattleBless(int level, Creature? caster)
        {
            List<Trait> traits = [Trait.Enchantment, Trait.Mental, ModTrait.BattleAura];
            return Spells.CreateModern(IllustrationName.Bless,
                "Battle Bless",
                [..traits],
                "Blessings from beyond help your companions strike true.",
                "For the rest of the encounter, you and your allies gain a +1 status bonus to attack rolls while within a 15-foot-radius emanation around you.\n\nOnce per turn, starting the turn after you cast bless, you can Sustain the spell to increase the emanation's radius by 10 feet.",
                Target.Self(),
                level,
                null).WithSoundEffect(SfxName.Bless)
                .WithEffectOnSelf(async (CombatAction action, Creature cr) =>
                {
                    BlessBaneEffectOnSelf(action, cr, IllustrationName.Bless, true, 3, 2);
                });
        }

        public static CombatAction BattleBane(int level, Creature? caster)
        {
            List<Trait> traits = [Trait.Enchantment, Trait.Mental, ModTrait.BattleAura];
            return Spells.CreateModern(IllustrationName.Bane,
                "Battle Bane",
                [..traits],
                "You fill the minds of your enemies with doubt.",
                "You create a 10-foot-radius emanation around you. Enemies within it must succeed at a Will save or take a –1 status penalty to attack rolls as long as they are in the area.\n\nAn enemy who failed and then leaves the area and reenters later is automatically affected again. An enemy who enters the area for the first time rolls a Will save as they enter.\n\nOnce per turn, starting the turn after you cast bane, you can Sustain the spell to increase the emanation's radius by 10 feet and force enemies in the area that weren't yet affected to attempt another saving throw.",
                Target.Self(),
                level,
                null).WithSoundEffect(SfxName.Bless)
                .WithEffectOnSelf(async (CombatAction action, Creature cr) =>
                {
                    BlessBaneEffectOnSelf(action, cr, IllustrationName.Bane, false, 2, 2);
                });
        }

        private static void BeneMaledictionEffectOnSelf(CombatAction action, Creature caster, Illustration illustration, bool isBenediction, int initialRadius, int sustainExpansion)
        {
            // Stops visual studio from crying about nullability
            (int, bool) DeconstructTag(QEffect qEffect)
            {
                if (qEffect.Tag == null) return (initialRadius, true);
                else return ((int, bool))qEffect.Tag;
            };
            AuraAnimation auraAnimation = new MagicCircleAuraAnimation(isBenediction ? BenedictionCircleArt : MaledictionCircleArt, Color.White, initialRadius);
            caster.AnimationData.AddAuraAnimation(auraAnimation);
            QEffect casterEffect = new QEffect(isBenediction ? "Benediction" : "Malediction", "[this condition has no description]", ExpirationCondition.Never, caster, IllustrationName.None)
            {
                WhenExpires = delegate
                {
                    auraAnimation.MoveTo(0f);
                },
                // first item is the radius of the aura, second is if it's been sustained this turn
                Tag = (initialRadius, true),
                StartOfYourEveryTurn = async delegate (QEffect self, Creature _)
                {
                    self.Tag = (DeconstructTag(self).Item1, false);
                },
                ProvideContextualAction = delegate (QEffect self)
                {
                    (int auraSize, bool sustainedThisTurn) = DeconstructTag(self);
                    if (!sustainedThisTurn)
                        return new ActionPossibility(
                            new CombatAction(self.Owner, illustration, isBenediction ? "Increase Benediction radius" : "Increase Malediction radius",
                            [Trait.Concentrate, Trait.SustainASpell, ModTrait.BattleAura], 
                            $"Increase the radius of the {(isBenediction ? "benediction" : "malediction")} emanation by {5 * sustainExpansion} feet.",
                            Target.Self((Creature cr, AI ai) => -2.14748365E+09f)).WithEffectOnSelf((Creature _) =>
                            {
                                int newAuraSize = auraSize + sustainExpansion;
                                self.Tag = (newAuraSize, true);
                                auraAnimation.MoveTo(newAuraSize);
                                if (!isBenediction)
                                {
                                    foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(self.Owner) <= newAuraSize && cr.EnemyOf(self.Owner)))
                                    {
                                        enemy.RemoveAllQEffects((QEffect qf) => qf.Id == ModQEffectId.RolledAgainstMalediction && qf.Tag == self);
                                    }
                                }
                            })).WithPossibilityGroup("Maintain an activity");
                    else return null;
                }
            };
            if (isBenediction)
            {
                casterEffect.StateCheck = (QEffect self) =>
                {
                    int auraSize = DeconstructTag(self).Item1;
                    foreach (Creature friend in self.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(self.Owner) <= auraSize && cr.FriendOf(self.Owner) && !cr.HasTrait(Trait.Mindless) && !cr.HasTrait(Trait.Object)))
                    {
                        friend.AddQEffect(new QEffect("Benediction", "You gain a +1 status bonus to AC.", ExpirationCondition.Ephemeral, self.Owner, BenedictionArt)
                        {
                            CountsAsABuff = true,
                            BonusToDefenses = (QEffect _, CombatAction? _, Defense defense) => defense == Defense.AC ? new Bonus(1, BonusType.Status, "benediction") : null
                        });
                    }
                };
            }
            else
            {
                casterEffect.StateCheckWithVisibleChanges = async delegate (QEffect self)
                {
                    int auraSize = DeconstructTag(self).Item1;
                    foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(self.Owner) <= auraSize
                            && cr.EnemyOf(self.Owner)
                            && (!cr.HasTrait(Trait.Mindless) || (self.Owner.HasFeat(ModFeatName.ExigentAura) && self.Owner.HasFeat(ModFeatName.AuraEnhancement)))
                            && !cr.HasTrait(Trait.Object))
                    )
                    {
                        if (!enemy.QEffects.Any((QEffect qf) => qf.ImmuneToTrait == Trait.Mental))
                        {
                            if (enemy.QEffects.Any((QEffect qf) => qf.Id == ModQEffectId.FailedAgainstMalediction && qf.Tag == self))
                            {
                                ApplyMaledictionEffect(enemy);
                            }
                            else if (!enemy.QEffects.Any((QEffect qf) => qf.Id == ModQEffectId.RolledAgainstMalediction && qf.Tag == self))
                            {
                                // Give mindless creatures the +4 circ bonus
                                if (enemy.HasTrait(Trait.Mindless))
                                    enemy.AddQEffect(new QEffect()
                                    {
                                        BonusToDefenses = (QEffect self, CombatAction? action, Defense defense) =>
                                        {
                                            if (action?.SpellId == ModSpellId.BattleMalediction)
                                                return new Bonus(4, BonusType.Circumstance, "Mindless (Exigent Aura)");
                                            else return null;
                                        },
                                        ExpiresAt = ExpirationCondition.Ephemeral
                                    });
                                // battle aura; use class DC
                                CheckResult checkResult = await CommonSpellEffects.RollSavingThrowAsync(enemy, action, Defense.Will, self.Owner.ClassDC());
                                enemy.AddQEffect(new QEffect(ExpirationCondition.Never)
                                {
                                    Id = ModQEffectId.RolledAgainstMalediction,
                                    Tag = self
                                });
                                if (checkResult <= CheckResult.Failure)
                                {
                                    enemy.AddQEffect(new QEffect(ExpirationCondition.Never)
                                    {
                                        Id = ModQEffectId.FailedAgainstMalediction,
                                        Tag = self
                                    });
                                    ApplyMaledictionEffect(enemy);
                                }
                            }
                        }
                    }

                    void ApplyMaledictionEffect(Creature enemy)
                    {
                        enemy.AddQEffect(new QEffect("Malediction", "You take a -1 status penalty to AC.", ExpirationCondition.Ephemeral, self.Owner, MaledictionArt)
                        {
                            Key = "MaledictionPenalty",
                            BonusToDefenses = (QEffect _, CombatAction? _, Defense defense) => defense == Defense.AC ? new Bonus(-1, BonusType.Status, "malediction") : null
                        });
                    }
                };
            }
            caster.AddQEffect(casterEffect);
        }

        private static void BlessBaneEffectOnSelf(CombatAction action, Creature caster, Illustration illustration, bool isBless, int initialRadius, int sustainExpansion)
        {
            // Stop visual studio from crying about nullability
            // Tag is (auraSize, sustainedThisTurn, bonusValue/penaltyValue)
            (int, bool, int) DeconstructTag(QEffect qEffect)
            {
                if (qEffect.Tag == null) return (initialRadius, true, 1);
                else return ((int, bool, int))qEffect.Tag;
            };
            AuraAnimation auraAnimation = caster.AnimationData.AddAuraAnimation(isBless ? IllustrationName.BlessCircle : IllustrationName.BaneCircle, initialRadius);
            QEffect casterEffect = new QEffect(isBless ? "Bless" : "Bane", "[this condition has no description]", ExpirationCondition.Never, caster, IllustrationName.None)
            {
                WhenExpires = delegate
                {
                    auraAnimation.MoveTo(0f);
                },
                Traits = [ModTrait.BattleAura],
                Tag = (initialRadius, true, 1),
                StartOfYourEveryTurn = async delegate (QEffect self, Creature _)
                {
                    // reset the sustain flag, allowing the spell to be sustained again
                    var oldTag = DeconstructTag(self);
                    self.Tag = (oldTag.Item1, false, oldTag.Item3);
                },
                ProvideContextualAction = delegate (QEffect self)
                {
                    (int auraSize, bool sustainedThisTurn, int bonusValue) = DeconstructTag(self);
                    if (!sustainedThisTurn)
                        return new ActionPossibility(
                            new CombatAction(self.Owner, illustration, isBless ? "Increase Bless radius" : "Increase Bane radius",
                            [Trait.Concentrate, Trait.SustainASpell, ModTrait.BattleAura],
                            $"Increase the radius of the {(isBless ? "bless" : "bane")} emanation by {5 * sustainExpansion} feet.",
                            Target.Self((Creature cr, AI ai) => -2.14748365E+09f)).WithEffectOnSelf((Creature _) =>
                            {
                                int newAuraSize = auraSize + sustainExpansion;
                                self.Tag = (newAuraSize, true, bonusValue);
                                auraAnimation.MoveTo(newAuraSize);
                                if (!isBless)
                                {
                                    foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(self.Owner) <= newAuraSize && cr.EnemyOf(self.Owner)))
                                    {
                                        enemy.RemoveAllQEffects((QEffect qf) => qf.Id == QEffectId.RolledAgainstBane && qf.Tag == self);
                                    }
                                }
                            })).WithPossibilityGroup("Maintain an activity");
                    else return null;
                }
            };
            if (isBless)
            {
                casterEffect.StateCheck = (QEffect self) =>
                {
                    (int auraSize, bool _, int bonusValue) = DeconstructTag(self);
                    foreach (Creature friend in self.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(self.Owner) <= auraSize && cr.FriendOf(self.Owner) && !cr.HasTrait(Trait.Mindless) && !cr.HasTrait(Trait.Object)))
                    {
                        friend.AddQEffect(new QEffect("Bless", $"You gain a +{bonusValue} status bonus to attack rolls.", ExpirationCondition.Ephemeral, self.Owner, IllustrationName.Bless)
                        {
                            CountsAsABuff = true,
                            BonusToAttackRolls = (QEffect _, CombatAction action, Creature? _) => action.HasTrait(Trait.Attack) ? new Bonus(bonusValue, BonusType.Status, "bless") : null
                        });
                    }
                };
            }
            else
            {
                casterEffect.StateCheckWithVisibleChanges = async delegate (QEffect self)
                {
                    (int auraSize, bool _, int bonusValue) = DeconstructTag(self);
                    foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(self.Owner) <= auraSize
                        && cr.EnemyOf(self.Owner)
                        && (!cr.HasTrait(Trait.Mindless) || self.Owner.HasFeat(ModFeatName.ExigentAura))
                        && !cr.HasTrait(Trait.Object)))
                    {
                        if (!enemy.QEffects.Any((QEffect qf) => qf.ImmuneToTrait == Trait.Mental))
                        {
                            if (enemy.QEffects.Any((QEffect qf) => qf.Id == QEffectId.FailedAgainstBane && qf.Tag == self))
                            {
                                ApplyBaneEffect(enemy, bonusValue);
                            }
                            else if (!enemy.QEffects.Any((QEffect qf) => qf.Id == QEffectId.RolledAgainstBane && qf.Tag == self))
                            {
                                // Give mindless creatures the +4 circ bonus
                                if (enemy.HasTrait(Trait.Mindless))
                                    enemy.AddQEffect(new QEffect()
                                    {
                                        BonusToDefenses = (QEffect self, CombatAction? action, Defense defense) =>
                                        {
                                            if (action?.SpellId == ModSpellId.BattleBane)
                                                return new Bonus(4, BonusType.Circumstance, "Mindless (Exigent Aura)");
                                            else return null;
                                        },
                                        ExpiresAt = ExpirationCondition.Ephemeral
                                    });
                                // battle aura; use class DC
                                CheckResult checkResult = await CommonSpellEffects.RollSavingThrowAsync(enemy, action, Defense.Will, self.Owner.ClassDC());
                                enemy.AddQEffect(new QEffect(ExpirationCondition.Never)
                                {
                                    Id = QEffectId.RolledAgainstBane,
                                    Tag = self
                                });
                                if (checkResult <= CheckResult.Failure)
                                {
                                    enemy.AddQEffect(new QEffect(ExpirationCondition.Never)
                                    {
                                        Id = QEffectId.FailedAgainstBane,
                                        Tag = self
                                    });
                                    ApplyBaneEffect(enemy, bonusValue);
                                }
                            }
                        }
                    }

                    void ApplyBaneEffect(Creature enemy, int value)
                    {
                        enemy.AddQEffect(new QEffect("Bane", $"You take a -{value} status penalty to attack rolls.", ExpirationCondition.Ephemeral, self.Owner, IllustrationName.Bane)
                        {
                            Key = "BanePenalty",
                            BonusToAttackRolls = (QEffect _, CombatAction action, Creature? _) => action.HasTrait(Trait.Attack) ? new Bonus(-value, BonusType.Status, "bane") : null
                        });
                    }
                };
            }
            caster.AddQEffect(casterEffect);
        }
    }
}
