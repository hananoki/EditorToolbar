
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;

using E = Hananoki.EditorToolbar.SettingsEditor;
using P = Hananoki.EditorToolbar.SettingsProject;


namespace Hananoki.EditorToolbar {
	[Serializable]
	public class SettingsProject {
		static string jsonPath => $"{Environment.CurrentDirectory}/ProjectSettings/Hananoki.EditorToolbar.json";

		[System.Serializable]
		public class Module {
			public string assemblyName;
			public string className;
			public Module( string assemblyName, string className ) {
				this.assemblyName = assemblyName;
				this.className = className;
			}
		}

		public List<Module> reg;

		public static P i;

		SettingsProject() {
			reg = new List<Module>();
		}

		public static void Load() {
			i = JsonUtility.FromJson<P>( fs.ReadAllText( jsonPath ) );
			if( i == null ) {
				i = new P();
				//Debug.Log( "new EditorToolbarSettings" );
				Save();
			}
		}

		public static void Save() {
			File.WriteAllText( jsonPath, JsonUtility.ToJson( i, true ) );
		}
	}

#if UNITY_2018_3_OR_NEWER
	public class EditorToolbarSettingsProvider : SettingsProvider {
		public EditorToolbarSettingsProvider( string path, SettingsScope scope ) : base( path, scope ) {
		}

		//public override void OnActivate( string searchContext, VisualElement rootElement ) {}

		//public override void OnDeactivate() {}

		public override void OnTitleBarGUI() {
			GUILayout.Label( $"{Package.version}", EditorStyles.miniLabel );
		}

		public static void DrawGUI() {
			using( new GUILayout.HorizontalScope() ) {
				if( GUILayout.Button( "Register Class" ) ) {
					var t = typeof( EditorToolbarClass );
					P.i.reg = new List<P.Module>();
					foreach( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() ) {
						foreach( Type type in assembly.GetTypes() ) {
							if( type.GetCustomAttribute( t ) == null ) continue;
							P.i.reg.Add( new P.Module( assembly.FullName.Split( ',' )[ 0 ], type.FullName ) );
						}
					}
					P.Save();
					EditorToolbar.MakeMenuCommand();
				}
				if( GUILayout.Button( "Unregister Class" ) ) {
					P.i.reg = new List<P.Module>();
					P.Save();
					EditorToolbar.MakeMenuCommand();
				}
			}
			if( P.i.reg != null ) {
				foreach( var p in P.i.reg ) {
					EditorGUILayout.LabelField( $"{p.assemblyName} : {p.className}" );
				}
			}
		}

		public override void OnGUI( string searchContext ) {
			DrawGUI();
		}


		//public override void OnFooterBarGUI() {}

		[SettingsProvider]
		private static SettingsProvider Create() {
			if( !E.i.enableProjectSettingsProvider ) return null;
			var provider = new EditorToolbarSettingsProvider( $"Hananoki/{Package.name}", SettingsScope.Project );

			return provider;
		}
	}
#endif
}
