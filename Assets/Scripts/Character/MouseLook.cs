using System;
using UnityEngine;
using Luminosity.IO;

namespace UnityStandardAssets.Characters.FirstPerson {
    [Serializable]
    public class MouseLook {
        public float fXSensitivity = 2f;
        public float fYSensitivity = 2f;
        public bool bClampVerticalRotation = true;
        public float fMinimumX = -90F;
        public float fMaximumX = 90F;
        public bool bSmooth;
        public float fSmoothTime = 5f;
        public bool bLockCursor = true;


        private Quaternion qCharacterTargetRot;
        private Quaternion qCameraTargetRot;
        private bool bCursorIsLocked = true;

        public void Init(Transform character, Transform camera) {
            qCharacterTargetRot = character.localRotation;
            qCameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform character, Transform camera) {
            float fYRot = InputManager.GetAxis("LookHorizontal") * fXSensitivity;
            float fXRot = InputManager.GetAxis("LookVertical") * fYSensitivity;

            qCharacterTargetRot *= Quaternion.Euler(0f, fYRot, 0f);
            qCameraTargetRot *= Quaternion.Euler(-fXRot, 0f, 0f);

            if (bClampVerticalRotation) {
                qCameraTargetRot = ClampRotationAroundXAxis(qCameraTargetRot);
            }

            if (bSmooth) {
                character.localRotation = Quaternion.Slerp(character.localRotation, qCharacterTargetRot,
                    fSmoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, qCameraTargetRot,
                    fSmoothTime * Time.deltaTime);
            } else {
                character.localRotation = qCharacterTargetRot;
                camera.localRotation = qCameraTargetRot;
            }
            UpdateCursorLock();
        }

        public void SetCursorLock(bool value) {
            bLockCursor = value;
            if (!bLockCursor) {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock() {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (bLockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate() {
            if (InputManager.GetKeyUp(KeyCode.Escape)) {
                bCursorIsLocked = false;
            } else if (InputManager.GetMouseButtonUp(0)) {
                bCursorIsLocked = true;
            }

            if (bCursorIsLocked) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else if (!bCursorIsLocked) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        Quaternion ClampRotationAroundXAxis(Quaternion q) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, fMinimumX, fMaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
