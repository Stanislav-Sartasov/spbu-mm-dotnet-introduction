using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using AppDomains_Bashkirov.Utils;
using CalculatorExtensionPoint;
using NUnit.Framework;

namespace AppDomains_Bashkirov.Tests
{
public class ExtensionLoaderTests
{
    private const string SolutionPathFromHere = @"..\..\..\";
    private const string ExtensionAssemblyName = "CalculatorExtension.dll";
#if DEBUG
    private const string ExtensionAssembliesPath = @"CalculatorExtension\bin\Debug";
#else
    private const string ExtensionAssembliesPath = @"CalculatorExtension\bin\Release";
#endif

    private static string PathToExtensions => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, SolutionPathFromHere, ExtensionAssembliesPath, ExtensionAssemblyName);

    private static PermissionSet AllPermissionsWithoutFileSystemAccess
    {
        get
        {
            // var permissions = new PermissionSet(PermissionState.Unrestricted);
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            // is needed for `AssemblyResolve` event setting
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            return permissions;
        }
    }

    [Test]
    public void TestCalculatorsWorkProperly()
    {
        using var holder = LoadTestExtensions();

        Assert.AreEqual(2, holder.Extensions.Count);
        foreach (var (calculator, _) in holder.Extensions)
        {
            Assert.AreEqual(calculator.Sum(2, 3), 5);
        }
    }

    [Test]
    public void TestCalculatorsDoNotHaveAccessToFileSystem()
    {
        using var holder = LoadTestExtensions();

        Assert.AreEqual(2, holder.Extensions.Count);
        foreach (var (calculator, _) in holder.Extensions)
        {
            Assert.IsFalse(((IFileSystemAccessor) calculator).TestWriteAccessToFileSystem(out _));
            Assert.IsFalse(((IFileSystemAccessor) calculator).TestReadAccessToFileSystem(out _));
        }
    }

    [Test]
    public void TestRuntimeAssembliesDeployment()
    {
        using var holder = LoadTestExtensions();

        Assert.IsFalse(AppDomainUtils.CreateInstance<DomainAssembliesInfoExtractor>(AppDomain.CurrentDomain)
            .AssemblyShortNames
            .Contains("CalculatorExtension"));

        Assert.AreEqual(2, holder.Extensions.Count);
        foreach (var (_, domain) in holder.Extensions)
        {
            var assemblyInfo = AppDomainUtils.CreateInstance<DomainAssembliesInfoExtractor>(domain);
            Assert.AreEqual(
                new[]
                {
                    "Application",
                    "CalculatorExtension",
                    "CalculatorExtensionPoint",
                    "mscorlib",
                    "nunit.framework",
                    "System.Core"
                },
                assemblyInfo.AssemblyShortNames.OrderBy(it => it).ToArray());
        }
    }

    private static ExtensionsHolder<ICalculator> LoadTestExtensions()
    {
        return new ExtensionsHolder<ICalculator>(
            ExtensionsManager.LoadExtensionsInSeparateDomainsFrom<ICalculator>(
                PathToExtensions,
                AppDomain.CurrentDomain.SetupInformation,
                ExtensionAssemblyName,
                new Evidence(),
                AppDomain.CurrentDomain.SetupInformation,
                AllPermissionsWithoutFileSystemAccess));
    }
}
}