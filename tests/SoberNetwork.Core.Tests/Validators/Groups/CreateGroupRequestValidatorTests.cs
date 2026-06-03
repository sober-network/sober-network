using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Validators.Groups;

namespace SoberNetwork.Core.Tests.Validators.Groups;

public class CreateGroupRequestValidatorTests
{
    private readonly CreateGroupRequestValidator _validator = new();

    private static CreateGroupRequest Valid() => new("AA Monday Night", "aa-monday-night");

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = Valid();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void empty_name_fails()
    {
        // Arrange
        var request = Valid() with { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void empty_slug_fails()
    {
        // Arrange
        var request = Valid() with { Slug = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("UPPERCASE")]
    [InlineData("has spaces")]
    [InlineData("special!chars")]
    [InlineData("under_score")]
    public void invalid_slug_pattern_fails(string slug)
    {
        // Arrange
        var request = Valid() with { Slug = slug };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("aa")]
    [InlineData("aa-monday-night")]
    [InlineData("group123")]
    public void valid_slug_passes(string slug)
    {
        // Arrange
        var request = Valid() with { Slug = slug };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void name_too_long_fails()
    {
        // Arrange
        var request = Valid() with { Name = new string('a', 101) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
