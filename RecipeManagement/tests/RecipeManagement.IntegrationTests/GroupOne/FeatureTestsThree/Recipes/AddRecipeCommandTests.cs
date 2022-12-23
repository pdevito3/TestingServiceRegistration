namespace RecipeManagement.IntegrationTests.GroupOne.FeatureTestsThree.Recipes;

using System.Threading.Tasks;
using FluentAssertions;
using HeimGuard;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RecipeManagement.Domain;
using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using SharedKernel.Exceptions;

public class AddRecipeCommandTests : TestBase
{
    [Test]
    [NonParallelizable]
    public async Task must_be_permitted()
    {
        var userPolicyHandler = GetService<IHeimGuardClient>();
        Mock.Get(userPolicyHandler)
            .Setup(x => x.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddRecipes))
            .ThrowsAsync(new ForbiddenAccessException());
        
        // Arrange
        var fakeRecipeOne = new FakeRecipeForCreationDto().Generate();

        // Act
        var command = new AddRecipe.Command(fakeRecipeOne);
        var act = () => SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
    
    [Test]
    public async Task can_add_new_recipe_to_db()
    {
        // Arrange
        var fakeRecipeOne = new FakeRecipeForCreationDto().Generate();

        // Act
        var command = new AddRecipe.Command(fakeRecipeOne);
        var recipeReturned = await SendAsync(command);
        var recipeCreated = await ExecuteDbContextAsync(db => db.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeReturned.Id));

        // Assert
        recipeReturned.Title.Should().Be(fakeRecipeOne.Title);
        recipeReturned.Visibility.Should().Be(fakeRecipeOne.Visibility);
        recipeReturned.Directions.Should().Be(fakeRecipeOne.Directions);
        recipeReturned.Rating.Should().Be(fakeRecipeOne.Rating);
        recipeReturned.DateOfOrigin.Should().Be(fakeRecipeOne.DateOfOrigin);
        recipeReturned.HaveMadeItMyself.Should().Be(fakeRecipeOne.HaveMadeItMyself);

        recipeCreated.Title.Should().Be(fakeRecipeOne.Title);
        recipeCreated.Visibility.Should().Be(fakeRecipeOne.Visibility);
        recipeCreated.Directions.Should().Be(fakeRecipeOne.Directions);
        recipeCreated.Rating.Should().Be(fakeRecipeOne.Rating);
        recipeCreated.DateOfOrigin.Should().Be(fakeRecipeOne.DateOfOrigin);
        recipeCreated.HaveMadeItMyself.Should().Be(fakeRecipeOne.HaveMadeItMyself);
    }
}