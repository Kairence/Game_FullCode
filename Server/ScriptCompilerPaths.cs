using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
	public enum ScriptSourceRootMode
	{
		LegacyOnly,
		LegacyFirst,
		NewFirst,
		NewOnly
	}

	public sealed class ScriptCompilerPaths
	{
		private static ScriptCompilerPaths _current;
		private static bool _newOnlyEmptySourceWarningShown;

		public static ScriptCompilerPaths Current
		{
			get
			{
				if (_current == null)
				{
					_current = Load();
				}

				return _current;
			}
		}

		public string LegacyScriptSourceRoot { get; private set; }
		public string NewScriptSourceRoot { get; private set; }
		public ScriptSourceRootMode SourceRootMode { get; private set; }
		public string ScriptOutputRoot { get; private set; }
		public string AssembliesConfigPath { get; private set; }
		public string[] ScriptSourceRoots { get; private set; }
		public HashSet<string> ExcludedScriptDirectoryNames { get; private set; }

		private ScriptCompilerPaths()
		{
		}

		private static ScriptCompilerPaths Load()
		{
			ScriptCompilerPaths paths = new ScriptCompilerPaths();

			paths.LegacyScriptSourceRoot = ResolvePath(Config.Get("Paths.LegacySourceRoot", "Scripts"), "Scripts");
			paths.NewScriptSourceRoot = ResolveOptionalPath(Config.Get("Paths.NewSourceRoot", String.Empty));
			paths.ScriptOutputRoot = ResolvePath(Config.Get("Paths.OutputRoot", "Scripts/Output"), "Scripts/Output");
			paths.AssembliesConfigPath = ResolvePath(Config.Get("Paths.AssembliesConfigPath", "Data/Assemblies.cfg"), "Data/Assemblies.cfg");

			string mode = Config.Get("Paths.SourceRootMode", "LegacyFirst");
			ScriptSourceRootMode parsedMode;

			if (!Enum.TryParse(mode, true, out parsedMode))
			{
				parsedMode = ScriptSourceRootMode.LegacyFirst;
			}

			paths.SourceRootMode = parsedMode;

			if (paths.SourceRootMode == ScriptSourceRootMode.NewOnly && String.IsNullOrWhiteSpace(paths.NewScriptSourceRoot) && !_newOnlyEmptySourceWarningShown)
			{
				_newOnlyEmptySourceWarningShown = true;
				Utility.PushColor(ConsoleColor.Yellow);
				Console.WriteLine("Config: Warning, Paths.SourceRootMode is NewOnly but Paths.NewSourceRoot is empty. No script source roots will be scanned.");
				Utility.PopColor();
			}

			paths.ExcludedScriptDirectoryNames = ParseExcludedDirectories(
				Config.Get("Paths.ExcludedDirectoryNames", String.Empty));

			paths.ScriptSourceRoots = BuildSourceRoots(paths.LegacyScriptSourceRoot, paths.NewScriptSourceRoot, paths.SourceRootMode);

			return paths;
		}

		private static string[] BuildSourceRoots(string legacyRoot, string newRoot, ScriptSourceRootMode mode)
		{
			List<string> roots = new List<string>(2);

			switch (mode)
			{
				case ScriptSourceRootMode.LegacyOnly:
					AddDistinct(roots, legacyRoot);
					break;
				case ScriptSourceRootMode.NewFirst:
					AddDistinct(roots, newRoot);
					AddDistinct(roots, legacyRoot);
					break;
				case ScriptSourceRootMode.NewOnly:
					AddDistinct(roots, newRoot);
					break;
				default:
					AddDistinct(roots, legacyRoot);
					AddDistinct(roots, newRoot);
					break;
			}

			if (roots.Count == 0 && mode != ScriptSourceRootMode.NewOnly)
			{
				AddDistinct(roots, legacyRoot);
			}

			return roots.ToArray();
		}

		private static void AddDistinct(List<string> roots, string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				return;
			}

			for (int i = 0; i < roots.Count; i++)
			{
				if (Insensitive.Equals(roots[i], path))
				{
					return;
				}
			}

			roots.Add(path);
		}

		private static HashSet<string> ParseExcludedDirectories(string value)
		{
			HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (String.IsNullOrWhiteSpace(value))
			{
				return excluded;
			}

			string[] tokens = value.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < tokens.Length; i++)
			{
				string token = tokens[i].Trim();

				if (token.Length > 0)
				{
					excluded.Add(token);
				}
			}

			return excluded;
		}

		private static string ResolveOptionalPath(string value)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			return ResolvePath(value, null);
		}

		private static string ResolvePath(string value, string fallback)
		{
			string resolved = value;

			if (String.IsNullOrWhiteSpace(resolved))
			{
				resolved = fallback;
			}

			if (String.IsNullOrWhiteSpace(resolved))
			{
				return null;
			}

			resolved = resolved.Trim();

			if (!Path.IsPathRooted(resolved))
			{
				resolved = Path.Combine(Core.BaseDirectory, resolved);
			}

			return Path.GetFullPath(resolved);
		}
	}
}
