using Sandbox;
using Sandbox.UI;
using VRUI;
using VRUI.panels;

namespace MagicGuard.UI {
	public partial class MagicGuardHud : Sandbox.HudEntity<RootPanel> {
		public MagicGuardHud() {
			if(!IsClient) return;
			if(!Input.VR.IsActive) {
				Log.Info("Is not in VR");

				RootPanel.AddChild<ChatBox>();
				RootPanel.AddChild<NameTags>();
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
			} else {
				DelayedUI();
			}
		}
		public static async void DelayedUI() {
			if(!Input.VR.IsActive) return;
			await GameTask.DelayRealtimeSeconds(2f);
			Log.Info("Is in VR");
			VRUIManager.CreateVRUI<SettingButton>();
		}
	}

}
