using System.Diagnostics;
using UnityEngine;

[System.Serializable]
public class PID {
	public float pFactor, iFactor, dFactor;
		
	float integral = 0;
	float lastError = 0;
	
	
	public PID(float pFactor, float iFactor, float dFactor) {
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}
	
	
	public float Update(float setpoint, float actual, float timeFrame) {
		float present = setpoint - actual;
		integral += present * timeFrame;
		float deriv = (present - lastError) / timeFrame;
		UnityEngine.Debug.Log(deriv);
		lastError = present;
		return Mathf.Clamp(present * pFactor + integral * iFactor + deriv * dFactor, -1f, 1f);
	}
}
