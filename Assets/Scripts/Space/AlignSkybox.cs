using UnityEngine;

public class AlignSkybox : MonoBehaviour
{
  [SerializeField] OrbitController orbitController;
  [SerializeField] LocalSpaceController localSpaceController;
  [SerializeField] Material skyboxMaterial;

  void LateUpdate()
  {
    QuaternionD skyRotInJSpace = orbitController.frame.toLocalRot(QuaternionD.identity);
    QuaternionD skyRotInLocalSpace = localSpaceController.frame.toLocalRot(skyRotInJSpace);
    Vector4 skyRotVector = (Vector4)skyRotInLocalSpace;

    skyboxMaterial.SetVector("_Rotation", skyRotVector);
  }

  void OnDestroy()
  {
    skyboxMaterial.SetVector("_Rotation", new Vector4(0, 0, 0, 1));
  }
}
