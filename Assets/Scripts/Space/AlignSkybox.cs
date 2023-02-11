using UnityEngine;

public class AlignSkybox : MonoBehaviour
{
  [SerializeField] OrbitController orbitController;
  [SerializeField] LocalSpaceController localSpaceController;
  [SerializeField] Material skyboxMaterial;
  static readonly int Rotation = Shader.PropertyToID("_Rotation");

  void LateUpdate()
  {
    QuaternionD skyRotInJSpace = orbitController.frame.toLocalRot(QuaternionD.identity);
    QuaternionD skyRotInLocalSpace = localSpaceController.frame.toLocalRot(skyRotInJSpace);
    Vector4 skyRotVector = (Vector4)skyRotInLocalSpace;

    skyboxMaterial.SetVector(Rotation, skyRotVector);
  }

  void OnDestroy()
  {
    // Set Rotation to identity
    skyboxMaterial.SetVector(Rotation, new Vector4(0, 0, 0, 1));
  }
}
