using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Supplies an enumerator ButtonType, and ModSettings.
/// </summary>
namespace TheOctadecayotton
{
    /// <summary>
    /// The mod settings that can be adjusted by a user, usually from the ModSelector.
    /// </summary>
    public class ModSettingsJSON
    {
        /// <summary>
        /// The amount of dimensions, by default, 9.
        /// </summary>
        [JsonProperty("Dimensions")]
        public int Dimension { get; set; }

        /// <summary>
        /// The amount of rotations, by default, 3.
        /// </summary>
        [JsonProperty("Rotations")]
        public int Rotation { get; set; }

        /// <summary>
        /// The amount of steps needed for update, by default, 1.
        /// </summary>
        [JsonProperty("25FPS")]
        public bool LowFPS { get; set; }

        /// <summary>
        /// The amount of steps needed for update, by default, 1.
        /// </summary>
        [JsonProperty("InOutBounce")]
        public bool IsUsingBounce { get; set; }

        /// <summary>
        /// Scales the spheres individually on the X, Y, and Z axis rather than taking the max out of those 3.
        /// </summary>
        [JsonProperty("StretchToFit")]
        public bool StretchToFit { get; private set; }

        /// <summary>
        /// Gets the value from ModSettings.
        /// </summary>
        /// <param name="octadecayotton">The instance of the module.</param>
        /// <param name="dimension">The amount of dimensions.</param>
        /// <param name="rotation">The amount of rotations.</param>
        /// <param name="stepRequired">The amount of frames required for the next step of the rotation to be displayed.</param>
        public static void Get(TheOctadecayottonScript octadecayotton, out int dimension, out int rotation, out int stepRequired, out bool isUsingBounce, out bool stretchToFit)
        {
            // Default values.
            dimension = 9;
            rotation = 3;
            stepRequired = 1;
            isUsingBounce = false;
            stretchToFit = false;

            try
            {
                // Try loading settings.
                var settings = JsonConvert.DeserializeObject<ModSettingsJSON>(octadecayotton.ModSettings.Settings);

                // Do settings exist?
                if (settings != null)
                {
                    dimension = Mathf.Clamp(settings.Dimension, 3, 12);
                    rotation = Mathf.Clamp(settings.Rotation, 0, 100);
                    stepRequired = settings.LowFPS.AsInt() + 1;
                    isUsingBounce = settings.IsUsingBounce;
                    stretchToFit = settings.StretchToFit;
                    Debug.LogFormat("[The Octadecayotton #{0}]: JSON loaded successfully, values are: [Dimensions = {1}], [Rotations = {2}], [25FPS: {3}], [InOutBounce: {4}], and [StretchToFit: {5}].",
                        octadecayotton.moduleId,
                        dimension,
                        rotation,
                        settings.LowFPS,
                        isUsingBounce,
                        stretchToFit);
                }

                else
                    Debug.LogFormat("[The Octadecayotton #{0}]: JSON is null, resorting to default values.", octadecayotton.moduleId);
            }
            catch (JsonReaderException e)
            {
                // In the case of catastrophic failure and devastation.
                Debug.LogFormat("[The Octadecayotton #{0}]: JSON error: \"{1}\", resorting to default values.", octadecayotton.moduleId, e.Message);
            }
        }
    }
}
