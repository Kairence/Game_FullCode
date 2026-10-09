using System;
using Server.Spells;
using Server.Spells.Bard;

namespace Server.Custom.Bard
{
    public static class BardSpellRegistry
    {
        public static readonly int Offset = 700;

        public static void Initialize()
        {
            SpellRegistry.Register(Offset + 1, typeof(AriaOfResilienceSpell));
            SpellRegistry.Register(Offset + 2, typeof(HeroicMarchSpell));
            
            SpellRegistry.Register(Offset + 5, typeof(LullabySpell));
            SpellRegistry.Register(Offset + 6, typeof(HealingChorusSpell));
            
            SpellRegistry.Register(Offset + 9, typeof(SonicBreakdownSpell));
            SpellRegistry.Register(Offset + 10, typeof(ResonanceOfDoomSpell));

            SpellRegistry.Register(Offset + 13, typeof(MentalDeliriumSpell));
            SpellRegistry.Register(Offset + 15, typeof(SirensCallSpell));
            
            Console.WriteLine("Bard Spellbook System Initialized.");
        }
    }
}