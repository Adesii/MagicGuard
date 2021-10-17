using System;
using Sandbox;


namespace MagicGuard {
	public abstract partial class BaseStaff : Prop, IGrabable {
		public virtual string StaffModel { get; set; } = "models/megustaff.vmdl";

		[Net, Predicted] private bool m_holding { get; set; } = false;
		public bool IsHolding {
			get => m_holding; set {
				if(value && value != m_holding) {
					m_holding = value;
					StartedHolding();
				} else if(value != m_holding) {
					m_holding = value;
					StoppedHolding();
				}
			}
		}
		[Net, Predicted] public Input.VrHand HoldingHand { get; set; }
		[Net, Predicted] public ModelEntity HoldingEntity { get; set; }

		[Net, Predicted] public float ChargingSince { get; set; }



		RealTimeSince LastAimat = 0f;

		public override void Spawn() {
			base.Spawn();
			Transmit = TransmitType.Always;

			SetModel(StaffModel);
			SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
			CollisionGroup = CollisionGroup.Weapon;
			Tags.Add("VRWeapon");
		}

		public virtual void Grab(Input.VrHand vrHand) {
			if(IsServer) {
				PhysicsBody.GravityEnabled = false;
				Position -= (Position - vrHand.Transform.Position).Normal * 30f * (Position.Distance(vrHand.Transform.Position) / 2) * Time.Delta;
			}
		}

		[Event.Tick]
		public virtual void Tickstaff() {
			if(LastAimat > 0.1f) {
				if(IsClient)
					GlowActive = false;
				PhysicsBody.GravityEnabled = true;
			}
		}

		public virtual void Holding() {
			SimulateStaff(HoldingHand);
		}

		public virtual bool IsGrabable(Input.VrHand vrHand) {
			return true;
		}
		public virtual void AimingAt() {
			PhysicsBody.GravityEnabled = true;
			if(IsHolding) return;

			GlowActive = true;
			GlowColor = Color.Green;
			GlowState = GlowStates.GlowStateOn;
			LastAimat = 0;

		}

		public virtual void StartedHolding() {
			if(IsServer) {
				//EnableAllCollisions = false;
				Parent = HoldingEntity;
				Transform = HoldingEntity.Transform;
				Rotation = HoldingEntity.Transform.Rotation * Rotation.From(-90f, 90f, 90f);
			}

		}
		public virtual void StoppedHolding() {
			if(IsServer) {
				EnableAllCollisions = true;
				Velocity = HoldingHand.Velocity;
				Parent = null;
			}
		}




		public virtual void SimulateStaff(Input.VrHand hand) {

			if(hand.Trigger > 0.5f) {
				ChargingSince += Time.Delta;
			} else {
				ChargingSince = 0;
			}
		}
	}
}
