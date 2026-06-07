using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Common;
using SoberNetwork.Core.Validators.Common;

namespace SoberNetwork.Core.Tests.Validators.Common;

public class PaginationQueryValidatorTests
{
    private readonly PaginationQueryValidator _validator = new();

    [Fact]
    public void valid_query_passes()
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = 10 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void valid_page_values_pass(int page)
    {
        // Arrange
        var query = new PaginationQuery { Page = page, PageSize = 25 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void page_less_than_one_fails(int page)
    {
        // Arrange
        var query = new PaginationQuery { Page = page, PageSize = 25 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void valid_pagesize_values_pass(int pageSize)
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = pageSize };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void pagesize_out_of_range_fails(int pageSize)
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = pageSize };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
