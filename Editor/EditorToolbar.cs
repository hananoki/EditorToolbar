#pragma warning disable 618

using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using HananokiEditor.SharedModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;
using UnityReflection;

using E = HananokiEditor.EditorToolbar.SettingsEditor;
using P = HananokiEditor.EditorToolbar.SettingsProject;
using EE = HananokiEditor.SharedModule.SettingsEditor;


namespace HananokiEditor.EditorToolbar {

	[InitializeOnLoad]
	public class EditorToolbar {

		public struct BuildTargetInfo {
			public BuildTargetGroup group;
			public Texture2D icon;
			public BuildTargetInfo( BuildTargetGroup group, Texture2D icon ) {
				this.group = group;
				this.icon = icon;
			}
		}


		public class Styles {
			public GUIStyle Button;
			public GUIStyle ButtonLeft;
			public GUIStyle ButtonMid;
			public GUIStyle ButtonRight;
			public GUIStyle DropDown;
			public GUIStyle DropDown2;
			public GUIStyle DropDownButton;

			public GUIStyle Button2;
			public GUIStyle toggle;

			public Texture2D IconCS;

			public float IconButtonSize;

			public void LoadProjectIcon() {
				var ico = AssetDatabaseUtils.LoadAssetAtGUID<Texture2D>( E.i.iconOpenCSProject );
				IconCS = ico ?? EditorIcon.icons_processed_dll_script_icon_asset;
			}

			public Styles() {
				IconButtonSize = 30;
				if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
					IconButtonSize = 32;
				}

				LoadProjectIcon();


				ButtonLeft = new GUIStyle( "ButtonLeft" );
				var r = new RectOffset( 6, 6, 0, 0 );
				ButtonLeft.padding = r;

				ButtonMid = new GUIStyle( "ButtonMid" );
				ButtonMid.padding = r;

				ButtonRight = new GUIStyle( "ButtonRight" );
				ButtonRight.padding = r;

				if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
					Button = new GUIStyle( "AppCommand" );
					Button.margin = new RectOffset( 3, 3, 2, 2 );
					Button.imagePosition = ImagePosition.ImageLeft;
					Button.fixedWidth = 0;
					//Button.padding = new RectOffset( 4, 4, 3+3, 3 + 3 );
				}
				else {
					Button = new GUIStyle( "button" );
					Button.fixedHeight = 18;
				}
				//Button.padding = r;
				Button.padding = new RectOffset( 4, 4, 3, 3 );
				Button.alignment = TextAnchor.MiddleCenter;


				if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
					var AppCommand = (GUIStyle) "AppCommand";
					Button2 = new GUIStyle( "button" );
					Button2.margin = new RectOffset( 3, 3, 2, 2 );
					Button2.imagePosition = ImagePosition.ImageLeft;
					Button2.fixedHeight = AppCommand.fixedHeight;
					//Button2.stretchWidth = true;
					Button2.padding = new RectOffset( 4, 4, 3, 3 );
					//Button2.active.textColor = Color.black;
					//Button2.onActive.textColor = Color.black;
					//Button2.onFocused.textColor = Color.black;
					//Button2.focused.textColor = Color.black;
				}
				else {
					Button2 = new GUIStyle( "button" );
					Button2.fixedHeight = 18;
					Button2.padding = EditorStyles.label.padding;
				}

				//Button2.margin = EditorStyles.label.margin;


				DropDown = new GUIStyle( "DropDown" );
				//Button.padding = new RectOffset( 6, 6, 1, 1 );
				DropDown.alignment = TextAnchor.MiddleCenter;
				//Button.lineHeight = ButtonRight.lineHeight;

				DropDown2 = new GUIStyle( "DropDown" );
				DropDown2.padding = new RectOffset( DropDown2.padding.left, DropDown2.padding.right, 0, 0 );
				DropDown2.alignment = TextAnchor.MiddleCenter;

				if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
					DropDownButton = new GUIStyle( "DropDown" );
					//DropDownButton.padding = new RectOffset( DropDownButton.padding.left, DropDownButton.padding.right, 0, 0 );
				}
				else {
					DropDownButton = new GUIStyle( "DropDownButton" );
					DropDownButton.padding = new RectOffset( 6, DropDownButton.padding.right, 2, 2 );
					DropDownButton.fixedHeight = 18;
				}

				toggle = new GUIStyle( EditorStyles.toggle );
				if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
					toggle.margin.top = 5;
				}
			}
		}

		public static Styles s_styles;

		const float SPACE = 8;
		static List<BuildTargetInfo> s_buildTargetInfo;

		static List<Action> addon;

		static bool m_lockReloadAssemblies;

		//static ScriptableObject[] s_urpAssets;
		//static ScriptableObject[] s_hdrpAssets;

		static EditorToolbar() {
			E.Load();
			s_buildTargetInfo = new List<BuildTargetInfo>( 64 );

			foreach( var p in PlatformUtils.GetSupportList() ) {
				s_buildTargetInfo.Add( new BuildTargetInfo( p, p.Icon() ) );
			}

			if( !UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				s_buildTargetInfo.Add( new BuildTargetInfo( BuildTargetGroup.Facebook, EditorIcon.buildsettings_facebook_small ) );
			}

			ToolbarExtender.LeftToolbarGUI.Add( OnLeftToolbarGUI );
			ToolbarExtender.RightToolbarGUI.Add( OnRightToolbarGUI );

			MakeMenuCommand();
			//if( UnityTypes.UnityEngine_Rendering_Universal_UniversalRenderPipelineAsset != null ) {
			//	s_urpAssets = AssetDatabaseUtils.FindAssetsAndLoad( UnityTypes.UnityEngine_Rendering_Universal_UniversalRenderPipelineAsset ).Cast<ScriptableObject>().ToArray();
			//}
			//if( UnityTypes.UnityEngine_Rendering_HighDefinition_HDRenderPipelineAsset != null ) {
			//	s_hdrpAssets = AssetDatabaseUtils.FindAssetsAndLoad( UnityTypes.UnityEngine_Rendering_HighDefinition_HDRenderPipelineAsset ).Cast<ScriptableObject>().ToArray();
			//}
		}



		public static void Repaint() {
			ToolbarCallback.Repaint();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static void MakeMenuCommand() {
			addon = new List<Action>();
			var lst = AssemblieUtils.GetAllMethodsWithAttribute<EditorToolbarMethod>().ToList();
			if( 0 < P.i.reg.Count ) {
				foreach( var p in P.i.reg ) {
					var pp = lst.Find( x => x.Module.Assembly.FullName.Split( ',' )[ 0 ] == p.assemblyName );
					if( pp != null ) {
						addon.Add( (Action) Delegate.CreateDelegate( typeof( Action ), null, pp ) );
					}
				}
			}
			//foreach( var p in P.i.reg ) {

			//	var lst = R.Methods( typeof( EditorToolbarMethod ), p.className, p.assemblyName );
			//	foreach( var pp in lst ) {
			//		addon.Add( (Action) Delegate.CreateDelegate( typeof( Action ), null, pp ) );
			//	}
			//}
			Repaint();
		}



		public static void CallbackEventOnSwitchPlatform( object userData ) {
			BuildTargetGroup group = (BuildTargetGroup) userData;

			PlatformUtils.SwitchActiveBuildTarget( group );
		}



		static void Button_Setting() {
			var cont = EditorHelper.TempContent( EditorIcon.settings, S._OpenSettings );
			var ssr = GUILayoutUtility.GetRect( cont, s_styles.DropDown2 );
			ssr.width = 40;

			if( EditorHelper.HasMouseClick( GUILayoutUtility.GetLastRect() ) ) {
				var m = new GenericMenu();
				m.AddItem( S._Preferences, false, () => UnityEditorMenu.Edit_Preferences() );
				if( UnitySymbol.UNITY_2018_3_OR_NEWER ) {
					m.AddItem( S._ProjectSettings, false, () => UnityEditorMenu.Edit_Project_Settings() );
				}
				else {
					m.AddSeparator( "" );
					m.AddItem( new GUIContent( S._Input ), false, () => UnityEditorMenu.Edit_Project_Settings_Input() );
					m.AddItem( new GUIContent( S._TagsandLayers ), false, () => UnityEditorMenu.Edit_Project_Settings_Tags_and_Layers() );
					m.AddItem( new GUIContent( S._Audio ), false, () => UnityEditorMenu.Edit_Project_Settings_Audio() );
					m.AddItem( new GUIContent( S._Time ), false, () => UnityEditorMenu.Edit_Project_Settings_Time() );
					m.AddItem( new GUIContent( S._Player ), false, () => UnityEditorMenu.Edit_Project_Settings_Player() );
					m.AddItem( new GUIContent( S._Physics ), false, () => UnityEditorMenu.Edit_Project_Settings_Physics() );
					m.AddItem( new GUIContent( S._Physics2D ), false, () => UnityEditorMenu.Edit_Project_Settings_Physics_2D() );
					m.AddItem( new GUIContent( S._Quality ), false, () => UnityEditorMenu.Edit_Project_Settings_Quality() );
					m.AddItem( new GUIContent( S._Graphics ), false, () => UnityEditorMenu.Edit_Project_Settings_Graphics() );
					m.AddItem( new GUIContent( S._Network ), false, () => UnityEditorMenu.Edit_Project_Settings_Network() );
					m.AddItem( new GUIContent( S._Editor ), false, () => UnityEditorMenu.Edit_Project_Settings_Editor() );
					m.AddItem( new GUIContent( S._ScriptExecutionOrder ), false, () => UnityEditorMenu.Edit_Project_Settings_Script_Execution_Order() );
				}
				//if( EditorHelper.IsDefine( "ENABLE_HANANOKI_SETTINGS" ) ) {
				m.AddSeparator( "" );
				m.AddItem( new GUIContent( "Hananoki-Settings" ), false, () => UnityEditorMenu.Window_Hananoki_Settings() );
				//}
				m.DropDownLastRect();
			}

			GUI.Button( ssr, cont, s_styles.DropDown2 );
		}


		static void Button_Platform() {
			var cont = EditorHelper.TempContent( s_buildTargetInfo.Find( x => x.group == UnityEditorEditorUserBuildSettings.activeBuildTargetGroup ).icon, S._OpenBuildSettings );

			Rect r = GUILayoutUtility.GetRect( cont, s_styles.DropDownButton, GUILayout.Width( 50 ) );
			Rect rr = r;
			rr.width = 20;
			rr.x += 30;

			if( EditorHelper.HasMouseClick( rr ) ) {
				var m = new GenericMenu();
				m.AddDisabledItem( "SwitchPlatform" );
				m.AddSeparator( "" );
				foreach( var e in s_buildTargetInfo ) {
					m.AddItem( e.group.GetShortName(), UnityEditorEditorUserBuildSettings.activeBuildTargetGroup == e.group, CallbackEventOnSwitchPlatform, e.group );
				}
				m.DropDown( r.PopupRect() );
				Event.current.Use();
			}
			if( GUI.Button( r, cont, s_styles.DropDownButton ) ) {
				UnityEditorMenu.File_Build_Settings();
			}

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				//EditorGUI.DrawRect( rr, new Color(0,0,1,0.2f));
				rr.y += 3;
				rr.height -= 6;
				GUI.Label( rr, GUIContent.none, "DopesheetBackground" );
			}
		}
		//static void Button_RenderPipe() {
		//	var renderPipelineAsset = GraphicsSettingsUtils.currentRenderPipeline;

		//	var cont = EditorHelper.TempContent( "Built-in RP" );
		//	if( renderPipelineAsset != null ) {
		//		var rpType = renderPipelineAsset.GetType();

		//		if( rpType == UnityTypes.UnityEngine_Rendering_Universal_UniversalRenderPipelineAsset ) {
		//			cont.text = "URP";
		//		}
		//		else if( rpType == UnityTypes.UnityEngine_Rendering_HighDefinition_HDRenderPipelineAsset ) {
		//			cont.text = "HDRP";
		//		}
		//		else {
		//			cont.text = "Custom RP";
		//		}
		//	}
		//	var ssr = GUILayoutUtility.GetRect( cont, s_styles.DropDown2 );
		//	if( EditorHelper.HasMouseClick( GUILayoutUtility.GetLastRect() ) ) {
		//		var m = new GenericMenu();
		//		m.AddItem( "Built-in RP", renderPipelineAsset == null, () => UnityEditorMenu.Edit_Preferences() );
		//		if( !s_urpAssets.IsEmpty() ) {
		//			m.AddSeparator();
		//			foreach( var p in s_urpAssets ) {
		//				m.AddItem( p.name, () => GraphicsSettingsUtils.currentRenderPipeline = (UnityEngine.Rendering.RenderPipelineAsset) p );
		//			}
		//		}
		//		if( !s_hdrpAssets.IsEmpty() ) {
		//			m.AddSeparator();
		//			foreach( var p in s_hdrpAssets ) {
		//				m.AddItem( p.name, () => UnityEditorMenu.Edit_Preferences() );
		//			}
		//		}
		//		m.DropDownLastRect();
		//	}

		//	GUI.Button( ssr, cont, s_styles.DropDown2 );
		//}

		static void Button_OpenCSProject() {
			if( GUILayout.Button( EditorHelper.TempContent( EditorIcon.cs_script, S._OpenCSharpProject ), s_styles.Button, GUILayout.Width( s_styles.IconButtonSize ) ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Open C# Project" );
			}
		}


		static void Button_ScreenShot() {
			var cont = EditorHelper.TempContent( EditorIcon.sceneviewfx, S._GameViewScreenShot );
			Rect r = GUILayoutUtility.GetRect( cont, s_styles.DropDownButton, GUILayout.Width( 50 ) );

			Rect rr = r;
			rr.width = 20;
			rr.x += 30;
			if( EditorHelper.HasMouseClick( rr ) ) {
				var m = new GenericMenu();
				var dname = Directory.GetCurrentDirectory() + "/ScreenShot";
				if( Directory.Exists( dname ) ) {
					m.AddItem( S._OpenOutputFolder, false, () => {
						ShellUtils.OpenDirectory( dname );
					} );
				}
				else {
					m.AddDisabledItem( S._OpenOutputFolder );
				}
				m.DropDown( r.PopupRect() );
				Event.current.Use();
			}

			if( GUI.Button( r, cont, s_styles.DropDownButton ) ) {
				EditorHelper.SaveScreenCapture();
			}

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				rr.y += 3;
				rr.height -= 6;
				GUI.Label( rr, GUIContent.none, "DopesheetBackground" );
			}
		}


		static void Button_AssetStore() {
			if( GUILayout.Button( EditorHelper.TempContent( EditorIcon.asset_store, S._OpenAssetStore ), s_styles.Button, GUILayout.Width( s_styles.IconButtonSize ) ) ) {
				HEditorWindow.ShowWindow( UnityTypes.UnityEditor_AssetStoreWindow, EE.IsUtilityWindow( UnityTypes.UnityEditor_AssetStoreWindow ) );
			}
		}

		static void Button_Avs() {
			if( GUILayout.Button( EditorHelper.TempContent( "VS" ), s_styles.Button, GUILayout.Width( s_styles.IconButtonSize ) ) ) {
				//HEditorWindow.ShowWindow( UnityTypes.UnityEditor_AssetStoreWindow, EE.IsUtilityWindow( UnityTypes.UnityEditor_AssetStoreWindow ) );

				//ScriptEditorUtility.GetScriptEditorFromPath( CodeEditor.CurrentEditorInstallation ) == ScriptEditorUtility.ScriptEditor.Other;
				UnityEditorSyncVS.SyncSolution();
			}
		}


		/// <summary>
		/// 左側のツールバー
		/// </summary>
		static void OnLeftToolbarGUI() {
			if( s_styles == null ) s_styles = new Styles();

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
			}
			else {
				GUILayout.BeginVertical();
				GUILayout.Space( 2 );
				GUILayout.BeginHorizontal();
			}

			_OnLeftToolbarGUI();
			//GUILayout.FlexibleSpace();
			//GUILayout.Button( "aaa" );

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				GUILayout.Space( 10 );
			}
			else {
				GUILayout.Space( 20 );
			}

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
			}
			else {
				GUILayout.EndHorizontal();
				GUILayout.Space( 2 );
				GUILayout.EndVertical();
			}
		}



		static void _OnLeftToolbarGUI() {
			GUILayout.FlexibleSpace();

			GUILayout.Label( EditorHelper.TempContent( SceneManager.GetActiveScene().name, EditorIcon.sceneasset ), s_styles.DropDown, GUILayout.Width( 160 ) );
			if( EditorHelper.HasMouseClick( GUILayoutUtility.GetLastRect() ) ) {
				var m = new GenericMenu();
				var lst = EditorHelper.GetBuildSceneNames();
				if( 0 < lst.Length ) {
					foreach( var path in lst ) {
						if( File.Exists( path ) ) {
							m.AddItem( path.FileNameWithoutExtension(), false, obj => EditorHelper.OpenScene( (string) obj ), path );
						}
						else {
							m.AddDisabledItem( $"{path.FileNameWithoutExtension()} : {S._Filenotfound}" );
						}
					}
				}
				else {
					m.AddDisabledItem( S._Therearenoscenesregisteredinthebuildsettings );
				}
				m.DropDown( GUILayoutUtility.GetLastRect().PopupRect() );
				Event.current.Use();
			}

			if( GUILayout.Button( EditorHelper.TempContent( EditorIcon.refresh, S._ReloadScene ), s_styles.Button, GUILayout.Width( 30 ) ) ) {
				if( Application.isPlaying ) {
					SceneManager.LoadScene( SceneManager.GetActiveScene().name );
				}
				else {
					EditorSceneManager.OpenScene( SceneManager.GetActiveScene().path );
				}
			}

			GUILayout.Space( SPACE );


			Button_Platform();

			Button_Setting();


			//GUILayout.Space( 2 );
			//rc222 = GUILayoutUtility.GetRect( 30, 18 );
			//if( GUI.Button( rc222, Icon.Get( "PlayButtonProfile" ), s_styles.Button ) ) {
			//	if( EditorUtility.DisplayDialog( "確認", "ビルド実行を行いますがよろしいですか？", "OK", "Cancel" ) ) {
			//		UnityEditorMenu.BuildAndRun();
			//	}
			//}
			GUILayout.Space( SPACE );

			if( addon != null ) {
				foreach( var p in addon ) {
					p.Invoke();
				}
			}
		}


		/// <summary>
		/// 右側のツールバー
		/// </summary>
		static void OnRightToolbarGUI() {
			if( s_styles == null ) s_styles = new Styles();

			ScopeChange.Begin();
			m_lockReloadAssemblies = GUILayout.Toggle( m_lockReloadAssemblies, EditorIcon.assemblylock, s_styles.Button, GUILayout.Width( s_styles.IconButtonSize ) );
			if( ScopeChange.End() ) {
				if( m_lockReloadAssemblies ) {
					EditorApplication.LockReloadAssemblies();
					EditorHelper.ShowMessagePop( "Lock Reload Assemblies" );
				}
				else {
					EditorApplication.UnlockReloadAssemblies();
					AssetDatabase.Refresh();
					EditorHelper.ShowMessagePop( "Unlock Reload Assemblies" );
				}
			}

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				ScopeChange.Begin();
				var cont = EditorHelper.TempContent( "PlayMode", UnityEditorEditorSettings.enterPlayModeOptionsEnabled ? EditorIcon.lightmeter_greenlight : EditorIcon.lightmeter_lightrim );
				var toggle = GUILayout.Toggle( UnityEditorEditorSettings.enterPlayModeOptionsEnabled, cont, s_styles.Button2, GUILayout.Width( cont.CalcWidth_label() - 16 ) );
				if( ScopeChange.End() ) {
					UnityEditorEditorSettings.enterPlayModeOptionsEnabled = toggle;
				}
			}



			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
			}
			else {
				GUILayout.BeginVertical();
				GUILayout.Space( 2 );
				GUILayout.BeginHorizontal();
			}

			Button_OpenCSProject();

			GUILayout.Space( SPACE );

			Button_ScreenShot();

			GUILayout.Space( SPACE );

			Button_AssetStore();

			//Button_Avs();
#if UNITY_2019_3_OR_NEWER
			var renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
#else
			var renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
#endif
			//Button_RenderPipe();

			if( renderPipelineAsset == null ) {
				HEditorGUILayout.LabelBox( "Built-in RP" );
			}
			else {
				var rpType = renderPipelineAsset.GetType();

				if( rpType == UnityTypes.UnityEngine_Rendering_Universal_UniversalRenderPipelineAsset ) {
					HEditorGUILayout.LabelBox( "URP" );
				}
				else if( rpType == UnityTypes.UnityEngine_Rendering_HighDefinition_HDRenderPipelineAsset ) {
					HEditorGUILayout.LabelBox( "HDRP" );
				}
				else {
					HEditorGUILayout.LabelBox( "Custom RP" );
				}
				//	GUILayout.Label( UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetTypeName() );
				//Debug.Log( UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().AssemblyQualifiedName );
			}

			GUILayout.FlexibleSpace();
			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
			}
			else {
				GUILayout.EndHorizontal();
				GUILayout.Space( 2 );
				GUILayout.EndVertical();
			}
		}
	}
}
