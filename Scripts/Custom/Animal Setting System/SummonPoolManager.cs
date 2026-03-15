using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Spells
{
    public struct SummonInfo
    {
        public Type Type;
        public double MinTame;

        public SummonInfo(Type type, double minTame)
        {
            Type = type;
            MinTame = minTame;
        }
    }

    public class SummonPoolManager
    {
        public static readonly SummonInfo[] AnimalPool = new SummonInfo[]
        {
            new SummonInfo( typeof( Bird ), -18.9 ), new SummonInfo( typeof( Cat ), -18.9 ),
            new SummonInfo( typeof( Chicken ), -18.9 ), new SummonInfo( typeof( Dog ), -18.9 ),
            new SummonInfo( typeof( Ferret ), -18.9 ), new SummonInfo( typeof( Goat ), -18.9 ),
            new SummonInfo( typeof( Pig ), -18.9 ), new SummonInfo( typeof( Rabbit ), -18.9 ),
            new SummonInfo( typeof( Rat ), -18.9 ), new SummonInfo( typeof( Sheep ), -18.9 ),
            new SummonInfo( typeof( JackRabbit ), -18.9 ), new SummonInfo( typeof( Parrot ), 0.0 ),
            new SummonInfo( typeof( Dolphin ), 9.9 ), new SummonInfo( typeof( Cow ), 11.1 ),
            new SummonInfo( typeof( Llama ), 11.1 ), new SummonInfo( typeof( Hind ), 15.1 ),
            new SummonInfo( typeof( SeaHorse ), 15.1 ), new SummonInfo( typeof( Boar ), 29.1 ),
            new SummonInfo( typeof( DesertOstard ), 29.1 ), new SummonInfo( typeof( Horse ), 29.1 ),
            new SummonInfo( typeof( Crane ), 29.1 ), new SummonInfo( typeof( ForestOstard ), 29.1 ),
            new SummonInfo( typeof( Palomino ), 29.1 ), new SummonInfo( typeof( RidableLlama ), 29.1 ),
            new SummonInfo( typeof( Walrus ), 31.1 ), new SummonInfo( typeof( BlackBear ), 35.1 ),
            new SummonInfo( typeof( GreatHart ), 35.1 ), new SummonInfo( typeof( MountainGoat ), 35.1 ),
            new SummonInfo( typeof( TimberWolf ), 35.1 ), new SummonInfo( typeof( BullFrog ), 35.0 ),
            new SummonInfo( typeof( Cougar ), 41.1 ), new SummonInfo( typeof( BrownBear ), 47.1 ),
            new SummonInfo( typeof( Gorilla ), 53.1 ), new SummonInfo( typeof( SnowLeopard ), 53.1 ),
            new SummonInfo( typeof( GrizzlyBear ), 59.1 ), new SummonInfo( typeof( WhiteWolf ), 65.1 ),
            new SummonInfo( typeof( Bull ), 71.1 ), new SummonInfo( typeof( GreyWolf ), 71.1 ),
            new SummonInfo( typeof( Ridgeback ), 83.1 ), new SummonInfo( typeof( Panther ), 85.1 ),
            new SummonInfo( typeof( PolarBear ), 95.1 ), new SummonInfo( typeof( SkeletalCat ), 95.1 )
        };

        // 공통 로직: 시전자의 실력에 따른 무작위 동물 타입 반환
        public static Type GetEligibleAnimal(Mobile caster)
        {
            // 기획 공식: 20 + (보너스 * 0.004)
            double bonus = SpellHelper.GetMagicValue(caster, 0.004);
            double maxTameReq = 20.0 + bonus;

            List<Type> eligible = new List<Type>();

            foreach (SummonInfo info in AnimalPool)
            {
                if (info.MinTame <= maxTameReq)
                    eligible.Add(info.Type);
                else
                    break; // 정렬되어 있으므로 이후는 생략
            }

            if (eligible.Count > 0)
                return eligible[Utility.Random(eligible.Count)];

            return typeof(Dog); // 만약의 사태를 대비한 기본값
        }
    }
}