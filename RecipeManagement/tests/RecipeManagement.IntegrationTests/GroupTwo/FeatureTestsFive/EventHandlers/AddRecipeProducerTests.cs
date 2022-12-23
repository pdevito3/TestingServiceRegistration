namespace RecipeManagement.IntegrationTests.GroupTwo.FeatureTestsFive.EventHandlers;

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RecipeManagement.Domain.Recipes.Features;
using SharedKernel.Messages;
using static TestFixture;

public class AddRecipeProducerTests : TestBase
{
    [Test]
    public async Task can_produce_RecipeAdded_message()
    {
        // Arrange
        var command = new AddRecipeProducer.AddRecipeProducerCommand();

        // Act
        await SendAsync(command);

        // Assert
        (await IsPublished<IRecipeAdded>()).Should().BeTrue();
    }
}