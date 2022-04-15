using UnityEngine;

[RequireComponent(typeof(Skybox))]
public class AlignSkybox : MonoBehaviour
{
  public OrbitController orbitController;
  public LocalSpaceController localSpaceController;

  private Material material;

  void Start()
  {
    // Clone the original material, so that modifications are not stored
    this.material = new Material(this.GetComponent<Skybox>().material);
    this.GetComponent<Skybox>().material = this.material;
  }

  void Update()
  {
    QuaternionD skyRotInJSpace = orbitController.frame.toLocalRot(QuaternionD.identity);
    QuaternionD skyRotInLocalSpace = localSpaceController.frame.toLocalRot(skyRotInJSpace);
    Vector4 skyRotVector = (Vector4)skyRotInLocalSpace;

    material.SetVector("_Rotation", skyRotVector);
  }
}
