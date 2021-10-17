using System;

namespace VRUI.panels {
	public class SettingButton : BaseVRUI {
		protected override bool isActive => true;

		public override VRUIManager.AttachmentPoint Attachment => VRUIManager.AttachmentPoint.LeftHand;
		public override Vector3 LocalOffset => new Vector3(4, 8, -4);
		public override Angles LocalRotationOffset => new Angles(-0, 90, 55);
		public override string TemplatePath => "/ui/VRUI/panels/SettingButton.html";
		public override Rect PanelBounds => new(-15, -15, 30, 30);
	}
}
