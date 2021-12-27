#nullable enable

namespace ScoringEngine.Common.Utilities
{
    public class Optional
    {
        public static Func<T1?, T2?> HandleOptional<T1, T2> (Func<T1,T2> func)
            where T1 : class
            where T2 : class
        {
            return val => val is not null ? func(val) : null;
        }
    }
}