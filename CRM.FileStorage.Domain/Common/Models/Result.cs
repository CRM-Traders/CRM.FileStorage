using System.Text.Json.Serialization;

namespace CRM.FileStorage.Domain.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    protected Result(bool isSuccess, string? error, string? errorCode,
        IReadOnlyDictionary<string, string[]>? validationErrors,
        IReadOnlyDictionary<string, object>? metadata)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
        Metadata = metadata;
    }

    public static Result Success(Dictionary<string, object>? metadata = null)
    {
        return new Result(true, null, null, null, metadata?.AsReadOnly());
    }

    public static Result<TValue> Success<TValue>(TValue value, Dictionary<string, object>? metadata = null)
    {
        return new Result<TValue>(value, true, null, null, null, metadata?.AsReadOnly());
    }

    public static Result Failure(string error, string? errorCode = null, Dictionary<string, object>? metadata = null)
    {
        return new Result(false, error, errorCode, null, metadata?.AsReadOnly());
    }

    public static Result<TValue> Failure<TValue>(string error, string? errorCode = null,
        Dictionary<string, object>? metadata = null)
    {
        return new Result<TValue>(default, false, error, errorCode, null, metadata?.AsReadOnly());
    }

    public static Result ValidationFailure(IReadOnlyDictionary<string, string[]> validationErrors, string? error = null,
        string? errorCode = null)
    {
        return new Result(false, error ?? "Validation failed", errorCode, validationErrors, null);
    }

    public static Result<TValue> ValidationFailure<TValue>(IReadOnlyDictionary<string, string[]> validationErrors,
        string? error = null, string? errorCode = null)
    {
        return new Result<TValue>(default, false, error ?? "Validation failed", errorCode, validationErrors, null);
    }

    public Result WithMetadata(Dictionary<string, object> metadata)
    {
        if (metadata == null || !metadata.Any())
            return this;

        var newMetadata = new Dictionary<string, object>(Metadata ?? new Dictionary<string, object>());
        foreach (var item in metadata)
        {
            newMetadata[item.Key] = item.Value;
        }

        return new Result(IsSuccess, Error, ErrorCode, ValidationErrors, newMetadata.AsReadOnly());
    }
}

public class Result<TValue> : Result
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TValue? Value { get; }

    internal Result(TValue? value, bool isSuccess, string? error, string? errorCode,
        IReadOnlyDictionary<string, string[]>? validationErrors,
        IReadOnlyDictionary<string, object>? metadata)
        : base(isSuccess, error, errorCode, validationErrors, metadata)
    {
        Value = value;
    }

    public TValue GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException($"Cannot get value of a failed result. Error: {Error}");

        return Value!;
    }

    public TValue GetValueOrDefault(TValue defaultValue = default!)
    {
        return IsSuccess ? Value! : defaultValue;
    }

    public new Result<TValue> WithMetadata(Dictionary<string, object> metadata)
    {
        if (metadata == null || !metadata.Any())
            return this;

        var newMetadata = new Dictionary<string, object>(Metadata ?? new Dictionary<string, object>());
        foreach (var item in metadata)
        {
            newMetadata[item.Key] = item.Value;
        }

        return new Result<TValue>(Value, IsSuccess, Error, ErrorCode, ValidationErrors, newMetadata.AsReadOnly());
    }

    public static implicit operator Result<TValue>(TValue value)
    {
        return Result.Success(value);
    }
}