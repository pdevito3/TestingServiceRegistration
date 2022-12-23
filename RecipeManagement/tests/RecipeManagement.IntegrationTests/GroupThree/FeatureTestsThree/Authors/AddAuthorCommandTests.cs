namespace RecipeManagement.IntegrationTests.GroupThree.FeatureTestsThree.Authors;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RecipeManagement.Domain.Authors.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Author;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using static TestFixture;

public class AddAuthorCommandTests : TestBase
{
    [Test]
    public async Task can_add_new_author_to_db()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        await InsertAsync(fakeRecipeOne);

        var fakeAuthorOne = new FakeAuthorForCreationDto()
            .RuleFor(a => a.RecipeId, _ => fakeRecipeOne.Id).Generate();

        // Act
        var command = new AddAuthor.Command(fakeAuthorOne);
        var authorReturned = await SendAsync(command);
        var authorCreated = await ExecuteDbContextAsync(db => db.Authors
            .FirstOrDefaultAsync(a => a.Id == authorReturned.Id));

        // Assert
        authorReturned.Name.Should().Be(fakeAuthorOne.Name);
        authorReturned.RecipeId.Should().Be(fakeAuthorOne.RecipeId);

        authorCreated.Name.Should().Be(fakeAuthorOne.Name);
        authorCreated.RecipeId.Should().Be(fakeAuthorOne.RecipeId);
    }
}