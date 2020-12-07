using App;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMethod()
        {
            using (var app = new AppBase())
            {
                app.Start("CalculatorImpl");

                foreach (var impl in app.Implementations)
                {
                    int r = 0;
                    
                    try
                    {
                        r = impl.Sum(1, 2);
                        Assert.AreEqual(3, r);
                    }
                    catch (System.Security.SecurityException)
                    {
                        Assert.AreEqual("CalculatorImpl.CalculatorFile", impl.ToString());
                    }
                }
            }
        }
    }
}
