#pragma warning disable CS4014

using RayGUI_cs;

namespace Orion_Desktop
{
    /*
     * The following file defines all the needed function for the terminal GUI to work
     * properly. It mainly contains update function for buttons and textboxes.
     */
    internal static partial class Conceptor2D
    {
        /// <summary>Tries to update the latitude of the current viewpoint.</summary>
        /// <param name="args">Arguments passed from the textbox.</param>
        /// <param name="value">Text value of the textbox.</param>
        private static void UpdateLatitude(string[] args, string value)
        {
            if (float.TryParse(value, out OrionSim.ViewerLatitude)) OrionSim.UpdateViewPoint();
            else ((Textbox)TerminalGui["txbCurrentLat"]).Text = OrionSim.ViewerLatitude.ToString();
        }

        /// <summary>Tries to update the longitude of the current viewpoint.</summary>
        /// <param name="args">Arguments passed from the textbox.</param>
        /// <param name="value">Text value of the textbox.</param>
        private static void UpdateLongitude(string[] args, string value) 
        {
            if (float.TryParse(value, out OrionSim.ViewerLongitude)) OrionSim.UpdateViewPoint();
            else ((Textbox)TerminalGui["txbCurrentLon"]).Text = OrionSim.ViewerLongitude.ToString();
        }

        /// <summary>Sends movement information to the robot's motors.</summary>
        private static void SubmitWebSocketInstruction()
        {
            WebsocketRequests.SendMotorInstruction(StepMotorID.M2, 360);
        }

        /// <summary>Increments the current astral target index.</summary>
        private static void SwitchTargetLeft()
        {
            OrionSim.SwitchTarget(-1);
        }

        /// <summary>Decrements the current astral target index.</summary>
        private static void SwitchTargetRight()
        {
            OrionSim.SwitchTarget(1);
        }
    }
}
