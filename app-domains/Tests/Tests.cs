using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CalcLoader;
using System.Reflection;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        static string assemblyPath = AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\Implementation\bin\Debug\Implementation.dll";

        private AppDomain CreateSandbox()
        {
            AppDomainSetup domainSetup = new AppDomainSetup
            {
                ApplicationBase = System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            };

            return AppDomain.CreateDomain("SandBox", null, domainSetup);
        }

        [TestMethod]
        public void SuccessfulWorkTest()
        {
            var sandbox = CreateSandbox();
            var calcLoader = (CalcLoader.CalcLoader) sandbox.CreateInstanceAndUnwrap(typeof(CalcLoader.CalcLoader).Assembly.FullName, typeof(CalcLoader.CalcLoader).FullName);
            var holders = calcLoader.Load(assemblyPath);

            foreach (var holder in holders)
            {
                var calc = holder.Calc;
                Assert.AreEqual(4, calc.Sum(2, 2));
                holder.Dispose();
            }

            AppDomain.Unload(sandbox);
        }

        [TestMethod]
        public void PermissionTest()
        {
            var sandbox = CreateSandbox();
            var calcLoader = (CalcLoader.CalcLoader)sandbox.CreateInstanceAndUnwrap(typeof(CalcLoader.CalcLoader).Assembly.FullName, typeof(CalcLoader.CalcLoader).FullName);
            var holders = calcLoader.Load(assemblyPath);

            foreach (var holder in holders)
            {
                var calc = holder.Calc;
                try
                {
                    calc.Sum(333, 333);
                    Assert.IsTrue(false);
                }
                catch (System.Security.SecurityException e)
                {
                    Assert.IsTrue(true);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(false);
                }
                holder.Dispose();
            }

            AppDomain.Unload(sandbox);
        }

        [TestMethod]
        public void NotLoadedCalcImplementationInDefaultAppDomainCheckTest()
        {
            var sandbox = CreateSandbox();
            var calcLoader = (CalcLoader.CalcLoader)sandbox.CreateInstanceAndUnwrap(typeof(CalcLoader.CalcLoader).Assembly.FullName, typeof(CalcLoader.CalcLoader).FullName);
            var holders = calcLoader.Load(assemblyPath);

            foreach (var assembly in Thread.GetDomain().GetAssemblies())
            {
                Assert.AreNotEqual(assembly.GetName().Name, "Implementation");
            }

            foreach (var holder in holders)
            {
                holder.Dispose();
            }
            AppDomain.Unload(sandbox);
        }
    }
}
