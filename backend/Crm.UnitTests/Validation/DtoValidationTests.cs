using System.ComponentModel.DataAnnotations;
using Crm.Application.DTOs.Auth;
using Crm.Application.DTOs.Clients;
using Crm.Application.DTOs.Leads;
using FluentAssertions;

namespace Crm.UnitTests.Validation;

public class DtoValidationTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void LoginRequest_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var request = new LoginRequest { Email = "not-an-email", Password = "Password123" };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginRequest_WithShortPassword_ShouldFail()
    {
        // Arrange
        var request = new LoginRequest { Email = "admin@example.com", Password = "123" };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Password"));
    }

    [Fact]
    public void CreateClientRequest_WithMissingName_ShouldFail()
    {
        // Arrange
        var request = new CreateClientRequest { LegalName = "Acme" };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void CreateLeadRequest_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var request = new CreateLeadRequest 
        { 
            Title = "New Lead", 
            ContactName = "John Doe", 
            Email = "invalid" 
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Email"));
    }
}
