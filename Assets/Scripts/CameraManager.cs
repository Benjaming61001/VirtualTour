using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCameraFPS;
    [SerializeField] CinemachineVirtualCamera virtualCameraTPS;
    [SerializeField] CinemachineVirtualCamera virtualCameraBEV;

    CinemachineComponentBase componentBase;
    PlayerInput playerInput;

    [HideInInspector] public bool isFPS = true;
    [HideInInspector] public bool isTPS = false;
    [HideInInspector] public bool isBEV = false;

    float cameraDistance;

    [Header("BEV")]
    [SerializeField] float bev_MinDistance = 1f;
    [SerializeField] float bev_MaxDistance = 25f;
    [SerializeField] float bev_Sensitivity = 10f;

    [Header("TPS")]
    [SerializeField] float tps_MinDistance = 1f;
    [SerializeField] float tps_MaxDistance = 5f;
    [SerializeField] float tps_Sensitivity = 3f;

    [Header("Game Component")]
    public DialogueManager dialogueManager;
    public DOFcontroller dofController;

    private void Awake()
    {
        playerInput = new PlayerInput{};

        playerInput.Player.CameraSwap.started += switchCamera;
    }

    void switchCamera (InputAction.CallbackContext context)
    {
        // If dialogue is present or Focusing on object then Ignore switchCamera
        if (dialogueManager.onDialogue || dofController.isFocusingObj) return;
    
        // FPS >>> TPS
        if (CameraSwitcher.IsActiveCamera(virtualCameraFPS))
        {
            CameraSwitcher.SwitchCamera(virtualCameraTPS);
            isTPS = true;

            isFPS = false;
            isBEV = false;
        }
        //TPS >>> BEV
        else if (CameraSwitcher.IsActiveCamera(virtualCameraTPS))
        {
            CameraSwitcher.SwitchCamera(virtualCameraBEV);
            isBEV = true;
            
            isFPS = false;
            isTPS = false;
        }
        //BEV >>> FPS
        else if (CameraSwitcher.IsActiveCamera(virtualCameraBEV))
        {
            CameraSwitcher.SwitchCamera(virtualCameraFPS);
            isFPS = true;

            isTPS = false;
            isBEV = false;
        }
        
    }

    private void Update()
    {
        CameraDistance();
    }

    void CameraDistance()
    {
        if (isFPS) return;

        if (isTPS)
        {
            componentBase = virtualCameraTPS.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }
        if (isBEV)
        {
            componentBase = virtualCameraBEV.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //Third Person
            cameraDistance = Input.GetAxis("Mouse ScrollWheel") * tps_Sensitivity;

            if (componentBase is Cinemachine3rdPersonFollow)
            {
                (componentBase as Cinemachine3rdPersonFollow).CameraDistance -= cameraDistance;

                if ((componentBase as Cinemachine3rdPersonFollow).CameraDistance <= tps_MinDistance)
                {
                    (componentBase as Cinemachine3rdPersonFollow).CameraDistance = tps_MinDistance;
                }
                else if ((componentBase as Cinemachine3rdPersonFollow).CameraDistance >= tps_MaxDistance)
                {
                    (componentBase as Cinemachine3rdPersonFollow).CameraDistance = tps_MaxDistance;
                }
            }

            //Bird's eye view
            cameraDistance = Input.GetAxis("Mouse ScrollWheel") * bev_Sensitivity;

            if (componentBase is CinemachineFramingTransposer)
            {
                (componentBase as CinemachineFramingTransposer).m_CameraDistance -= cameraDistance;

                if ((componentBase as CinemachineFramingTransposer).m_CameraDistance <= bev_MinDistance)
                {
                    (componentBase as CinemachineFramingTransposer).m_CameraDistance = bev_MinDistance;
                }
                else if ((componentBase as CinemachineFramingTransposer).m_CameraDistance >= bev_MaxDistance)
                {
                    (componentBase as CinemachineFramingTransposer).m_CameraDistance = bev_MaxDistance;
                }
            }
        }
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();

        CameraSwitcher.Register(virtualCameraFPS);
        CameraSwitcher.Register(virtualCameraTPS);
        CameraSwitcher.Register(virtualCameraBEV);

        //Set default camera's mode to FPS
        CameraSwitcher.SwitchCamera(virtualCameraFPS);
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();

        CameraSwitcher.UnRegister(virtualCameraFPS);
        CameraSwitcher.UnRegister(virtualCameraTPS);
        CameraSwitcher.UnRegister(virtualCameraBEV);
    }
}
