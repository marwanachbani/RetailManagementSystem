using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Identity.Domain.Entities;
using RMS.Modules.Identity.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Identity.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_Should_CreateUserAndRaiseEvent()
    {
        var email = Email.Create("test@example.com");
        var hash = PasswordHash.Create("hashed_password_123");

        var user = User.Create(
            Guid.NewGuid(),
            "jdoe",
            email,
            hash,
            "John Doe",
            UserRole.Manager);

        user.UserName.Should().Be("jdoe");
        user.Email.Should().Be(email);
        user.FullName.Should().Be("John Doe");
        user.Role.Should().Be(UserRole.Manager);
        user.IsActive.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RMS.Modules.Identity.Domain.Events.UserRegisteredEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyUserName_Should_Throw(string? userName)
    {
        var email = Email.Create("test@example.com");
        var hash = PasswordHash.Create("hashed_password_123");

        Action act = () => User.Create(Guid.NewGuid(), userName!, email, hash, "John Doe");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "User.UserNameEmpty");
    }

    [Fact]
    public void Create_WithShortUserName_Should_Throw()
    {
        var email = Email.Create("test@example.com");
        var hash = PasswordHash.Create("hashed_password_123");

        Action act = () => User.Create(Guid.NewGuid(), "ab", email, hash, "John Doe");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "User.UserNameTooShort");
    }

    [Fact]
    public void Create_WithLongUserName_Should_Throw()
    {
        var email = Email.Create("test@example.com");
        var hash = PasswordHash.Create("hashed_password_123");

        Action act = () => User.Create(Guid.NewGuid(), new string('x', 51), email, hash, "John Doe");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "User.UserNameTooLong");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyFullName_Should_Throw(string fullName)
    {
        var email = Email.Create("test@example.com");
        var hash = PasswordHash.Create("hashed_password_123");

        Action act = () => User.Create(Guid.NewGuid(), "jdoe", email, hash, fullName);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "User.FullNameEmpty");
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        var user = CreateValidUser();
        user.Deactivate();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_Should_SetIsActiveToTrue()
    {
        var user = CreateValidUser();
        user.Deactivate();
        user.Activate();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChangeRole_Should_UpdateRole()
    {
        var user = CreateValidUser();
        user.ChangeRole(UserRole.Admin);
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void ChangePassword_Should_UpdatePasswordHash()
    {
        var user = CreateValidUser();
        var newHash = PasswordHash.Create("new_hash_456");
        user.ChangePassword(newHash);
        user.PasswordHash.Should().Be(newHash);
    }

    [Fact]
    public void UpdateProfile_Should_UpdateFullNameAndEmail()
    {
        var user = CreateValidUser();
        var newEmail = Email.Create("new@example.com");
        user.UpdateProfile("Jane Doe", newEmail);
        user.FullName.Should().Be("Jane Doe");
        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void Create_Should_ClearDomainEvents_WhenCalled()
    {
        var user = CreateValidUser();
        user.ClearDomainEvents();
        user.DomainEvents.Should().BeEmpty();
    }

    private static User CreateValidUser()
    {
        return User.Create(
            Guid.NewGuid(),
            "jdoe",
            Email.Create("jdoe@example.com"),
            PasswordHash.Create("hashed_password_123"),
            "John Doe",
            UserRole.Cashier);
    }
}
