using System;
using System.IO;
using System.Linq;
using Interface;

namespace Loader
{
    public class EntryPoint
    {
        private static string TryGetSolutionDirectoryInfo()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            
            return directory?.FullName ?? "";
        }

        private static string TryGetAssemblyFilePath(DirectoryInfo directory, string assemblyName)
        {
            foreach (var file in directory.GetFiles())
            {
                if (file.Name == assemblyName)
                {
                    return directory.FullName + "\\" + file.Name;
                }
            }

            foreach (var dir in directory.GetDirectories())
            {
                try
                {
                    return TryGetAssemblyFilePath(dir, assemblyName);
                }
                catch (Exception)
                {    
                    // nothing
                }
            }
            
            throw new Exception($"{directory} does not contains {assemblyName} file");
        }
        
        public static void Main(string[] args)
        {
            var solutionDirectory = TryGetSolutionDirectoryInfo();
            var assemblyPath = TryGetAssemblyFilePath(new DirectoryInfo(solutionDirectory), "Implementation.dll");
            
            var loader = new ImplementationLoader(assemblyPath);

            foreach (var calculator in loader.Instances)
            {
                Console.Out.WriteLine($"{1} + {2} = {calculator.Sum(1, 2)}");
            }

            loader.Dispose();
        }
    }
}