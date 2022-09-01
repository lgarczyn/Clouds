using UnityEngine;

public abstract class PIDController<T>
{
  private T y0;              // Current output
  private T y1;              // Output one iteration old
  private T y2;              // Output two iterations old
  private T e0;              // Current error
  private T e1;              // Error one iteration old
  private T e2;              // Error two iterations old

  /// <summary>
  /// PID Constructor
  /// </summary>
  /// <param name="Kp">Proportional Gain</param>
  /// <param name="Ki">Integral Gain</param>
  /// <param name="Kd">Derivative Gain</param>
  /// <param name="N">Derivative Filter Coefficient</param>
  /// <param name="OutputUpperLimit">Controller Upper Output Limit</param>
  /// <param name="OutputLowerLimit">Controller Lower Output Limit</param>
  public PIDController(float Kp = 0.3f, float Ki = 0.3f, float Kd = 0.3f, float N = 1f, float OutputUpperLimit = 1f)
  {
    this.ResetController();
    this.Kp = Kp;
    this.Ki = Ki;
    this.Kd = Kd;
    this.N = N;
    this.OutputUpperLimit = OutputUpperLimit;
  }

  /// <summary>
  /// PID iterator, call this function every sample period to get the current controller output.
  /// setpoint and processValue should use the same units.
  /// </summary>
  /// <param name="setPoint">Current Desired Setpoint</param>
  /// <param name="processValue">Current Process Value</param>
  /// <param name="deltaTime">Timespan Since Last Iteration, Use Default Sample Period for First Call</param>
  /// <returns>Current Controller Output</returns>
  public T Iterate(T setPoint, T processValue, float deltaTime)
  {
    // Ensure the timespan is not too small or zero.
    float Ts = (deltaTime >= TsMin) ? deltaTime : TsMin;

    // Calculate rollup parameters
    float k = 2 / Ts;
    float kS = k * k;
    float b0 = kS * Kp + k * Ki + Ki * N + k * Kp * N + kS * Kd * N;
    float b1 = 2 * Ki * N - 2 * kS * Kp - 2 * kS * Kd * N;
    float b2 = kS * Kp - k * Ki + Ki * N - k * Kp * N + kS * Kd * N;
    float a0 = kS + N * k;
    float a1 = -2 * kS;
    float a2 = kS - k * N;

    // Age errors and output history
    e2 = e1;                        // Age errors one iteration
    e1 = e0;                        // Age errors one iteration
    e0 = Add(setPoint, Scale(processValue, -1));// Compute new error
    y2 = y1;                        // Age outputs one iteration
    y1 = y0;                        // Age outputs one iteration

    T value = Scale(e0, b0);
    value = Add(value, Scale(e1, b1));
    value = Add(value, Scale(e2, b2));
    value = Add(value, Scale(y1, -a1));
    value = Add(value, Scale(y2, -a2));
    value = Scale(value, 1 / a0);

    y0 = value;
    return ClampMagnitude(value, OutputUpperLimit);
  }

  // Protected vars allowing generic interpretations of the PID

  protected abstract T Add(T a, T b);
  protected abstract T Scale(T a, float b);
  protected abstract T Zero();
  protected abstract T ClampMagnitude(T a, float max);

  /// <summary>
  /// Reset controller history effectively resetting the controller.
  /// </summary>
  public void ResetController()
  {
    e2 = Zero();
    e1 = Zero();
    e0 = Zero();
    y2 = Zero();
    y1 = Zero();
    y0 = Zero();
  }

  /// <summary>
  /// Proportional Gain, consider resetting controller if this parameter is drastically changed.
  /// </summary>
  public float Kp;

  /// <summary>
  /// Integral Gain, consider resetting controller if this parameter is drastically changed.
  /// </summary>
  public float Ki;

  /// <summary>
  /// Derivative Gain, consider resetting controller if this parameter is drastically changed.
  /// </summary>
  public float Kd;

  /// <summary>
  /// Derivative filter coefficient.
  /// A smaller N for more filtering.
  /// A larger N for less filtering.
  /// Consider resetting controller if this parameter is drastically changed.
  /// </summary>
  public float N;

  /// <summary>
  /// Minimum allowed sample period to avoid dividing by zero!
  /// The Ts value can be mistakenly set to too low of a value or zero on the first iteration.
  /// TsMin by default is set to 1 millisecond.
  /// </summary>
  public float TsMin;

  /// <summary>
  /// Upper output limit of the controller.
  /// This should obviously be a numerically greater value than the lower output limit.
  /// </summary>
  public float OutputUpperLimit;
}