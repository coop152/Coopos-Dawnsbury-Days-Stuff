using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Dawnsbury.Mods.Ancestries.Tengu;

// TODO: 
// * maybe make tengu weapon familiarity grant proficiency with the sword in your hand at the start of combat? proficiency with all swords is a little jank
// * level 9 feat: wind god's fan - ugrades tengu feather fan, which i didnt make yet. gotta do that first

public static class TenguAncestryLoader
{
    public static readonly Trait TenguTrait = ModManager.RegisterTrait("Tengu", new TraitProperties("Tengu", true) { IsAncestryTrait = true });

    public static readonly FeatName SoaringFlightFeatName = ModManager.RegisterFeatName("Soaring Flight {icon:Action}");

    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        // enable the debugger in debug mode, and assert that the right version of the game's DLL is being built against
#if DEBUG  
        Debugger.Launch();
#endif
        ModManager.AssertV3();

        Feat TenguAncestry = new AncestrySelectionFeat(
            ModManager.RegisterFeatName("Tengu"),
            description: "Tengus are gregarious and resourceful avian humanoids who collect knowledge and treasures alike. They are natural survivalists and conversationalists, equally at home living off the wilderness and finding a niche in dense cities. Tengu are known to accumulate knowledge, tools, and companions, adding them to their collection as they travel.\n\n{b}Sharp Beak{/b} With your sharp beak, you are never without a weapon. You have a beak unarmed attack that deals 1d6 piercing damage. Your beak is in the brawling weapon group and has the finesse and unarmed traits.",
            traits: [Trait.Humanoid, TenguTrait],
            hp: 6,
            speed: 5,
            abilityBoosts: [
                new EnforcedAbilityBoost(Ability.Dexterity),
                new FreeAbilityBoost()
            ],
            heritages: GetHeritages().ToList())
            .WithOnCreature((Creature cr) =>
            {
                cr.AddQEffect(new QEffect()
                {
                    AdditionalUnarmedStrike = new Item(new ModdedIllustration("TenguAssets/beak.png"), "beak", [Trait.Finesse, Trait.Weapon, Trait.Melee, Trait.Unarmed, Trait.Brawling])
                    .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing))
                });
            });

        ModManager.AddFeat(TenguAncestry);

        Items.RegisterItems();

        AddFeats(GetAncestryFeats());
        /*
        ModManager.AddFeat(new TrueFeat(ModManager.RegisterFeatName("Dual-Handed Assault"),
            4,
            "You snap your free hand over to grip your weapon just long enough to add momentum and deliver a more powerful blow to your opponent.",
            "{b}Requirements{/b} You are wielding a one-handed melee weapon and have a free hand\n\nMake a Strike with the required weapon. You quickly switch your grip during the Strike in order to make the attack with two hands. If the weapon doesn't have the two-hand trait, increase its weapon damage die by one step for this attack (1d4 » 1d6 » 1d8 » 1d10 » 1d12.) If the weapon has the two-hand trait, you gain the benefit of that trait as well as a circumstance bonus to damage equal to the weapon's number of damage dice. When the Strike is complete, you resume gripping the weapon with only one hand. This action doesn't end any stance or fighter feat effect that requires you to have one hand free.",
            [Trait.Fighter, Trait.Flourish]).WithActionCost(1)
            .WithPermanentQEffect("You quickly grip your one-handed weapon with both hands to make a stronger attack.", (QEffect self) =>
            {
                string name = "Dual-Handed Assault";
                self.ProvideStrikeModifier = (Item weapon) =>
                {
                    if (!self.Owner.HasOneWeaponAndFist) return null;
                    if (weapon == null || weapon.WeaponProperties == null) return null;
                    if (!weapon.HasTrait(Trait.Melee)) return null;
                    if (weapon.HasTrait(Trait.Unarmed)) return null;
                    if (weapon.HasTrait(Trait.TwoHanded)) return null;
                    // for whatever reason, OverrideItemDamageDie is only checked when the Strike action is created, not when it's used. therefore, we add it to the feat's effect just so that the strike can grab it, then remove it.
                    // TODO: this should use the IncreaseDamageDie thing instead if the weapon isnt Two-Hand, so that it doesnt stack with other dice increasing effects like deadly simplicity. This is only gonna happen with a very very strange multiclass build, so low priority
                    self.OverrideItemDamageDie = (QEffect qf, Item weapon2, StrikeModifiers strikeModifiers) =>
                        {
                            //if (Items.WeaponHasTwoHand(weapon, out Dice d)) return d;
                            if (weapon.TwoHandCapable) return (Dice)weapon.WeaponProperties.TwoHandDieSize;
                            else
                            {
                                int startingSize = weapon.WeaponProperties.DamageDieSize;
                                return (Dice)DamageDiceUtils.IncreaseDamageDiceByOneStep(startingSize);
                            }
                        };
                    // (also change the name temporarily to prevent an action symbol showing up in the dice breakdown)
                    var unadulteratedName = self.Name;
                    self.Name = name;
                    CombatAction combatAction = self.Owner.CreateStrike(weapon);
                    // undo our mischevious changes
                    self.OverrideItemDamageDie = null;
                    self.Name = unadulteratedName;

                    combatAction.Name = name;
                    combatAction.Illustration = new SideBySideIllustration(combatAction.Illustration, Items.ChangeGripArt);
                    combatAction.ActionCost = 1;
                    combatAction.Traits.AddRange([Trait.Fighter, Trait.Flourish, Trait.Basic]);
                    string addedDamageText = weapon.TwoHandCapable ?
                        $"which grants the benefit of the Two-Hand trait as well as a +{weapon.WeaponProperties.DamageDieCount} circumstance bonus to damage for this attack." :
                        "which increases the weapon's damage dice by one step for this attack.";
                    combatAction.Description = StrikeRules.CreateBasicStrikeDescription2(combatAction.StrikeModifiers, weaponDieIncreased: true, additionalAftertext: "You resume gripping the weapon with only one hand. This doesn't end any stance or effect that requires you to have one hand free.", additionalAttackRollText: "You quickly switch your grip during the Strike in order to make the attack with two hands, " + addedDamageText);

                    return combatAction;
                };
                self.BonusToDamage = (QEffect self, CombatAction action, Creature defender) =>
                {
                    Item? weapon = self.Owner.PrimaryWeapon;
                    if (weapon == null || weapon.WeaponProperties == null) return null;
                    if (action.Name == name && weapon.TwoHandCapable)
                        return new Bonus(weapon.WeaponProperties.DamageDieCount, BonusType.Circumstance, "Dual-Handed Assault", true);
                    else return null;
                };
            }));
        */
        ModManager.RegisterFeatNameReplacement("Dual-Handed Assault", FeatName.DualHandedAssault);
    }

    static void AddFeats(IEnumerable<Feat> feats)
    {
        foreach (Feat f in feats)
        {
            ModManager.AddFeat(f);
        }
    }

    // static public bool IsAssemblyExists(string assemblyName)
    // {
    //     foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    //     {
    //         if (assembly.FullName.StartsWith(assemblyName))
    //             return true;
    //     }
    //     return false;
    // }

    static IEnumerable<Feat> GetAncestryFeats()
    {
        Spell marinersFire = AllSpells.CreateModernSpellTemplate(TenguSpells.MarinersFireSpellId, TenguTrait);
        ModManager.RegisterInlineTooltip("marinersFireExplanation", "Mariner's Fire is functionally identical to the spell {i}Ignition{/i}.");
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Mariner's Fire"),
            1,
            "You conjure uncanny orbs of spiritual flame that float above or below the water's surface.",
            $"You can cast the {{tooltip:marinersFireExplanation}}mariner's fire{{/}} cantrip as a primal innate spell at will. Your spellcasting ability for this spell is Charisma. You can cast this cantrip underwater.",
            [TenguTrait]
            ).WithIllustration(marinersFire.Illustration).WithRulesBlockForSpell(marinersFire.SpellId).WithOnCreature(delegate (Creature cr)
            {
                cr.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, TenguTrait, Ability.Charisma, Trait.Primal).WithSpells([marinersFire.SpellId], cr.MaximumSpellRank);
            }).WithOnSheet(calculatedSheet =>
            {
                calculatedSheet.SetProficiency(Trait.Spell, Proficiency.Trained);
            });
        yield return new TrueFeat(
            ModManager.RegisterFeatName("One-Toed Hop", "One-Toed Hop {icon:Action}"),
            1,
            "Assuming a peculiar stance, you make a short hop on each toe.",
            "You make a short 5ft Leap which does not trigger reactions that are triggered by movement, such as Attack of Opportunity.\n\n{b}Special{/b} If you also have the Powerful Leap feat, your Leap goes 5ft further, and you go high enough to jump over other creatures.",
            [TenguTrait]
            ).WithOnCreature((Creature cr) =>
            {
                cr.AddQEffect(new QEffect("One-Toed Hop {icon:Action}", "Make a short Leap which doesn't trigger reactions to movement.")
                {
                    ProvideActionIntoPossibilitySection = (QEffect self, PossibilitySection section) =>
                    {
                        PossibilitySectionId location = PossibilitySectionId.Movement;
                        if (section.PossibilitySectionId == location)
                        {
                            return new ActionPossibility(OneToedHop(cr));
                        }
                        else return null;
                    }
                });
            });
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Scavenger's Search"),
            1,
            "You're always on the lookout for supplies and valuables.",
            "You gain a +2 circumstance bonus to locate objects (such as secret doors and hazards) you search for within 30 feet with a Seek action.",
            [TenguTrait]
            ).WithOnCreature((Creature cr) =>
            {
                cr.AddQEffect(new QEffect("Scavenger's Search", "You have a +2 circumstance bonus when Seeking objects.")
                {
                    BonusToAttackRolls = (QEffect self, CombatAction action, Creature? target) =>
                    {
                        if (target == null) return null;
                        // triggers on Seek actions, and Pseudocreature is the secret sauce that indicates it's a tile being seeked
                        if (action.ActionId == ActionId.Seek && target.HasTrait(Trait.Pseudocreature)) return new Bonus(2, BonusType.Circumstance, "Scavenger's Search");
                        else return null;
                    }
                });
            });
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Squawk!", "Squawk! {icon:Reaction}"),
            1,
            "You let out an awkward squawk, ruffle your feathers, or fake some other birdlike tic to cover up a poor attempt to intimidate.",
            "{b}Frequency{/b} Once per day\n{b}Trigger{/b} You fail or critically fail an Intimidation check to Demoralize a creature without the tengu trait\n\nReroll the failed Intimidation check and keep the better result.",
            [TenguTrait]
            ).WithOnCreature((Creature cr) =>
            {
                if (cr.PersistentUsedUpResources.UsedUpActions.Contains("squawk")) return;

                cr.AddQEffect(new QEffect("Squawk! {icon:Reaction}", "Reroll a failed check to Demoralize.")
                {
                    RerollActiveRoll = async (QEffect self, CheckBreakdownResult result, CombatAction action, Creature target) =>
                    {
                        if (action.ActionId != ActionId.Demoralize) return RerollDirection.DoNothing;
                        if (result.CheckResult == CheckResult.Success || result.CheckResult == CheckResult.CriticalSuccess) return RerollDirection.DoNothing;
                        bool playerConfirms = await self.Owner.AskToUseReaction("You failed your check to Demoralize. Do you want to use {b}Squawk!{/b} to reroll the check and take the better result?");
                        if (!playerConfirms) return RerollDirection.DoNothing;
                        self.ExpiresAt = ExpirationCondition.Immediately;
                        self.Owner.PersistentUsedUpResources.UsedUpActions.Add("squawk");
                        return RerollDirection.RerollAndKeepBest;
                    }
                });
            });
        Spell electricArc = AllSpells.CreateModernSpellTemplate(SpellId.ElectricArc, TenguTrait);
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Storm's Lash"),
            1,
            "Wind and lightning have always been friends to you.",
            $"You can cast the {electricArc.ToSpellLink()} cantrip as a primal innate spell at will. Your spellcasting ability for this spell is Charisma.",
            [TenguTrait]
            ).WithIllustration(electricArc.Illustration).WithRulesBlockForSpell(electricArc.SpellId).WithOnCreature(delegate (Creature cr)
            {
                cr.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, TenguTrait, Ability.Charisma, Trait.Primal).WithSpells([electricArc.SpellId], cr.MaximumSpellRank);
            }).WithOnSheet(calculatedSheet =>
            {
                calculatedSheet.SetProficiency(Trait.Spell, Proficiency.Trained);
            });
        // Tengu Weapon Familiarity and Subfeats
        List<Trait> familiarWeapons = [Items.Katana, Items.Khakkara, Items.TempleSword, Items.Wakizashi, Items.TenguGaleBlade, Trait.Sword];
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Tengu Weapon Familiarity"),
            1,
            "You have eclectic experience with all sorts of weapons.",
            "You have familiarity with all weapons with the tengu trait, all weapons in the sword group, plus the katana, khakkara, temple sword, and wakizashi. For the purpose of proficiency, you treat any of these that are martial weapons as simple weapons and any that are advanced weapons as martial weapons.\n\nAt 5th level, whenever you get a critical hit with one of these weapons, you get its {tooltip:criteffect}critical specialization effect{/}.",
            [TenguTrait]
            ).WithOnSheet((sheet) =>
            {
                // Increase proficiency for all groups listed (there's no need to do it for the actual tengu trait because there's only one tengu weapon and we listed that)
                foreach (Trait t in familiarWeapons)
                {
                    // legacy/remaster compatibility: all remaster classes have simple prof, but some legacy ones (like wizard) don't.
                    // for these classes, grant training instead of an adjustment, as if they had simple proficiency.
                    // (they suck too bad to get a proficiency increase before level 8 so it doesn't matter beyond Trained)
                    if (sheet.GetProficiency(Trait.Simple) == Proficiency.Untrained)
                    {
                        sheet.Proficiencies.Set([t, Trait.Simple], Proficiency.Trained);
                        sheet.Proficiencies.Set([t, Trait.Martial], Proficiency.Trained);
                    }
                    else
                    {
                        sheet.Proficiencies.AddProficiencyAdjustment(traits => traits.Contains(t) && traits.Contains(Trait.Martial), Trait.Simple);
                        sheet.Proficiencies.AddProficiencyAdjustment(traits => traits.Contains(t) && traits.Contains(Trait.Advanced), Trait.Martial);
                    }
                }
            }).WithOnCreature((Creature cr) =>
            {
                // grant crit spec with listed weapons at level 5
                if (cr.Level < 5) return;
                cr.AddQEffect(new QEffect()
                {
                    YouHaveCriticalSpecialization = (QEffect self, Item weapon, CombatAction _, Creature _) => familiarWeapons.Any(trait => weapon.HasTrait(trait))
                });
            });
        // Uncanny Agility
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Uncanny Agility"),
            1,
            "You have near-supernatural poise that lets you move swiftly across the most unsteady surfaces.",
            "You gain the Feather Step skill feat, which allows you to Step into difficult terrain.",
            [TenguTrait]
            ).WithOnSheet((calculatedSheet) =>
            {
                calculatedSheet.GrantFeat(FeatName.FeatherStep);
            });
        // Magpie Snatch
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Magpie Snatch"),
            5,
            "You quickly snatch whatever shiny items catch your eye.",
            "{b}Requirements{/b} Both of your hands are empty\n\nYou Interact to pick up as many items as you can hold, up to two.",
            [TenguTrait]
            ).WithActionCost(1).WithPermanentQEffect(qf =>
            {
                qf.ProvideActionIntoPossibilitySection = (QEffect self, PossibilitySection section) =>
                {
                    if (section.PossibilitySectionId != PossibilitySectionId.ItemActions) return null;
                    if (self.Owner.HeldItems.Count != 0) return null;
                    return new ActionPossibility(
                        new CombatAction(self.Owner, IllustrationName.PickUp, "Magpie Snatch", [Trait.Manipulate, Trait.Interact],
                        "{i}You quickly snatch whatever shiny items catch your eye.{/i}\n\n{b}Requirements{/b} Both of your hands are empty\n\nYou Interact to pick up as many items as you can hold, up to two.",
                        Target.Self().WithAdditionalRestriction(cr => cr.Occupies.DroppedItems.Count == 0 &&
                            cr.Occupies.Neighbours.All(edge => edge.Tile.DroppedItems.Count == 0) ? "There is nothing for you to pick up." : null)
                        ).WithEffectOnSelf(async cr =>
                        {
                            // enumerate all items on the occupied tile plus neighbours (you are allowed to pick up items in adjacent tiles)
                            List<(Item item, Action pickUp)> floorItems = [];
                            floorItems.AddRange(cr.Occupies.DroppedItems.Select<Item, (Item, Action)>(item => (item, () =>
                                {
                                    cr.AddHeldItem(item);
                                    cr.Occupies.DroppedItems.Remove(item);
                                    floorItems.RemoveFirst(tuple => tuple.item == item);
                                })));
                            foreach (Edge edge in cr.Occupies.Neighbours)
                            {
                                floorItems.AddRange(edge.Tile.DroppedItems.Select<Item, (Item, Action)>(item => (item, () =>
                                {
                                    cr.AddHeldItem(item);
                                    edge.Tile.DroppedItems.Remove(item);
                                    floorItems.RemoveFirst(tuple => tuple.item == item);
                                }
                                )));
                            }
                            int handsRequired = floorItems.Sum(tuple => tuple.item.TwoHanded ? 2 : 1);
                            if (handsRequired <= 2)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    if (floorItems.Count > 0) floorItems.First().pickUp();
                                }
                            }
                            else
                            {
                                // pick up first item
                                List<string> choices = floorItems.Select(tuple => tuple.item.Name).ToList();
                                var firstChoice = await cr.AskForChoiceAmongButtons(IllustrationName.PickUp, "Magpie Snatch — What item do you pick up first?", [.. choices]);
                                (Item firstItem, Action pickUpFirstItem) = floorItems.FirstOrDefault(tuple => tuple.item.Name == firstChoice.Text);
                                pickUpFirstItem();
                                cr.Battle.Log($"\t{cr.Name} nabs {{b}}{firstItem.Name}{{/b}}.");
                                // hands are full - stop now
                                if (firstItem.TwoHanded)
                                    return;
                                // pick up second item
                                choices = floorItems.Where(tuple => !tuple.item.TwoHanded).Select(tuple => tuple.item.Name).ToList();
                                var secondChoice = await cr.AskForChoiceAmongButtons(IllustrationName.PickUp, "Magpie Snatch — What item do you pick up second?", [.. choices]);
                                (Item secondItem, Action pickUpSecondItem) = floorItems.FirstOrDefault(tuple => tuple.item.Name == secondChoice.Text);
                                pickUpSecondItem();
                                cr.Battle.Log($"\t{cr.Name} nabs {{b}}{secondItem.Name}{{/b}}.");
                            }
                        }));
                };
            });

        // Soaring Flight
        yield return new TrueFeat(
            SoaringFlightFeatName,
            5,
            "You take to the skies, if only for a moment.",
            "{b}Frequency{/b} once per round\n\nYou make a 20ft Leap. This Leap is high enough to go above other creatures.\n\n{b}Special{/b} If you also have the Powerful Leap feat, your Leap goes 5ft further.\n\n{i}(This action can be found in 'Other actions'.){/i}",
            [TenguTrait]
            ).WithOnCreature((Creature cr) =>
            {
                cr.AddQEffect(new QEffect("Soaring Flight {icon:Action}", "Fly up to your speed, landing at the end of the movement.")
                {
                    ProvideActionIntoPossibilitySection = (QEffect self, PossibilitySection section) =>
                    {
                        PossibilitySectionId location = PossibilitySectionId.Movement;
                        if (section.PossibilitySectionId == location)
                        {
                            return new ActionPossibility(SoaringFlight(cr));
                        }
                        else return null;
                    }
                });
            });
        // Tengu Feather Fan
        /*
        Spell gustOfWind = AllSpells.CreateModernSpellTemplate(SpellId.BurningHands, TenguTrait);
        Spell wallOfFire = AllSpells.CreateModernSpellTemplate(SpellId.WallOfFire, TenguTrait);
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Tengu Feather Fan"),
            5,
            "You've learned to bind some of your feathers together into a fan to focus your ancestral magic.",
            $"You can cast {gustOfWind.ToSpellLink()} as a 1st-level divine innate spell. Your spellcasting ability for this spell is Charisma.\n" +
            $"These innate spells work differently than usual. You gain one tengu spell charge per day which you can use to cast any tengu spell you know. Further feats may give you access to additional tengu spells and charges, which you can spend to cast any of your known tengu spells; for example, if you took the Wind God's Fan feat to learn {{i}}{wallOfFire.Name.ToLower()}{{/i}} and gain a second tengu spell charge, you could choose to cast {{i}}{gustOfWind.Name.ToLower()}{{/i}} and {{i}}{wallOfFire.Name.ToLower()}{{/i}} once each per day, or you could cast {{i}}{wallOfFire.Name.ToLower()}{{/i}} twice per day.\n\n" +
            $"In addition, you gain access to the {{i}}tengu feather fan{{/i}}, a held magic item. While holding a tengu feather fan, the save DC of your tengu spells is the highest of your spell DC and your class DC, and your tengu spells use your highest spellcasting ability modifier instead of Charisma.\n\n{{i}}(The tengu feather fan item is found in the 'Other' category in the store.){{/i}}",
            [TenguTrait]
            ).WithOnCreature((Creature cr) =>
            {
                // add regular innate spell
                //cr.GetOrCreateSpellcastingSource(SpellcastingKind.Spontaneous, TenguTrait, Ability.Charisma, Trait.Primal).WithSpontaneousSlots(1).WithSpells([gustOfWind.SpellId], 1);
                // handle feather fan DC modification
                cr.AddQEffect(new QEffect()
                {
                    BonusToSpellSaveDCsForSpecificSpell = (QEffect self, CombatAction action) =>
                    {
                        // only apply if condition owner is holding a feather fan
                        if (!self.Owner.HeldItems.Select(item => item.MainTrait == Items.TenguFeatherFan).Any()) return null;
                        if (action.SpellcastingSource == null) return null;
                        // if spell is a tengu spell with a saving throw, and the character's ClassOrSpellDC is better, swap the DC for that instead
                        if (action.SpellcastingSource.ClassOfOrigin == TenguTrait
                            && action.SavingThrow != null
                            && action.Owner.ClassOrSpellDC() > action.SpellcastingSource.GetSpellSaveDC())
                        {
                            action.WithSavingThrow(new SavingThrow(action.SavingThrow.Defense, action.Owner.ClassOrSpellDC()));
                        }
                        return null;
                    },
                    BonusToAttackRolls = (QEffect self, CombatAction action, Creature? defender) =>
                    {
                        // only apply if condition owner is holding a feather fan
                        if (!self.Owner.HeldItems.Select(item => item.MainTrait == Items.TenguFeatherFan).Any()) return null;
                        if (action.SpellcastingSource == null) return null;
                        // HOMEBREW: if action is a tengu attack spell, swap the spellcasting ability to the highest one (so that the tengu attack cantrips also benefit)
                        if ((action.HasTrait(Trait.Cantrip) || action.HasTrait(Trait.Spell))
                            && action.SpellcastingSource.ClassOfOrigin == TenguTrait
                            && action.HasTrait(Trait.Attack))
                        {
                            Ability bestSpellcastingAbility = action.SpellcastingSource.Spellcasting.Sources.MaxBy(source => source.SpellcastingAbilityModifier).SpellcastingAbility;
                            //action.SpellcastingSource.SpellcastingAbility = bestSpellcastingAbility;
                            action.SpellcastingSource = new SpellcastingSource(
                                action.SpellcastingSource.Spellcasting,
                                action.SpellcastingSource.Kind,
                                bestSpellcastingAbility,
                                action.SpellcastingSource.SpellcastingTradition,
                                action.SpellcastingSource.ClassOfOrigin);
                        }
                        return null;
                    }
                });
            }).WithOnSheet((calculatedSheet) =>
            {
                calculatedSheet.SetProficiency(Trait.Spell, Proficiency.Trained);
            });

        yield return new TrueFeat(
            ModManager.RegisterFeatName("Wind God's Fan"),
            9,
            "Your fan can stir up even more powerful winds.",
            $"You gain an additional tengu spell charge every day.\n" +
            $"Add 3rd-level {wallOfFire.ToSpellLink()} to your list of tengu feather fan spells.",
            [TenguTrait]
            ).WithOnCreature((Creature cr) =>
            {
                //cr.GetOrCreateSpellcastingSource(SpellcastingKind.Spontaneous, TenguTrait, Ability.Charisma, Trait.Primal).WithSpontaneousSlots(0, 0, 2).WithSpells([wallOfFire.SpellId], 3);
            });
        */
        // Soaring Form
        yield return new TrueFeat(
            ModManager.RegisterFeatName("Soaring Form"),
            9,
            "With experience your wings have grown strong, capable of carrying you great distances.",
            "{b}Prerequisites{/b} Soaring Flight\nYou have a fly speed at all times {i}(You ignore difficult and hazardous terrain and can move over water, lava and chasms.){/i}",
            [TenguTrait]
            ).WithPrerequisite(SoaringFlightFeatName, "Soaring Flight")
            .WithOnCreature((Creature cr) =>
            {
                cr.AddQEffect(QEffect.Flying());
            });

    }

    static IEnumerable<Feat> GetHeritages()
    {
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Dogtooth Tengu"),
            "In addition to a beak, your mouth also features a number of vicious, pointed teeth. Some legends claim your powerful jaws can even bite through steel. While you aren't that strong yet, your fangs can still leave terrible wounds.",
            "Your beak unarmed attack gains the deadly d8 trait."
            ).WithOnCreature(delegate (Creature cr)
            {
                cr.GetAttackItem("beak")?.Traits.Add(Trait.DeadlyD8);
            });
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Jinxed Tengu"),
            "Your lineage has been exposed to curse after curse, and now they slide off your feathers like rain.",
            "If you succeed at a saving throw against a curse effect, you get a critical success instead. When you would gain the doomed condition, attempt a DC 17 flat check. On a success, reduce the value of the doomed condition you would gain by 1."
            ).WithOnCreature(delegate (Creature cr)
            {
                cr.AddQEffect(new QEffect("Jinxed Tengu", "When saving against curse effects, your successes are upgraded to critical successes. When you gain the doomed condition, make a DC17 flat check to reduce the gained value by 1.")
                {
                    AdjustSavingThrowCheckResult = (QEffect self, Defense defense, CombatAction action, CheckResult result) =>
                    {
                        if (action.HasTrait(Trait.Curse) && defense.IsSavingThrow() && result == CheckResult.Success)
                            return CheckResult.CriticalSuccess;
                        else return result;
                    },
                    YouAcquireQEffect = (QEffect self, QEffect applied) =>
                    {
                        if (applied.Id == QEffectId.Doomed)
                        {
                            (CheckResult result, string breakdown) = Checks.RollFlatCheck(17);
                            if (result == CheckResult.Success || result == CheckResult.CriticalSuccess)
                            {
                                self.Owner.Overhead("Jinx!", Color.DarkGreen, self.Owner.Name + " reduced an incoming doomed condition by 1 via Jinxed Tengu.", "Jinxed Tengu", "DC 17 flat check = " + breakdown);
                                applied.Value--;
                                if (applied.Value == 0) return null;
                                else return applied;
                            }
                            else
                            {
                                self.Owner.Battle.Log(self.Owner.Name + "'s Jinxed Tengu failed to reduce an incoming doomed condition.", "Jinxed Tengu", "DC 17 flat check = " + breakdown);
                                return applied;
                            }
                        }
                        else
                        {
                            return applied;
                        }
                    }
                });
            });
        Spell mountainkeepersLash = AllSpells.CreateModernSpellTemplate(TenguSpells.MountainkeepersLashSpellId, TenguTrait);
        ModManager.RegisterInlineTooltip("mountainkeepersLashExplanation", "Mountainkeeper's Lash is functionally identical to the spell {i}Vitality Lash{/i}.");
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Mountainkeeper Tengu"),
            "You come from a line of tengu ascetics, leaving you with a link to the spirits of the world.",
            $"You can cast the {{tooltip:mountainkeepersLashExplanation}}mountainkeeper's lash{{/}} cantrip as a primal innate spell at will. Your spellcasting ability for this spell is Charisma."
            ).WithRulesBlockForSpell(mountainkeepersLash.SpellId).WithOnCreature((Creature cr) =>
            {
                cr.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, TenguTrait, Ability.Charisma, Trait.Primal).WithSpells([mountainkeepersLash.SpellId], cr.MaximumSpellRank);
            }).WithOnSheet((calculatedSheet) =>
            {
                calculatedSheet.SetProficiency(Trait.Spell, Proficiency.Trained);
            });
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Skyborn Tengu"),
            "Your bones may be especially light, you may be a rare tengu with wings, or your connection to the spirits of wind and sky might be stronger than most, slowing your descent through the air.",
            $"You gain a +1 circumstance bonus to saving throws against effects that have the air trait, and if you roll a success on a save against an air effect, you get a critical success instead. In addition, you gain the Powerful Leap feat."
            ).WithOnSheet((calculatedSheet) =>
            {
                calculatedSheet.GrantFeat(FeatName.PowerfulLeap);
            }).WithOnCreature((Creature cr) =>
            {
                cr.AddQEffect(new QEffect("Skyborn Tengu", "You have a +1 bonus to saves against air effects, and successful saves become critical successes.")
                {
                    AdjustSavingThrowCheckResult = (QEffect self, Defense defense, CombatAction action, CheckResult result) =>
                    {
                        if (action.HasTrait(Trait.Air) && defense.IsSavingThrow() && result == CheckResult.Success)
                            return CheckResult.CriticalSuccess;
                        else return result;
                    },
                    BonusToDefenses = (QEffect self, CombatAction? action, Defense defense) =>
                    {
                        if (!defense.IsSavingThrow()) return null;
                        else if (action != null && action.HasTrait(Trait.Air)) return new Bonus(1, BonusType.Circumstance, "Skyborn Tengu");
                        else return null;
                    }
                });
            });
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Stormtossed Tengu"),
            "Whether due to a storm god's blessing or hatching from your egg during a squall, you are resistant to storms.",
            "You gain electricity resistance equal to half your level (minimum 1)."
            ).WithOnCreature(delegate (Creature cr)
            {
                int resistance = Math.Max(1, cr.Level / 2);
                cr.WeaknessAndResistance.AddResistance(DamageKind.Electricity, resistance);
            });
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Taloned Tengu"),
            "Your talons are every bit as sharp and strong as your beak.",
            "You gain a talons unarmed attack that deals 1d4 slashing damage. Your talons are in the brawling group and have the agile, finesse, unarmed, and versatile P traits."
            ).WithOnCreature(delegate (Creature cr)
            {
                cr.AddQEffect(new QEffect()
                {
                    AdditionalUnarmedStrike = new Item(new ModdedIllustration("TenguAssets/talons.png"), "talons", [Trait.Brawling, Trait.Agile, Trait.Finesse, Trait.Weapon, Trait.Melee, Trait.Unarmed, Trait.VersatileP])
                    .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing)).WithSoundEffect(SfxName.Fist2)
                });
            });
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Wavediver Tengu"),
            "You're one of the rare tengu who can cut through water like a bird through air, and you often lurk in rivers or oceans where few expect you.",
            "You gain a swim Speed. This allows you to:\n• Move through deep water tiles.\n• Ignore difficult terrain imposed by shallow water.\n• Ignore the flat-footed and difficult terrain penalties from being underwater."
            ).WithOnCreature(delegate (Creature cr)
            {
                cr.AddQEffect(QEffect.Swimming());
            });
        yield return new HeritageSelectionFeat(
            ModManager.RegisterFeatName("Unusual Tengu"),
            "You're not quite like the other tengu.",
            "You have two free ability boosts instead of a tengu's normal ability boosts."
            ).WithOnSheet(sheet =>
            {
                sheet.AbilityBoostsFabric.AncestryBoosts = [
                    new FreeAbilityBoost(),
                    new FreeAbilityBoost()
                    ];
            });
    }

    static CombatAction OneToedHop(Creature self)
    {
        int leapDistance = self.HasEffect(QEffectId.PowerfulLeap) ? 2 : 1;
        return new CombatAction(self, IllustrationName.Jump, "One-Toed Hop",
            [Trait.Move, Trait.Basic],
            "{i}Assuming a peculiar stance, you make a short hop on each toe.{/i}\n\nMake a short " + S.HeightenedVariable(leapDistance * 5, 5) + "ft Leap which does not trigger reactions which are triggered by movement. This Leap is high enough that you can jump over other creatures.",
            new TileTarget((Creature jumper, Tile tile) => jumper.Occupies != null && tile.IsTrulyGenuinelyFreeTo(jumper) && jumper.DistanceTo(tile) <= leapDistance && jumper.Occupies.HasLineOfEffectToIgnoreLesser(tile) != CoverKind.Blocked, null)
            ).WithEffectOnChosenTargets(async delegate (CombatAction action, Creature jumper, ChosenTargets target)
            {
                if (target.ChosenTile == null) return;
                await jumper.SingleTileMove(target.ChosenTile, action);
            }).WithActionId(ActionId.Leap);
    }

    static readonly QEffectId SoaringFlightCooldown = ModManager.RegisterEnumMember<QEffectId>("soaringFlightCooldown");
    static CombatAction SoaringFlight(Creature self)
    {
        int leapDistance = self.HasEffect(QEffectId.PowerfulLeap) ? 5 : 4;
        return new CombatAction(self, IllustrationName.Fly, "Soaring Flight",
            [Trait.Move, Trait.Basic, Trait.ProvokesAsActionBegins],
            "{i}You take to the skies, if only for a moment.{/i}\n\n{b}Frequency{/b} once per round\n\nYou make a " + S.HeightenedVariable(leapDistance * 5, 20) + "ft Leap. This Leap is high enough to go above other creatures.",
            new TileTarget((Creature jumper, Tile tile) => jumper.Occupies != null && tile.IsTrulyGenuinelyFreeTo(jumper) && jumper.DistanceTo(tile) <= leapDistance && jumper.Occupies.HasLineOfEffectToIgnoreLesser(tile) != CoverKind.Blocked, null)
            .WithAdditionalTargetingRequirement((jumper, _) => jumper.HasEffect(SoaringFlightCooldown) ? Usability.NotUsable("Soaring Flight can only be used once per turn.") : Usability.Usable)
            ).WithEffectOnChosenTargets(async delegate (CombatAction action, Creature jumper, ChosenTargets target)
            {
                if (target.ChosenTile == null) return;
                await jumper.SingleTileMove(target.ChosenTile, action);
                jumper.AddQEffect(new QEffect() { Id = SoaringFlightCooldown, ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn });
            }).WithActionId(ActionId.Leap);
    }
}
