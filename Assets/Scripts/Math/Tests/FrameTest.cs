using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FrameTest
{
  void TestCoordEqual(TransformD point, TransformD expected, string message = "Fail", double epsilon = 0.00000000000001)
  {
    bool isEqual = point.Approximately(expected, epsilon);
    if (isEqual) return;
    Debug.Log("Diff");

    var pointStr = point.ToString("F100");
    var expectedStr = point.ToString("F100");

    if (pointStr == expectedStr)
    {
      Debug.Log("Diff3");
      Assert.Fail("Transformed point not equal but same string representation: " + point);
    }
    else
    {
      Debug.Log("Diff2");
      Assert.AreEqual(pointStr, expectedStr);
    }
  }

  [Test]
  public void TestEquality()
  {
    Debug.Log("Testing equality");
    for (int i = 0; i < 1000; i++)
    {
      TransformD a = new TransformD(new Vector3D(0, 0, 0), QuaternionD.Euler(45, 0, 0), 1);
      TransformD b = new TransformD(new Vector3D(0, 0.00000001, 0), QuaternionD.Euler(45, 0.000000001, 0), 1);
      TransformD c = new TransformD(new Vector3D(0, 0.000000000000000000000001, 0), QuaternionD.Euler(45, 0.00000000000000000000001, 0), 1);

      TestCoordEqual(a, a, "Same struct should be equal to itself");
      Assert.AreNotEqual(a, b, "Struct should not be equal to slightly different struct");
      TestCoordEqual(a, c, "Struct should be equal considering less than epsilon differences");
    }
  }

  [Test]
  public void TestReversible()
  {
    var testPos = new Vector3D(10, 2, 30000);
    var testRot = QuaternionD.Euler(45, 45, 45);
    var testCoord = new TransformD(testPos, testRot, 2);
    // Reversible offset
    Debug.Log("Testing reversible offset");
    var offsetFrame = new FrameOfReference(new TransformD(new Vector3D(10, 30, 1000), QuaternionD.identity, 1));
    TestFrameReversible(offsetFrame, testCoord);
    // Reversible rotation
    Debug.Log("Testing reversible rotation");
    var rotationFrame = new FrameOfReference(new TransformD(Vector3D.zero, QuaternionD.Euler(36f, 96f, 105f), 1));
    TestFrameReversible(rotationFrame, testCoord);
    // Reversible scale
    Debug.Log("Testing reversible scale");
    var scaledFrame = new FrameOfReference(new TransformD(Vector3D.zero, QuaternionD.identity, 10));
    TestFrameReversible(scaledFrame, testCoord);
    // Reversible combined
    Debug.Log("Testing reversible combined");
    var frame = new FrameOfReference(new TransformD(new Vector3D(10, 30, 1000), QuaternionD.Euler(36f, 96f, 105f), 10));
    TestFrameReversible(frame, testCoord);
  }
  void TestFrameReversible(FrameOfReference frame, TransformD point)
  {
    var toLocalAndBack = frame.toLocal(frame.toGlobalCoord(point));
    Assert.IsTrue(point.Approximately(toLocalAndBack), "Frame" + frame + " toLocal not reversible: " + point + "!=" + toLocalAndBack);
    var toGlobalAndBack = frame.toGlobalCoord(frame.toLocal(point));
    Assert.IsTrue(point.Approximately(toGlobalAndBack), "Frame" + frame + " toGlobal not reversible: " + point + "!=" + toGlobalAndBack);
  }

  [Test]
  public void TestCorrect()
  {
    var testPos = new Vector3D(10, 2, 30000);
    var testRot = QuaternionD.Euler(45, 45, 45);
    var testCoord = new TransformD(testPos, testRot, 2);
    // Correct scale
    Debug.Log("Testing correct scale");
    var appliedScale = 10;
    var scaledFrame = new FrameOfReference(new TransformD(Vector3D.zero, QuaternionD.identity, appliedScale));
    var scaledCoord = scaledFrame.toLocal(testCoord);
    var expectedScaledCoord = new TransformD(
      testCoord.position / appliedScale,
      testCoord.rotation,
      testCoord.scale / appliedScale);
    TestCoordEqual(scaledCoord, expectedScaledCoord, "Scale Fail: ");
    // Correct offset
    Debug.Log("Testing correct offset");
    var appliedOffset = new Vector3D(10, 30, 1000);
    var offsetFrame = new FrameOfReference(new TransformD(appliedOffset, QuaternionD.identity, 1));
    var offsetCoord = offsetFrame.toLocal(testCoord);
    var expectedOffsetCoord = new TransformD(
      testCoord.position - appliedOffset,
      testCoord.rotation,
      testCoord.scale);
    TestCoordEqual(offsetCoord, expectedOffsetCoord, "Offset Fail: ");
    // Correct rotation
    Debug.Log("Testing correct rotation");
    var appliedRotation = QuaternionD.Euler(36f, 96f, 105f);
    var rotationFrame = new FrameOfReference(new TransformD(Vector3D.zero, appliedRotation, 1));
    var rotationCoord = rotationFrame.toLocal(testCoord);
    var expectedRotationCoord = new TransformD(
      QuaternionD.Inverse(appliedRotation) * testCoord.position,
      QuaternionD.Inverse(appliedRotation) * testCoord.rotation,
      testCoord.scale);
    TestCoordEqual(rotationCoord, expectedRotationCoord, "Rotation Fail: ");
  }


  [Test]
  public void TestSoundness()
  {
    TestSoundnessLocalToGlobal();
    TestSoundnessGlobalToLocal();
  }

  void TestSoundnessLocalToGlobal()
  {
    Debug.Log("Comparing results with Unity for local to global");
    var parent = new GameObject();
    var child = new GameObject();

    child.transform.parent = parent.transform;

    var parentPos = new Vector3D(9900, 543.002, 0.0001);
    var parentRot = QuaternionD.Euler(24.2, 45.1, -70);
    var parentScale = -10;
    var parentCoord = new TransformD(parentPos, parentRot, parentScale);

    parent.transform.localPosition = (Vector3)parentPos;
    parent.transform.localRotation = (Quaternion)parentRot;
    parent.transform.localScale = Vector3.one * parentScale;

    var childPos = new Vector3D(10, 30, 1000);
    var childRot = QuaternionD.Euler(50, 10, 30);
    var childScale = 10;
    var childCoord = new TransformD(childPos, childRot, childScale);

    child.transform.localPosition = (Vector3)childPos;
    child.transform.localRotation = (Quaternion)childRot;
    child.transform.localScale = Vector3.one * childScale;

    var childInGlobalFrame = new FrameOfReference(parentCoord).toGlobalCoord(childCoord);

    var childInGlobalFrameUnity = new TransformD(
      child.transform.position,
      child.transform.rotation,
      (child.transform.lossyScale.x + child.transform.lossyScale.y + child.transform.lossyScale.z) / 3
    );

    TestCoordEqual(childInGlobalFrame, childInGlobalFrameUnity, "To Global Fail:", 0.0001f);
  }
  void TestSoundnessGlobalToLocal()
  {
    Debug.Log("Comparing results with Unity for global to local");
    var parent = new GameObject();
    var child = new GameObject();

    child.transform.parent = parent.transform;

    var parentPos = new Vector3D(9900, 543.002, 0.0001);
    var parentRot = QuaternionD.Euler(24.2, 45.1, -70);
    var parentScale = -10;
    var parentCoord = new TransformD(parentPos, parentRot, parentScale);

    parent.transform.localPosition = (Vector3)parentPos;
    parent.transform.localRotation = (Quaternion)parentRot;
    parent.transform.localScale = Vector3.one * parentScale;

    var childPos = new Vector3D(10, 30, 1000);
    var childRot = QuaternionD.Euler(50, 10, 30);
    var childScale = 10;
    var childCoord = new TransformD(childPos, childRot, childScale);

    child.transform.position = (Vector3)childPos;
    child.transform.rotation = (Quaternion)childRot;
    // Since localScale cannot be set, simulate it
    child.transform.localScale = Vector3.one * (childScale / parentScale);

    var childInLocalFrame = new FrameOfReference(parentCoord).toLocal(childCoord);

    var childInLocalFrameUnity = new TransformD(
      child.transform.localPosition,
      child.transform.localRotation,
      (child.transform.localScale.x + child.transform.localScale.y + child.transform.localScale.z) / 3
    );

    TestCoordEqual(childInLocalFrame, childInLocalFrameUnity, "To Local Fail:", 0.000001f);
  }
}
