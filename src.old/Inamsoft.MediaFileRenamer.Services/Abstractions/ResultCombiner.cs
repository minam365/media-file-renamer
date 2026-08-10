using TinyResult;

namespace Inamsoft.MediaFileRenamer.Services.Abstractions;

public static class ResultCombiner
{
    public static Result<T> FirstSuccess<T>(params Func<Result<T>>[] attempts)
    {
        Result<T>? lastError = null;

        foreach (var attempt in attempts)
        {
            var result = attempt();
            if (result.IsSuccess)
                return result;

            lastError = result;
        }

        return lastError!;
    }


    public static Result<T> FirstSuccess<T>(IEnumerable<Func<Result<T>>> attempts)
    {
        Result<T>? lastError = null;

        foreach (var attempt in attempts)
        {
            var result = attempt();
            if (result.IsSuccess)
                return result;

            lastError = result;
        }

        return lastError!;
    }

}