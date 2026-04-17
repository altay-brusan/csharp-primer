namespace Patterns.Functional;

/// <summary>
/// Demonstrates the Result / Either pattern — a functional alternative to exceptions
/// for expected error paths.
///
/// Returning a Result&lt;T, TError&gt; instead of throwing makes the happy path and error
/// path explicit in the method signature. The caller is forced to handle both cases.
///
/// Use exceptions for unexpected failures (bugs, infrastructure outages).
/// Use Result for expected, recoverable business errors (validation, not-found, etc.).
/// </summary>
public sealed class Result<TValue, TError>
{
    private readonly TValue?  _value;
    private readonly TError?  _error;
    private readonly bool     _isSuccess;

    private Result(TValue value)  { _value = value; _isSuccess = true; }
    private Result(TError error)  { _error = error; _isSuccess = false; }

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    // ── Construction ──────────────────────────────────────────────────────────
    public static Result<TValue, TError> Ok(TValue value)    => new(value);
    public static Result<TValue, TError> Fail(TError error)  => new(error);

    // ── Deconstruction ────────────────────────────────────────────────────────
    public void Deconstruct(out bool isSuccess, out TValue? value, out TError? error)
        => (isSuccess, value, error) = (_isSuccess, _value, _error);

    // ── Map (transform value, propagate errors) ───────────────────────────────
    public Result<TNew, TError> Map<TNew>(Func<TValue, TNew> mapper) =>
        _isSuccess ? Result<TNew, TError>.Ok(mapper(_value!))
                   : Result<TNew, TError>.Fail(_error!);

    // ── Bind / FlatMap (chain operations that may fail) ───────────────────────
    public Result<TNew, TError> Bind<TNew>(Func<TValue, Result<TNew, TError>> next) =>
        _isSuccess ? next(_value!) : Result<TNew, TError>.Fail(_error!);

    // ── Match (exhaustive handling) ───────────────────────────────────────────
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<TError, TOut> onFailure) =>
        _isSuccess ? onSuccess(_value!) : onFailure(_error!);

    // ── Unsafe accessors (use Match / Bind in preference) ─────────────────────
    public TValue  Value => _isSuccess ? _value!  : throw new InvalidOperationException("Result is a failure.");
    public TError  Error => !_isSuccess ? _error! : throw new InvalidOperationException("Result is a success.");

    public override string ToString() =>
        _isSuccess ? $"Ok({_value})" : $"Fail({_error})";
}

/// <summary>
/// Example domain usage of Result&lt;T, TError&gt;.
/// </summary>
public static class ResultPatternExamples
{
    public enum UserError { NotFound, InvalidEmail, DuplicateEmail }

    public record User(int Id, string Name, string Email);

    // ── Service methods return Result ─────────────────────────────────────────
    public static Result<User, UserError> FindUser(IReadOnlyList<User> users, int id)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        return user is null
            ? Result<User, UserError>.Fail(UserError.NotFound)
            : Result<User, UserError>.Ok(user);
    }

    public static Result<string, UserError> ValidateEmail(string email) =>
        email.Contains('@') && email.Contains('.')
            ? Result<string, UserError>.Ok(email.ToLowerInvariant())
            : Result<string, UserError>.Fail(UserError.InvalidEmail);

    // ── Chaining with Bind ────────────────────────────────────────────────────
    public static Result<User, UserError> UpdateEmail(
        IReadOnlyList<User> users, int id, string newEmail) =>
        FindUser(users, id)
            .Bind(user => ValidateEmail(newEmail)
                .Map(email => user with { Email = email }));

    // ── Consuming with Match ──────────────────────────────────────────────────
    public static string DescribeResult(Result<User, UserError> result) =>
        result.Match(
            onSuccess: u  => $"User found: {u.Name} <{u.Email}>",
            onFailure: err => $"Error: {err}");
}
