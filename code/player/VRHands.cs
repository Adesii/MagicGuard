using Sandbox;

namespace MagicGuard {

	[Library("vr_hands", Title = "VR Hands", Spawnable = true)]
	partial class VR_Hands : VRWeapon {
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override float PrimaryRate => 15.0f;
		public override float SecondaryRate => 1.0f;

		public TimeSince TimeSinceDischarge { get; set; }

		public override void Spawn() {
			base.Spawn();

			SetModel("weapons/rust_pistol/rust_pistol.vmdl");
		}

		public override bool CanPrimaryAttack() {
			var VRTriggerPulled = false;
			if(Input.VR.RightHand.Trigger.Value > .8) VRTriggerPulled = true;
			return base.CanPrimaryAttack() && Input.Pressed(InputButton.Attack1) || VRTriggerPulled;
		}

		public override bool CanSecondaryAttack() {
			var VRTriggerPulled = false;
			if(Input.VR.LeftHand.Trigger.Value > .8) VRTriggerPulled = true;
			return base.CanSecondaryAttack() && Input.Pressed(InputButton.Attack2) || VRTriggerPulled;
		}

		public override void AttackPrimary() {
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			(Owner as AnimEntity)?.SetAnimBool("b_attack", true);

			ShootEffects();
			PlaySound("rust_pistol.shoot");

			Transform LocalRightHand = Input.VR.RightHand.Transform;

			Rotation GunRotation = LocalRightHand.Rotation * new Angles(46f, 25f, 0f).ToRotation(); ;

			ShootBullet(LocalRightHand.Position, GunRotation.Forward, 0.05f, 1.5f, 9.0f, 3.0f);
		}

		private void Discharge() {
			if(TimeSinceDischarge < 0.5f)
				return;

			TimeSinceDischarge = 0;

			var muzzle = GetAttachment("muzzle") ?? default;
			var pos = muzzle.Position;
			var rot = muzzle.Rotation;

			ShootEffects();
			PlaySound("rust_pistol.shoot");

			ShootBullet(pos, rot.Forward, 0.05f, 1.5f, 9.0f, 3.0f);

			ApplyAbsoluteImpulse(rot.Backward * 200.0f);
		}

		public override void SimulateAnimator(PawnAnimator anim) {
			anim.SetParam("holdtype", 1);
			//anim.SetParam( "aimat_weight", 1.0f );
			(Owner as AnimEntity)?.SetAnimBool("b_vr", true);
		}

		protected override void OnPhysicsCollision(CollisionEventData eventData) {
			if(eventData.Speed > 500.0f) {
				Discharge();
			}
		}

		public override void AttackSecondary() {
			base.Reload();
		}
	}
}
