using UnityEditor;
using UnityEngine;

namespace HananokiEditor.EditorToolbar {

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

		//public Texture2D IconCS;

		public float IconButtonSize;

		//public void LoadProjectIcon() {
		//	var ico = AssetDatabaseUtils.LoadAssetAtGUID<Texture2D>( E.i.iconOpenCSProject );
		//	IconCS = ico ?? EditorIcon.icons_processed_dll_script_icon_asset;
		//}

		public Styles() {
			IconButtonSize = 30;
			if( UnitySymbol.UNITY_2019_3_OR_NEWER ) {
				IconButtonSize = 32;
			}

			//LoadProjectIcon();

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



			if( UnitySymbol.UNITY_2021_1_OR_NEWER ) {
				Button.fixedHeight = 20;
				Button.padding = new RectOffset( 0, 0, 0, 0 );

				DropDown.fixedHeight = 20;
				DropDown.padding = new RectOffset( 0, 0, 2, 2 );

				DropDownButton.fixedHeight = 20;
				DropDownButton.padding = new RectOffset( DropDownButton.padding.left, DropDownButton.padding.right, 2, 2 );

				DropDown2.fixedHeight = 20;
				Button2.fixedHeight = 20;
			}
		}
	}

}
