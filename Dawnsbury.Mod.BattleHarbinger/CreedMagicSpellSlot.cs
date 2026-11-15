using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Display;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.Mechanics.Enumerations;

namespace Dawnsbury.Mods.BattleHarbinger
{
    public class CreedMagicSpellSlot(int level, string key) : PreparedSpellSlot(level, key)
    {
        public override string SlotName => "Creed magic";

        public List<SpellId> CreedSpells = [];
        // public List<SpellId> CreedSpells = [SpellId.ResistEnergy, SpellId.SeeInvisibility, SpellId.TrueStrike];

        public CreedMagicSpellSlot(int level, string key, List<SpellId> extraCreedSpells) : this(level, key)
        {
            CreedSpells.AddRange(extraCreedSpells);
            // Change the key when more spells are added, to prevent reducing a character's level and leaving illegal spells prepared
            foreach (SpellId spell in extraCreedSpells)
            {
                Key += spell.HumanizeTitleCase2();
            }
        }

        public override string? DisallowsSpellBecause(Spell preparedSpell, CharacterSheet sheet, PreparedSpellSlots preparedSpellSlots)
        {
            if (CreedSpells.Contains(preparedSpell.SpellId))
            {
                return base.DisallowsSpellBecause(preparedSpell, sheet, preparedSpellSlots);
            }
            string allowed = CreedSpells.Select(id => AllSpells.CreateModernSpellTemplate(id, Trait.Cleric).Name).Aggregate((x, y) => $"{x}\n{y}");
            return "You can only prepare these spells in your creed magic slots:\n" + allowed;
            // return "You can only prepare {i}resist energy{/i}, {i}see invisibility{/i} and {i}true strike{/i} in your creed magic slots.";
        }
    }
}

