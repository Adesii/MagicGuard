using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;

using MagicGuard.Player;
using MagicGuard.UI;
using MagicGuard.Util;

namespace MagicGuard {

	public partial class MagicGuardGame : Game {
		public MagicGuardGame() {
			if(IsServer) {
				Log.Info("[SV] Gamemode created");

				_ = new MagicGuardHud();

			}

			if(IsClient) {
				Log.Info("[CL] Gamemode created");
			}
			Global.TickRate = 90;
		}

		public override void ClientJoined(Client client) {
			base.ClientJoined(client);


			if(client.IsUsingVr) {
				var VRplayer = new VRPlayer();
				client.Pawn = VRplayer;
				VRplayer.InitialRespawn();
			} else {
				var player = new MagicGuardPlayer();
				client.Pawn = player;
				player.InitialRespawn();
			}




		}

		public override void DoPlayerSuicide(Client cl) {
			if(cl.Pawn == null) return;

			cl.Pawn.Kill();
		}
	}
}
