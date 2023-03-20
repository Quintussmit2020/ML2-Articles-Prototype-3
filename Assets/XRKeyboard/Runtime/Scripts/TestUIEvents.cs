using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TestUIEvents : MonoBehaviour,
                            IPointerEnterHandler,
                            IPointerExitHandler,
                            IPointerDownHandler,
                            IPointerUpHandler,
                            IPointerClickHandler,
                            IInitializePotentialDragHandler,
                            IBeginDragHandler,
                            IDragHandler,
                            IEndDragHandler,
                            IDropHandler,
                            IScrollHandler,
                            ISelectHandler,
                            IDeselectHandler,
                            IMoveHandler,
                            ISubmitHandler,
                            ICancelHandler
{



	/// <inheritdoc />
	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log("ADRIAN-OnPointerEnter");
	}

	/// <inheritdoc />
	public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log("ADRIAN-OnPointerExit");
	}

	/// <inheritdoc />
	public void OnPointerDown(PointerEventData eventData)
	{
		Debug.Log("ADRIAN-OnPointerDown");
	}

	/// <inheritdoc />
	public void OnPointerUp(PointerEventData eventData)
	{
		Debug.Log("ADRIAN-OnPointerUp");
	}

	/// <inheritdoc />
	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("ADRIAN-OnPointerClick");
	}

    /// <inheritdoc />
    public void OnInitializePotentialDrag(PointerEventData eventData) {
		Debug.Log("ADRIAN-OnInitializePotentialDrag");
	}

    /// <inheritdoc />
    public void OnBeginDrag(PointerEventData eventData) {
		Debug.Log("ADRIAN-OnBeginDrag");
	}

    /// <inheritdoc />
    public void OnDrag(PointerEventData eventData) {
		Debug.Log("ADRIAN-OnDrag");
	}

    /// <inheritdoc />
    public void OnEndDrag(PointerEventData eventData) {
		Debug.Log("ADRIAN-OnEndDrag");
	}

    /// <inheritdoc />
    public void OnDrop(PointerEventData eventData) {
		Debug.Log("ADRIAN-OnDrop");
	}

    /// <inheritdoc />
    public void OnScroll(PointerEventData eventData) {
		Debug.Log("ADRIAN-OnScroll");
	}

    /// <inheritdoc />


    /// <inheritdoc />
    public void OnSelect(BaseEventData eventData) {
		Debug.Log("ADRIAN-OnSelect");
	}

    /// <inheritdoc />
    public void OnDeselect(BaseEventData eventData) {
		Debug.Log("ADRIAN-OnDeselect");
	}

    /// <inheritdoc />
    public void OnMove(AxisEventData eventData) {
		Debug.Log("ADRIAN-OnMove");
	}

    /// <inheritdoc />
    public void OnSubmit(BaseEventData eventData) {
		Debug.Log("ADRIAN-OnSubmit");
	}

    /// <inheritdoc />
    public void OnCancel(BaseEventData eventData) {
		Debug.Log("ADRIAN-OnCancel");
	}
}
