using System;
using Sandbox;
using static Sandbox.Input;

namespace MagicGuard {
	public interface IGrabable {

		bool IsHolding { get; set; }
		VrHand HoldingHand { get; set; }
		ModelEntity HoldingEntity { get; set; }
		bool IsGrabable(VrHand vrHand);
		void Grab(VrHand vrHand);

		void Holding();
		void AimingAt();
	}
}
