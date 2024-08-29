using Fauna.Util.Extensions;
using NUnit.Framework;

namespace Fauna.Test.Util
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        private class NonClosureClass
        {
        }

        internal class TestClassWithInterface : IComparable<TestClassWithInterface>
        {
            public int CompareTo(TestClassWithInterface? other) => 0;
        }

        private class TestClassWithBaseClass : List<int>
        {
        }

        [Test]
        public void IsClosureType_WithNonClosureType_ReturnsFalse()
        {
            var type = typeof(NonClosureClass);
            var result = type.IsClosureType();
            Assert.IsFalse(result);
        }

        [Test]
        public void GetGenInst_WithGenericInterface_ReturnsInterfaceType()
        {
            var type = typeof(TestClassWithInterface);
            var result = type.GetGenInst(typeof(IComparable<>));

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(IComparable<TestClassWithInterface>), result);
        }

        [Test]
        public void GetGenInst_WithGenericBaseClass_ReturnsBaseClassType()
        {
            var type = typeof(TestClassWithBaseClass);
            var result = type.GetGenInst(typeof(List<>));

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(List<int>), result);
        }

        [Test]
        public void GetGenInst_WithUnrelatedType_ReturnsNull()
        {
            var type = typeof(NonClosureClass);
            var result = type.GetGenInst(typeof(IComparable<>));

            Assert.IsNull(result);
        }

        [Test]
        public void GetGenInst_WithNonGenericTypeDefinition_ThrowsArgumentException()
        {
            var type = typeof(TestClassWithInterface);

            Assert.Throws<ArgumentException>(() => type.GetGenInst(typeof(IComparable)));
        }
    }
}
