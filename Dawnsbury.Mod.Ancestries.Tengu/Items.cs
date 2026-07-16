using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Ancestries.Tengu
{
    internal static class Items
    {
        static public readonly Trait TenguWeaponTrait = ModManager.RegisterTrait("Tengu Weapon", new TraitProperties("Tengu Weapon", false) { ProficiencyName = "Tengu weapons" });
        public static Trait Brace;
        private static bool externalBraceUsed;

        static public readonly Trait Katana = ModManager.RegisterTrait("Katana", new TraitProperties("Katana", false));
        static public readonly Trait Khakkara = ModManager.RegisterTrait("Khakkara", new TraitProperties("Khakkara", false));
        static public readonly Trait TempleSword = ModManager.RegisterTrait("TempleSword", new TraitProperties("Temple Sword", false));
        static public readonly Trait Wakizashi = ModManager.RegisterTrait("Wakizashi", new TraitProperties("Wakizashi", false));
        static public readonly Trait TenguGaleBlade = ModManager.RegisterTrait("TenguGaleBlade", new TraitProperties("Tengu Gale Blade", false));
        static public readonly Trait Nodachi = ModManager.RegisterTrait("Nodachi", new TraitProperties("Nodachi", false));
        //static public readonly Trait TenguFeatherFan = ModManager.RegisterTrait("TenguFeatherFan", new TraitProperties("Tengu Feather Fan", false));

        static public readonly Illustration ChangeGripArt = new ModdedIllustration("TenguAssets/changeGrip.png");

        public static void RegisterItems()
        {
            // check if a brace trait already exists (e.g. from More Basic Actions); if it does, then use that, otherwise use our own
            if (ModManager.TryParse<Trait>("Brace", out Brace))
            {
                externalBraceUsed = true;
            }
            else
            {
                externalBraceUsed = false;
                Brace = ModManager.RegisterTrait("Brace",
                new TraitProperties("Brace", true, "A brace weapon is effective at damaging moving opponents. You gain the {i}Brace Your Weapon{/i} action, which allows you to immediately end your turn in exchange for an extra 2 precision damage per weapon die on reaction Strikes."));
            }

            ModManager.RegisterNewItemIntoTheShop("katana", (ItemName name) =>
            {
                return new Item(name, new ModdedIllustration("TenguAssets/katana.png"), "katana", 0, 2, [Trait.Weapon, Trait.Melee, Trait.Martial, Trait.Sword, Trait.DeadlyD8, Trait.TwoHand1d10, Trait.VersatileP])
                {
                    WeaponProperties = new WeaponProperties("1d6", DamageKind.Slashing)
                }.WithMainTrait(Katana);
            });
            ModManager.RegisterNewItemIntoTheShop("khakkara", (ItemName name) =>
            {
                return new Item(name, new ModdedIllustration("TenguAssets/khakkara.png"), "khakkara", 0, 2, [Trait.Weapon, Trait.Melee, Trait.Martial, Trait.Club, Trait.MonkWeapon, Trait.TwoHand1d10, Trait.VersatileP])
                {
                    WeaponProperties = new WeaponProperties("1d6", DamageKind.Bludgeoning)
                }.WithMainTrait(Khakkara);
            });
            ModManager.RegisterNewItemIntoTheShop("temple sword", (ItemName name) =>
            {
                return new Item(name, new ModdedIllustration("TenguAssets/templeSword.png"), "temple sword", 0, 2, [Trait.Weapon, Trait.Melee, Trait.Martial, Trait.Sword, Trait.MonkWeapon, Trait.Trip])
                {
                    WeaponProperties = new WeaponProperties("1d8", DamageKind.Slashing)
                }.WithMainTrait(TempleSword);
            });
            ModManager.RegisterNewItemIntoTheShop("wakizashi", (ItemName name) =>
            {
                return new Item(name, new ModdedIllustration("TenguAssets/wakizashi.png"), "wakizashi", 0, 2, [Trait.Weapon, Trait.Melee, Trait.Martial, Trait.Sword, Trait.Agile, Trait.DeadlyD8, Trait.Finesse, Trait.VersatileP])
                {
                    WeaponProperties = new WeaponProperties("1d4", DamageKind.Slashing)
                }.WithMainTrait(Wakizashi);
            });
            ModManager.RegisterNewItemIntoTheShop("tengu gale blade", (ItemName name) =>
            {
                return new Item(name, new ModdedIllustration("TenguAssets/tenguGaleBlade.png"), "tengu gale blade", 0, 2, [Trait.Weapon, Trait.Melee, Trait.Martial, Trait.Sword, Trait.Agile, Trait.Disarm, Trait.Finesse, TenguAncestryLoader.TenguTrait])
                {
                    WeaponProperties = new WeaponProperties("1d6", DamageKind.Slashing)
                }.WithMainTrait(TenguGaleBlade);
            });
            ModManager.RegisterNewItemIntoTheShop("nodachi", (ItemName name) =>
            {
                return new Item(name, new ModdedIllustration("TenguAssets/nodachi.png"), "nodachi", level: 0, price: 6,
                    [Trait.Weapon, Trait.Melee, Trait.Advanced, Trait.Sword, Trait.TwoHanded, Trait.DeadlyD12, Trait.Reach, Brace])
                {
                    WeaponProperties = new WeaponProperties("1d8", DamageKind.Slashing)
                }.WithMainTrait(Nodachi).ImplementBrace();
            });
            // Register replacement for this mod's removed bastard sword, so old save files use the base game weapon
            ModManager.RegisterItemNameReplacement("bastard sword", ItemName.BastardSword);
            //ModManager.RegisterNewItemIntoTheShop("tengu feather fan", (ItemName name) =>
            //{
            //    Illustration fanArt = new ModdedIllustration("TenguAssets/tenguFeatherFan.png");
            //    return new Item(name, fanArt, "tengu feather fan", level: 0, price: 0, [TenguAncestryLoader.TenguTrait, Trait.Mod, Trait.Primal])
            //    {
            //        MainTrait = TenguFeatherFan,
            //        Description = "You must be a Tengu with the Tengu Feather Fan feat to use this item.\n\nWhile holding a tengu feather fan, the save DC of your tengu spells is the highest of your spell DC and your class DC, and your tengu spells use your highest spellcasting ability modifier instead of Charisma.",
            //        StateCheckWhenWielded = (Creature wielder, Item fan) =>
            //        {
            //            // ****************************************************************************************** //
            //            // TODO:
            //            // next, add the limited usages (probably using the same type of stuff as other once-per-day limits)
            //            // and then check you have the right feats to cast the spells. or move this into those feats and just check if a feather fan is equipped.
            //            // and then hide this qeffect.
            //            // TODO CONCLUDED
            //            // ****************************************************************************************** //
            //            wielder.AddQEffect(new QEffect("tengu feather fan being wielded", "effect applied by statecheckwhenwielded")
            //            {
            //                Illustration = fanArt,
            //                ExpiresAt = ExpirationCondition.Ephemeral,
            //                ProvideActionsIntoPossibilitySection = (QEffect self, PossibilitySection section) =>
            //                {
            //                    if (section.PossibilitySectionId != PossibilitySectionId.ItemActions) return [];

            //                    CombatAction pushingGust = AllSpells.CreateSpellInCombat(SpellId.PushingGust, self.Owner, 1, TenguAncestryLoader.TenguTrait);
            //                    if (pushingGust.SavingThrow != null)
            //                        pushingGust.WithSavingThrow(new SavingThrow(pushingGust.SavingThrow.Defense, wielder.ClassOrSpellDC()));
            //                    pushingGust.CastFromScroll = fan;
            //                    pushingGust.Illustration = new SideBySideIllustration(fanArt, pushingGust.Illustration);
            //                    //pushingGust.Name = "Tengu feather fan {i}(" + pushingGust.Name + "){/i}";
            //                    pushingGust.Name = pushingGust.Name + " {i}(tengu feather fan){/i}";
            //                    Possibility spellPossibility1 = Possibilities.CreateSpellPossibility(pushingGust);
            //                    spellPossibility1.PossibilitySize = PossibilitySize.Half;
            //                    spellPossibility1.PossibilityGroup = "Use worn item";

            //                    CombatAction wallOfFire = AllSpells.CreateSpellInCombat(SpellId.WallOfFire, self.Owner, 4, TenguAncestryLoader.TenguTrait);
            //                    if (wallOfFire.SavingThrow != null)
            //                        wallOfFire.WithSavingThrow(new SavingThrow(wallOfFire.SavingThrow.Defense, wielder.ClassOrSpellDC()));
            //                    wallOfFire.CastFromScroll = fan;
            //                    wallOfFire.Illustration = new SideBySideIllustration(fanArt, wallOfFire.Illustration);
            //                    //wallOfFire.Name = "Tengu feather fan {i}(" + wallOfFire.Name + "){/i}";
            //                    wallOfFire.Name = wallOfFire.Name + " {i}(tengu feather fan){/i}";
            //                    Possibility spellPossibility2 = Possibilities.CreateSpellPossibility(wallOfFire);
            //                    spellPossibility2.PossibilitySize = PossibilitySize.Half;
            //                    spellPossibility2.PossibilityGroup = "Use worn item";

            //                    return [spellPossibility1, spellPossibility2];
            //                }
            //            });
            //        }
            //    };


            //.WithMainTrait(TenguFeatherFan)
            //    .WithDescription("You must be a Tengu with the Tengu Feather Fan feat to use this item.\n\nWhile holding a tengu feather fan, the save DC of your tengu spells is the highest of your spell DC and your class DC, and your tengu spells use your highest spellcasting ability modifier instead of Charisma.")


            //.WithPermanentQEffectWhenWorn((QEffect self, Item fan) =>
            //{
            //    CombatAction pushingGust = AllSpells.CreateSpellInCombat(SpellId.PushingGust, self.Owner, 1, TenguAncestryLoader.TenguTrait);
            //    pushingGust.CastFromScroll = fan;
            //    pushingGust.Illustration = new SideBySideIllustration(fanArt, pushingGust.Illustration);
            //    pushingGust.Name = "Tengu feather fan {i}(" + pushingGust.Name + "){/i}";
            //    Possibility spellPossibility1 = Dawnsbury.Core.Possibilities.Possibilities.CreateSpellPossibility(pushingGust);
            //    spellPossibility1.PossibilitySize = PossibilitySize.Half;
            //    spellPossibility1.PossibilityGroup = "Use worn item";

            //    CombatAction wallOfFire = AllSpells.CreateSpellInCombat(SpellId.WallOfFire, self.Owner, 4, TenguAncestryLoader.TenguTrait);
            //    wallOfFire.CastFromScroll = fan;
            //    wallOfFire.Illustration = new SideBySideIllustration(fanArt, wallOfFire.Illustration);
            //    wallOfFire.Name = "Tengu feather fan {i}(" + wallOfFire.Name + "){/i}";
            //    Possibility spellPossibility2 = Dawnsbury.Core.Possibilities.Possibilities.CreateSpellPossibility(wallOfFire);
            //    spellPossibility2.PossibilitySize = PossibilitySize.Half;
            //    spellPossibility2.PossibilityGroup = "Use worn item";

            //    self.ProvideActionsIntoPossibilitySection = (QEffect self, PossibilitySection section) => section.PossibilitySectionId == PossibilitySectionId.ItemActions ? [spellPossibility1, spellPossibility2] : [];
            //});
            //});
        }


        private static Item ImplementBrace(this Item item)
        {
            // don't do anything if another mod is already handling the brace implementation
            if (externalBraceUsed) return item;
            // warning doesnt matter, ProvidesItemAction has incorrect nullability
            item.ProvidesItemAction = (Creature cr, Item self) => BraceYourWeapon(cr, self);
            return item;
        }

        // TODO: check this works on ranger, monk, etc. slightly different AoOs
        private static ActionPossibility? BraceYourWeapon(Creature owner, Item item)
        {
            if (!owner.HasEffect(QEffectId.AttackOfOpportunity)) return null;
            if (item.WeaponProperties == null) return null;
            int extraDamage = item.WeaponProperties.DamageDieCount * 2;
            string damageString = S.HeightenedVariable(extraDamage, 2);
            return new CombatAction(owner, ChangeGripArt, $"Brace Your Weapon", [Trait.Concentrate],
                "{i}You ready your weapon to hit unattentive foes where it really hurts.{/i}\n\nYour turn ends immediately when you use this action. Until the start of your next turn, Strikes you make as part of a reaction deal an additional 2 precision damage per weapon damage die (total " + damageString + " damage).",
                Target.Self()).WithEffectOnChosenTargets(async (CombatAction action, Creature self, ChosenTargets _) =>
                {
                    // confirm if the player wants to waste actions, if they have them left
                    if (!self.Actions.IsOutOfActions())
                    {
                        var endTurn = await self.AskForConfirmation(IllustrationName.EndTurn, "You have actions left that would be wasted. Do you want to Brace your Weapon and end your turn anyway?", "End turn", "No");
                        if (!endTurn)
                        {
                            self.Actions.RevertExpendingOfResources(action.ActionCost, action);
                            return;
                        }
                    }
                    self.AddQEffect(new QEffect("Weapon braced", $"Reaction Strikes deal {damageString} extra precision damage until the start of your next turn.")
                    {
                        YouDealDamageWithStrike = (QEffect self, CombatAction action, DiceFormula diceFormula, Creature defender) =>
                        {
                            if (defender.IsImmuneTo(Trait.PrecisionDamage)) return diceFormula;
                            return diceFormula.Add(DiceFormula.FromText(extraDamage.ToString(), "Weapon braced"));
                        },
                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
                        Illustration = ChangeGripArt
                    });
                    // End turn immediately
                    self.Actions.ActionsLeft = 0;
                    self.Actions.UsedQuickenedAction = true;
                    self.Actions.WishesToEndTurn = true;
                }).WithActionCost(1).WithShortDescription($"End your turn immediately, then deal {damageString} extra damage on reaction Strikes until the start of your next turn.");
        }
    }
}
