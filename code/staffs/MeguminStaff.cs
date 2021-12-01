using System;
using Sandbox;

namespace MagicGuard {
	public partial class MeguminStaff : BaseStaff {

		[Net, Predicted] bool soundplayed { get; set; } = false;
		[Net, Predicted] Sound soundinstance { get; set; }

		Particles GroundGlyphs { get; set; }
		Particles StaffGlyphs { get; set; }

		RealTimeUntil LastFire = 0;






		public override void SimulateStaff(Input.VrHand hand) {
			base.SimulateStaff(hand);

			if(!LastFire) return;
			if(ChargingSince > 0.5f) {

				if(GroundGlyphs == null && StaffGlyphs == null && IsClient) {

					GroundGlyphs = Particles.Create("particles/megumin/ground_glyphs_parent.vpcf", Parent.Client.Pawn);
					StaffGlyphs = Particles.Create("particles/megumin/staff_glyphs_parent.vpcf");
					StaffGlyphs.SetEntityAttachment(0, this, "Particle_End");
				}
				StaffGlyphs?.SetOrientation(1, GetAttachment("Particle_End").Value.Rotation);
			} else {
				GroundGlyphs?.Destroy();
				GroundGlyphs = null;

				StaffGlyphs?.Destroy();
				StaffGlyphs = null;
			}

			if(ChargingSince > 4f && !soundplayed) {
				soundplayed = true;
				if(IsServer) {
					playsound();
				}

			} else if(ChargingSince < 4f && soundplayed) {
				soundplayed = false;
				soundinstance.Stop();
			}


			if(ChargingSince > 5.5f) {
				Explosion();
			}
		}
		[ClientRpc]
		public void playsound() {
			Sound.FromWorld("meguminexplosion", Position);
		}

		public async void Explosion() {
			soundplayed = false;
			soundinstance.Stop();
			//Log.Error("Explosion!");
			ChargingSince = 0;

			LastFire = 5f;

			GroundGlyphs?.Destroy();
			GroundGlyphs = null;
			StaffGlyphs?.Destroy();
			StaffGlyphs = null;


			var trans = GetAttachment("Particle_End").Value;

			var traceresult = Trace.Ray(trans.Position + trans.Rotation.Forward, trans.Position + trans.Rotation.Up * 100000f).Ignore(this).Radius(10f).WorldAndEntities().Run();
			var beam = Particles.Create("particles/megumin/staff_beam.vpcf", trans.Position);
			beam.SetPosition(1, traceresult.EndPos);

			if(traceresult.Hit) {
				var p = Particles.Create("particles/megumin/nuke/nuke_explosion.vpcf", traceresult.EndPos);
				p.SetPosition(1, traceresult.EndPos);
				p.SetPosition(2, traceresult.EndPos);

				Sound.FromWorld("explosionsoundeffect", traceresult.EndPos);
				await GameTask.DelayRealtimeSeconds(0.5f);
				var radius = 500f;
				var sourcePos = traceresult.EndPos;
				var overlaps = Physics.GetEntitiesInSphere(sourcePos, radius);


				foreach(var overlap in overlaps) {
					if(overlap is not ModelEntity ent || !ent.IsValid())
						continue;

					if(ent.IsWorld)
						continue;

					var targetPos = ent.PhysicsBody.MassCenter;




					var forceDir = (targetPos - sourcePos).Normal;

					ent.TakeDamage(DamageInfo.Explosion(sourcePos, forceDir * 300F, 500F)
						.WithAttacker(this));
				}
			}
		}




	}
}
