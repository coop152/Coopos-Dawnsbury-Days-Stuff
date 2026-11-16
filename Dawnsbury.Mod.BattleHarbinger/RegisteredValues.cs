using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Modding;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;

namespace Dawnsbury.Mods.BattleHarbinger.RegisteredValues
{
    // Class to contain any registered enum members which are used to refer to things, e.g. QEffectId, ItemName, FeatName

    internal class ModTrait
    {
        public static readonly Trait BattleHarbinger = ModManager.RegisterTrait("Battle Harbinger");
        public static readonly Trait BattleAura = ModManager.RegisterTrait("Battle Aura");
    }

    internal class ModFeatName
    {
        public static readonly FeatName BattleHarbingerDoctrine = ModManager.RegisterFeatName("Battle Harbinger");
        public static readonly FeatName AuraEnhancement = ModManager.RegisterFeatName("Aura Enhancement");
        public static readonly FeatName ExigentAura = ModManager.RegisterFeatName("Exigent Aura");
        public static readonly FeatName TandemOnslaughtFeatName = ModManager.RegisterFeatName("Tandem Onslaught");
        public static readonly FeatName EmpoweredOnslaughtFeatName = ModManager.RegisterFeatName("Empowered Onslaught", "Empowered Onslaught {icon:Reaction}");
        public static FeatName BattleHarbingerDedication; // is set by the code that creates the dedication
    }

    internal class ModQEffectId
    {
        public static readonly QEffectId TandemOnslaughtOncePerTurn = ModManager.RegisterEnumMember<QEffectId>("tandemOnslaughtUsedOnceThisTurnAlready");
        public static readonly QEffectId RolledAgainstMalediction = ModManager.RegisterEnumMember<QEffectId>("RolledAgainstMalediction");
        public static readonly QEffectId FailedAgainstMalediction = ModManager.RegisterEnumMember<QEffectId>("FailedAgainstMalediction");
    }

    // Spells registered in ModSpells
    internal class ModSpellId
    {
        static public SpellId BattleBenediction;
        static public SpellId BattleMalediction;
        static public SpellId BattleBless;
        static public SpellId BattleBane;
    }
}
