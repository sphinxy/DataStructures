using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests.Helpers
{
    public static class AssertEx
    {
        public static T Throws<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var expectedException = ex as T;
                if (expectedException != null) return expectedException;
                Assert.Fail("Expected {0}, but {1} was thrown.", typeof(T), ex.GetType().Name);
            }
            Assert.Fail("Failed to throw exception {0}", typeof(T));
            return null;
        }
    }
}
