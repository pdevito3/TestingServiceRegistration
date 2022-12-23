namespace RecipeManagement.IntegrationTests.GroupOne.FeatureTestsFive.Recipes;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using static TestFixture;

public class UpdateRecipeCommandTests : TestBase
{
    [Test]
    public async Task can_update_existing_recipe_in_db()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        var updatedRecipeDto = new FakeRecipeForUpdateDto().Generate();
        await InsertAsync(fakeRecipeOne);

        var recipe = await ExecuteDbContextAsync(db => db.Recipes
            .FirstOrDefaultAsync(r => r.Id == fakeRecipeOne.Id));
        var id = recipe.Id;

        // Act
        var command = new UpdateRecipe.Command(id, updatedRecipeDto);
        await SendAsync(command);
        var updatedRecipe = await ExecuteDbContextAsync(db => db.Recipes.FirstOrDefaultAsync(r => r.Id == id));

        // Assert
        updatedRecipe.Title.Should().Be(updatedRecipeDto.Title);
        updatedRecipe.Visibility.Should().Be(updatedRecipeDto.Visibility);
        updatedRecipe.Directions.Should().Be(updatedRecipeDto.Directions);
        updatedRecipe.Rating.Should().Be(updatedRecipeDto.Rating);
        updatedRecipe.DateOfOrigin.Should().Be(updatedRecipeDto.DateOfOrigin);
        updatedRecipe.HaveMadeItMyself.Should().Be(updatedRecipeDto.HaveMadeItMyself);
    }
}