using System.Collections.Generic;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public sealed class ConcurrentSkipListTests : CollectionTests
    {
        protected internal override ICollection<int> GetCollection()
        {
            return new ConcurrentSkipList<int>();
        }
    }
}
