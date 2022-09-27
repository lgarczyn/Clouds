using UnityEngine;

/**
 * The class responsible for generating the 3D noise used in the cloud shader
 * It is also capable of saving and loading noise textures, in collaboration with NoiseGenEditor
 * Since noise will have to be generated at runtime, there's little point to saving and loading it
 * The duplication of code for detail and shape is also less than ideal
 * The same instance should handle only one texture, and collaborate with multiple others
 * Linking should not be done using public variable, but using shared RenderTextures
 */
public class NoiseGenerator : MonoBehaviour {

    const int computeThreadGroupSize = 8;
    public const string detailNoiseName = "DetailNoise";
    public const string shapeNoiseName = "ShapeNoise";

    public enum CloudNoiseType { Shape, Detail }
    public enum TextureChannel { R, G, B, A }

    [Header ("Editor Settings")]
    public CloudNoiseType activeTextureType;
    public TextureChannel activeChannel;
    public bool autoUpdate;
    public bool logComputeTime;

    [Header ("Noise Settings")]
    public int shapeResolution = 132;
    public int detailResolution = 32;

    public Vector4 shapeWeights;
    public WorleyNoiseSettings[] shapeSettings;
    public Vector4 detailWeights;
    public WorleyNoiseSettings[] detailSettings;
    public ComputeShader noiseCompute;
    public ComputeShader copy;

    public ComputeShader flatten;

    // Internal
    List<ComputeBuffer> buffersToRelease;
    bool updateNoise;

    [HideInInspector]
    public bool showSettingsEditor = true;
    [SerializeField, HideInInspector]
    public RenderTexture shapeTexture;
    [SerializeField, HideInInspector]
    public RenderTexture detailTexture;
    [SerializeField]
    public RenderTexture shapeTextureFlat;
    [SerializeField]
    public RenderTexture detailTextureFlat;

    public void UpdateNoise () {
        ValidateParamaters ();
        CreateTexture (ref shapeTexture, shapeResolution, shapeNoiseName);
        CreateTexture (ref detailTexture, detailResolution, detailNoiseName);
        CreateFlatTexture (ref shapeTextureFlat, shapeResolution, shapeNoiseName, this.shapeWeights);
        CreateFlatTexture (ref detailTextureFlat, detailResolution, detailNoiseName, this.detailWeights);

        if (updateNoise && noiseCompute) {
            var timer = System.Diagnostics.Stopwatch.StartNew ();

            updateNoise = false;
            WorleyNoiseSettings activeSettings = ActiveSettings;
            if (activeSettings == null) {
                return;
            }

            buffersToRelease = new List<ComputeBuffer> ();

            int activeTextureResolution = ActiveTexture.width;

            // Set values:
            noiseCompute.SetFloat ("persistence", activeSettings.persistence);
            noiseCompute.SetInt ("resolution", activeTextureResolution);
            noiseCompute.SetVector ("channelMask", ChannelMask);

            // Set noise gen kernel data:
            noiseCompute.SetTexture (0, "Result", ActiveTexture);
            var minMaxBuffer = CreateBuffer (new int[] { int.MaxValue, 0 }, sizeof (int), "minMax", 0);
            UpdateWorley (ActiveSettings);
            noiseCompute.SetTexture (0, "Result", ActiveTexture);
            //var noiseValuesBuffer = CreateBuffer (activeNoiseValues, sizeof (float) * 4, "values");

            // Dispatch noise gen kernel
            int numThreadGroups = Mathf.CeilToInt (activeTextureResolution / (float) computeThreadGroupSize);
            noiseCompute.Dispatch (0, numThreadGroups, numThreadGroups, numThreadGroups);

            // Set normalization kernel data:
            noiseCompute.SetBuffer (1, "minMax", minMaxBuffer);
            noiseCompute.SetTexture (1, "Result", ActiveTexture);
            // Dispatch normalization kernel
            noiseCompute.Dispatch (1, numThreadGroups, numThreadGroups, numThreadGroups);

            if (logComputeTime) {
                // Get minmax data just to force main thread to wait until compute shaders are finished.
                // This allows us to measure the execution time.
                var minMax = new int[2];
                minMaxBuffer.GetData (minMax);

                Debug.Log ($"Noise Generation: {timer.ElapsedMilliseconds}ms");
            }

            // Release buffers
            foreach (var buffer in buffersToRelease) {
                buffer.Release ();
            }

            FlattenNoise();
        }
    }

    public void FlattenNoise() {
        flatten.SetTexture (0, "texIn", ActiveTexture);
        flatten.SetTexture (0, "texOut", ActiveFlatTexture);
        flatten.SetVector("weights", ActiveWeights);
        int numThreadGroups = Mathf.CeilToInt (ActiveTexture.width / 8f);
        flatten.Dispatch (0, numThreadGroups, numThreadGroups, numThreadGroups);
    }

    public void LoadAll() {
        Load (NoiseGenerator.shapeNoiseName, shapeTexture);
        Load (NoiseGenerator.detailNoiseName, detailTexture);
        LoadFlat (NoiseGenerator.shapeNoiseName, shapeTextureFlat, shapeWeights);
        LoadFlat (NoiseGenerator.detailNoiseName, detailTextureFlat, detailWeights);
    }

    public void Load (string saveName, RenderTexture target) {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name;
        saveName = sceneName + "_" + saveName;
        Texture3D savedTex = (Texture3D) Resources.Load (saveName);
        if (savedTex != null && savedTex.width == target.width) {
            copy.SetTexture (0, "tex", savedTex);
            copy.SetTexture (0, "renderTex", target);
            int numThreadGroups = Mathf.CeilToInt (savedTex.width / 8f);
            copy.Dispatch (0, numThreadGroups, numThreadGroups, numThreadGroups);
        }
    }

    public void LoadFlat (string saveName, RenderTexture target, Vector4 weights) {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name;
        saveName = sceneName + "_" + saveName;
        Texture3D savedTex = (Texture3D) Resources.Load (saveName);
        if (savedTex != null && savedTex.width == target.width) {
            flatten.SetTexture(0, "texIn", savedTex);
            flatten.SetTexture(0, "texOut", target);
            // Read real hint
            flatten.SetVector("weights", weights);
            int numThreadGroups = Mathf.CeilToInt (savedTex.width / 8f);
            flatten.Dispatch (0, numThreadGroups, numThreadGroups, numThreadGroups);
        }
    }

    public RenderTexture ActiveTexture {
        get {
            return (activeTextureType == CloudNoiseType.Shape) ? shapeTexture : detailTexture;
        }
    }
    public RenderTexture ActiveFlatTexture {
        get {
            return (activeTextureType == CloudNoiseType.Shape) ? shapeTextureFlat : detailTextureFlat;
        }
    }
    public Vector3 ActiveWeights {
        get {
            return (activeTextureType == CloudNoiseType.Shape) ? shapeWeights : detailWeights;
        }
    }

    public WorleyNoiseSettings ActiveSettings {
        get {
            WorleyNoiseSettings[] settings = (activeTextureType == CloudNoiseType.Shape) ? shapeSettings : detailSettings;
            int activeChannelIndex = (int) activeChannel;
            if (activeChannelIndex >= settings.Length) {
                return null;
            }
            return settings[activeChannelIndex];
        }
    }

    public Vector4 ChannelMask {
        get {
            Vector4 channelWeight = new Vector4 (
                (activeChannel == NoiseGenerator.TextureChannel.R) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.G) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.B) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.A) ? 1 : 0
            );
            return channelWeight;
        }
    }

    void UpdateWorley (WorleyNoiseSettings settings) {
        var prng = new System.Random (settings.seed);
        CreateWorleyPointsBuffer (prng, settings.numDivisionsA, "pointsA");
        CreateWorleyPointsBuffer (prng, settings.numDivisionsB, "pointsB");
        CreateWorleyPointsBuffer (prng, settings.numDivisionsC, "pointsC");

        noiseCompute.SetInt ("numCellsA", settings.numDivisionsA);
        noiseCompute.SetInt ("numCellsB", settings.numDivisionsB);
        noiseCompute.SetInt ("numCellsC", settings.numDivisionsC);
        noiseCompute.SetBool ("invertNoise", settings.invert);
        noiseCompute.SetInt ("tile", settings.tile);

    }

    void CreateWorleyPointsBuffer (System.Random prng, int numCellsPerAxis, string bufferName) {
        var points = new Vector3[numCellsPerAxis * numCellsPerAxis * numCellsPerAxis];
        float cellSize = 1f / numCellsPerAxis;

        for (int x = 0; x < numCellsPerAxis; x++) {
            for (int y = 0; y < numCellsPerAxis; y++) {
                for (int z = 0; z < numCellsPerAxis; z++) {
                    float randomX = (float) prng.NextDouble ();
                    float randomY = (float) prng.NextDouble ();
                    float randomZ = (float) prng.NextDouble ();
                    Vector3 randomOffset = new Vector3 (randomX, randomY, randomZ) * cellSize;
                    Vector3 cellCorner = new Vector3 (x, y, z) * cellSize;

                    int index = x + numCellsPerAxis * (y + z * numCellsPerAxis);
                    points[index] = cellCorner + randomOffset;
                }
            }
        }

        CreateBuffer (points, sizeof (float) * 3, bufferName);
    }

    // Create buffer with some data, and set in shader. Also add to list of buffers to be released
    ComputeBuffer CreateBuffer (System.Array data, int stride, string bufferName, int kernel = 0) {
        var buffer = new ComputeBuffer (data.Length, stride, ComputeBufferType.Structured);
        buffersToRelease.Add (buffer);
        buffer.SetData (data);
        noiseCompute.SetBuffer (kernel, bufferName, buffer);
        return buffer;
    }

    void CreateTexture (ref RenderTexture texture, int resolution, string name) {

        RenderTextureDescriptor desc = TextureTools.GetDescriptorNoise3D_RGBA(resolution);

        TextureTools.VerifyTexture(ref texture, desc, name);

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Trilinear;

        Load(name, texture);
    }

    void CreateFlatTexture (ref RenderTexture texture, int resolution, string name, Vector4 weights) {

        RenderTextureDescriptor desc = TextureTools.GetDescriptorNoise3D_R(resolution);

        TextureTools.VerifyTexture(ref texture, desc, name);

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Trilinear;

        LoadFlat(name, texture, weights);
    }

    public void ManualUpdate () {
        updateNoise = true;
        UpdateNoise ();
    }

    void OnValidate () { }

    public void ActiveNoiseSettingsChanged () {
        if (autoUpdate) {
            updateNoise = true;
        }
    }

    void ValidateParamaters () {
        detailResolution = Mathf.Max (1, detailResolution);
        shapeResolution = Mathf.Max (1, shapeResolution);
    }
}