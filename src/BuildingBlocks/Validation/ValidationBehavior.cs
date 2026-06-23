using FluentValidation;
using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.BuildingBlocks.Validation;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before
/// the handler executes. If validation fails, a Result.Failure is returned
/// without ever reaching the handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Any())
        {
            var first = failures.First();
            var resultType = typeof(TResponse);
            object failureResult;

            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = resultType.GetGenericArguments()[0];
                var method = typeof(Result).GetMethod(nameof(Result.Failure), 1, new[] { typeof(string), typeof(string) })!;
                var genericMethod = method.MakeGenericMethod(valueType);
                failureResult = genericMethod.Invoke(null, new object?[] { first.ErrorMessage, first.ErrorCode })!;
            }
            else
            {
                failureResult = Result.Failure(first.ErrorMessage, first.ErrorCode);
            }

            return (TResponse)failureResult;
        }

        return await next();
    }

}
