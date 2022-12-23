namespace RecipeManagement.IntegrationTests.GroupFive.FeatureTestsThree.Recipes;

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RecipeManagement.Domain;
using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using SharedKernel.Exceptions;

public class RecipeQueryTests : TestBase
{
    [Test]
    public async Task can_get_existing_recipe_with_accurate_props()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        await InsertAsync(fakeRecipeOne);

        // Act
        var query = new GetRecipe.Query(fakeRecipeOne.Id);
        var recipe = await SendAsync(query);

        // Assert
        recipe.Title.Should().Be(fakeRecipeOne.Title);
        recipe.Visibility.Should().Be(fakeRecipeOne.Visibility);
        recipe.Directions.Should().Be(fakeRecipeOne.Directions);
        recipe.Rating.Should().Be(fakeRecipeOne.Rating);
        recipe.DateOfOrigin.Should().Be(fakeRecipeOne.DateOfOrigin);
        recipe.HaveMadeItMyself.Should().Be(fakeRecipeOne.HaveMadeItMyself);
    }

    [Test]
    public async Task get_recipe_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var badId = Guid.NewGuid();

        // Act
        var query = new GetRecipe.Query(badId);
        Func<Task> act = () => SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Test]
    [NonParallelizable]
    public async Task must_be_permitted()
    {
        // Arrange
        SetUserNotPermitted(Permissions.CanReadRecipes);
        
        var query = new GetRecipe.Query(Guid.NewGuid());
        Func<Task> act = () => SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}