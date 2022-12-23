namespace RecipeManagement.IntegrationTests.GroupOne.FeatureTestsThree.EventHandlers;

using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RecipeManagement.Domain.Recipes.Features;
using SharedKernel.Messages;
using static TestFixture;

public class AddToBookTests : TestBase
{
    [Test]
    public async Task can_consume_RecipeAdded_message()
    {
        // Arrange
        var message = new Mock<IRecipeAdded>();

        // Act
        await PublishMessage<IRecipeAdded>(message);

        // Assert
        (await IsConsumed<IRecipeAdded>()).Should().Be(true);
        (await IsConsumed<IRecipeAdded, AddToBook>()).Should().Be(true);
    }
}