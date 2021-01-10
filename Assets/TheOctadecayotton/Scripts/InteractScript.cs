using TheOctadecayotton;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Linq;

public class InteractScript : MonoBehaviour
{
    public GameObject Polygon, Sphere;
    public KMBombInfo Info;
    public Renderer ModuleRenderer;
    public Texture EvilTexture, NeutralTexture;

    internal List<SphereScript> Spheres { get; set; }
    internal Rotation[][][] Rotations { get; private set; }
    internal Dictionary<Axis, bool> AnchorSphere { get; private set; }

    internal bool isRotating, isSubmitting, isActive, isStarting;
    internal int Dimension { get { return _dimension; } set { if (_dimension == 0) _dimension = Mathf.Clamp(value, 3, 12); } }
    internal int GetLastDigitOfTimer { get { return (int)(Info.GetTime() % (Dimension > 10 ? 20 : 10)); } }
    internal float rotationProgress;
    internal Dictionary<Axis, bool> startingSphere = new Dictionary<Axis, bool>();
    internal static IEnumerable<Axis> allAxies = Enum.GetValues(typeof(Axis)).Cast<Axis>();

    private bool _isUsingBounce;
    private int _moduleId, _breakCount, _step, _stepRequired, _dimension;
    private Axis[] _order;
    private List<Axis> _inputs;
    private Dictionary<Axis, int> _axesUsed = new Dictionary<Axis, int>();
    private Animate _animate;
    private TheOctadecayottonScript _octadecayotton;

    internal KMSelectable.OnInteractHandler Init(TheOctadecayottonScript octadecayotton, int dimension, int rotation, int stepRequired, bool isUsingBounce)
    {
        return () =>
        {
            if (isStarting || isActive || octadecayotton.IsSolved)
                return true;

            isStarting = true;
            rotationProgress = 0;

            _octadecayotton = octadecayotton;
            _animate = new Animate(this, _octadecayotton);
            _moduleId = octadecayotton.ModuleId;
            _stepRequired = stepRequired;
            _isUsingBounce = isUsingBounce;

            TheOctadecayottonScript.Activated++;
            Dimension = dimension + TheOctadecayottonScript.Activated;

            StartCoroutine(_animate.CreateHypercube(Dimension));

            octadecayotton.PlaySound(Dimension > 9 ? "StartupHard" : "Startup");
            octadecayotton.ModuleSelectable.AddInteractionPunch(16);

            Rotations = !Application.isEditor || octadecayotton.ForceRotation.IsNullOrEmpty()
                      ? TheOctadecayottonExtensions.GetRandomRotations(new RotationOptions(dimension: Dimension, rotationCount: rotation))
                      : octadecayotton.ForceRotation.ToRotations();

            Debug.LogFormat("[The Octadecayotton #{0}]: The rotations are {1}.",
                _moduleId,
                Rotations.ToLog());

            AnchorSphere = Rotations.Get(Dimension, _moduleId);
            Debug.LogFormat("[The Octadecayotton #{0}]: The anchor sphere is in {1}. ({2}-ordered)",
                _moduleId,
                AnchorSphere.Select(a => a.Value ? "+" : "-").Join(""),
                "XYZWVURSTOPQ".Substring(0, Dimension));

            CreateStartingSphere();
            Debug.LogFormat("[The Octadecayotton #{0}]: To solve this module, press anywhere to enter submission, submit the digits from left-to-right when the {1} matches the digit shown, then submit on every digit from {2} down to 0.",
                _moduleId,
                Dimension > 10 ? "timer modulo 20" : "last digit of the timer",
                Dimension - 1);
            Debug.LogFormat("[The Octadecayotton #{0}]: Example full solution (not knowing axes) => {1}.",
                _moduleId,
                startingSphere.GetAnswer(AnchorSphere, _axesUsed, _order, true).Select(i => i.Join(Dimension > 9 ? " " : "")).Join(", "));
            Debug.LogFormat("[The Octadecayotton #{0}]: Quickest solution (knowing axes) => {1}.",
                _moduleId,
                startingSphere.GetAnswer(AnchorSphere, _axesUsed, _order, false).Select(i => i.Join(Dimension > 9 ? " " : "")).Join(", "));

            return true;
        };
    }

    internal KMSelectable.OnInteractHandler OnInteract()
    {
        return () =>
        {
            _octadecayotton.ModuleSelectable.AddInteractionPunch();
            _octadecayotton.PlaySound("InteractInterrupt");
            if (!isActive || _octadecayotton.IsSolved)
                return false;
            return isSubmitting ? HandleSubmission() : !(isSubmitting = true);
        };
    }

    internal int[][] GetAnswer()
    {
        List<int[]> temp = startingSphere.GetAnswer(AnchorSphere, _axesUsed, _order, false).ToList();
        temp.Add(Enumerable.Range(0, Dimension).Reverse().ToArray());
        return temp.ToArray();
    }

    private void FixedUpdate()
    {
        if (_octadecayotton == null || _octadecayotton.IsSolved)
            return;

        if (isSubmitting && !isRotating && GetLastDigitOfTimer == (Dimension > 10 ? 19 : 9) && _inputs.Count != 0 && !(Dimension == 10 && _inputs.Count == 1))
        {
            if (!_inputs.Validate(startingSphere, AnchorSphere, _axesUsed, _order, ref _breakCount, Dimension, ref _moduleId))
                StartCoroutine(_animate.Strike());

            else if (_inputs.Count == Dimension)
                StartCoroutine(_animate.Solve());

            else if (_inputs.Count == (Dimension == 3 ? 1 : 3))
                _octadecayotton.PlaySound("StartingSphere");

            for (int i = 0; i < Spheres.Count; i++)
                Spheres[i].HandleUpdate();

            _inputs = new List<Axis>();
        }

        if (!isActive || !isRotating || ++_step % _stepRequired != 0)
            return;

        if (Rotations.Length == 0)
            isSubmitting = true;

        if (rotationProgress >= Rotations.Length + 0.4f)
            rotationProgress = 0;

        if (rotationProgress < Rotations.Length || Rotations.Length == 0)
        {
            if (rotationProgress % 1 == 0)
            {
                if (isSubmitting)
                {
                    _octadecayotton.PlaySound("StartingSphere");
                    isRotating = false;
                }

                for (int i = 0; i < Spheres.Count && Rotations.Length != 0; i++)
                    Spheres[i].pos.SetRotation(Rotations[(int)rotationProgress]);
            }

            for (int i = 0; i < Spheres.Count; i++)
                Spheres[i].UpdateSphere(rotationProgress, _isUsingBounce);
        }

        rotationProgress += 1f / (256 / _stepRequired);
    }

    private void CreateStartingSphere()
    {
        do
        {
            _breakCount = 0;
            _inputs = new List<Axis>();
            startingSphere = new Dictionary<Axis, bool>();
            _axesUsed = new Dictionary<Axis, int>();

            for (int i = 0; i < Dimension; i++)
            {
                startingSphere.Add(allAxies.ElementAt(i),
                    !Application.isEditor || _octadecayotton.ForceStartingSphere.IsNullOrEmpty()
                    ? Rnd.Range(0, 1f) > 0.5f
                    : _octadecayotton.ForceStartingSphere.Where(c => c == '-' || c == '+').ElementAtOrDefault(i) == '+');
                _axesUsed.Add(allAxies.ElementAt(i), 0);
            }
        } while (startingSphere.Select((a, n) => a.Value != AnchorSphere.ElementAt(n).Value).All(b => !b));

        _order = allAxies.Take(Dimension).ToArray().Shuffle().ToArray();
        Debug.LogFormat("[The Octadecayotton #{0}]: The axes (from 0 to {1}) for the last digits of the timer is {2}.",
            _moduleId,
            Dimension - 1,
            _order.Join(""));
        Debug.LogFormat("[The Octadecayotton #{0}]: The starting sphere is in {1}. ({2}-ordered)",
            _moduleId,
            startingSphere.Select(a => a.Value ? "+" : "-").Join(""),
            "XYZWVURSTOPQ".Substring(0, Dimension));
    }

    private bool HandleSubmission()
    {
        if (GetLastDigitOfTimer >= Dimension || !isSubmitting)
            return false;

        _octadecayotton.PlaySound("Interact");

        if (!_inputs.Contains((Axis)GetLastDigitOfTimer))
            _inputs.Add((Axis)GetLastDigitOfTimer);

        for (int i = 0; i < Spheres.Count; i++)
        {
            Spheres[i].SphereRenderer.material.color = Color.gray;
            Spheres[i].HandleUpdate();
        }

        return false;
    }
}
