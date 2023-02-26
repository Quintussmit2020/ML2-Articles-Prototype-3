using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabWithRayXRI : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Vector3 originalPosition;
    private bool isHovering = false;
    private float timeSinceRaycastHit = 0f;
    private const float moveDuration = 3f;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalPosition = transform.position;
        grabInteractable.onHoverEntered.AddListener(OnHoverEnter);
        grabInteractable.onHoverExited.AddListener(OnHoverExit);
    }

    private void OnHoverEnter(XRBaseInteractor interactor)
    {
        isHovering = true;
    }

    private void OnHoverExit(XRBaseInteractor interactor)
    {
        isHovering = false;
        transform.position = originalPosition;
        timeSinceRaycastHit = 0f;
    }

    private void Update()
    {
        if (isHovering)
        {
            RaycastHit hitInfo;
            if (grabInteractable.selectingInteractor.TryGetComponent(out XRController controller) &&
                Physics.Raycast(controller.transform.position, controller.transform.forward, out hitInfo))
            {
                if (hitInfo.collider.gameObject == gameObject)
                {
                    timeSinceRaycastHit += Time.deltaTime;
                    if (timeSinceRaycastHit >= moveDuration)
                    {
                        transform.position = controller.transform.position + controller.transform.forward;
                    }
                }
                else
                {
                    timeSinceRaycastHit = 0f;
                    transform.position = originalPosition;
                }
            }
            else
            {
                timeSinceRaycastHit = 0f;
                transform.position = originalPosition;
            }
        }
    }
}

