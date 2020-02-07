using UnityEngine;
using System.Collections;

public class TimeGroupController : MonoBehaviour {
    #region Events
    public delegate void OnGroupPauseEvent(BaseTimedClass _TimedClass);
    public event OnGroupPauseEvent OnGroupPause;
    public delegate void OnGroupSlowDownEvent(BaseTimedClass _TimedClass);
    public event OnGroupSlowDownEvent OnGroupSlowDown;
    public delegate void OnGroupSpeedUpEvent(BaseTimedClass _TimedClass);
    public event OnGroupSpeedUpEvent OnGroupSpeedUp;
    public delegate void OnGroupResumeEvent(BaseTimedClass _TimedClass);
    public event OnGroupResumeEvent OnGroupResume;
    #endregion

    public float ControllerSpeedPercent = 100f;
    public float ControllerSpeedZeroToOne = 1f;
    public int GroupID = 1;

    void BroadcastEvents(string _EventName) {
        BaseTimedGameObject[] thisGameObject = GameObject.FindObjectsOfType(typeof(BaseTimedGameObject)) as BaseTimedGameObject[];

        for (int iCount = 0; iCount < thisGameObject.Length; ++iCount) {
            thisGameObject[iCount].SendMessage(_EventName, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    ///This is what is used to Broadcast messages to the TimeGameObjects using only the event name and also passes the SWP_TimedClass too which contains additional information.
    /// </summary>
    void BroadcastEvents(string _EventName, object _PassedObject) {
        BaseTimedGameObject[] thisGameObject = GameObject.FindObjectsOfType(typeof(BaseTimedGameObject)) as BaseTimedGameObject[];

        for (int iCount = 0; iCount < thisGameObject.Length; ++iCount) {
            thisGameObject[iCount].SendMessage(_EventName, _PassedObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    ///This method is used where you want to time control DeltaTime.  Just replace your Time.DeltaTime calls with a call to this function.
    /// </summary>
    public float TimedDeltaTime() {
        if (ControllerSpeedPercent != 0) {
            return Time.deltaTime / (100f / ControllerSpeedPercent);
        } else {
            return 0f;
        }
    }

    /// <summary>
    ///This sets the two controller speeds (Percentage speed and ZeroToOne speed)
    /// </summary>
    void SetControllerSpeed(float _NewSpeed) {
        ControllerSpeedPercent = _NewSpeed;
        ControllerSpeedZeroToOne = (ControllerSpeedPercent == 0f ? 0f : ControllerSpeedPercent / 100f);
    }

    /// <summary>
    ///The main PauseTime control method.
    /// </summary>
    public void PauseGroupTime() {
        SetControllerSpeed(0f);

        BaseTimedClass thisTimedClass = new BaseTimedClass(GroupID, ControllerSpeedPercent);

        BroadcastEvents("OnGroupPauseBroadcast", thisTimedClass);

        if (OnGroupPause != null)
            OnGroupPause(thisTimedClass);
    }

    /// <summary>
    ///The main SlowDownTime control method pass a percentage speed where 100% is normal speed.
    /// </summary>
    public void SlowDownGroupTime(float _NewTime) {
        SetControllerSpeed(_NewTime);

        BaseTimedClass thisTimedClass = new BaseTimedClass(GroupID, ControllerSpeedPercent);

        BroadcastEvents("OnGroupSlowDownBroadcast", thisTimedClass);

        if (OnGroupSlowDown != null)
            OnGroupSlowDown(thisTimedClass);
    }

    /// <summary>
    ///The main SpeedUpTime control method pass a percentage speed where 100% is normal speed.
    /// </summary>
    public void SpeedUpGroupTime(float _NewTime) {
        SetControllerSpeed(_NewTime);

        BaseTimedClass thisTimedClass = new BaseTimedClass(GroupID, ControllerSpeedPercent);

        BroadcastEvents("OnGroupSpeedUpBroadcast", thisTimedClass);

        if (OnGroupSpeedUp != null)
            OnGroupSpeedUp(thisTimedClass);
    }

    /// <summary>
    ///The main return to normal time control method.
    /// </summary>
    public void ResumeGroupTime() {
        SetControllerSpeed(100f);

        BaseTimedClass thisTimedClass = new BaseTimedClass(GroupID, ControllerSpeedPercent);

        BroadcastEvents("OnGroupResumeBroadcast", thisTimedClass);

        if (OnGroupResume != null)
            OnGroupResume(thisTimedClass);
    }
}