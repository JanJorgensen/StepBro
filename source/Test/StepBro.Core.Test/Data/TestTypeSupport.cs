using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBroCoreTest.Data;

namespace StepBro.Core.Test.Data
{
    [TestClass]
    public class TestTypeSupport
    {
        [TestMethod]
        public void TestTypeReferenceHighLevelInheritance()
        {
            SomeHighlevelType a = new SomeHighlevelType();
            SomeHighlevelType b = new SomeHighlevelType(a);
            SomeHighlevelType c = new SomeHighlevelType(b);

            TypeReference trBase = new TypeReference(typeof(SomeHighlevelType));
            TypeReference trA1 = new TypeReference(typeof(SomeHighlevelType), a);
            TypeReference trA2 = new TypeReference(typeof(SomeHighlevelType), a);
            TypeReference trB1 = new TypeReference(typeof(SomeHighlevelType), b);
            TypeReference trB2 = new TypeReference(typeof(SomeHighlevelType), b);
            TypeReference trC1 = new TypeReference(typeof(SomeHighlevelType), c);
            TypeReference trC2 = new TypeReference(typeof(SomeHighlevelType), c);


            Assert.IsTrue(trA1.IsAssignableFrom(trA1));
            Assert.IsTrue(trA1.IsAssignableFrom(trA2));
            Assert.IsTrue(trA1.IsAssignableFrom(trB1));
            Assert.IsTrue(trA1.IsAssignableFrom(trC1));

            Assert.IsTrue(trBase.IsAssignableFrom(trA1));
            Assert.IsTrue(trBase.IsAssignableFrom(trB1));
            Assert.IsTrue(trBase.IsAssignableFrom(trC1));

            Assert.IsTrue(trA1.IsAssignableFrom(trC1));
        }


        [TestMethod]
        public void TestTypeReferenceSimple()
        {
            TypeReference trA1 = TypeReference.TypeVerdict;
            TypeReference trA2 = TypeReference.TypeVerdict;

            Assert.IsTrue(trA1.IsAssignableFrom(trA2));
        }

        [TestMethod]
        public void TestTypeReferenceProcedureReference()
        {

        }

        [TestMethod]
        public void TestTypeReferenceTypedef()
        {
            var dummyType = new TypeReference(typeof(DummyClass));

            TypeDef tdA = new TypeDef("A", dummyType);
            TypeReference trA1 = new TypeReference(tdA);
            TypeReference trA2 = new TypeReference(tdA);

            Assert.IsTrue(trA1.IsAssignableFrom(trA2));         // trA1 is the same type as trA2; both are a tdA.
            Assert.IsTrue(dummyType.IsAssignableFrom(trA1));    // trA1 is also a DummyClass.
            Assert.IsFalse(trA1.IsAssignableFrom(dummyType));   // DummyClass is not the same type as tdA.


            TypeDef tdB = new TypeDef("B", trA1);
            TypeReference trB1 = new TypeReference(tdB);
            TypeReference trB2 = new TypeReference(tdB);

            Assert.IsTrue(trB2.IsAssignableFrom(trB1));              // trB1 is the same type as trB2.
            Assert.IsTrue(trA1.IsAssignableFrom(trB1));              // trB1 is inherited from trA1
            Assert.IsTrue(trA2.IsAssignableFrom(trB1));              // trB1 is inherited from a type equal to trA1
            Assert.IsTrue(dummyType.IsAssignableFrom(trB1));         // trB1 is also a DummyClass.
            Assert.IsFalse(trB1.IsAssignableFrom(trA1));             // trA1 is not a trB1, even though both are derived from tdA.
            Assert.IsFalse(trB1.IsAssignableFrom(dummyType));        // DummyClass is not the same type as tdB or tdA.


            TypeDef tdC = new TypeDef("C", trB1);
            TypeReference trC1 = new TypeReference(tdC);
            TypeReference trC2 = new TypeReference(tdC);

            Assert.IsTrue(trC2.IsAssignableFrom(trC1));         // trC1 is the same type as trC2.
            Assert.IsTrue(trB1.IsAssignableFrom(trC1));         // trC1 is inherited from trB1
            Assert.IsTrue(trB2.IsAssignableFrom(trC1));         // trC1 is inherited from a type equal to trB1
            Assert.IsTrue(trA1.IsAssignableFrom(trC1));         // trC1 is also a tdA.
            Assert.IsTrue(trA2.IsAssignableFrom(trC1));         // trC1 is inherited from a type equal to trA1
            Assert.IsTrue(dummyType.IsAssignableFrom(trC1));    // trC1 is also a DummyClass.
            Assert.IsFalse(trC1.IsAssignableFrom(trA1));        // trA1 is not a trC1, even though both are derived from tdA.
            Assert.IsFalse(trC1.IsAssignableFrom(trB1));        // trB1 is not a trC1, even though both are derived from tdB.
            Assert.IsFalse(trC1.IsAssignableFrom(dummyType));   // DummyClass is not the same type as tdC, tdB or tdA.


            TypeDef tdD = new TypeDef("D", dummyType);          // New one derived from DummyClass.
            TypeReference trD = new TypeReference(tdD);
            Assert.IsTrue(dummyType.IsAssignableFrom(trD));     // trD is also a DummyClass.
            Assert.IsFalse(trA1.IsAssignableFrom(trD));         // trD is not a tdA, even though both are derived from DummyClass.

            TypeDef tdE = new TypeDef("E", trA1);               // New one derived from tdA.
            TypeReference trE = new TypeReference(tdE);
            Assert.IsTrue(trA1.IsAssignableFrom(trE));          // trE is also a tdA.
            Assert.IsTrue(dummyType.IsAssignableFrom(trE));     // trE is also a DummyClass.
            Assert.IsFalse(trB1.IsAssignableFrom(trE));         // trE is not a tdB, even though both are derived from tdA.

            TypeDef tdF = new TypeDef("F", trB1);               // New one derived from tdB.
            TypeReference trF = new TypeReference(tdF);
            Assert.IsTrue(trB1.IsAssignableFrom(trF));          // trF is also a tdB.
            Assert.IsTrue(trA1.IsAssignableFrom(trF));          // trF is also a tdA.
            Assert.IsTrue(dummyType.IsAssignableFrom(trF));     // trF is also a DummyClass.
            Assert.IsFalse(trC1.IsAssignableFrom(trF));         // trE is not a tdC, even though both are derived from tdB.
        }
    }

    class SomeHighlevelType : IInheritable
    {
        public SomeHighlevelType m_base = null;
        public SomeHighlevelType(SomeHighlevelType @base = null)
        {
            m_base = @base;
        }

        public IInheritable Base
        {
            get { return m_base; }
        }
    }

    class SomeOtherHighlevelType : IInheritable
    {
        public SomeOtherHighlevelType m_base = null;
        public SomeOtherHighlevelType(SomeOtherHighlevelType @base = null)
        {
            m_base = @base;
        }

        public IInheritable Base
        {
            get { return m_base; }
        }
    }
}
