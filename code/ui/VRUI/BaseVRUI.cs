using System;
using MagicGuard;
using Sandbox;
using Sandbox.UI;
using static VRUI.VRUIManager;

namespace VRUI {
	[Library]
	public class BaseVRUI : Entity {

		protected virtual bool isActive { get; set; }

		public bool IsActive {
			get => isActive; set {
				if(value && value != isActive) {
					isActive = value;
					Open();
				} else if(value != isActive) {
					isActive = value;
					Close();
				}
			}
		}

		public virtual Vector3 LocalOffset { get; set; }
		public virtual Angles LocalRotationOffset { get; set; }
		public virtual AttachmentPoint Attachment { get; set; }
		public virtual string TemplatePath { get; set; }
		public virtual Rect PanelBounds { get; set; } = new(-100, -100, 200, 200);

		public bool BlockLeftInput;
		public bool BlockRightInput;

		private WorldPanel UI;

		public BaseVRUI() {

		}

		public void CreateWorldPanel() {
			UI = new WorldPanel();
			UI.SetTemplate(TemplatePath);
		}

		public virtual void Init() {

		}

		public virtual void Open() {

		}
		public virtual void Close() {

		}

		public virtual void SimulateUI(AnimEntity LeftHand, Input.VrHand LeftHandInput, AnimEntity RightHand, Input.VrHand RightHandInput) {

			LocalRotation = LocalRotationOffset.ToRotation();
			LocalPosition = LocalOffset * LocalRotation;
			UI.Transform = Transform;
			UI.PanelBounds = PanelBounds;
			if(VRPlayer.DebugVR)
				DebugOverlay.Axis(Position, Rotation);

		}
	}
}
