namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class AccountLockedException(DateTime lockoutEnd) : Exception(
    $"Account is locked until {lockoutEnd:O}.")
{
    public DateTime LockoutEnd { get; } = lockoutEnd;
}
