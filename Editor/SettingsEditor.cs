#define ENABLE_HANANOKI_SETTINGS

using HananokiEditor.SharedModule;
using UnityEditor;
using UnityEngine;
using HananokiEditor.Extensions;

using E = HananokiEditor.EditorToolbar.SettingsEditor;
using P = HananokiEditor.EditorToolbar.SettingsProject;

namespace HananokiEditor.EditorToolbar {
	[InitializeOnLoad]
	[System.Serializable]
	public class SettingsEditor {


		public bool enableProjectSettingsProvider;

		public string iconOpenCSProject;

		public static E i;


		public static void Load() {
			if( i != null ) return;
			i = EditorPrefJson<E>.Get( Package.editorPrefName );
			P.Load();
		}


		public static void Save() {
			EditorPrefJson<E>.Set( Package.editorPrefName, i );
			P.Save();
		}
	}



	public class SettingsEditorWindow : HSettingsEditorWindow {

		public static void Open() {
			var w = GetWindow<SettingsEditorWindow>();
			w.SetTitle( new GUIContent( Package.name, EditorIcon.settings ) );
			w.headerMame = Package.name;
			w.headerVersion = Package.version;
			w.gui = DrawGUI;
		}



		/// <summary>
		/// 
		/// </summary>
		public static void DrawGUI() {
			E.Load();

			EditorGUI.BeginChangeCheck();
			E.i.enableProjectSettingsProvider = HEditorGUILayout.ToggleLeft( S._ProjectSettingsProvider, E.i.enableProjectSettingsProvider );
			E.i.iconOpenCSProject = HEditorGUILayout.GUIDObjectField<Texture2D>( nameof( E.i.iconOpenCSProject ).nicify(), E.i.iconOpenCSProject );

			if( EditorGUI.EndChangeCheck() ) {
				EditorToolbar.s_styles.LoadProjectIcon();
				EditorToolbar.Repaint();
				E.Save();
			}

			if( E.i.enableProjectSettingsProvider ) return;

			GUILayout.Space( 8f );

			GUILayout.Label( S._ProjectSettings, "ShurikenModuleTitle" );
#if UNITY_2018_3_OR_NEWER
			EditorToolbarSettingsProvider.DrawGUI();
#endif
		}


#if !ENABLE_HANANOKI_SETTINGS
#if UNITY_2018_3_OR_NEWER && !ENABLE_LEGACY_PREFERENCE
		[SettingsProvider]
		public static SettingsProvider PreferenceView() {
			var provider = new SettingsProvider( $"Preferences/Hananoki/{Package.name}", SettingsScope.User ) {
				label = $"{Package.name}",
				guiHandler = PreferencesGUI,
				titleBarGuiHandler = () => GUILayout.Label( $"{Package.version}", EditorStyles.miniLabel ),
			};
			return provider;
		}

		public static void PreferencesGUI( string searchText ) {
#else
		[PreferenceItem( Package.name )]
		public static void PreferencesGUI() {
#endif
			using( new LayoutScope() ) DrawGUI();
		}
#endif
	}



#if ENABLE_HANANOKI_SETTINGS
	public class SettingsEvent {
		[HananokiSettingsRegister]
		public static SettingsItem Changed() {
			return new SettingsItem() {
				//mode = 1,
				displayName = Package.nameNicify,
				version = Package.version,
				gui = SettingsEditorWindow.DrawGUI,
			};
		}
	}
#endif
}

