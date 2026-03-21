using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

namespace Server
{
    public class AssemblyEmitter
    {
        private readonly string m_AssemblyName;
        private readonly AssemblyBuilder m_AssemblyBuilder;
        private readonly ModuleBuilder m_ModuleBuilder;
        private readonly bool m_CanSave;

        public AssemblyEmitter(string assemblyName, bool canSave)
        {
            this.m_AssemblyName = assemblyName;
            this.m_CanSave = canSave;

            var name = new AssemblyName(assemblyName);

            // .NET 8/9 환경에 맞춘 Dynamic Assembly 생성
            this.m_AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            this.m_ModuleBuilder = this.m_AssemblyBuilder.DefineDynamicModule(assemblyName);
        }

        public TypeBuilder DefineType(string typeName, TypeAttributes attrs, Type parentType)
        {
            return this.m_ModuleBuilder.DefineType(typeName, attrs, parentType);
        }

        public void Save()
        {
            if (!m_CanSave) return;
            // .NET Core/5+ 환경에서는 표준 Save()가 지원되지 않으므로 경고만 출력합니다.
            Console.WriteLine("Warning: Assembly saving is not supported in this .NET version.");
        }
    }

	public class MethodEmitter
    {
        private TypeBuilder m_Type;
        private MethodBuilder m_Method;
        private ILGenerator m_IL;

        public ILGenerator Generator => m_IL; 
        public MethodBuilder Method => m_Method;
        
        // [수정] DistinctCompiler.cs (Line 175) 대응: 다시 Type으로 변경
        public Type Active { get; set; } 

        public MethodEmitter(TypeBuilder type)
        {
            m_Type = type;
        }

        public void Define(string name, MethodAttributes attr, Type returnType, Type[] paramTypes)
        {
            m_Method = m_Type.DefineMethod(name, attr, returnType, paramTypes);
            m_IL = m_Method.GetILGenerator();
        }

        // IL 명령 전달 메서드들
        public void LoadArgument(int index) => m_IL.Emit(OpCodes.Ldarg, index);
        public void LoadNull() => m_IL.Emit(OpCodes.Ldnull);
        public void LoadNull(Type type) => m_IL.Emit(OpCodes.Ldnull);
        public void Load(int value) => m_IL.Emit(OpCodes.Ldc_I4, value);
        public void Load(long value) => m_IL.Emit(OpCodes.Ldc_I8, value);
        public void Load(float value) => m_IL.Emit(OpCodes.Ldc_R4, value);
        public void Load(double value) => m_IL.Emit(OpCodes.Ldc_R8, value);
        public void Load(string value) => m_IL.Emit(OpCodes.Ldstr, value);
        public void Load(bool value) => m_IL.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        public void Load(Enum value) => m_IL.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));

        public void StoreLocal(LocalBuilder local) => m_IL.Emit(OpCodes.Stloc, local);
        public void LoadLocal(LocalBuilder local) => m_IL.Emit(OpCodes.Ldloc, local);
        public void Return() => m_IL.Emit(OpCodes.Ret);
        public void CastAs(Type type) => m_IL.Emit(OpCodes.Isinst, type);
        public void Compare(OpCode op) => m_IL.Emit(op);
        public void Xor() => m_IL.Emit(OpCodes.Xor);

        public void LogicalNot()
        {
            m_IL.Emit(OpCodes.Ldc_I4_0);
            m_IL.Emit(OpCodes.Ceq);
        }

        public void BranchIfFalse(Label label) => m_IL.Emit(OpCodes.Brfalse, label);
        public void BranchIfTrue(Label label) => m_IL.Emit(OpCodes.Brtrue, label);
        public void Branch(Label label) => m_IL.Emit(OpCodes.Br, label);
        public Label CreateLabel() => m_IL.DefineLabel();
        public void MarkLabel(Label label) => m_IL.MarkLabel(label);
        public LocalBuilder CreateLocal(Type type) => m_IL.DeclareLocal(type);
        public LocalBuilder AcquireTemp(Type type) => m_IL.DeclareLocal(type);
        public void ReleaseTemp(LocalBuilder local) { }

        // [수정] ConditionalCompiler.cs (Line 325) 대응: 인수 없는 Pop 추가
        public void Pop() => m_IL.Emit(OpCodes.Pop);

        // [수정] DistinctCompiler.cs (Line 197) 대응: Type 받는 Pop 유지
        public void Pop(Type type) => m_IL.Emit(OpCodes.Pop);

        public void LoadField(FieldInfo field) => m_IL.Emit(OpCodes.Ldfld, field);

        // 호출 관련
        public void BeginCall(MethodInfo method) { }
        public void FinishCall() { }
        public void Call(MethodInfo method) 
        { 
            if (method.IsVirtual) m_IL.Emit(OpCodes.Callvirt, method);
            else m_IL.Emit(OpCodes.Call, method);
        }

        public void Chain(object property)
        {
            var compileMethod = property.GetType().GetMethod("Compile", new Type[] { typeof(MethodEmitter) });
            if (compileMethod != null)
                compileMethod.Invoke(property, new object[] { this });
        }

        public bool CompareTo(int value, Action loadValue)
        {
            loadValue();
            return true;
        }
    }
}