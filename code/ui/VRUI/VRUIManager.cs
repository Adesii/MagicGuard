using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MagicGuard;
using Sandbox;
using Sandbox.UI;

namespace VRUI {
	/// <summary>
	/// Client Only Entity that manages UI Elements for VR
	/// </summary>
	public class VRUIManager : Entity {
		private static VRUIManager instance;
		public static VRUIManager Instance {
			get {
				if(instance == null || !instance.IsValid()) instance = new();
				return instance;
			}
		}

		AnimEntity RH => (Local.Pawn as VRPlayer).RH;
		AnimEntity LH => (Local.Pawn as VRPlayer).LH;

		string indexFinderR = "finger_index_2_R";


		List<BaseVRUI> WorldPanels = new();


		public override void ClientSpawn() {
			base.ClientSpawn();
			if(Instance != null) Delete();
		}

		[Event.Tick.Client]
		public void SimulateUI() {
			foreach(var item in WorldPanels) {
				if(item.IsActive) {
					item.SimulateUI(LH, Input.VR.LeftHand, RH, Input.VR.RightHand);
				}
			}
		}
		[Event.Hotload]
		public void updateUI() {
			foreach(var item in WorldPanels) {
				item.Init();
			}
		}

		public static void CreateVRUI<T>() where T : BaseVRUI {
			T UI = Library.Create<T>();
			UI.CreateWorldPanel();
			switch(UI.Attachment) {
				case AttachmentPoint.LeftHand:
					UI.Position = Instance.LH.Position;
					UI.Rotation = Instance.LH.Rotation;
					UI.Parent = Instance.LH;
					break;
				case AttachmentPoint.RightHand:
					UI.Position = Instance.RH.Position;
					UI.Rotation = Instance.RH.Rotation;

					UI.Parent = Instance.RH;
					break;
				case AttachmentPoint.HMD:
				case AttachmentPoint.None:
					break;
			}

			UI.Init();

			Log.Info("Created VR UI of type: " + UI.GetType());

			Instance.WorldPanels.Add(UI);
		}


		public enum AttachmentPoint {
			None,
			LeftHand,
			RightHand,
			HMD
		}
	}
}
