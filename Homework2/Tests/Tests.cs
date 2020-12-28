using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using Application;
using Homework2;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestThatPluginsAreLoaded()
        {
            var plugins = PluginLoader.LoadPlugins<ICalculator>(GetCalculatorImplementationPath(), new Evidence());
            Assert.True(plugins.Count == 2);
        }

        [Test]
        public void TestThatPluginsHaveNoFileSystemAccess()
        {
            var plugins = PluginLoader.LoadPlugins<ICalculator>(GetCalculatorImplementationPath(), new Evidence());
            foreach (var pluginData in plugins)
            {
                try
                {
                    pluginData.Plugin.Sum(2, 2);
                }
                catch (Exception e)
                {
                    return;
                }
            }

            throw new AssertionException("All plugins succeeded, including the one that accessed the file system");
        }

        [Test]
        public void TestThatPluginAssembliesAreNotLoaded()
        {
            PluginLoader.LoadPlugins<ICalculator>(GetCalculatorImplementationPath(), new Evidence());
            Assert.False(
                AppDomain.CurrentDomain.GetAssemblies().Any(it => it.FullName.Contains("CalculatorImplementation")),
                "Plugin assembly was loaded into the main domain");
        }

        [Test]
        public void TestThatPluginDomainsAreDifferent()
        {
            var plugins = PluginLoader.LoadPlugins<ICalculator>(GetCalculatorImplementationPath(), new Evidence());
            Assert.That(plugins[0].Domain != plugins[1].Domain, "Plugins were loaded in the same domain");
        }

        private static string GetCalculatorImplementationPath()
        {
            string result = Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Assembly.GetExecutingAssembly().Location
                            )
                        )
                    )
                )!,
                "CalculatorImplementation", "bin", "Debug", "CalculatorImplementation.dll"
            );
            Assert.That(File.Exists(result), "Please, build the solution before running tests");
            return result;
        }
    }
}
