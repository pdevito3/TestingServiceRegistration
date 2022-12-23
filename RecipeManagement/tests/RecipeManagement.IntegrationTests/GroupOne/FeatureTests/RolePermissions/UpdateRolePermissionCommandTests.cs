namespace RecipeManagement.IntegrationTests.GroupOne.FeatureTests.RolePermissions;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RecipeManagement.Domain.RolePermissions.Features;
using RecipeManagement.SharedTestHelpers.Fakes.RolePermission;

public class UpdateRolePermissionCommandTests : TestBase
{
    [Test]
    public async Task can_update_existing_rolepermission_in_db()
    {
        // Arrange
        var fakeRolePermissionOne = FakeRolePermission.Generate(new FakeRolePermissionForCreationDto().Generate());
        var updatedRolePermissionDto = new FakeRolePermissionForUpdateDto().Generate();
        await InsertAsync(fakeRolePermissionOne);

        var rolePermission = await ExecuteDbContextAsync(db => db.RolePermissions
            .FirstOrDefaultAsync(r => r.Id == fakeRolePermissionOne.Id));
        var id = rolePermission.Id;

        // Act
        var command = new UpdateRolePermission.Command(id, updatedRolePermissionDto);
        await SendAsync(command);
        var updatedRolePermission = await ExecuteDbContextAsync(db => db.RolePermissions.FirstOrDefaultAsync(r => r.Id == id));

        // Assert
        updatedRolePermission?.Permission.Should().Be(updatedRolePermissionDto.Permission);
        updatedRolePermission?.Role.Value.Should().Be(updatedRolePermissionDto.Role);
    }
}