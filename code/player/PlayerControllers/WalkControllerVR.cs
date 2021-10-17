namespace Sandbox {
	[Library]
	public class WalkControllerVR : WalkController {
		public WalkControllerVR() {
			Unstuck = new Unstuck(this);
		}

		/// <summary>
		/// This is temporary, get the hull size for the player's collision
		/// </summary>
		public override BBox GetHull() {
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3(-girth, -girth, (GroundEntity == null) ? (BodyHeight / 2f) : 0);
			var maxs = new Vector3(+girth, +girth, BodyHeight);

			return new BBox(mins, maxs);
		}

		float BBoxBaseHeight = 0f;

		public override void UpdateBBox() {
			var girth = BodyGirth * 0.5f;

			BBoxBaseHeight = MathX.LerpTo(BBoxBaseHeight, 0, 0.1f);

			Transform LocalHead = Pawn.Transform.ToLocal(Input.VR.Head);

			var mins = (new Vector3(-girth, -girth, BBoxBaseHeight) + (LocalHead.Position.WithZ(0) * Rotation)) * Pawn.Scale;
			var maxs = (new Vector3(+girth, +girth, BodyHeight) + (LocalHead.Position.WithZ(0) * Rotation)) * Pawn.Scale;

			SetBBox(mins, maxs);
		}

		public override TraceResult TraceBBox(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0) {
			if(liftFeet > 0) {
				start += Vector3.Up * liftFeet;
				maxs = maxs.WithZ(maxs.z - liftFeet);
			}

			//pFilter->m_attr.SetCollisionGroup( collisionGroup ); // COLLISION_GROUP_PLAYER_MOVEMENT
			// pFilter->m_attr.SetHitSolidRequiresGenerateContacts( true );

			var tr = Trace.Ray(start + TraceOffset, end + TraceOffset)
						.Size(mins, maxs)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.Solid, true)
						.HitLayer(CollisionLayer.GRATE, true)
						.HitLayer(CollisionLayer.PLAYER_CLIP, true)
						.WithoutTags("VRWeapon")
						.WithoutTags("hand")
						.Ignore(Pawn)
						.Run();

			tr.EndPos -= TraceOffset;
			return tr;
		}

		Rotation PlayerRot;
		public Vector2 LeftJoy, RightJoy;


		public override void Simulate() {
			EyePosLocal = Vector3.Up * (EyeHeight * Pawn.Scale);
			UpdateBBox();

			EyePosLocal += TraceOffset;
			EyeRot = PlayerRot;

			LeftJoy = Input.VR.LeftHand.Joystick.Value;

			RestoreGroundPos();

			if(Unstuck.TestAndFix())
				return;

			CheckLadder();
			Swimming = Pawn.WaterLevel.Fraction > 0.6f;

			//
			// Start Gravity
			//
			if(!Swimming && !IsTouchingLadder) {
				Velocity -= new Vector3(0, 0, Gravity * 0.5f) * Time.Delta;
				Velocity += new Vector3(0, 0, BaseVelocity.z) * Time.Delta;

				BaseVelocity = BaseVelocity.WithZ(0);
			}

			// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
			//  we don't slow when standing still, relative to the conveyor.
			bool bStartOnGround = GroundEntity != null;
			//bool bDropSound = false;
			if(bStartOnGround) {
				//if ( Velocity.z < FallSoundZ ) bDropSound = true;

				Velocity = Velocity.WithZ(0);
				//player->m_Local.m_flFallVelocity = 0.0f;

				if(GroundEntity != null) {
					ApplyFriction(GroundFriction * SurfaceFriction);
				}
			}

			//
			// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
			//
			WishVelocity = new Vector3(LeftJoy.y, -LeftJoy.x, 0);
			var inSpeed = WishVelocity.Length.Clamp(0, 1);

			if(!Swimming && !IsTouchingLadder) {
				WishVelocity = WishVelocity.WithZ(0);
			}

			WishVelocity = WishVelocity.Normal * inSpeed;
			WishVelocity *= GetWishSpeed();
			WishVelocity *= Input.VR.Head.Rotation;

			bool bStayOnGround = false;
			if(Swimming) {
				ApplyFriction(1);
				WaterMove();
			} else if(IsTouchingLadder) {
				LadderMove();
			} else if(GroundEntity != null) {
				bStayOnGround = true;
				WalkMove();
			} else {
				AirMove();
			}

			CategorizePosition(bStayOnGround);

			// FinishGravity
			if(!Swimming && !IsTouchingLadder) {
				Velocity -= new Vector3(0, 0, Gravity * 0.5f) * Time.Delta;
			}


			if(GroundEntity != null) {
				Velocity = Velocity.WithZ(0);
			}

			// CheckFalling(); // fall damage etc

			// Land Sound
			// Swim Sounds

			SaveGroundPos();

			if(Debug) {
				DebugOverlay.Box(Position + TraceOffset, mins, maxs, Color.Red);
				DebugOverlay.Box(Position, mins, maxs, Color.Blue);

				var lineOffset = 0;
				if(Host.IsServer) lineOffset = 10;

				DebugOverlay.ScreenText(lineOffset + 0, $"        Position: {Position}");
				DebugOverlay.ScreenText(lineOffset + 1, $"        Velocity: {Velocity}");
				DebugOverlay.ScreenText(lineOffset + 2, $"    BaseVelocity: {BaseVelocity}");
				DebugOverlay.ScreenText(lineOffset + 3, $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]");
				DebugOverlay.ScreenText(lineOffset + 4, $" SurfaceFriction: {SurfaceFriction}");
				DebugOverlay.ScreenText(lineOffset + 5, $"    WishVelocity: {WishVelocity}");
			}

		}

		bool IsTouchingLadder = false;
		Vector3 LadderNormal;

		public override void CheckLadder() {
			if(IsTouchingLadder && Input.VR.RightHand.ButtonA.IsPressed) {
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;
			}

			const float ladderDistance = 1.0f;
			var start = Position;
			Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal) * ladderDistance;

			var pm = Trace.Ray(start, end)
						.Size(mins, maxs)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.LADDER, true)
						.Ignore(Pawn)
						.Run();

			IsTouchingLadder = false;

			if(pm.Hit) {
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}

		void RestoreGroundPos() {
			if(GroundEntity == null || GroundEntity.IsWorld)
				return;

			//var Position = GroundEntity.Transform.ToWorld( GroundTransform );
			//Pos = Position.Position;
		}

		void SaveGroundPos() {
			if(GroundEntity == null || GroundEntity.IsWorld)
				return;

			//GroundTransform = GroundEntity.Transform.ToLocal( new Transform( Pos, Rot ) );
		}

	}
}
