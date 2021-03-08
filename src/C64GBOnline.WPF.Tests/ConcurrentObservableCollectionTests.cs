namespace C64GBOnline.WPF.Tests;

public class ConcurrentObservableCollectionTests
{
    private readonly ConcurrentObservableCollection<string> _sut;

    public ConcurrentObservableCollectionTests() => _sut = new();

    [Fact]
    public void New_WhenNewingUpCollection_ThenCollectionIsEmpty()
    {
        // Arrange

        // Act

        // Assert
        _sut.Should().BeEmpty();
    }

    [Fact]
    public void New_WhenNewingUpCollectionWithInitialItems_ThenCollectionIsInitializedWithItems()
    {
        // Arrange
        string[] expected = { "First", "Second" };

        // Act
        ConcurrentObservableCollection<string> sut = new(expected);

        // Assert
        sut.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Add_WhenAddingNothing_ThenDoNothing()
    {
        // Arrange

        // Act
        _sut.Add();

        // Assert
        _sut.Should().BeEmpty();
    }

    [Fact]
    public void Add_WhenAddingToEmptyCollection_ThenItemAppearsInList()
    {
        // Arrange
        string expected = "First";

        // Act
        _sut.Add(expected);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.HaveElementAt(0, expected);
    }

    [Fact]
    public void Add_WhenAddingMultipleItemsToEmptyCollection_ThenItemsAppearsInList()
    {
        // Arrange
        string[] expected = { "First", "Second" };

        // Act
        _sut.Add(expected);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Add_WhenAddingMultipleItemsToNonEmptyCollection_ThenNewItemsAppearsLastInList()
    {
        // Arrange
        _sut.Add("First", "Second");
        string[] expected = { "First", "Second", "Third", "Fourth" };

        // Act
        _sut.Add("Third", "Fourth");

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(4)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Clear_WhenCleared_ThenNoItemsAppearsInList()
    {
        // Arrange
        _sut.Add("First", "Second");

        // Act
        _sut.Clear();

        // Assert
        _sut.Should().BeEmpty();
    }

    [Fact]
    public void Insert_WhenInsertingNothing_ThenDoNothing()
    {
        // Arrange

        // Act
        _sut.Insert(0);

        // Assert
        _sut.Should().BeEmpty();
    }

    [Fact]
    public void Insert_WhenInsertingOneItemAtIndex_ThenItemAppearsInListAtThatIndex()
    {
        // Arrange
        _sut.Add("First", "Third");
        int index = 1;
        string expected = "Second";

        // Act
        _sut.Insert(index, expected);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(3)
            .And.HaveElementAt(index, expected);
    }

    [Fact]
    public void Insert_WhenInsertingMultipleItemsAtIndex_ThenItemsAppearsInListFromThatIndex()
    {
        // Arrange
        _sut.Add("First", "Fourth");
        string[] expected = { "First", "Second", "Third", "Fourth" };

        // Act
        _sut.Insert(1, "Second", "Third");

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(4)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Move_WhenMovingNothing_ThenDoNothing()
    {
        // Arrange
        string[] expected = { "First", "Second" };
        _sut.Add(expected);

        // Act
        _sut.Move();

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Move_WhenMovingAnItemBackwardsToNewIndex_ThenItemAppearsInListAtNewIndex()
    {
        // Arrange
        _sut.Add("First", "Second", "Fourth", "Fifth", "Third", "Sixth");
        string[] expected = { "First", "Second", "Third", "Fourth", "Fifth", "Sixth" };

        // Act
        _sut.Move(4, 2);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(6)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Move_WhenMovingAnItemForwardsToNewIndex_ThenItemAppearsInListAtNewIndex()
    {
        // Arrange
        _sut.Add("First", "Fourth", "Second", "Third", "Fifth", "Sixth");
        string[] expected = { "First", "Second", "Third", "Fourth", "Fifth", "Sixth" };

        // Act
        _sut.Move(1, 3);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(6)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Move_WhenMovingAnItemForwardsToLastIndex_ThenItemAppearsInListAtNewIndex()
    {
        // Arrange
        _sut.Add("First", "Second", "Third", "Fourth", "Sixth", "Fifth");
        string[] expected = { "First", "Second", "Third", "Fourth", "Fifth", "Sixth" };

        // Act
        _sut.Move(4, 5);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(6)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Move_WhenMovingMultipleItems_ThenItemsAppearsInListAtNewIndexes()
    {
        // Arrange
        _sut.Add("Third", "Fourth", "Sixth", "Second", "First", "Fifth");
        string[] expected = { "First", "Second", "Third", "Fourth", "Fifth", "Sixth" };

        // Act
        _sut.Move((4, 0), (4, 1), (4, 5));

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(6)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Remove_WhenRemovingNothing_ThenDoNothing()
    {
        // Arrange
        string expected = "First";
        _sut.Add(expected);

        // Act
        _sut.Remove();

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.HaveElementAt(0, expected);
    }

    [Fact]
    public void Remove_WhenRemovingAnItem_ThenItemDisappearsFromList()
    {
        // Arrange
        string third = "Third";
        _sut.Add("First", "Second", third, "Fourth", "Fifth", "Sixth");
        string[] expected = { "First", "Second", "Fourth", "Fifth", "Sixth" };

        // Act
        _sut.Remove(third);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(5)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Remove_WhenRemovingItems_ItemsDisappearsFromList()
    {
        // Arrange
        string third = "Third";
        string fifth = "Fifth";
        string sixth = "Sixth";
        _sut.Add("First", "Second", third, "Fourth", fifth, sixth);
        string[] expected = { "First", "Second", "Fourth" };

        // Act
        _sut.Remove(fifth, sixth, third);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(3)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void RemoveAt_WhenRemovingNothing_ThenDoNothing()
    {
        // Arrange
        string expected = "First";
        _sut.Add(expected);

        // Act
        _sut.RemoveAt();

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.HaveElementAt(0, expected);
    }

    [Fact]
    public void RemoveAt_WhenRemovingAnItemAtIndex_ThenItemDisappearsFromList()
    {
        // Arrange
        _sut.Add("First", "Second", "Third", "Fourth", "Fifth", "Sixth");
        string[] expected = { "First", "Second", "Fourth", "Fifth", "Sixth" };

        // Act
        _sut.RemoveAt(2);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(5)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void RemoveAt_WhenRemovingMultipleItemsAtIndexes_ThenItemsDisappearsFromList()
    {
        // Arrange
        _sut.Add("First", "Second", "Third", "Fourth", "Fifth", "Sixth");
        string[] expected = { "First", "Second", "Fourth" };

        // Act
        _sut.RemoveAt(4, 5, 2);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(3)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Replace_WhenReplacingNothing_ThenDoNothing()
    {
        // Arrange
        string expected = "First";
        _sut.Add(expected);

        // Act
        _sut.Replace();

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Replace_WhenReplacingAnItem_ThenExistingItemIsReplacedWithNewItem()
    {
        // Arrange
        string initial = "First";
        string expected = "Second";
        _sut.Add(initial);

        // Act
        _sut.Replace(initial, expected);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void Replace_WhenReplacingMultipleItems_ThenExistingItemsAreReplacedWithNewItems()
    {
        // Arrange
        string first = "First";
        string second = "Second";
        string third = "Third";
        string fourth = "Fourth";
        string[] expected = { third, fourth };
        _sut.Add(first, second);

        // Act
        _sut.Replace((first, third), (second, fourth));

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void ReplaceAt_WhenReplacingNothing_ThenDoNothing()
    {
        // Arrange
        string[] expected = { "First", "Second" };
        _sut.Add(expected);

        // Act
        _sut.ReplaceAt();

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void ReplaceAt_WhenReplacingAnItemAtIndex_ThenExistingItemIsReplacedWithNewItemAtThatIndex()
    {
        // Arrange
        string expected = "Second";
        _sut.Add("First");

        // Act
        _sut.ReplaceAt(0, expected);

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.ContainInOrder(expected);
    }

    [Fact]
    public void ReplaceAt_WhenReplacingMultipleItemsAtIndexes_ThenExistingItemsAreReplacedWithNewItemsAtThoseIndexes()
    {
        // Arrange
        string first = "First";
        string second = "Second";
        string third = "Third";
        string fourth = "Fourth";
        string[] expected = { third, fourth };
        _sut.Add(first, second);

        // Act
        _sut.ReplaceAt((0, third), (1, fourth));

        // Assert
        _sut.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(expected);
    }
}
