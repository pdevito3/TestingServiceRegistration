namespace RecipeManagement.IntegrationTests.GroupOne.FeatureTestsFive.Ingredients;

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RecipeManagement.Domain.Ingredients.Dtos;
using RecipeManagement.Domain.Ingredients.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Ingredient;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using static TestFixture;

public class IngredientListQueryTests : TestBase
{
    
    [Test]
    public async Task can_get_ingredient_list()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        var fakeRecipeTwo = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        await InsertAsync(fakeRecipeOne, fakeRecipeTwo);

        var fakeIngredientOne = FakeIngredient.Generate(new FakeIngredientForCreationDto()
            .RuleFor(i => i.RecipeId, _ => fakeRecipeOne.Id).Generate());
        var fakeIngredientTwo = FakeIngredient.Generate(new FakeIngredientForCreationDto()
            .RuleFor(i => i.RecipeId, _ => fakeRecipeTwo.Id).Generate());
        var queryParameters = new IngredientParametersDto();

        await InsertAsync(fakeIngredientOne, fakeIngredientTwo);

        // Act
        var query = new GetIngredientList.Query(queryParameters);
        var ingredients = await SendAsync(query);

        // Assert
        ingredients.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}