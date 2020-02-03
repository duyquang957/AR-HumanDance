using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TrackableType = UnityEngine.XR.ARSubsystems.TrackableType;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project.AR
{
    public class PlaceObject : MonoBehaviour
    {
        [SerializeField] ARSessionOrigin arOrigin = default;
        ARRaycastManager arRaycast = default;
        [SerializeField] Pose placementPose = default;
        bool placementPoseIsValid = false;
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        bool is_active = true;

        [SerializeField] GameObject placementIndicator = default;
        [SerializeField] GameObject prefabSpawn = default;
        [SerializeField] Animator animUI = default;
        [SerializeField] Slider sizeSlider = default;
        [SerializeField] Button btnPlace = default;
        [SerializeField] Button btnActive = default;

        private void Start()
        {
            arRaycast = arOrigin.gameObject.GetComponent<ARRaycastManager>();
            btnPlace.onClick.AddListener(OnButtonPlaceObject);
            btnActive.onClick.AddListener(OnButtonActive);
            sizeSlider.onValueChanged.AddListener(e => OnSizeSliderChange());
            OnSizeSliderChange();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Update()
        {
            UpdatePlacementPose();
        }


        private void UpdatePlacementPose()
        {
            if (!is_active) return;

            arRaycast.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f), hits, TrackableType.Planes);
            placementPoseIsValid = hits.Count > 0;
            if (placementPoseIsValid)
            {
                placementPose = hits[0].pose;
                var cameraForward = Camera.current.transform.forward;
                cameraForward.y = 0;
                placementPose.rotation = Quaternion.LookRotation(cameraForward.normalized);
            }


            if (placementPoseIsValid)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            }
            else
            {
                placementIndicator.SetActive(false);
            }
        }

        public void OnButtonPlaceObject()
        {
            if (!placementPoseIsValid) return;
            var temp = Instantiate(prefabSpawn, placementPose.position, placementPose.rotation);
            temp.transform.localScale = Vector3.one * (sizeSlider.value / 15f);
            OnButtonActive();
        }

        public void OnSizeSliderChange()
        {
            placementIndicator.transform.GetChild(0).localScale = Vector3.one * sizeSlider.value;
        }

        public void OnButtonActive()
        {
            is_active = !is_active;
            animUI.SetBool("is_active", is_active);
            arOrigin.gameObject.GetComponent<ARRaycastManager>().enabled = is_active;
            arOrigin.gameObject.GetComponent<ARPointCloudManager>().enabled = is_active;
            arOrigin.gameObject.GetComponent<ARPlaneManager>().enabled = is_active;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(PlaceObject))]
    [CanEditMultipleObjects]
    public class PlaceObjectEditor : Editor
    {
        private void OnSceneGUI()
        {
            GUILayout.Window(0, new Rect(5, 20, 0, 0), delegate (int windowID) {
                GUILayout.Box((Texture)EditorGUIUtility.Load("Assets/haircreatorbadge.png"), GUILayout.Width(400), GUILayout.Height(200));
                }, "Get It");
            if (GUI.changed) SceneView.RepaintAll();
        }
    }
#endif
}
