using System;
using Sandbox;

namespace MagicGuard {
	public partial class VRPlayer : Sandbox.Player {

		[Net, Predicted]
		public AnimEntity LH { get; set; }

		[Net, Predicted]
		public AnimEntity RH { get; set; }

		[ConVar.Replicated]
		public static bool DebugVR { get; set; } = false;

		[Net, Predicted] Entity RHItem { get; set; }
		[Net, Predicted] Entity LHItem { get; set; }


		public VRPlayer() {
			Inventory = new Inventory(this);
		}

		public virtual void InitialRespawn() {
			Respawn();
		}

		[ClientVar("vr_height", Help = "height offset, in inches")]
		public static int VRHeight { get; set; } = 0;
		public override void Respawn() {
			base.Respawn();

			SetModel("models/citizen/citizen.vmdl");

			Controller = new WalkControllerVR();
			var anim = new StandardPlayerAnimatorVR();
			Animator = anim;

			Tags.Add("VRIgnore");
			if(DevController != null)
				DevController = null;

			if(LH == null) {
				LH = new AnimEntity();
				LH.SetModel("models/handleft.vmdl");
				LH.SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
				LH.Owner = this;
				LH.Tags.Add("hand", "Left");
			}

			if(RH == null) {
				RH = new AnimEntity();
				RH.SetModel("models/handright.vmdl");
				RH.SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
				RH.Owner = this;
				RH.Tags.Add("hand", "Right");
			}

			SetAnimBool("b_vr", true);
			SetBodyGroup(3, 1);

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = false;
			EnableShadowInFirstPerson = true;

		}
		[ServerCmd]
		public static void spawnstaff() {
			_ = new MeguminStaff {
				Position = Input.VR.Head.Position + Input.VR.Head.Rotation.Forward * 10
			};
		}

		public override void FrameSimulate(Client cl) {
			base.FrameSimulate(cl);

			Controller.FrameSimulate(cl, this, Animator);
			SetBodyGroup(0, 1);

			HandleVR();
		}

		public void HandleVR() {


			VR.Scale = Scale;

			HandleVRHands();
			HandleHands();
		}

		Vector3 PositionOffsetLeftR => new(0, 0, 0f);
		Vector3 PositionOffsetRightR => new(0, 0, 0f);
		Rotation RotationOffset => Rotation.From(0f, 0f, 0);


		private void HandleVRHands() {
			if(!LH.IsValid() || !RH.IsValid()) return;



			LH.Transform = Input.VR.LeftHand.Transform;
			LH.Rotation *= RotationOffset;
			LH.Position += PositionOffsetLeftR * Input.VR.LeftHand.Transform.Rotation;

			RH.Transform = Input.VR.RightHand.Transform;
			RH.Rotation *= RotationOffset;
			RH.Position += PositionOffsetRightR * Input.VR.RightHand.Transform.Rotation;


			LH.Position += Controller.Pawn.Velocity * Time.Delta;
			RH.Position += Controller.Pawn.Velocity * Time.Delta;


			Transform LocalLeftHand = Transform.ToLocal(Input.VR.LeftHand.Transform);
			Transform LocalRightHand = Transform.ToLocal(Input.VR.RightHand.Transform);
			Transform LocalHead = Transform.ToLocal(Input.VR.Head);

			Vector3 PositionOffsetLeft = new Vector3(-6, 1f, 2.5f) * Transform.RotationToLocal(LH.Rotation);
			Vector3 PositionOffsetRight = new Vector3(-6f, -1, 2.5f) * Transform.RotationToLocal(RH.Rotation);



			SetAnimVector("left_hand_ik.position", LocalLeftHand.Position + PositionOffsetLeft);
			SetAnimVector("right_hand_ik.position", LocalRightHand.Position + PositionOffsetRight);




			SetAnimRotation("left_hand_ik.rotation", LH.Rotation * RotationOffset);
			SetAnimRotation("right_hand_ik.rotation", RH.Rotation * RotationOffset);



			SetAnimFloat("duck", (1 - (LocalHead.Position.z / 60f)) * 3f);



			LH.SetAnimFloat("Thumb", Input.VR.LeftHand.GetFingerValue(FingerValue.ThumbCurl));
			LH.SetAnimFloat("Index", Input.VR.LeftHand.GetFingerValue(FingerValue.IndexCurl));
			LH.SetAnimFloat("Middle", Input.VR.LeftHand.GetFingerValue(FingerValue.MiddleCurl));
			LH.SetAnimFloat("Ring", Input.VR.LeftHand.GetFingerValue(FingerValue.RingCurl));

			RH.SetAnimFloat("Thumb", Input.VR.RightHand.GetFingerValue(FingerValue.ThumbCurl));
			RH.SetAnimFloat("Index", Input.VR.RightHand.GetFingerValue(FingerValue.IndexCurl));
			RH.SetAnimFloat("Middle", Input.VR.RightHand.GetFingerValue(FingerValue.MiddleCurl));
			RH.SetAnimFloat("Ring", Input.VR.RightHand.GetFingerValue(FingerValue.RingCurl));
		}



		public override void Simulate(Client cl) {
			base.Simulate(cl);

			Controller.Simulate(cl, this, Animator);


			SetAnimBool("b_vr", true);
			SetBodyGroup(3, 1);
			if(Input.VR.LeftHand.ButtonA.WasPressed && IsClient) {
				spawnstaff();
			}

			HandleVR();
		}

		public override void TakeDamage(DamageInfo info) {

		}


		public void HandleHands() {

			Angles RotationOffsetLeft = new(50f, 0f, 90f);
			if(RHItem == null) {
				var RightHandResult = Trace.Ray(RH.Position, RH.Position + (RH.Rotation * RotationOffsetLeft.ToRotation()).Forward * 1000f).Radius(5f).Ignore(this).EntitiesOnly().WithoutTags("hand").Run();
				HandleGrabbable(RightHandResult, RH, Input.VR.RightHand);

				if(DebugVR) {
					if(RightHandResult.Hit) {
						DebugOverlay.Line(RightHandResult.StartPos, RightHandResult.EndPos, Color.Green);
						DebugOverlay.Sphere(RightHandResult.EndPos, 5f, Color.Green);
						DebugOverlay.Box(RightHandResult.Entity.WorldSpaceBounds.Mins, RightHandResult.Entity.WorldSpaceBounds.Maxs, Color.Green, 0f);

					} else {
						DebugOverlay.Line(RightHandResult.StartPos, RightHandResult.EndPos, Color.Red);
						DebugOverlay.Sphere(RightHandResult.EndPos, 5f, Color.Red);
					}
				}
			} else {
				if(Input.VR.RightHand.Grip > 0.5f) {
					var it = RHItem as IGrabable;
					it.HoldingHand = Input.VR.RightHand;
					it.IsHolding = true;
					it.Holding();
				} else {
					var it = RHItem as IGrabable;
					it.IsHolding = false;
					RHItem = null;
				}
			}


			if(LHItem == null) {
				var LeftHandResult = Trace.Ray(LH.Position, LH.Position + (LH.Rotation * RotationOffsetLeft.ToRotation()).Forward * 1000f).Radius(5f).Ignore(this).EntitiesOnly().WithoutTags("hand").Run();
				HandleGrabbable(LeftHandResult, LH, Input.VR.LeftHand);
				if(DebugVR) {

					if(LeftHandResult.Hit) {
						DebugOverlay.Line(LeftHandResult.StartPos, LeftHandResult.EndPos, Color.Green);
						DebugOverlay.Sphere(LeftHandResult.EndPos, 5f, Color.Green);
						DebugOverlay.Box(LeftHandResult.Entity.WorldSpaceBounds.Mins, LeftHandResult.Entity.WorldSpaceBounds.Maxs, Color.Green, 0f);

					} else {
						DebugOverlay.Line(LeftHandResult.StartPos, LeftHandResult.EndPos, Color.Red);
						DebugOverlay.Sphere(LeftHandResult.EndPos, 5f, Color.Red);
					}
				}

			} else {
				if(Input.VR.LeftHand.Grip > 0.5f) {
					var it = LHItem as IGrabable;
					it.HoldingHand = Input.VR.LeftHand;
					it.IsHolding = true;
					it.Holding();
				} else {
					var it = LHItem as IGrabable;
					it.IsHolding = false;
					LHItem = null;
				}
			}




		}

		private void HandleGrabbable(TraceResult result, AnimEntity handEntity, Input.VrHand hand) {
			if(result.Hit && result.Entity is IGrabable ig) {
				var rh = hand;
				if(rh.Grip > 0.5f && ig.IsGrabable(rh) && rh.Transform.Position.Distance(result.Entity.Position) > 10f) {
					ig.Grab(rh);
				} else if(rh.Grip > 0.5f) {
					ig.HoldingEntity = handEntity;
					ig.HoldingHand = hand;
					ig.IsHolding = true;
					ig.Holding();
					if(hand.Equals(Input.VR.RightHand)) {
						RHItem = result.Entity;
					} else {
						LHItem = result.Entity;
					}
				} else {
					ig.AimingAt();
				}
			}
		}
	}
}
