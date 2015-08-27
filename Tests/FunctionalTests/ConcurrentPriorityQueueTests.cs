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
    }
}
