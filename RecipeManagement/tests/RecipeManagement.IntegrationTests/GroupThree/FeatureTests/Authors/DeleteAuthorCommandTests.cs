namespace RecipeManagement.IntegrationTests.GroupThree.FeatureTests.Authors;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RecipeManagement.Domain.Authors.Features;
using RecipeManagement.SharedTestHelpers.Fakes.Author;
using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using SharedKernel.Exceptions;
using static TestFixture;

public class DeleteAuthorCommandTests : TestBase
{
    [Test]
    public async Task can_delete_author_from_db()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        await InsertAsync(fakeRecipeOne);

        var fakeAuthorOne = FakeAuthor.Generate(new FakeAuthorForCreationDto()
            .RuleFor(a => a.RecipeId, _ => fakeRecipeOne.Id).Generate());
        await InsertAsync(fakeAuthorOne);
        var author = await ExecuteDbContextAsync(db => db.Authors
            .FirstOrDefaultAsync(a => a.Id == fakeAuthorOne.Id));

        // Act
        var command = new DeleteAuthor.Command(author.Id);
        await SendAsync(command);
        var authorResponse = await ExecuteDbContextAsync(db => db.Authors.CountAsync(a => a.Id == author.Id));

        // Assert
        authorResponse.Should().Be(0);
    }

    [Test]
    public async Task delete_author_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteAuthor.Command(badId);
        Func<Task> act = () => SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task can_softdelete_author_from_db()
    {
        // Arrange
        var fakeRecipeOne = FakeRecipe.Generate(new FakeRecipeForCreationDto().Generate());
        await InsertAsync(fakeRecipeOne);

        var fakeAuthorOne = FakeAuthor.Generate(new FakeAuthorForCreationDto()
            .RuleFor(a => a.RecipeId, _ => fakeRecipeOne.Id).Generate());
        await InsertAsync(fakeAuthorOne);
        var author = await ExecuteDbContextAsync(db => db.Authors
            .FirstOrDefaultAsync(a => a.Id == fakeAuthorOne.Id));

        // Act
        var command = new DeleteAuthor.Command(author.Id);
        await SendAsync(command);
        var deletedAuthor = await ExecuteDbContextAsync(db => db.Authors
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == author.Id));

        // Assert
        deletedAuthor?.IsDeleted.Should().BeTrue();
    }
}