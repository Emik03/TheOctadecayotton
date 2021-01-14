using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using TheOctadecayotton;
using UnityEngine;

public class TheOctadecayottonScript : MonoBehaviour
{
    public InteractScript Interact;
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMModSettings ModSettings;
    public KMSelectable ModuleSelectable, SubModuleSelectable;
    public MeshRenderer SelectableRenderer;

    public string ForceRotation;
    public string ForceStartingSphere;

    internal static int ModuleIdCounter { get; private set; }
    internal static int Activated { get; set; }
    internal int moduleId;
    internal static bool stretchToFit;
    internal bool IsSolved { get; set; }
    internal bool ZenModeActive;
    internal string souvenirSphere;
    internal string souvenirRotations;

    private static bool _isUsingBounce;
    private static int _dimension, _rotation, _stepRequired;

    private void Start()
    {
        Activated = 0;
        moduleId = ++ModuleIdCounter;
        ModSettingsJSON.Get(this, out _dimension, out _rotation, out _stepRequired, out _isUsingBounce, out stretchToFit);
        
        ModuleSelectable.OnInteract += Interact.Init(this, _dimension - Info.GetSolvableModuleNames().Where(i => i == "The Octadecayotton").Count(), _rotation, _stepRequired, _isUsingBounce);
        SubModuleSelectable.OnInteract += Interact.OnInteract(this, _dimension - Info.GetSolvableModuleNames().Where(i => i == "The Octadecayotton").Count(), _rotation, _stepRequired, _isUsingBounce);
        SubModuleSelectable.OnHighlight += () => SelectableRenderer.enabled = true;
        SubModuleSelectable.OnHighlightEnded += () => SelectableRenderer.enabled = false;
    }

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} succumb (Activate module/Enter submission) | !{0} submit <#> <#> <#>... (Submits on digit #)";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.Split();

        if (Regex.IsMatch(split[0], @"^\s*succumb\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (Interact.isSubmitting)
                ModuleSelectable.OnInteract();
            else
                SubModuleSelectable.OnInteract();
        }

        if (Regex.IsMatch(split[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            split = split.Skip(1).ToArray();
            int n;

            if (!Interact.isActive)
                yield return "sendtochaterror The module isn't active. Use the \"succumb\" command.";

            else if (!Interact.isSubmitting)
                yield return "sendtochaterror The module isn't in submission. Use the \"succumb\" command.";

            else if (split.Length == 0)
                yield return "sendtochaterror Digits are expected to be provided as well. Expected: 0 to " + (Interact.Dimension - 1) + ".";

            else if (split.Any(s => !int.TryParse(s, out n)))
                yield return "sendtochaterror At least one of the arguments are not digits. Expected: 0 to " + (Interact.Dimension - 1) + ".";

            else if (split.Any(s => int.Parse(s) >= Interact.Dimension))
                yield return "sendtochaterror At least one of the arguments exceeded the amount of dimensions. Expected: 0 to " + (Interact.Dimension - 1) + ".";

            else
            {
                int[] times = split.Select(s => int.Parse(s)).ToArray();
                for (int i = 0; i < times.Length; i++)
                {
                    while (Interact.GetLastDigitOfTimer != times[i])
                        yield return true;
                    SubModuleSelectable.OnInteract();
                }
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (!Interact.isActive)
            ModuleSelectable.OnInteract();
        while (!Interact.isActive)
            yield return true;

        if (!Interact.isSubmitting)
            SubModuleSelectable.OnInteract();
        while (!Interact.isSubmitting || Interact.isRotating || (Interact.Dimension == 10 && Interact.GetPreciseLastDigitOfTimer > 9.75f))
            yield return true;

        int[][] answer = Interact.GetAnswer(ZenModeActive);
        for (int i = 0; i < answer.Length; i++)
        {
            while (Interact.GetLastDigitOfTimer != (Interact.Dimension > 10 ? 19 : 9) || (Interact.GetPreciseLastDigitOfTimer > 9.125f && Interact.Dimension == 10))
                yield return true;

            for (int j = 0; j < answer[i].Length; j++)
            {
                while (Interact.GetLastDigitOfTimer != answer[i][j])
                    yield return true;
                SubModuleSelectable.OnInteract();
            }
        }
    }
}
