using UnityEngine;
using System.Collections;
using static UnityEngine.ParticleSystem;

public class TimedGameObject : BaseTimedGameObject {
    Animation aniLegacy;
    ParticleSystem psLegacy;
    Rigidbody rbRigidbody;
    Vector3 vecSavedSpeed;
    Vector3 vecSavedSpin;

    /// <summary>
    ///Awake function is used to cache the components if using SearchObjects so it is only called once.
    /// </summary>
    void Awake() {
        if (SearchObjects) {
            aniLegacy = gameObject.GetComponent<Animation>();
            psLegacy = gameObject.GetComponent<ParticleSystem>();
            rbRigidbody = gameObject.GetComponent<Rigidbody>();
        }
    }

    /// <summary>
    ///When not using SeachObjects the caches need to be cleaned.
    /// </summary>
    protected override void ClearAssignedObjects() {
        aniLegacy = null;
        psLegacy = null;
        rbRigidbody = null;
    }

    /// <summary>
    ///When not using SeachObject this function loops through the object array and sets the speed for the objects.
    /// </summary>
    protected override void SetSpeedLooping(float _fNewSpeed, float _fCurrentSpeedPercent, float _fCurrentSpeedZeroToOne) {
        for (int thisCounter = 0; thisCounter < AssignedObjects.Length; thisCounter++) {
            if (AssignedObjects[thisCounter].GetType() == typeof(Animation)) {
                aniLegacy = (Animation)AssignedObjects[thisCounter];
            } else if (AssignedObjects[thisCounter].GetType() == typeof(ParticleSystem)) {
                psLegacy = (ParticleSystem)AssignedObjects[thisCounter];
            } else if (AssignedObjects[thisCounter].GetType() == typeof(Rigidbody)) {
                rbRigidbody = (Rigidbody)AssignedObjects[thisCounter];
            }
            SetSpeedAssigned(_fNewSpeed, _fCurrentSpeedPercent, _fCurrentSpeedZeroToOne);
        }
        ClearAssignedObjects();
    }

    /// <summary>
    ///This function sets the new speed of the objects.
    /// </summary>
    protected override void SetSpeedAssigned(float _fNewSpeed, float _fCurrentSpeedPercent, float _fCurrentSpeedZeroToOne) {
        if (aniLegacy != null) {
            foreach (AnimationState thisAnimationState in aniLegacy) {
                aniLegacy[thisAnimationState.name].speed = _fCurrentSpeedZeroToOne;
            }
        }
        if (psLegacy != null) {
            if (_fNewSpeed == 0f) {
                psLegacy.Pause();
            } else {
                psLegacy.Play();
                ParticleSystem.Particle[] parParticles = new Particle[psLegacy.main.maxParticles];
                int iNumParticles = psLegacy.GetParticles(parParticles);
                for (int i = 0; i < iNumParticles; i++) {
                    parParticles[i].velocity = new Vector3(GetNewSpeedFromPercentage(parParticles[i].velocity.x), GetNewSpeedFromPercentage(parParticles[i].velocity.y), GetNewSpeedFromPercentage(parParticles[i].velocity.z));
                }
                psLegacy.SetParticles(parParticles);
            }
        }
        if (rbRigidbody != null) {
            if (_fNewSpeed == 0f) {
                vecSavedSpeed = rbRigidbody.velocity;
                vecSavedSpin = rbRigidbody.angularVelocity;
                rbRigidbody.isKinematic = true;
            } else if (_fNewSpeed != 100f) {
                rbRigidbody.isKinematic = false;
                rbRigidbody.velocity = vecSavedSpeed;
                rbRigidbody.angularVelocity = vecSavedSpin;
            } else {
                rbRigidbody.isKinematic = false;
                rbRigidbody.velocity = vecSavedSpeed;
                rbRigidbody.angularVelocity = vecSavedSpin;
            }
        }
    }
}