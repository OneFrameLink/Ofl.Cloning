using Xunit;

namespace Ofl.Cloning.Tests
{
    public class UnitTest1
    {
        private class From
        {
            public object Property { get; } = new object();
        }

        private class To
        {
            public object Property { get; } = new object();
        }

        [Fact]
        public void Test_CloneProperties()
        {
            // Create from and to.
            var from = new From();
            var to = new To();

            // Properties not equal.
            Assert.NotEqual(from.Property, to.Property);

            // Clone.
            from.CloneProperties(to);

            // Should be the same.
            Assert.Equal(from.Property, to.Property);
        }
    }
}
