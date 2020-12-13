
using System;

namespace HananokiEditor.EditorToolbar {
	[AttributeUsage( AttributeTargets.Class )]
	public class EditorToolbarClass : Attribute {
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class EditorToolbarMethod : Attribute {

		public int priority;

		public EditorToolbarMethod( int pri = 1 ) {
			this.priority = pri;
		}
	}
}


