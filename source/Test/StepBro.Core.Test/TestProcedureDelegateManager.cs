using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using StepBro.Core.CodeGeneration;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestProcedureDelegateManager
    {
        [TestMethod]
        public void SignatureStringGeneration()
        {
            Assert.AreEqual("ret_System_Void", ProcedureDelegateManager.CreateSignatureString(TypeReference.TypeVoid));
            Assert.AreEqual("ret_System_Int64", ProcedureDelegateManager.CreateSignatureString(TypeReference.TypeInt64));
            Assert.AreEqual("ret_StepBroCoreTest_TestProcedureDelegateManager", ProcedureDelegateManager.CreateSignatureString((TypeReference)this.GetType()));

            Assert.AreEqual("ret_System_Int64_PAR_System_Int64", ProcedureDelegateManager.CreateSignatureString(TypeReference.TypeInt64, TypeReference.TypeInt64));
            Assert.AreEqual("ret_System_String_PAR_System_Boolean", ProcedureDelegateManager.CreateSignatureString(TypeReference.TypeString, TypeReference.TypeBool));

            Assert.AreEqual("ret_System_Int64_PAR_System_Int64&", ProcedureDelegateManager.CreateSignatureString(TypeReference.TypeInt64, (TypeReference)typeof(long).MakeByRefType()));

            Assert.AreEqual("ret_System_AttributeTargets_PAR_System_Int64&_PAR_System_String&_PAR_System_Boolean",
                ProcedureDelegateManager.CreateSignatureString(
                    (TypeReference)typeof(AttributeTargets),
                    (TypeReference)typeof(long).MakeByRefType(),
                    (TypeReference)typeof(string).MakeByRefType(),
                    TypeReference.TypeBool));
        }

        [TestMethod]
        public void CreateDelegateWithByRefParameters()
        {
            //var delegateType = ProcedureDelegateManager.TryMakeStandardDelegateType(typeof(long), typeof(StepBro.Core.Data.PropertyBlockEntryType).MakeByRefType());
            //var methodInfo = delegateType.GetMethod("Invoke");
            //Assert.AreEqual(typeof(long), methodInfo.ReturnType);
            //Assert.AreEqual(1, methodInfo.GetParameters().Length);
            //Assert.AreEqual(typeof(StepBro.Core.Data.PropertyBlockEntryType).MakeByRefType(), methodInfo.GetParameters()[0].ParameterType);

            var delegateType = ProcedureDelegateManager.CreateDelegateType(null, TypeReference.TypeInt64, new NamedData<TypeReference>("firstParameter", new TypeReference(typeof(bool).MakeByRefType())));
            var methodInfo = delegateType.GetMethod("Invoke");
            Assert.AreEqual(typeof(long), methodInfo.ReturnType);
            Assert.AreEqual(2, methodInfo.GetParameters().Length);
            Assert.AreEqual("callcontext", methodInfo.GetParameters()[0].Name);
            Assert.AreEqual(typeof(ICallContext), methodInfo.GetParameters()[0].ParameterType);
            Assert.AreEqual("firstParameter", methodInfo.GetParameters()[1].Name);
            Assert.AreEqual(typeof(bool).MakeByRefType(), methodInfo.GetParameters()[1].ParameterType);
        }
    }
}
