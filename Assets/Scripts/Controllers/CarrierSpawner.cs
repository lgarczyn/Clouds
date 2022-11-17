using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public struct WaveInfo
{
  public bool disabled;
  public int count;
  public float waveTimer;
  public float killAtLeast;
  public Rigidbody prefab;
}
public class CarrierSpawner : MonoBehaviour
{
  [SerializeField] float startDelay;
  [SerializeField] List<WaveInfo> waves;

  [SerializeField] float minDistance = 10;
  [SerializeField] float maxDistance = 2000;

  [SerializeField] float startingSpeed = 30f;
  [SerializeField] float spawnTimer = 1f;

  [SerializeField] UnityEvent<bool> onSpawningChange;
  [SerializeField] UnityEvent<int> onSpawn;

  [SerializeField][RequiredComponent] EnemyManagerBridge reqEnemyManagerBridge;

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;


  public void Start()
  {
    StartCoroutine(SpawnCoroutine(waves));
  }

  IEnumerator SpawnCoroutine(IEnumerable<WaveInfo> waves)
  {
    yield return new WaitForSeconds(startDelay);

    foreach (WaveInfo wave in waves)
    {
      yield return StartCoroutine(WaitStartSpawnCoroutine(wave));
      yield return StartCoroutine(SpawnWaveCoroutine(wave));
      yield return StartCoroutine(WaitEndSpawnCoroutine(wave));

    }
  }

  IEnumerator WaitStartSpawnCoroutine(WaveInfo wave)
  {
    float distance;
    do
    {
      distance = reqPlayerManagerBridge.Distance(transform.position);
      yield return new WaitForSeconds(1f);
    } while (
        distance < minDistance ||
        distance > maxDistance
    );
  }

  IEnumerator WaitEndSpawnCoroutine(WaveInfo wave)
  {
    float duration = 0f;
    while (true)
    {
      int enemyCount = FindObjectsOfType<Missile>().Count((m) => m.enabled);

      if (enemyCount == 0) yield break;

      if (duration >= wave.waveTimer && enemyCount <= (wave.count - wave.killAtLeast)) yield break;

      yield return new WaitForSeconds(1f);

      duration += 1f;
    }
  }

  IEnumerator SpawnWaveCoroutine(WaveInfo wave)
  {
    if (wave.disabled) yield break;

    onSpawningChange.Invoke(true);
    onSpawn.Invoke(wave.count);
    for (int i = 0; i < wave.count; i++)
    {
      Rigidbody missile = GameObject.Instantiate(wave.prefab, transform.position, transform.rotation);

      Transform parent = reqEnemyManagerBridge.instance.transform;
      missile.transform.SetParent(parent, true);
      missile.velocity = startingSpeed * transform.forward;

      yield return new WaitForSeconds(spawnTimer);
    }

    onSpawningChange.Invoke(false);
  }
}
