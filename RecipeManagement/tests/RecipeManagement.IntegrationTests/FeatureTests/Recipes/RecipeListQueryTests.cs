namespace RecipeManagement.IntegrationTests.FeatureTests.Recipes;

using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using SharedKernel.Exceptions;
using RecipeManagement.Domain.Recipes.Features;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using Domain;
using HeimGuard;
using Moq;
using static TestFixture;
using RecipeManagement.SharedTestHelpers.Fakes.Author;

public class RecipeListQueryTests : TestBase
{
    
    [Test]
    public async Task can_get_recipe_list()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        var fakeRecipeTwo = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        var queryParameters = new RecipeParametersDto();

        await InsertAsync(fakeRecipeOne, fakeRecipeTwo);

        // Act
        var query = new GetRecipeList.Query(queryParameters);
        var recipes = await SendAsync(query);

        // Assert
        recipes.Count.Should().BeGreaterThanOrEqualTo(2);
    }
    
    [Test]
    [NonParallelizable]
    public async Task must_be_permitted()
    {
        // Arrange
        SetUserNotPermitted(Permissions.CanReadRecipes);
        var queryParameters = new RecipeParametersDto();
            
        // Act
        var query = new GetRecipeList.Query(queryParameters);
        var act = () => SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}