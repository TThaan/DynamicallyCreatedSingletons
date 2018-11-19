using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSingletons
{
    /// <summary>
    /// Just some code to practice with C#, Reflection.Emit, Intermediate Language and GitHub.
    /// I'm emitting singletons all from Jon Skeets book/homepage:
    /// http://csharpindepth.com/Articles/General/Singleton.aspx
    /// Lately I've been more busy with another repository: 'ExtensionPropertiesForCSharp', 
    /// where I emit sort of singletons too, so this program is still in the beginning phase.
    /// Hopefully I can finish this exercise very soon.
    /// </summary>

    enum TypePresets
    {
        NotThreadSafe,
        SimpleThreadSafe,
        DoubleCheckLocking,
        NoLocks,
        FullLazy,
        GenericLazy
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Build.CustomType:
            var simpleSingleton01 = Builder.CustomType(TypePresets.NotThreadSafe, "s01");
            Console.WriteLine($"singleton01              : {simpleSingleton01}");
            Console.WriteLine($"singleton01.GetType()    : {simpleSingleton01.GetType()}");
            Console.WriteLine($"singleton01.GetTypeInfo(): {simpleSingleton01.GetTypeInfo()}\n");

            //Check if calling a static method of class 'singleton'(='s01') works (with reflection)
            MethodInfo method_Static01 = simpleSingleton01.GetMethod("AnyMethod_Static");
            method_Static01.Invoke(simpleSingleton01, null);
            Console.WriteLine();

            //Build.InstanceOfCustomType:
            var simpleSingleton02 = Builder.InstanceOfCustomType(TypePresets.NotThreadSafe, "s02");
            Console.WriteLine($"singleton02              : {simpleSingleton02}");
            Console.WriteLine($"singleton02.GetType()    : {simpleSingleton02.GetType()}");
            Console.ReadLine();            
        }
    }
}
