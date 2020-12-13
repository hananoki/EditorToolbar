using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityReflection;
using HananokiEditor;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace UnityToolbarExtender {
	public static class ToolbarCallback {
		//static Type m_toolbarType;
		//static Type m_guiViewType;
		static PropertyInfo m_viewVisualTree;
		static FieldInfo m_imguiContainerOnGui;// = typeof( IMGUIContainer ).GetField( "m_OnGUIHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		static ScriptableObject m_currentToolbar;

		static Type _VisualElement;

		static Type m_iWindowBackendType;
		static PropertyInfo m_windowBackend;
		const BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		///// <summary>
		/// Callback for toolbar OnGUI method.
		/// </summary>
		public static Action OnToolbarGUI;

		static ToolbarCallback() {
			if( UnitySymbol.Has( "UNITY_2020_1_OR_NEWER" ) ) {
				m_iWindowBackendType = typeof( Editor ).Assembly.GetType( "UnityEditor.IWindowBackend" );
				m_windowBackend = UnityTypes.UnityEditor_GUIView.GetProperty( "windowBackend", bindFlags );
				m_viewVisualTree = m_iWindowBackendType.GetProperty( "visualTree", bindFlags );
			}
			else {
				m_viewVisualTree = UnityTypes.UnityEditor_GUIView.GetProperty( "visualTree", bindFlags );
			}

			var _IMGUIContainer = UnityTypes.UnityEngine_UIElements_IMGUIContainer;
			m_imguiContainerOnGui = _IMGUIContainer.GetField( "m_OnGUIHandler", bindFlags );
			_VisualElement = UnityTypes.UnityEngine_UIElements_VisualElement;

			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
		}


		public static void Repaint() => UnityEditorToolbar.RepaintToolbar();


		static void OnUpdate() {
			// Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
			if( m_currentToolbar != null ) return;

			// Find toolbar
			var toolbars = Resources.FindObjectsOfTypeAll( UnityTypes.UnityEditor_Toolbar );
			m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[ 0 ] : null;

			if( m_currentToolbar == null ) return;

			// Get it's visual tree
			var pi = _VisualElement.GetProperty( "Item", BindingFlags.Public | BindingFlags.Instance );

			object visualTree;
			if( UnitySymbol.Has( "UNITY_2020_1_OR_NEWER" ) ) {
				var windowBackend = m_windowBackend.GetValue( m_currentToolbar );
				visualTree = /*(VisualElement)*/ m_viewVisualTree.GetValue( windowBackend, null );
			}
			else {
				visualTree = /*(VisualElement)*/ m_viewVisualTree.GetValue( m_currentToolbar, null );
			}
			// Get first child which 'happens' to be toolbar IMGUIContainer
			//var container = (IMGUIContainer) visualTree[ 0 ];
			var container = pi.GetValue( visualTree, new object[] { 0 } );

			// (Re)attach handler
			var handler = (Action) m_imguiContainerOnGui.GetValue( container );
			handler -= OnGUI;
			handler += OnGUI;
			m_imguiContainerOnGui.SetValue( container, handler );
		}

		static void OnGUI() {
			var handler = OnToolbarGUI;
			if( handler != null ) handler();
		}
	}
}
