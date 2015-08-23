using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public sealed class SkipListTests : CollectionTests<SkipList<int>>
    {
        protected internal override SkipList<int> GetCollection()
        {
            return new SkipList<int>();
        }
    }
}
