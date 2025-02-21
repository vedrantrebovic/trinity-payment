namespace Trinity.PaymentPlatform.Model.Util;

public static class MathHelper
{
    
    public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
    {
        int i = 0;
        foreach(var item in items)
        {
            if(count == 1)
                yield return new T[] { item };
            else
            {
                foreach(var result in GetPermutations(items.Skip(i + 1), count - 1))
                    yield return new T[] { item }.Concat(result);
            }

            ++i;
        }
    }
    
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>
        (this IEnumerable<IEnumerable<T>> enumerables)
    {
        IEnumerable<IEnumerable<T>> Seed() { yield return Enumerable.Empty<T>(); }

        return enumerables.Aggregate(Seed(), (accumulator, enumerable)
            => accumulator.SelectMany(x => enumerable.Select(x.Append)));
    }
    
    public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
    {
        return k == 0 ? new[] { new T[0] } :
            elements.SelectMany((e, i) =>
                elements.Skip(i + 1).DifferentCombinations(k - 1).Select(c => (new[] {e}).Concat(c)));
    }

    private static Dictionary<(uint,uint), long> _combinations = new Dictionary<(uint,uint), long>(); 
    const int _maxAllowedNumberOfBets = 40;
    public static long GetNumberOfSystemCombinations(uint system, uint totalNumberOfBets)
    {
        
        if (system <= 0)
            throw new ArgumentOutOfRangeException(nameof(system), "err.number_of_system_bets_zero_or_less");
        if (totalNumberOfBets > _maxAllowedNumberOfBets)
            throw new ArgumentOutOfRangeException(nameof(totalNumberOfBets),
                $"err.total_number_of_bets_greater_than_{_maxAllowedNumberOfBets}");
        if (system > totalNumberOfBets)
            throw new ArgumentOutOfRangeException();

        if (system == totalNumberOfBets) return 1;

        if (_combinations.Count == 0)
            PopulateCombinations(_maxAllowedNumberOfBets);

        return _combinations[(system, totalNumberOfBets)];
    }

    public static IEnumerable<(uint, uint)> GetSystemsWithLessOrEqualNumberOfCombinations(long numberOfCombinations)
    {
        if (_combinations.Count == 0)
            PopulateCombinations(_maxAllowedNumberOfBets);

        return _combinations.Where(p => p.Value <= numberOfCombinations).Select(x => x.Key);
    }

    private static void PopulateCombinations(int maxAllowedNumberOfBets)
    {
        for (uint i = 1; i <= maxAllowedNumberOfBets; i++)
        {
            for (uint j = 1; j <= i; j++)
            {
                string key = $"{j}_{i}";
                long result = NumberOfCombinations(i, j);
                _combinations.Add((j,i), result);
            }
        }
    }

    static long NumberOfCombinations(uint n, uint k)
    {
        // number combinations
        if (n < 0 || k < 0)
            throw new Exception("Negative argument in Choose()");
        if (n < k) return 0; // special
        if (n == k) return 1; // short-circuit

        uint delta, iMax;

        if (k < n - k) // ex: Choose(100,3)
        {
            delta = n - k; iMax = k;
        }
        else           // ex: Choose(100,97)
        {
            delta = k; iMax = n - k;
        }

        long ans = delta + 1;
        for (int i = 2; i <= iMax; ++i)
            ans = (ans * (delta + i)) / i;

        return ans;
    }
}