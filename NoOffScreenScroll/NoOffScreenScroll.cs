using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace NoOffScreenScroll {
	public class NoOffScreenScroll: ThreadingExtensionBase, IUserMod {
		public string Name => "No Off-Screen Scrolling";
		public string Description => "Stop camera scrolling when mouse leaves game screen.";
		protected Vector3 prevPos;
		protected bool active;
		protected bool wasEdgeScrollEnabled;

		public NoOffScreenScroll() {
			Log("Instantiated");
			active = false;
		}

		#region logging

		/// <summary>
		/// Writes a message to the debug logs. "NoOffScreenScroll" tag
		/// and timestamp are automatically prepended.
		/// </summary>
		/// <param name="message">Message.</param>
		public static void Log(String message) {
			String time = DateTime.Now.ToUniversalTime()
				.ToString("yyyyMMdd' 'HHmmss'.'fff");
			message = $"{time}: {message}{Environment.NewLine}";
			try {
				UnityEngine.Debug.Log("[NoOffScreenScroll] " + message);
			}
			catch(NullReferenceException) {
				//Happens if Unity logger isn't set up yet
			}
		}

		#endregion logging

		#region ThreadingExtensionBase

		/// <summary>
		/// Called by the game after this instance is created.
		/// </summary>
		/// <param name="threading">The threading.</param>
		public override void OnCreated(IThreading threading) {
			base.OnCreated(threading);
			Log("Created");
		}

		/// <summary>
		/// Called by the game before this instance is about to be destroyed.
		/// </summary>
		public override void OnReleased() {
			Log("Released");
			base.OnReleased();
		}

		/// <summary>
		/// Called once per rendered frame.
		/// Thread: Main
		/// </summary>
		/// <param name="realTimeDelta">Seconds since previous frame.</param>
		/// <param name="simulationTimeDelta">Smoothly interpolated to be used
		/// from main thread. On normal speed it is roughly same as realTimeDelta.</param>
		public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
			var panel = UnityEngine.Object.FindObjectOfType<OptionsGameplayPanel>();
			if(panel == null) return;

			//Unity reports the mouse being at the very edge of the window
			//if it's actually outside.
			var mouse = Input.mousePosition;
			bool shouldStop = false;
			if(mouse.x <= 0 || mouse.y <= 0
			|| mouse.x >= Screen.width - 1 || mouse.y >= Screen.height - 1) {
				shouldStop = true;
			}

			//We actually just change the setting in the options panel.
			//Unfortunately this does trigger writing the settings file
			//every time we do it...
			if((!active) && shouldStop) { //mouse left screen
				wasEdgeScrollEnabled = panel.edgeScrolling;
			}
			else if(active && !shouldStop) { //mouse entered screen
				panel.edgeScrolling = wasEdgeScrollEnabled; //restore previous setting
			}
			if(shouldStop) panel.edgeScrolling = false;
			active = shouldStop;
		}

		#endregion ThreadingExtensionBase
	}
}
