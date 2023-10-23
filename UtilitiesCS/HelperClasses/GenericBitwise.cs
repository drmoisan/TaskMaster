using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.HelperClasses
{
    public class GenericBitwise<TFlagEnum> where TFlagEnum : Enum
    {
        private readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _and = null;
        private readonly Func<TFlagEnum, TFlagEnum> _not = null;
        private readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _or = null;
        private readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _xor = null;

        public GenericBitwise()
        {
            _and = And().Compile();
            _not = Not().Compile();
            _or = Or().Compile();
            _xor = Xor().Compile();
        }

        public TFlagEnum And(TFlagEnum value1, TFlagEnum value2) => _and(value1, value2);
        public TFlagEnum And(IEnumerable<TFlagEnum> list) => list.Aggregate(And);
        public TFlagEnum Not(TFlagEnum value) => _not(value);
        public TFlagEnum Or(TFlagEnum value1, TFlagEnum value2) => _or(value1, value2);
        public TFlagEnum Or(IEnumerable<TFlagEnum> list) => list.Aggregate(Or);
        public TFlagEnum Xor(TFlagEnum value1, TFlagEnum value2) => _xor(value1, value2);
        public TFlagEnum Xor(IEnumerable<TFlagEnum> list) => list.Aggregate(Xor);

        public TFlagEnum All()
        {
            var allFlags = Enum.GetValues(typeof(TFlagEnum)).Cast<TFlagEnum>();
            return Or(allFlags);
        }

        private Expression<Func<TFlagEnum, TFlagEnum>> Not()
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

        private Expression<Func<TFlagEnum, TFlagEnum, TFlagEnum>> And()
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

        private Expression<Func<TFlagEnum, TFlagEnum, TFlagEnum>> Or()
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

        private Expression<Func<TFlagEnum, TFlagEnum, TFlagEnum>> Xor()
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
}
