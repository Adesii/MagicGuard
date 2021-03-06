using Sandbox;
using System;
using System.Linq;

namespace MagicGuard.Player {
	partial class MagicGuardPlayer : Sandbox.Player {

		TimeSince timeSinceDied;
		public float RespawnTime = 1;

		[Net]
		public float MaxHealth { get; set; } = 100;

		public virtual void InitialRespawn() {
			Respawn();
		}

		public override void Respawn() {
			SetModel("models/citizen/citizen.vmdl");

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Host.AssertServer();

			LifeState = LifeState.Alive;
			Health = MaxHealth;
			Velocity = Vector3.Zero;
			WaterLevel.Clear();

			CreateHull();

			Game.Current?.MoveToSpawnpoint(this);
			ResetInterpolation();
		}

		public override void Simulate(Client cl) {
			if(LifeState == LifeState.Dead) {
				if(timeSinceDied > RespawnTime && IsServer) {
					Respawn();
				}
				return;
			}

			var controller = GetActiveController();
			controller?.Simulate(cl, this, GetActiveAnimator());

			SimulateActiveChild(cl, ActiveChild);
		}

		public override void OnKilled() {
			Game.Current?.OnKilled(this);

			timeSinceDied = 0;
			LifeState = LifeState.Dead;
			StopUsing();

			EnableDrawing = false;
		}
	}
}
