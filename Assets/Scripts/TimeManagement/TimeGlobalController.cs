using UnityEngine;
using System.Collections;

public class TimeGlobalController : MonoBehaviour {
    #region Events
    public delegate void OnGlobalPauseEvent();
    public event OnGlobalPauseEvent OnGlobalPause;
    public delegate void OnGlobalResumeEvent();
    public event OnGlobalResumeEvent OnGlobalResume;
    #endregion

    static public bool IsPaused = false;
    static public float GlobalTimeSinceLevelLoad = 0f;
    static public float TimeScaleModifier = 1000000f;

    void Update() {
        if (!TimeGlobalController.IsPaused) {
            TimeGlobalController.GlobalTimeSinceLevelLoad += Time.deltaTime;
        } else {
            TimeGlobalController.GlobalTimeSinceLevelLoad += Time.deltaTime * TimeScaleModifier;
        }
    }

    void BroadcastEvents(string _EventName) {
        BaseTimedGameObject[] thisGameObject = GameObject.FindObjectsOfType(typeof(BaseTimedGameObject)) as BaseTimedGameObject[];

        for (int thisCount = 0; thisCount < thisGameObject.Length; ++thisCount) {
            thisGameObject[thisCount].SendMessage(_EventName, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void PauseGlobalTime() {
        if (TimeGlobalController.IsPaused) {
            return;
        }
        BroadcastEvents("OnGlobalPauseBroadcast");
        //if (OnGlobalPause != null) {
        //    OnGlobalPause();
        //}
        OnGlobalPause?.Invoke();
        TimeGlobalController.IsPaused = true;
        Time.timeScale /= TimeGlobalController.TimeScaleModifier;

    }

    public void ResumeGlobalTime() {
        if (!TimeGlobalController.IsPaused) {
            return;
        }
        Time.timeScale *= TimeGlobalController.TimeScaleModifier;
        TimeGlobalController.IsPaused = false;
        BroadcastEvents("OnGlobalResumeBroadcast");
        //if (OnGlobalResume != null) {
        //    OnGlobalResume();
        //}
        OnGlobalResume?.Invoke();
    }
}