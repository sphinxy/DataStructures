using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public class ConcurrentPriorityQueueTests : PriorityQueueTests
    {
        protected internal override PriorityQueue<int> GetCollection(int? capacity = null)
        {
            if (capacity.HasValue)
            {
                return new ConcurrentPriorityQueue<int>(capacity.Value);
            }
            return new ConcurrentPriorityQueue<int>();
        }
        [TestMethod]
        public void ToArray()
        {
            ConcurrentPriorityQueue<int> target = new ConcurrentPriorityQueue<int>();

            const int count = 10;
            for (int i = 0; i < count; i++)
            {
                target.Add(i);
            }

            var result = target.ToArray();

            // Priority queue is max-based so greater items comes first
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(count - i - 1, result[i]);
            }
        }
    }
}
