//#define ENABLE_LEGACY_PREFERENCE

using Hananoki.SharedModule;
using UnityEditor;
using UnityEngine;

using E = Hananoki.EditorToolbar.SettingsEditor;
using P = Hananoki.EditorToolbar.SettingsProject;

namespace Hananoki.EditorToolbar {
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
			var window = GetWindow<SettingsEditorWindow>();
			window.SetTitle( new GUIContent( Package.name, Icon.Get( "SettingsIcon" ) ) );
		}

		void OnEnable() {
			drawGUI = DrawGUI;
			E.Load();
		}



		/// <summary>
		/// 
		/// </summary>
		static void DrawGUI() {
			using( new PreferenceLayoutScope() ) {
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
		}



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
			E.Load();
			DrawGUI();
		}
	}
}

