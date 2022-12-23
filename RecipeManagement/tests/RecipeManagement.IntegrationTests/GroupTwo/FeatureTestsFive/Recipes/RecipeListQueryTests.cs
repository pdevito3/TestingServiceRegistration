namespace RecipeManagement.IntegrationTests.GroupTwo.FeatureTestsFive.Recipes;

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using static TestFixture;

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
}