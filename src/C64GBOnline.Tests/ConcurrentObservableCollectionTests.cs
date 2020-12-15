using C64GBOnline.WPF;
using FluentAssertions;
using Xunit;

namespace C64GBOnline.Tests
{
    public class ConcurrentObservableCollectionTests
    {
        private readonly ConcurrentObservableCollection<string> _sut;

        public ConcurrentObservableCollectionTests() => _sut = new ConcurrentObservableCollection<string>();

        [Fact]
        public void GivenNewCollection_WhenDoingNothing_CollectionIsEmpty()
        {
            // Arrange

            // Act

            // Assert
            _sut.Should().BeEmpty();
        }

        [Fact]
        public void GivenNewCollectionWithInitialItems_WhenDoingNothing_CollectionIsInitializedWithItems()
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
        public void GivenEmptyCollection_WhenAddingOneItem_ItemAppearsInList()
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
        public void GivenEmptyCollection_WhenAddingMultipleItems_ItemsAppearsInList()
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
        public void GivenCollectionWithItems_WhenAddingMultipleItems_ItemsAppearsLastInList()
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
        public void GivenCollectionWithItems_WhenCleared_NoItemsAppearsInList()
        {
            // Arrange
            _sut.Add("First", "Second");

            // Act
            _sut.Clear();

            // Assert
            _sut.Should().BeEmpty();
        }

        [Fact]
        public void GivenCollectionWithItems_WhenInsertingOneItemInTheMiddle_ItemAppearsInListAtThatIndex()
        {
            // Arrange
            _sut.Add("First", "Third");
            var index = 1;
            string expected = "Second";

            // Act
            _sut.Insert(index, expected);

            // Assert
            _sut.Should().NotBeEmpty()
                .And.HaveCount(3)
                .And.HaveElementAt(index, expected);
        }

        [Fact]
        public void GivenCollectionWithItems_WhenInsertingMultipleItemsInTheMiddle_ItemsAppearsInListFromThatIndex()
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
        public void GivenCollectionWithItems_WhenMovingAnItemBackwards_ItemsAppearsInListAtNewIndex()
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
        public void GivenCollectionWithItems_WhenMovingAnItemForwards_ItemAppearsInListAtNewIndex()
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
        public void GivenCollectionWithItems_WhenMovingAnItemForwardsToLastPosition_ItemAppearsInListAtNewIndex()
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
        public void GivenCollectionWithItems_WhenMovingItems_ItemsAppearsInListAtNewIndexes()
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
        public void GivenCollectionWithItems_WhenRemovingAnItemAtIndex_ItemDisappearsFromList()
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
        public void GivenCollectionWithItems_WhenRemovingMultipleItemsAtIndexes_ItemsDisappearsFromList()
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
        public void GivenCollectionWithItems_WhenRemovingAnItem_ItemDisappearsFromList()
        {
            // Arrange
            var third = "Third";
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
        public void GivenCollectionWithAnItem_WhenReplacingTheItemAtIndex_InitialItemIsReplacedWithNewItemInList()
        {
            // Arrange
            var second = "Second";
            _sut.Add("First");

            // Act
            _sut.ReplaceAt(0, second);

            // Assert
            _sut.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.ContainInOrder(second);
        }

        [Fact]
        public void GivenCollectionWithAnItem_WhenReplacingTheItem_InitialItemIsReplacedWithNewItemInList()
        {
            // Arrange
            var first = "First";
            var second = "Second";
            _sut.Add(first);

            // Act
            _sut.Replace(first, second);

            // Assert
            _sut.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.ContainInOrder(second);
        }

        [Fact]
        public void GivenCollectionWithItems_WhenRemovingItems_ItemsDisappearsFromList()
        {
            // Arrange
            var third = "Third";
            var fifth = "Fifth";
            var sixth = "Sixth";
            _sut.Add("First", "Second", third, "Fourth", fifth, sixth);
            string[] expected = { "First", "Second", "Fourth" };

            // Act
            _sut.Remove(fifth, sixth, third);

            // Assert
            _sut.Should().NotBeEmpty()
                .And.HaveCount(3)
                .And.ContainInOrder(expected);
        }
    }
}