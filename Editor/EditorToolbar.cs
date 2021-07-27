// warning CS0618: 'BuildTargetGroup.Facebook' is obsolete: 'Facebook support was removed in 2019.3'
#pragma warning disable 618

using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityReflection;
using UnityToolbarExtender;
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


		public static Styles s_styles;

		const float SPACE = 8;
		static List<BuildTargetInfo> s_buildTargetInfo;

		//static List<Action> addon;

		static bool m_lockReloadAssemblies;


		static EditorToolbar() {
			//E.Load();
			s_buildTargetInfo = new List<BuildTargetInfo>( 64 );

			foreach( var p in PlatformUtils.GetSupportList() ) {
				s_buildTargetInfo.Add( new BuildTargetInfo( p, p.Icon() ) );
			}

			if( !UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				s_buildTargetInfo.Add( new BuildTargetInfo( BuildTargetGroup.Facebook, EditorIcon.buildsettings_facebook_small ) );
			}

			ToolbarExtender.LeftToolbarGUI.Add( OnLeftToolbarGUI );
			ToolbarExtender.RightToolbarGUI.Add( OnRightToolbarGUI );


#if UNITY_2021_1_OR_NEWER
				EditorApplication.update += add;
#endif
		}


#if UNITY_2021_1_OR_NEWER
		static UnityEditorToolbar m_UnityEditorToolbar;

		static void add() {
			var aa = Resources.FindObjectsOfTypeAll( UnityTypes.UnityEditor_Toolbar );
			if( aa.Length == 1 ) {
				m_UnityEditorToolbar = new UnityEditorToolbar( aa[ 0 ] );
			}
			else {
				return;
			}

			if( m_UnityEditorToolbar == null ) return;

			if( m_UnityEditorToolbar.m_Root == null ) return;

			m_UnityEditorToolbar.m_Root.SetFont( EditorStyles.standardFont );

			var ToolbarZoneLeftAlign = m_UnityEditorToolbar.m_Root.名前から検索する( "ToolbarZoneLeftAlign" );
			var ToolbarZonePlayMode = m_UnityEditorToolbar.m_Root.名前から検索する( "ToolbarZonePlayMode" );
			var ToolbarZoneRightAlign = m_UnityEditorToolbar.m_Root.名前から検索する( "ToolbarZoneRightAlign" );

			//var ToolbarZoneRightAlign = m_UnityEditorToolbar.m_Root.名前から検索する( "ToolbarZoneRightAlign" );

			//"ToolbarZonePlayMode"
			//Debug.Log( ToolbarZoneLeftAlign.name );
			var vs = new UnityEngine.UIElements.VisualElement();
			vs.name = "ToolSettings";

			var vss = new UnityEngine.UIElements.IMGUIContainer();
			vss.onGUIHandler += () => {
				//GUI.Button( new Rect( 0, 0, 32, 20 ), "aaa" );
				var xx = ToolbarZonePlayMode.layout.x - vss.layout.x;
				xx += 12;
				vss.style.width = xx;

				GUILayout.BeginArea( new Rect( 0, 0, xx, 24 ) );
				GUILayout.BeginHorizontal();
				//foreach( var handler in ToolbarExtender.LeftToolbarGUI ) {
				//	handler();
				//}
				OnLeftToolbarGUI();

				GUILayout.EndHorizontal();
				GUILayout.EndArea();

			};
			//string str = "StyleSheets/Toolbars/" + "MainToolbar";
			//var styleSheet = EditorGUIUtility.Load( str + "Common.uss" ) as UnityEngine.UIElements.StyleSheet;
			//UnityEditorToolbarsEditorToolbarUtility.LoadStyleSheets( "MainToolbar", vs );
			//vs.Add( vss );
			//vs.styleSheets.Add( styleSheet );
			//vs.style.height = 20;
			//vs.style.width = 120;
			vss.style.height = 20;
			vss.style.width = 120;
			ToolbarZoneLeftAlign.style.paddingRight = 0;
			ToolbarZoneLeftAlign.Add( vss );

			var rightIMGUIContainer = new UnityEngine.UIElements.IMGUIContainer();
			rightIMGUIContainer.onGUIHandler += () => {
				var xx = vs.worldBound.x - ToolbarZoneRightAlign.worldBound.x;
				//xx += 12;
				rightIMGUIContainer.style.width = xx;
				rightIMGUIContainer.style.height = 20;

				GUILayout.BeginArea( new Rect( 0, 0, xx, 20 ) );
				GUILayout.BeginHorizontal();

				OnRightToolbarGUI();

				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			};
			ToolbarZoneRightAlign.style.paddingLeft = 0;
			ToolbarZoneRightAlign.Add( vs );
			ToolbarZoneRightAlign.Add( rightIMGUIContainer );

			EditorApplication.update -= add;
		}
#endif




		public static void Repaint() {
			ToolbarCallback.Repaint();
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
				m.AddItem( S._Preferences, () => UnityEditorMenu.Edit_Preferences() );
				if( UnitySymbol.UNITY_2018_3_OR_NEWER ) {
					m.AddItem( S._ProjectSettings, () => UnityEditorMenu.Edit_Project_Settings() );
				}
				else {
					m.AddSeparator( "" );
					m.AddItem( S._Input, () => UnityEditorMenu.Edit_Project_Settings_Input() );
					m.AddItem( S._TagsandLayers, () => UnityEditorMenu.Edit_Project_Settings_Tags_and_Layers() );
					m.AddItem( S._Audio, () => UnityEditorMenu.Edit_Project_Settings_Audio() );
					m.AddItem( S._Time, () => UnityEditorMenu.Edit_Project_Settings_Time() );
					m.AddItem( S._Player, () => UnityEditorMenu.Edit_Project_Settings_Player() );
					m.AddItem( S._Physics, () => UnityEditorMenu.Edit_Project_Settings_Physics() );
					m.AddItem( S._Physics2D, () => UnityEditorMenu.Edit_Project_Settings_Physics_2D() );
					m.AddItem( S._Quality, () => UnityEditorMenu.Edit_Project_Settings_Quality() );
					m.AddItem( S._Graphics, () => UnityEditorMenu.Edit_Project_Settings_Graphics() );
					m.AddItem( S._Network, () => UnityEditorMenu.Edit_Project_Settings_Network() );
					m.AddItem( S._Editor, () => UnityEditorMenu.Edit_Project_Settings_Editor() );
					m.AddItem( S._ScriptExecutionOrder, () => UnityEditorMenu.Edit_Project_Settings_Script_Execution_Order() );
				}

				m.AddSeparator( "" );
				m.AddItem( "Hananoki-Settings", () => UnityEditorMenu.Window_Hananoki_Settings() );
				if( EditorHelper.HasMenuItem( "Window/Hananoki/Render Pipeline" ) ) {
					m.AddItem( "Window/Hananoki/Render Pipeline".FileName(), () => "Window/Hananoki/Render Pipeline".ExecuteMenuItem() );
				}

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
			var image =
#if UNITY_2020_1_OR_NEWER
				EditorIcon.package_manager;
#else
				EditorIcon.asset_store;
#endif
			var tooltip =
#if UNITY_2020_1_OR_NEWER
				"Package Manager";
#else
				S._OpenAssetStore;
#endif
			var windowT =
#if UNITY_2020_1_OR_NEWER
				UnityTypes.UnityEditor_PackageManager_UI_PackageManagerWindow;
#else
				UnityTypes.UnityEditor_AssetStoreWindow;
#endif

			if( GUILayout.Button( EditorHelper.TempContent( image, tooltip ), s_styles.Button, GUILayout.Width( s_styles.IconButtonSize ) ) ) {
				EditorWindowUtils.ShowWindow( windowT, EE.IsUtilityWindow( windowT ) );
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
			//if( E.i.enabled ) return;
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
				var lst = EditorHelper.GetBuildSceneNames( false );
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

			//if( addon != null ) {
			//	foreach( var p in addon ) {
			//		p.Invoke();
			//	}
			//}
		}


		/// <summary>
		/// 右側のツールバー
		/// </summary>
		static void OnRightToolbarGUI() {
			//if( E.i.enabled ) return;
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
				LabelBox( "Built-in RP" );
			}
			else {
				var rpType = renderPipelineAsset.GetType();

				if( rpType == UnityTypes.UnityEngine_Rendering_Universal_UniversalRenderPipelineAsset ) {
					LabelBox( "URP" );
				}
				else if( rpType == UnityTypes.UnityEngine_Rendering_HighDefinition_HDRenderPipelineAsset ) {
					LabelBox( "HDRP" );
				}
				else {
					LabelBox( "Custom RP" );
				}
				//	GUILayout.Label( UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetTypeName() );
				//Debug.Log( UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().AssemblyQualifiedName );
			}

			GUILayout.FlexibleSpace();

			if( GUILayout.Button( EditorHelper.TempContent( Icon.Get( "Unity Hub" ) ), s_styles.Button, GUILayout.Width( s_styles.IconButtonSize ) ) ) {
				Help.BrowseURL( "unityhub://" );
			}

			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
			}
			else {
				GUILayout.EndHorizontal();
				GUILayout.Space( 2 );
				GUILayout.EndVertical();
			}
		}


		static void LabelBox( string text ) {
			ScopeHorizontal.Begin( EditorStyles.helpBox );
			GUILayout.Label( text );
			ScopeHorizontal.End();
		}
	}
}
