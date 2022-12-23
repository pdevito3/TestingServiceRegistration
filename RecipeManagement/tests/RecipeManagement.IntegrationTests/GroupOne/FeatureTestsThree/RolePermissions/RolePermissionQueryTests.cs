namespace RecipeManagement.IntegrationTests.GroupOne.FeatureTestsThree.RolePermissions;

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RecipeManagement.Domain.RolePermissions.Features;
using RecipeManagement.SharedTestHelpers.Fakes.RolePermission;
using SharedKernel.Exceptions;
using static TestFixture;

public class RolePermissionQueryTests : TestBase
{
    [Test]
    public async Task can_get_existing_rolepermission_with_accurate_props()
    {
        // Arrange
        var fakeRolePermissionOne = FakeRolePermission.Generate(new FakeRolePermissionForCreationDto().Generate());
        await InsertAsync(fakeRolePermissionOne);

        // Act
        var query = new GetRolePermission.Query(fakeRolePermissionOne.Id);
        var rolePermission = await SendAsync(query);

        // Assert
        rolePermission.Permission.Should().Be(fakeRolePermissionOne.Permission);
        rolePermission.Role.Should().Be(fakeRolePermissionOne.Role.Value);
    }

    [Test]
    public async Task get_rolepermission_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var badId = Guid.NewGuid();

        // Act
        var query = new GetRolePermission.Query(badId);
        Func<Task> act = () => SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}