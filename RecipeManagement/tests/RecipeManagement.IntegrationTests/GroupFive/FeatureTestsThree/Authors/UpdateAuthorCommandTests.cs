namespace RecipeManagement.IntegrationTests.GroupFive.FeatureTestsThree.Authors;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RecipeManagement.Domain.Authors.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Author;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;

public class UpdateAuthorCommandTests : TestBase
{
    [Test]
    public async Task can_update_existing_author_in_db()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        await InsertAsync(fakeRecipeOne);

        var fakeAuthorOne = FakeAuthor.Generate(new FakeAuthorForCreationDto()
            .RuleFor(a => a.RecipeId, _ => fakeRecipeOne.Id).Generate());
        var updatedAuthorDto = new FakeAuthorForUpdateDto()
            .RuleFor(a => a.RecipeId, _ => fakeRecipeOne.Id).Generate();
        await InsertAsync(fakeAuthorOne);

        var author = await ExecuteDbContextAsync(db => db.Authors
            .FirstOrDefaultAsync(a => a.Id == fakeAuthorOne.Id));
        var id = author.Id;

        // Act
        var command = new UpdateAuthor.Command(id, updatedAuthorDto);
        await SendAsync(command);
        var updatedAuthor = await ExecuteDbContextAsync(db => db.Authors.FirstOrDefaultAsync(a => a.Id == id));

        // Assert
        updatedAuthor.Name.Should().Be(updatedAuthorDto.Name);
        updatedAuthor.RecipeId.Should().Be(updatedAuthorDto.RecipeId);
    }
}