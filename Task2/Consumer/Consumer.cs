using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using ICalc;
namespace ConsumerN
{
    public class Consumer
    {
        private readonly List<InstanceInfo> _implementations = new List<InstanceInfo>();
        public IEnumerable<InstanceInfo> Implementations => _implementations;
        static void Main(string[] args)
        {
            Console.WriteLine("Run the test instead");
        }

        public void AddImplementations(string path)
        {
            //Setup loader in separate domain -> get info -> unload loader's domain
            AppDomain ad = AppDomain.CreateDomain("LoaderDomain");
            Loader loader = (Loader)ad.CreateInstanceFromAndUnwrap(typeof(Loader).Assembly.Location, typeof(Loader).FullName);
            List<ClassInfo> classes = loader.GetTypes(path);
            AppDomain.Unload(ad);

            //Save implementations and their respective domains
            foreach (var found_class in classes)
            {
                InstanceInfo implementation = Consumer.GetImplementationInSeparateDomain(found_class);
                _implementations.Add(implementation);
            }
        }

        // Load implementations ->  try implementations
        private void ManualTest(string path)
        {
            var consumer = new Consumer();
            consumer.AddImplementations(path);
            var instances = consumer.Implementations;

            foreach (InstanceInfo implementation in instances)
            {
                Console.WriteLine("This calculator thinks that 1 + 2 = {0}", implementation.calculator.Sum(1, 2));
            }

        }

        //Create separate domain -> load implementation into it
        private static InstanceInfo GetImplementationInSeparateDomain(ClassInfo info)
        {
            AppDomain ad = AppDomain.CreateDomain(info.className);
            ICalculator instance = (ICalculator)ad.CreateInstanceFromAndUnwrap(info.path, info.className);
            return new InstanceInfo(ad, instance);
        }

        public void UnloadImplementation(InstanceInfo implementation)
        {
            if (_implementations.Remove(implementation))
            {
                AppDomain.Unload(implementation.domain);
            }

        }



    }
    [Serializable]
    public class ClassInfo
    {
        public readonly string path;
        public readonly string assemblyName;
        public readonly string className;

        public ClassInfo(string assemblyName, string className, string path)
        {
            this.assemblyName = assemblyName;
            this.className = className;
            this.path = path;
        }
    }

    [Serializable]
    public class InstanceInfo
    {
        public readonly AppDomain domain;
        public readonly ICalculator calculator;

        public InstanceInfo(AppDomain domain, ICalculator calculator)
        {
            this.domain = domain;
            this.calculator = calculator;
        }
    }


    public class Loader : MarshalByRefObject
    {
        //Load assembly -> Get classes, that can provide an implementation -> return them
        public List<ClassInfo> GetTypes(string path)
        {
            List<ClassInfo> types = new List<ClassInfo>();
            Assembly assembly = Assembly.LoadFrom(path);
            var local_types = assembly.GetTypes();
            foreach (Type type in local_types)
            {
                if (type.GetInterfaces().Contains(typeof(ICalculator)) & !type.IsInterface & !type.IsAbstract)
                {
                    types.Add(new ClassInfo(assemblyName: type.Assembly.FullName, className: type.FullName, path: path));
                }
            }
            return types;
        }
    }
}



