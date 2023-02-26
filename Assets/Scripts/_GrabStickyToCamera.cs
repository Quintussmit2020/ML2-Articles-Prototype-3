using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabStickyToCamera : MonoBehaviour
{
    public float hoverDuration = 3f;
    public float moveDistance = 1f;
    public Transform originalPosition;
    private XRBaseInteractable interactable;
    private float hoverTime = 0f;

    void Start()
    {
        interactable = GetComponent<XRBaseInteractable>();
        interactable.onHoverEntered.AddListener(OnHoverEnter);
        interactable.onHoverExited.AddListener(OnHoverExit);
    }

    void Update()
    {
        if (hoverTime >= hoverDuration)
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * moveDistance;
        }
    }

    void OnHoverEnter(XRBaseInteractor interactor)
    {
        hoverTime = 0f;
    }

    void OnHoverExit(XRBaseInteractor interactor)
    {
        transform.position = originalPosition.position;
        hoverTime = 0f;
    }

    void FixedUpdate()
    {
        if (interactable.isSelected)
        {
            hoverTime = 0f;
        }
        else
        {
            hoverTime += Time.fixedDeltaTime;
        }
    }
}
