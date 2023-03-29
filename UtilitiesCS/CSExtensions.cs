using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    /// <summary>
    /// Generic Extension to combine flags in an enumeration. 
    /// Extension takes the following overloads:
    /// var genericBitwise = new GenericBitwise<FlagType>();
    /// var combinedAnd = genericBitwise.And(new[] { FlagType.First, FlagType.Second, FlagType.Fourth });
    /// var combinedOr = genericBitwise.Or(new[] { FlagType.First, FlagType.Second, FlagType.Fourth });
    /// Solution from 
    /// https://stackoverflow.com/questions/53636974/c-sharp-method-to-combine-a-generic-list-of-enum-values-to-a-single-value
    /// by @madreflection and doctor-jones
    /// </summary>
    /// <typeparam name="TFlagEnum"></typeparam>
    public static class GenericBitwise<TFlagEnum> where TFlagEnum : Enum
    {
        private static readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _and = And().Compile();
        private static readonly Func<TFlagEnum, TFlagEnum> _not = Not().Compile();
        private static readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _or = Or().Compile();
        private static readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _xor = Xor().Compile();

        static GenericBitwise()
        {
            //_and = And().Compile();
            //_not = Not().Compile();
            //_or = Or().Compile();
            //_xor = Xor().Compile();
        }

        public static TFlagEnum And(TFlagEnum value1, TFlagEnum value2) => _and(value1, value2);
        public static TFlagEnum And(IEnumerable<TFlagEnum> list) => list.Aggregate(And);
        public static TFlagEnum Not(TFlagEnum value) => _not(value);
        public static TFlagEnum Or(TFlagEnum value1, TFlagEnum value2) => _or(value1, value2);
        public static TFlagEnum Or(IEnumerable<TFlagEnum> list) => list.Aggregate(Or);
        public static TFlagEnum Xor(TFlagEnum value1, TFlagEnum value2) => _xor(value1, value2);
        public static TFlagEnum Xor(IEnumerable<TFlagEnum> list) => list.Aggregate(Xor);

        public static TFlagEnum All()
        {
            var allFlags = Enum.GetValues(typeof(TFlagEnum)).Cast<TFlagEnum>();
            return Or(allFlags);
        }

        private static Expression<Func<TFlagEnum, TFlagEnum>> Not()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TFlagEnum));
            var v1 = Expression.Parameter(typeof(TFlagEnum));

            return Expression.Lambda<Func<TFlagEnum, TFlagEnum>>(
                Expression.Convert(
                    Expression.Not( // ~
                        Expression.Convert(v1, underlyingType)
                    ),
                    typeof(TFlagEnum) // convert the result of the tilde back into the enum type
                ),
                v1 // the argument of the function
            );
        }

        private static Expression<Func<TFlagEnum, TFlagEnum, TFlagEnum>> And()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TFlagEnum));
            var v1 = Expression.Parameter(typeof(TFlagEnum));
            var v2 = Expression.Parameter(typeof(TFlagEnum));

            return Expression.Lambda<Func<TFlagEnum, TFlagEnum, TFlagEnum>>(
                Expression.Convert(
                    Expression.And( // combine the flags with an AND
                        Expression.Convert(v1, underlyingType), // convert the values to a bit maskable type (i.e. the underlying numeric type of the enum)
                        Expression.Convert(v2, underlyingType)
                    ),
                    typeof(TFlagEnum) // convert the result of the AND back into the enum type
                ),
                v1, // the first argument of the function
                v2 // the second argument of the function
            );
        }

        private static Expression<Func<TFlagEnum, TFlagEnum, TFlagEnum>> Or()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TFlagEnum));
            var v1 = Expression.Parameter(typeof(TFlagEnum));
            var v2 = Expression.Parameter(typeof(TFlagEnum));

            return Expression.Lambda<Func<TFlagEnum, TFlagEnum, TFlagEnum>>(
                Expression.Convert(
                    Expression.Or( // combine the flags with an OR
                        Expression.Convert(v1, underlyingType), // convert the values to a bit maskable type (i.e. the underlying numeric type of the enum)
                        Expression.Convert(v2, underlyingType)
                    ),
                    typeof(TFlagEnum) // convert the result of the OR back into the enum type
                ),
                v1, // the first argument of the function
                v2 // the second argument of the function
            );
        }

        private static Expression<Func<TFlagEnum, TFlagEnum, TFlagEnum>> Xor()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TFlagEnum));
            var v1 = Expression.Parameter(typeof(TFlagEnum));
            var v2 = Expression.Parameter(typeof(TFlagEnum));

            return Expression.Lambda<Func<TFlagEnum, TFlagEnum, TFlagEnum>>(
                Expression.Convert(
                    Expression.ExclusiveOr( // combine the flags with an XOR
                        Expression.Convert(v1, underlyingType), // convert the values to a bit maskable type (i.e. the underlying numeric type of the enum)
                        Expression.Convert(v2, underlyingType)
                    ),
                    typeof(TFlagEnum) // convert the result of the OR back into the enum type
                ),
                v1, // the first argument of the function
                v2 // the second argument of the function
            );
        }
    }
    public static class CSExtensions
    {
        public static T ToCombined<T>(this IEnumerable<T> list)
            where T : Enum
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(T));

            var currentParameter = Expression.Parameter(typeof(T), "current");
            var nextParameter = Expression.Parameter(typeof(T), "next");

            Func<T, T, T> aggregator = Expression.Lambda<Func<T, T, T>>(
                Expression.Convert(
                    Expression.Or(
                        Expression.Convert(currentParameter, underlyingType),
                        Expression.Convert(nextParameter, underlyingType)
                        ),
                    typeof(T)
                    ),
                currentParameter,
                nextParameter
                ).Compile();

            return list.Aggregate(aggregator);
        }

    }
}
