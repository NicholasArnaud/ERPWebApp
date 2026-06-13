namespace ERPWebApp.UnitTests.Data;

[Trait("Category", "execute")]
public class CustomShippingTests
{
    [Fact]
    public void SetStoreId_ValidStoreId_SetsStoreIdCorrectly()
    {
        // Arrange
        var builder = new CustomShippingBuilder();
        var testOrder = OrderFixtures.GetTestOrders().First();
        long validStoreId = testOrder.advancedOptions.storeId;

        // Act
        _ = builder.SetStoreId(validStoreId);

        // Assert
        Assert.Equal(validStoreId, builder.StoreId);
    }

    [Fact]
    public void SetDefaultShippingInfo_SetsDefaultShippingInfoCorrectly()
    {
        // Arrange
        var builder = new CustomShippingBuilder();

        // Act
        _ = builder.SetDefaultShippingInfo();

        // Assert
        Assert.NotNull(builder.OrderShippingInfo);
        Assert.Equal("Fulfillment Center", builder.OrderShippingInfo.name);
        Assert.Equal("Lafayette", builder.OrderShippingInfo.city);
    }

    [Fact]
    public void UpdateValidShipperIdsByWeight_RemovesInvalidShippersBasedOnWeight()
    {
        // Arrange
        var builder = new CustomShippingBuilder();
        _ = builder.SetDefaultShippingInfo();
        var testOrder = OrderFixtures.GetTestOrders().First();
        var weight = testOrder.weight;

        // Act
        _ = builder.UpdateValidShipperIdsByWeight(weight);

        // Assert
        Assert.DoesNotContain("se-3191868", builder.AppliedShipperIds.Keys); // UPSGroundPlus
    }

    [Fact]
    public void Build_ReturnsCustomShippingInstance()
    {
        // Arrange
        var builder = new CustomShippingBuilder();
        var testOrder = OrderFixtures.GetTestOrders().First();
        _ = builder.SetDefaultShippingInfo().SetStoreId(testOrder.advancedOptions.storeId);

        // Act
        var customShipping = builder.Build();

        // Assert
        Assert.NotNull(customShipping);
        Assert.Equal(testOrder.advancedOptions.storeId, customShipping.StoreId);
    }

    [Fact]
    public void Construct_ValidOrder_ReturnsCustomShipping()
    {
        // Arrange
        var builder = new CustomShippingBuilder();
        var director = new CustomShippingDirector();
        var testOrder = OrderFixtures.GetTestOrders().First();

        // Act
        var customShipping = director.Construct(builder, testOrder);

        // Assert
        Assert.NotNull(customShipping);
        Assert.Equal(testOrder.advancedOptions.storeId, customShipping.StoreId);
    }
}