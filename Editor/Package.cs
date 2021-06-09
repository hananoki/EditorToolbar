
namespace HananokiEditor.EditorToolbar {
  public static class Package {
    public const string reverseDomainName = "com.hananoki.editor-toolbar";
    public const string name = "EditorToolbar";
    public const string nameNicify = "Editor Toolbar";
    public const string editorPrefName = "Hananoki.EditorToolbar";
    public const string version = "1.1.0";
		[HananokiEditorMDViewerRegister]
		public static string MDViewerRegister() {
			return "5568888b696de3541803738279cce7fa";
		}
  }
}
