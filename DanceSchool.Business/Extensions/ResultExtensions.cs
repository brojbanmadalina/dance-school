using FluentResults;
using Microsoft.AspNetCore.Mvc;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        return result.IsFailed
            ? new BadRequestObjectResult(new { message = result.Errors.First().Message })
            : new OkResult();
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.IsFailed
            ? new BadRequestObjectResult(new { message = result.Errors.First().Message })
            : new OkObjectResult(result.Value);
    }

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        Func<Result<T>, IActionResult>? onFail = null
    )
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return onFail?.Invoke(result)
            ?? new BadRequestObjectResult(new { message = result.Errors.First().Message });
    }
}
