using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    public abstract class ProducerConsumerCollectionTests<T> : CollectionTests<T> where T : ICollection<int>, IProducerConsumerCollection<int>
    {
        [TestMethod]
        public void TryAdd()
        {
            var target = GetCollection();

        }
    }
}
