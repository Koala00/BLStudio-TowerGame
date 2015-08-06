// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

// A slider-filled GUI window in the topleft corner that allows the parameters 
// defined in GlobalSettings.Instance.ballisticsSettings to be set in real time.
public class GlobalSettingsGUI : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Window(0, new Rect(10, 10, 200, 310), WindowFunction, "");
    }

    private static void WindowFunction(int windowID)
    {
        BallisticsSettings ballistics = GlobalSettings.Instance.ballisticsSettings;

        float yStep = 50, y = 10 - yStep;

        ballistics.gravity.y = -AddSlider("Gravity", "{0:0.0} m/s^2", 
            -ballistics.gravity.y, 1, 20, y += yStep);

        ballistics.terminalVelocity = AddSlider("Terminal velocity", "{0:0} m/s", 
                ballistics.terminalVelocity, 5, 100, y += yStep);

        ballistics.windVelocity.x = AddSlider("West wind", "{0:0} m/s", 
            ballistics.windVelocity.x, -25, 25, y += yStep);
        
        ballistics.windVelocity.z = AddSlider("South wind", "{0:0} m/s", 
            ballistics.windVelocity.z, -25, 25, y += yStep);
        
        ballistics.rotationStiffness = AddSlider("Rotation stiffness", "{0:0.0}", 
            ballistics.rotationStiffness, 0, 10, y += yStep);
        
        ballistics.rotationDamping = AddSlider("Rotation damping", "{0:0.0}", 
            ballistics.rotationDamping, 0, 2, y += yStep);
    }

    private static float AddSlider(string label, string format, float value, float min,
                                   float max, float y)
    {
        string valueText = string.Format(format, ((int)(value * 10)) * .1f);
        GUI.Label(new Rect(20, y, 250, 20), label);
        GUI.Label(new Rect(120, y + 20, 80, 20), valueText);
        return GUI.HorizontalSlider(new Rect(10, y + 25, 100, 20), value, min, max);
    }
}
