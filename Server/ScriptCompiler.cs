#region References
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Microsoft.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

#endregion

namespace Server
{
	public static class ScriptCompiler
	{
		public static Assembly[] Assemblies { get; set; }

		private static ScriptCompilerPaths Paths
		{
			get { return ScriptCompilerPaths.Current; }
		}

		private static readonly bool IsCaseSensitivePathPlatform = Path.DirectorySeparatorChar == '/';
		private static readonly StringComparer ScriptPathComparer = IsCaseSensitivePathPlatform ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
		private static readonly StringComparison ScriptPathComparison = IsCaseSensitivePathPlatform ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

		private static readonly List<string> m_AdditionalReferences = new List<string>();

		public static string[] GetReferenceAssemblies()
		{
			var list = new List<string>();

			var path = Paths.AssembliesConfigPath;

			if (File.Exists(path))
			{
				using (var ip = new StreamReader(path))
				{
					string line;

					while ((line = ip.ReadLine()) != null)
					{
						if (line.Length > 0 && !line.StartsWith("#"))
						{
							list.Add(line);
						}
					}
				}
			}

			list.Add(Core.ExePath);

			list.AddRange(m_AdditionalReferences);

			return list.ToArray();
		}

		public static string GetCompilerOptions(bool debug)
		{
			StringBuilder sb = null;

			AppendCompilerOption(ref sb, "/d:ServUO");

			AppendCompilerOption(ref sb, "/unsafe");

			if (!debug)
			{
				AppendCompilerOption(ref sb, "/optimize");
			}
			else
			{
				AppendCompilerOption(ref sb, "/debug");
				AppendCompilerOption(ref sb, "/d:DEBUG");
				AppendCompilerOption(ref sb, "/d:TRACE");
			}

#if MONO
			AppendCompilerOption( ref sb, "/d:MONO" );
#endif

			if (Core.Is64Bit)
			{
				AppendCompilerOption(ref sb, "/d:x64");
			}

#if NEWTIMERS
			AppendCompilerOption(ref sb, "/d:NEWTIMERS");
#endif

#if NEWPARENT
			AppendCompilerOption(ref sb, "/d:NEWPARENT");
#endif

			return (sb == null ? null : sb.ToString());
		}

		private static void AppendCompilerOption(ref StringBuilder sb, string define)
		{
			if (sb == null)
			{
				sb = new StringBuilder();
			}
			else
			{
				sb.Append(' ');
			}

			sb.Append(define);
		}

		private static byte[] GetHashCode(string compiledFile, string[] scriptFiles, bool debug)
		{
			using (var ms = new MemoryStream())
			{
				using (var bin = new BinaryWriter(ms))
				{
					var fileInfo = new FileInfo(compiledFile);

					bin.Write(fileInfo.LastWriteTimeUtc.Ticks);

					foreach (var scriptFile in scriptFiles)
					{
						fileInfo = new FileInfo(scriptFile);

						bin.Write(fileInfo.LastWriteTimeUtc.Ticks);
					}

					bin.Write(debug);
					bin.Write(Core.Version.ToString());

					ms.Position = 0;

					using (var sha1 = SHA1.Create())
					{
						return sha1.ComputeHash(ms);
					}
				}
			}
		}

		public static bool CompileCSScripts(out Assembly assembly)
		{
			return CompileCSScripts(false, true, out assembly);
		}

		public static bool CompileCSScripts(bool debug, out Assembly assembly)
		{
			return CompileCSScripts(debug, true, out assembly);
		}

		public static bool CompileCSScripts(bool debug, bool cache, out Assembly assembly)
		{
			Utility.PushColor(ConsoleColor.Yellow);
			Console.Write("Scripts: Compiling C# scripts (Roslyn)... ");
			Utility.PopColor();

			var files = GetScripts("*.cs");

			if (files.Length == 0)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("no files found.");

				if (Paths.SourceRootMode == ScriptSourceRootMode.NewOnly && String.IsNullOrWhiteSpace(Paths.NewScriptSourceRoot))
				{
					Console.WriteLine("Hint: Paths.SourceRootMode is NewOnly but Paths.NewSourceRoot is empty.");
				}

				Utility.PopColor();
				assembly = null;
				return true;
			}

			try
			{
				// 1. 소스 코드 읽기 및 문법 분석
				var syntaxTrees = files.Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f)).ToArray();

				// 2. 참조 어셈블리 리스트 생성
				var references = new List<MetadataReference>();

				// (A) 기본 참조 추가
				var baseRefs = GetReferenceAssemblies();
				foreach (var r in baseRefs)
				{
					if (!string.IsNullOrWhiteSpace(r) && File.Exists(r))
						references.Add(MetadataReference.CreateFromFile(r));
				}

				// (B) 프로세스 내 어셈블리 추가
				foreach (var assemblyObj in AppDomain.CurrentDomain.GetAssemblies())
				{
					try
					{
						if (!assemblyObj.IsDynamic && !string.IsNullOrWhiteSpace(assemblyObj.Location))
						{
							references.Add(MetadataReference.CreateFromFile(assemblyObj.Location));
						}
					}
					catch { }
				}

				// (C) 핵심 라이브러리 직접 보강
				var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
				string[] coreLibs = { 
				// [Core Runtime]
				"mscorlib.dll", 
				"System.Runtime.dll", 
				"netstandard.dll",
				"System.Private.CoreLib.dll",
				"System.Runtime.Extensions.dll",
				"System.Runtime.Loader.dll",

				// [Collections & Data]
				"System.Collections.dll", 
				"System.Collections.NonGeneric.dll", 
				"System.Collections.Specialized.dll",
				"System.Linq.dll",
				"System.Data.dll",
				"System.Data.Common.dll",
				"System.Data.DataSetExtensions.dll",

				// [IO & Compression]
				"System.IO.dll", 
				"System.IO.Compression.dll",
				"System.IO.Compression.FileSystem.dll",

				// [XML]
				"System.Xml.dll", 
				"System.Xml.ReaderWriter.dll", 
				"System.Xml.XmlDocument.dll", 
				"System.Xml.XDocument.dll", 
				"System.Private.Xml.dll",

				// [Network & Web]
				"System.Net.Primitives.dll", 
				"System.Net.Requests.dll", 
				"System.Net.Mail.dll",
				"System.Net.WebClient.dll",
				"System.Net.HttpListener.dll",
				"System.Net.NetworkInformation.dll",
				"System.Net.Sockets.dll",
				"System.Net.NameResolution.dll",
				"System.Private.Uri.dll",

				// [Drawing & Graphics] - 핵심 에러 해결 포인트
				"System.Drawing.dll",
				"System.Drawing.Common.dll", 
				"System.Drawing.Primitives.dll",
				"System.Drawing.Common.dll", // .NET 8에서는 이 파일이 핵심입니다.

				// [Text & Parallel]
				"System.Text.RegularExpressions.dll",
				"System.Threading.Tasks.Parallel.dll",
				"System.Console.dll",

				// [OS Services]
				"Microsoft.Win32.Registry.dll",
				"System.Diagnostics.FileVersionInfo.dll",
				"System.ComponentModel.TypeConverter.dll"
			};
				foreach (var lib in coreLibs)
				{
					var path = Path.Combine(coreDir, lib);
					if (File.Exists(path) && !references.Any(r => r.Display != null && r.Display.EndsWith(lib)))
						references.Add(MetadataReference.CreateFromFile(path));
				}

				try 
				{
					// .NET 8 런타임에 이미 로드된 Drawing 및 Compression 라이브러리를 직접 참조에 추가
					var drawingRef = MetadataReference.CreateFromFile(typeof(System.Drawing.Bitmap).Assembly.Location);
					if (!references.Any(r => r.Display == drawingRef.Display)) references.Add(drawingRef);

					var zipRef = MetadataReference.CreateFromFile(typeof(System.IO.Compression.ZipFile).Assembly.Location);
					if (!references.Any(r => r.Display == zipRef.Display)) references.Add(zipRef);
					
					// 추가적인 Drawing Primitives 대응
					var drawingPrimRef = MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "System.Drawing.Primitives").Location);
					if (!references.Any(r => r.Display == drawingPrimRef.Display)) references.Add(drawingPrimRef);
				}
				catch { 
					// 로드 실패 시 무시 (안정성 확보)
				}

				// 3. 컴파일 옵션 설정
				var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
					.WithAllowUnsafe(true)
					.WithOptimizationLevel(debug ? OptimizationLevel.Debug : OptimizationLevel.Release);

				// 4. 컴파일 객체 생성
				var compilation = CSharpCompilation.Create(
					"Scripts.CS.dll",
					syntaxTrees,
					references,
					compilationOptions);

				// 5. DLL 파일 생성
				var outputPath = GetUnusedPath("Scripts.CS");
				EnsureDirectory(Paths.ScriptOutputRoot);

				using (var ms = new FileStream(outputPath, FileMode.Create))
				{
					// 여기서 result가 정의됩니다.
					EmitResult result = compilation.Emit(ms);

					if (!result.Success)
					{
						Utility.PushColor(ConsoleColor.Red);
						Console.WriteLine("Failed!");

						// 파일을 경로별로 그룹화하여 출력
						var groupedDiagnostics = result.Diagnostics
							.Where(d => d.Severity == DiagnosticSeverity.Error)
							.GroupBy(d => d.Location.SourceTree?.FilePath ?? "Unknown File");

						foreach (var group in groupedDiagnostics)
						{
							Console.WriteLine(" File: {0}", Path.GetFullPath(group.Key));
							
							foreach (var diagnostic in group)
							{
								var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
								Console.WriteLine("  + Line {0}: {1} ({2})", line, diagnostic.GetMessage(), diagnostic.Id);
							}
						}

						Utility.PopColor();
						assembly = null;
						return false;
					}
				}

				Utility.PushColor(ConsoleColor.Green);
				Console.WriteLine("done.");
				Utility.PopColor();

				assembly = Assembly.LoadFrom(outputPath);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("\nCritical Compiler Error:");
				Console.WriteLine(ex.ToString());
				assembly = null;
				return false;
			}
		}

		public static void Display(CompilerResults results)
		{
			if (results.Errors.Count > 0)
			{
				var errors = new Dictionary<string, List<CompilerError>>(results.Errors.Count, StringComparer.OrdinalIgnoreCase);
				var warnings = new Dictionary<string, List<CompilerError>>(results.Errors.Count, StringComparer.OrdinalIgnoreCase);

				foreach (CompilerError e in results.Errors)
				{
					var file = e.FileName;

					// Ridiculous. FileName is null if the warning/error is internally generated in csc.
					if (string.IsNullOrEmpty(file))
					{
						Console.WriteLine("ScriptCompiler: {0}: {1}", e.ErrorNumber, e.ErrorText);
						continue;
					}

					var table = (e.IsWarning ? warnings : errors);

					List<CompilerError> list = null;
					table.TryGetValue(file, out list);

					if (list == null)
					{
						table[file] = list = new List<CompilerError>();
					}

					list.Add(e);
				}

				if (errors.Count > 0)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("Failed with: {0} errors, {1} warnings", errors.Count, warnings.Count);
					Utility.PopColor();
				}
				else
				{
					Utility.PushColor(ConsoleColor.Green);
					Console.WriteLine("Finished with: {0} errors, {1} warnings", errors.Count, warnings.Count);
					Utility.PopColor();
				}

				Utility.PushColor(ConsoleColor.Yellow);

				if (warnings.Count > 0)
				{
					Console.WriteLine("Warnings:");
				}

				foreach (var kvp in warnings)
				{
					var fileName = kvp.Key;
					var list = kvp.Value;

					var usedPath = GetScriptDisplayPath(fileName);

					Console.WriteLine(" + {0}:", usedPath);

					Utility.PushColor(ConsoleColor.DarkYellow);

					foreach (var e in list)
					{
						Console.WriteLine("    {0}: Line {1}: {2}", e.ErrorNumber, e.Line, e.ErrorText);
					}

					Utility.PopColor();
				}

				Utility.PopColor();

				Utility.PushColor(ConsoleColor.Red);

				if (errors.Count > 0)
				{
					Console.WriteLine("Errors:");
				}

				foreach (var kvp in errors)
				{
					var fileName = kvp.Key;
					var list = kvp.Value;

					var usedPath = GetScriptDisplayPath(fileName);

					Console.WriteLine(" + {0}:", usedPath);

					Utility.PushColor(ConsoleColor.Red);

					foreach (var e in list)
					{
						Console.WriteLine("    {0}: Line {1}: {2}", e.ErrorNumber, e.Line, e.ErrorText);
					}

					Utility.PopColor();
				}

				Utility.PopColor();
			}
			else
			{
				Utility.PushColor(ConsoleColor.Green);
				Console.WriteLine("Finished with: 0 errors, 0 warnings");
				Utility.PopColor();
			}
		}

		private static string GetScriptDisplayPath(string fileName)
		{
			if (String.IsNullOrWhiteSpace(fileName))
			{
				return fileName;
			}

			string fullPath = Path.GetFullPath(fileName);
			string bestRelative = null;
			int bestRootLength = -1;

			string[] roots = Paths.ScriptSourceRoots;

			for (int i = 0; i < roots.Length; i++)
			{
				string root = roots[i];

				if (String.IsNullOrWhiteSpace(root))
				{
					continue;
				}

				string fullRoot = Path.GetFullPath(root);

				if (!fullRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
				{
					fullRoot += Path.DirectorySeparatorChar;
				}

				if (fullPath.Length < fullRoot.Length)
				{
					continue;
				}

				if (String.Compare(fullPath, 0, fullRoot, 0, fullRoot.Length, ScriptPathComparison) == 0)
				{
					string relative = fullPath.Substring(fullRoot.Length);

					if (fullRoot.Length > bestRootLength)
					{
						bestRootLength = fullRoot.Length;
						bestRelative = relative;
					}
				}
			}

			return bestRelative ?? fullPath;
		}

		public static string GetUnusedPath(string name)
		{
			string outputRoot = Paths.ScriptOutputRoot;
			string path = Path.Combine(outputRoot, String.Format("{0}.dll", name));

			for (var i = 2; File.Exists(path) && i <= 1000; ++i)
			{
				path = Path.Combine(outputRoot, String.Format("{0}.{1}.dll", name, i));
			}

			return path;
		}

		public static void DeleteFiles(string mask)
		{
			try
			{
				var files = Directory.GetFiles(Paths.ScriptOutputRoot, mask);

				foreach (var file in files)
				{
					try
					{
						File.Delete(file);
					}
					catch
					{ }
				}
			}
			catch
			{ }
		}

		private delegate CompilerResults Compiler(bool debug);

		public static bool Compile()
		{
			return Compile(false);
		}

		public static bool Compile(bool debug)
		{
			return Compile(debug, true);
		}

		public static bool Compile(bool debug, bool cache)
		{
			string[] scriptRoots = Paths.ScriptSourceRoots;

			for (int i = 0; i < scriptRoots.Length; i++)
			{
				EnsureDirectory(scriptRoots[i]);
			}

			EnsureDirectory(Paths.ScriptOutputRoot);

			if (m_AdditionalReferences.Count > 0)
			{
				m_AdditionalReferences.Clear();
			}

			var assemblies = new List<Assembly>();

			Assembly assembly;

			if (CompileCSScripts(debug, cache, out assembly))
			{
				if (assembly != null)
				{
					assemblies.Add(assembly);
				}
			}
			else
			{
				return false;
			}

			if (assemblies.Count == 0)
			{
				return false;
			}

			Assemblies = assemblies.ToArray();

			Utility.PushColor(ConsoleColor.Yellow);
			Console.WriteLine("Scripts: Verifying...");
			Utility.PopColor();

			var watch = Stopwatch.StartNew();

			Core.VerifySerialization();

			watch.Stop();

			Utility.PushColor(ConsoleColor.Green);
			Console.WriteLine(
				"Finished ({0} items, {1} mobiles, {2} customs) ({3:F2} seconds)",
				Core.ScriptItems,
				Core.ScriptMobiles,
				Core.ScriptCustoms,
				watch.Elapsed.TotalSeconds);
			Utility.PopColor();

			return true;
		}

		public static void Invoke(string method)
		{
			var invoke = new List<MethodInfo>();

			foreach (var a in Assemblies)
			{
				var types = a.GetTypes();

				foreach (var t in types)
				{
					var m = t.GetMethod(method, BindingFlags.Static | BindingFlags.Public);

					if (m != null)
					{
						invoke.Add(m);
					}
				}
			}

			invoke.Sort(new CallPriorityComparer());

			foreach (var m in invoke)
			{
				m.Invoke(null, null);
			}
		}

		private static readonly Dictionary<Assembly, TypeCache> m_TypeCaches = new Dictionary<Assembly, TypeCache>();
		private static TypeCache m_NullCache;

		public static TypeCache GetTypeCache(Assembly asm)
		{
			if (asm == null)
			{
				return m_NullCache ?? (m_NullCache = new TypeCache(null));
			}

			TypeCache c;

			m_TypeCaches.TryGetValue(asm, out c);

			if (c == null)
			{
				m_TypeCaches[asm] = c = new TypeCache(asm);
			}

			return c;
		}

		public static Type FindTypeByFullName(string fullName)
		{
			return FindTypeByFullName(fullName, true);
		}

		public static Type FindTypeByFullName(string fullName, bool ignoreCase)
		{
			if (string.IsNullOrWhiteSpace(fullName))
			{
				return null;
			}

			Type type = null;

			for (var i = 0; type == null && i < Assemblies.Length; ++i)
			{
				type = GetTypeCache(Assemblies[i]).GetTypeByFullName(fullName, ignoreCase);
			}

			return type ?? GetTypeCache(Core.Assembly).GetTypeByFullName(fullName, ignoreCase);
		}

		public static IEnumerable<Type> FindTypesByFullName(string name)
		{
			return FindTypesByFullName(name, true);
		}

		public static IEnumerable<Type> FindTypesByFullName(string name, bool ignoreCase)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				yield break;
			}

			for (var i = 0; i < Assemblies.Length; ++i)
			{
				foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByFullName(name, ignoreCase))
				{
					yield return t;
				}
			}

			foreach (var t in GetTypeCache(Core.Assembly).GetTypesByFullName(name, ignoreCase))
			{
				yield return t;
			}
		}

        public static Type FindTypeByName(string name)
		{
			return FindTypeByName(name, true);
		}

		public static Type FindTypeByName(string name, bool ignoreCase)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			Type type = null;

			for (var i = 0; type == null && i < Assemblies.Length; ++i)
			{
				type = GetTypeCache(Assemblies[i]).GetTypeByName(name, ignoreCase);
			}

			return type ?? GetTypeCache(Core.Assembly).GetTypeByName(name, ignoreCase);
		}

		public static IEnumerable<Type> FindTypesByName(string name)
		{
			return FindTypesByName(name, true);
		}

		public static IEnumerable<Type> FindTypesByName(string name, bool ignoreCase)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
                yield break;
			}

			for (var i = 0; i < Assemblies.Length; ++i)
			{
				foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByName(name, ignoreCase))
				{
                    yield return t;
				}
			}

			foreach (var t in GetTypeCache(Core.Assembly).GetTypesByName(name, ignoreCase))
			{
				yield return t;
			}
        }

		public static void EnsureDirectory(string dir)
		{
			var path = dir;

			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(Core.BaseDirectory, path);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static string[] GetScripts(string filter)
		{
			var list = new List<string>();
			HashSet<string> seen = new HashSet<string>(ScriptPathComparer);
			HashSet<string> excludedDirs = Paths.ExcludedScriptDirectoryNames;

			foreach (var root in Paths.ScriptSourceRoots)
			{
				GetScripts(list, seen, excludedDirs, root, filter);
			}

			return list.ToArray();
		}

		public static void GetScripts(List<string> list, HashSet<string> seen, HashSet<string> excludedDirs, string path, string filter)
		{
			if (!Directory.Exists(path))
			{
				return;
			}

			foreach (var dir in Directory.GetDirectories(path))
			{
				string directoryName = Path.GetFileName(dir);

				if (excludedDirs.Contains(directoryName))
				{
					continue;
				}

				GetScripts(list, seen, excludedDirs, dir, filter);
			}

			foreach (var file in Directory.GetFiles(path, filter))
			{
				string fullPath = Path.GetFullPath(file);

				if (seen.Add(fullPath))
				{
					list.Add(fullPath);
				}
			}
		}

		public static void GetScripts(List<string> list, string path, string filter)
		{
			GetScripts(
				list,
				new HashSet<string>(ScriptPathComparer),
				new HashSet<string>(StringComparer.OrdinalIgnoreCase),
				path,
				filter);
		}
	}

	public class TypeCache
	{
		private readonly Type[] m_Types;
		private readonly TypeTable m_Names;
		private readonly TypeTable m_FullNames;

		public Type[] Types { get { return m_Types; } }
		public TypeTable Names { get { return m_Names; } }
		public TypeTable FullNames { get { return m_FullNames; } }

		public Type GetTypeByName(string name, bool ignoreCase)
		{
			return GetTypesByName(name, ignoreCase).FirstOrDefault(t => t != null);
		}

		public IEnumerable<Type> GetTypesByName(string name, bool ignoreCase)
		{
			return m_Names.Get(name, ignoreCase);
		}

		public Type GetTypeByFullName(string fullName, bool ignoreCase)
		{
			return GetTypesByFullName(fullName, ignoreCase).FirstOrDefault(t => t != null);
		}

		public IEnumerable<Type> GetTypesByFullName(string fullName, bool ignoreCase)
		{
			return m_FullNames.Get(fullName, ignoreCase);
		}

		public TypeCache(Assembly asm)
		{
			if (asm == null)
			{
				m_Types = Type.EmptyTypes;
			}
			else
			{
				m_Types = asm.GetTypes();
			}

			m_Names = new TypeTable(m_Types.Length);
			m_FullNames = new TypeTable(m_Types.Length);

			foreach (var g in m_Types.ToLookup(t => t.Name))
			{
				m_Names.Add(g.Key, g);
				
				foreach (var type in g)
				{
					m_FullNames.Add(type.FullName, type);

					var attr = type.GetCustomAttribute<TypeAliasAttribute>(false);

					if (attr != null)
					{
						foreach (var a in attr.Aliases)
						{
							m_FullNames.Add(a, type);
						}
					}
				}
			}

			m_Names.Prune();
			m_FullNames.Prune();

			m_Names.Sort();
			m_FullNames.Sort();
		}
	}

	public class TypeTable
	{
		private readonly Dictionary<string, List<Type>> m_Sensitive;
		private readonly Dictionary<string, List<Type>> m_Insensitive;

		public void Prune()
		{
			Prune(m_Sensitive);
			Prune(m_Insensitive);
		}

		private static void Prune(Dictionary<string, List<Type>> types)
		{
			var buffer = new List<Type>();

            foreach (var list in types.Values)
			{
				if (list.Count == 1)
				{
                    continue;
				}

				buffer.AddRange(list.Distinct());

				list.Clear();
				list.AddRange(buffer);

				buffer.Clear();
			}

			buffer.TrimExcess();
		}

		public void Sort()
		{
			Sort(m_Sensitive);
			Sort(m_Insensitive);
        }

		private static void Sort(Dictionary<string, List<Type>> types)
		{
			foreach (var list in types.Values)
			{
				list.Sort(InternalSort);
			}
		}

		private static int InternalSort(Type l, Type r)
		{
			if (l == r)
			{
				return 0;
			}

			if (l != null && r == null)
			{
				return -1;
			}

			if (l == null && r != null)
			{
				return 1;
			}

			var a = IsEntity(l);
			var b = IsEntity(r);

			if (a && b)
			{
				a = IsConstructable(l, out var x);
				b = IsConstructable(r, out var y);

				if (a && !b)
				{
					return -1;
				}

				if (!a && b)
				{
					return 1;
				}

				return x > y ? -1 : x < y ? 1 : 0;
			}

			return a ? -1 : b ? 1 : 0;
		}

		private static bool IsEntity(Type type)
		{
			return type.GetInterface("IEntity") != null;
		}

		private static bool IsConstructable(Type type, out AccessLevel access)
		{
			foreach (var ctor in type.GetConstructors().OrderBy(o => o.GetParameters().Length))
			{
				var attr = ctor.GetCustomAttribute<ConstructableAttribute>(false);

				if (attr != null)
				{
					access = attr.AccessLevel;
					return true;
				}
			}

			access = 0;
			return false;
		}

		public void Add(string key, IEnumerable<Type> types)
		{
			if (!string.IsNullOrWhiteSpace(key) && types != null)
			{
				Add(key, types.ToArray());
			}
		}

		public void Add(string key, params Type[] types)
		{
			if (string.IsNullOrWhiteSpace(key) || types == null || types.Length == 0)
			{
                return;
			}
			
			if (!m_Sensitive.TryGetValue(key, out var sensitive) || sensitive == null)
			{
				m_Sensitive[key] = new List<Type>(types);
			}
			else if (types.Length == 1)
			{
				sensitive.Add(types[0]);
			}
			else
			{
				sensitive.AddRange(types);
			}

            if (!m_Insensitive.TryGetValue(key, out var insensitive) || insensitive == null)
			{
				m_Insensitive[key] = new List<Type>(types);
			}
			else if (types.Length == 1)
			{
				insensitive.Add(types[0]);
			}
			else
			{
				insensitive.AddRange(types);
			}
		}

		public IEnumerable<Type> Get(string key, bool ignoreCase)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return Type.EmptyTypes;
			}

			List<Type> t;

			if (ignoreCase)
			{
				m_Insensitive.TryGetValue(key, out t);
			}
			else
			{
				m_Sensitive.TryGetValue(key, out t);
			}

			if (t == null)
			{
				return Type.EmptyTypes;
			}

			return t.AsEnumerable();
		}

		public TypeTable(int capacity)
		{
			m_Sensitive = new Dictionary<string, List<Type>>(capacity);
			m_Insensitive = new Dictionary<string, List<Type>>(capacity, StringComparer.OrdinalIgnoreCase);
		}
	}
}
