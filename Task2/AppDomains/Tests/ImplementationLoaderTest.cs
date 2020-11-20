using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using AppDomains;
using Loader;
using NUnit.Framework;

namespace Tests
{
    public class ImplementationLoaderTest
    {
        private static string TryGetSolutionDirectoryInfo()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            
            return directory?.ToString() ?? "";
        }

        private static string TryGetAssemblyFilePath(DirectoryInfo directory, string assemblyName)
        {
            foreach (var file in directory.GetFiles())
            {
                if (file.Name == assemblyName)
                {
                    return file.Name;
                }
            }

            foreach (var dir in directory.GetDirectories())
            {
                try
                {
                    var path = TryGetAssemblyFilePath(dir, assemblyName);
                    return dir.Name + "/" + path;
                }
                catch (Exception)
                {    
                    // nothing
                }
            }
            
            throw new Exception($"{directory} does not contains {assemblyName} file");
        }
        
        [Test]
        public void Test()
        {
            var solutionDirectory = TryGetSolutionDirectoryInfo();
            var assemblyPath = TryGetAssemblyFilePath(new DirectoryInfo(solutionDirectory), "Implementation.dll");
            
            var loader = new ImplementationLoader<ICalculator>(assemblyPath);   
        }
    }
}