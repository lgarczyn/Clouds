public interface IResourceCalculator : IManager<ResourceCalculator>
{
  public float GetLight();
  public float GetDensity();
}

public abstract class ResourceCalculator : Manager<ResourceCalculator>, IResourceCalculator
{
  public abstract float GetLight();
  public abstract float GetDensity();
}