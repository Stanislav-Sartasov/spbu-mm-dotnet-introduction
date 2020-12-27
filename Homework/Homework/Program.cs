using System.Collections;
using System.Collections.Generic;

namespace Homework
{
  internal class Program
  {
    public static void Main()
    {
      var a = new A
      {
        B = new B
        {
          C = new C
          {
            Value = "The meaning of life, universe and everything"
          }
        }
      };

      var anotherA = new A
      {
        B = new B()
      };
    }

    private object Access(object root, IEnumerable<string> properties)
    {
      
    }

    private class A
    {
      public B B { get; set; }
    }

    private class B
    {
      public C C { get; set; }
    }

    private class C
    {
      public string Value { get; set; }
    }
  }
}