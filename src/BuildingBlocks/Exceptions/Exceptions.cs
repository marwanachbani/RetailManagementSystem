namespace RMS.BuildingBlocks.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class BusinessRuleValidationException : Exception
{
    public BusinessRuleValidationException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }

    public string RuleName { get; }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.") { }
}
