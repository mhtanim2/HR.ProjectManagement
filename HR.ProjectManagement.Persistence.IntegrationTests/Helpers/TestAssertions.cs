using HR.ProjectManagement.Entities;

namespace HR.ProjectManagement.Persistence.IntegrationTests.Helpers;

public static class TestAssertions
{
    public static void AssertAuditFields(BaseEntity entity, int expectedUserId = 1)
    {
        Assert.NotNull(entity.CreatedDate);
        Assert.NotNull(entity.LastModifiedDate);
        Assert.Equal(expectedUserId, entity.CreatedBy);
        Assert.Equal(expectedUserId, entity.ModifiedBy);
    }

    public static void AssertAuditFieldsUpdated(BaseEntity entity, DateTime originalModifiedDate)
    {
        Assert.True(entity.LastModifiedDate > originalModifiedDate);
    }
}
