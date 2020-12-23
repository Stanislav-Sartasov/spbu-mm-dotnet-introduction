using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DomainsHome;
using System.Collections.Generic;
using System.Security;

namespace DomainsTests
{
    [TestClass]
    public class CalculatorTests
    {
        private const string CalculatorAssembleName = "CalculatorImpl";

        private CalculatorManager manager;

        [TestMethod]
        public void CalculatorsTest()
        {
            int a = 10;
            int b = 12;
          
            manager = new CalculatorManager(CalculatorAssembleName);

            List<string> calculators = manager.GetCalculatorsNames();

            
            Assert.AreEqual(calculators[0], "BaseCalculator");
            Assert.AreEqual(calculators[1], "MaxCalculator");
            Assert.AreEqual(calculators[2], "FileCalculator");

            Assert.AreEqual(manager.RunCalculator("BaseCalculator", a, b), a + b);
            Assert.AreEqual(manager.RunCalculator("MaxCalculator", a, b), Math.Max(a, b));
            
            try
            {
                manager.RunCalculator("FileCalculator", a, b);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e.GetBaseException(), typeof(SecurityException));
            }
            
        }
    }
}
