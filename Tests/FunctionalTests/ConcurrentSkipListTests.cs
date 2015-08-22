using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public sealed class ConcurrentSkipListTests : ProducerConsumerCollectionTests<ConcurrentSkipList<int>>
    {
        protected internal override ConcurrentSkipList<int> GetCollection()
        {
            return new ConcurrentSkipList<int>();
        }
    }
}
