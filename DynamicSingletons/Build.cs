using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSingletons
{
    class Build
    {
        static AssemblyBuilder assemblyBuilder;
        static ModuleBuilder moduleBuilder;

        //Presets
        public static object InstanceOfCustomType(TypePresets customType, string typeName)
        {
            if (customType == TypePresets.NotThreadSafe)
            {
                Type type = CustomType(customType, typeName);
                PropertyInfo pi = type.GetProperty("Instance");
                return pi.GetValue(type);
            }
            else return null;
        }
        public static Type CustomType(TypePresets customType, string typeName)
        {
            if(assemblyBuilder == null)
            {
                assemblyBuilder = GetAssemblyBuilder();
                moduleBuilder = GetModuleBuilder();
            }

            TypeBuilder typeBuilder = GetTypeBuilder(typeName, moduleBuilder);

            if(customType == TypePresets.NotThreadSafe)
            {
                ConstructorBuilder constructorBuilder = GetConstructorBuilder(typeBuilder);
                FieldBuilder field_instance = GetFieldBuilder(typeBuilder);                                     //wa: = default
                PropertyBuilder property_Instance = GetPropertyBuilder(typeBuilder, field_instance, constructorBuilder);

                MethodBuilder method_ShowContent_Static = GetMethodBuilder_ShowContent_Static(typeBuilder, typeName);
                MethodBuilder method_ShowContent_Instance = GetMethodBuilder_ShowContent_Instance(typeBuilder, typeName);
            }            

            Type type = typeBuilder.CreateType();
            return type;
        }   //Type vs T

        //..
        private static AssemblyBuilder GetAssemblyBuilder()
        {
            AssemblyName assemblyName = new AssemblyName("Singletons_Assembly");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder;
        }
        private static ModuleBuilder GetModuleBuilder()
        {
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Singletons_Module", true);
            return moduleBuilder;
        }
        private static TypeBuilder GetTypeBuilder(string typeName, ModuleBuilder moduleBuilder)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);
            return typeBuilder;
        }
        private static ConstructorBuilder GetConstructorBuilder(TypeBuilder typeBuilder)
        {
            ConstructorBuilder constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Private);
            return constructor;
        }
        private static FieldBuilder GetFieldBuilder(TypeBuilder typeBuilder)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField
                ("instance", typeBuilder, FieldAttributes.Private | FieldAttributes.Static);
            return fieldBuilder;
        }
        private static PropertyBuilder GetPropertyBuilder(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, ConstructorBuilder constructorBuilder)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty
                ("Instance", 
                PropertyAttributes.HasDefault, typeBuilder, null);

            MethodBuilder methodBuilder = GetMethodBuilder_GetInstance(typeBuilder, fieldBuilder, constructorBuilder);

            propertyBuilder.SetGetMethod(methodBuilder);
            return propertyBuilder;
        }

        //Methods
        private static MethodBuilder GetMethodBuilder_GetInstance(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, ConstructorBuilder constructorBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "get_Instance",
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig | 
                MethodAttributes.Static,    //?
                typeBuilder,
                Type.EmptyTypes);

            ILGenerator methodIL = methodBuilder.GetILGenerator();
            Label target = methodIL.DefineLabel();

            methodIL.Emit(OpCodes.Ldsfld, fieldBuilder);
            methodIL.Emit(OpCodes.Brtrue_S, target);
            methodIL.Emit(OpCodes.Newobj, constructorBuilder);
            methodIL.Emit(OpCodes.Stsfld, fieldBuilder);
            methodIL.MarkLabel(target);
            methodIL.Emit(OpCodes.Ldsfld, fieldBuilder);
            methodIL.Emit(OpCodes.Ret);

            return methodBuilder;
        }
        private static MethodBuilder GetMethodBuilder_ShowContent_Static(TypeBuilder typeBuilder, string typeName)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "ShowContent_Static",
                MethodAttributes.Public | MethodAttributes.Static,
                null,
                Type.EmptyTypes);

            ILGenerator methodIL = methodBuilder.GetILGenerator();
            methodIL.EmitWriteLine("Static Method shows: TypeName = ");
            methodIL.EmitWriteLine(typeName);
            methodIL.Emit(OpCodes.Ret);

            return methodBuilder;
        }
        private static MethodBuilder GetMethodBuilder_ShowContent_Instance(TypeBuilder typeBuilder, string typeName)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "ShowContent_Instance",
                MethodAttributes.Public,
                null,
                Type.EmptyTypes);

            ILGenerator methodIL = methodBuilder.GetILGenerator();
            methodIL.EmitWriteLine("Instance Method shows: TypeName = ");
            methodIL.EmitWriteLine(typeName);
            methodIL.Emit(OpCodes.Ret);

            return methodBuilder;
        }
    }
}
