﻿using System;
using Hover.Common.Items;
using Hover.Common.Items.Managers;
using Hover.Common.Items.Types;
using Hover.Common.Layouts;
using Hover.Common.Renderers.Contents;
using Hover.Common.Renderers.Fills;
using Hover.Common.Renderers.Helpers;
using Hover.Common.Utils;
using UnityEngine;

namespace Hover.Common.Renderers {

	/*================================================================================================*/
	[ExecuteInEditMode]
	[RequireComponent(typeof(HoverItem))]
	[RequireComponent(typeof(HoverItemHighlightState))]
	public class HoverRendererRectangleController : HoverRendererController, 
															IRectangleLayoutElement, ITreeUpdateable {
	
		public const string ButtonRendererName = "ButtonRenderer";
		public const string SliderRendererName = "SliderRenderer";
		public const string SizeXName = "SizeX";
		public const string SizeYName = "SizeY";

		public bool IsButtonRendererType { get; private set; }

		[DisableWhenControlled(DisplayMessage=true)]
		public GameObject ButtonRendererPrefab;

		[DisableWhenControlled]
		public GameObject SliderRendererPrefab;

		[SerializeField]
		[DisableWhenControlled]
		private Component _ButtonRenderer;

		[DisableWhenControlled]
		public HoverRendererRectangleSlider SliderRenderer;

		[DisableWhenControlled(RangeMin=0, RangeMax=100)]
		public float SizeX = 10;

		[DisableWhenControlled(RangeMin=0, RangeMax=100)]
		public float SizeY = 10;

		[DisableWhenControlled(RangeMin=0.05f, RangeMax=0.9f)]
		public float DisabledAlpha = 0.35f;

		[HideInInspector]
		[SerializeField]
		private bool _IsBuilt;
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public IHoverRendererRectangleButton ButtonRenderer {
			get { return ((IHoverRendererRectangleButton)_ButtonRenderer); }
			set { _ButtonRenderer = (Component)value; }
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Awake() {
			if ( !_IsBuilt ) {
				ButtonRendererPrefab = Resources.Load<GameObject>(
					"Prefabs/HoverRendererRectangleButtonStandard");
				SliderRendererPrefab = Resources.Load<GameObject>(
					"Prefabs/HoverRendererRectangleSliderStandard");
				_IsBuilt = true;
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public override void TreeUpdate() {
			base.TreeUpdate();
			
			HoverItem hoverItem = GetComponent<HoverItem>();
			HoverItemHighlightState highState = GetComponent<HoverItemHighlightState>();
			HoverItemSelectionState selState = GetComponent<HoverItemSelectionState>();

			TryRebuildWithItemType(hoverItem.ItemType);

			if ( ButtonRenderer != null ) {
				UpdateButtonControl();
				UpdateButtonSettings(hoverItem);
				UpdateButtonSettings(highState);
				UpdateButtonSettings(selState);
			}

			if ( SliderRenderer != null ) {
				UpdateSliderControl();
				UpdateSliderSettings(hoverItem);
				UpdateSliderSettings(hoverItem, highState);
				UpdateSliderSettings(selState);
			}
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public override Vector3 GetNearestWorldPosition(Vector3 pFromWorldPosition) {
			if ( ButtonRenderer != null ) {
				return ButtonRenderer.GetNearestWorldPosition(pFromWorldPosition);
			}

			if ( SliderRenderer != null ) {
				return SliderRenderer.GetNearestWorldPosition(pFromWorldPosition);
			}

			throw new Exception("No button or slider renderer.");
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void SetLayoutSize(float pSizeX, float pSizeY, ISettingsController pController) {
			Controllers.Set(SizeXName, pController);
			Controllers.Set(SizeYName, pController);

			SizeX = pSizeX;
			SizeY = pSizeY;
		}

		/*--------------------------------------------------------------------------------------------*/
		public void UnsetLayoutSize(ISettingsController pController) {
			Controllers.Unset(SizeXName, pController);
			Controllers.Unset(SizeYName, pController);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void TryRebuildWithItemType(HoverItem.HoverItemType pType) {
			if ( pType == HoverItem.HoverItemType.Slider ) {
				Controllers.Set(ButtonRendererName, this);
				Controllers.Unset(SliderRendererName, this);

				ButtonRenderer = RendererHelper.DestroyRenderer(ButtonRenderer);
				SliderRenderer = UseOrFindOrBuildSlider();
				IsButtonRendererType = false;
			}
			else {
				Controllers.Set(SliderRendererName, this);
				Controllers.Unset(ButtonRendererName, this);

				//TODO: SliderRenderer = RendererHelper.DestroyRenderer(SliderRenderer);
				ButtonRenderer = UseOrFindOrBuildButton();
				IsButtonRendererType = true;
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private IHoverRendererRectangleButton UseOrFindOrBuildButton() {
			if ( ButtonRenderer != null ) {
				return ButtonRenderer;
			}

			return RendererHelper.FindOrBuildRenderer<IHoverRendererRectangleButton>(
				gameObject.transform, ButtonRendererPrefab, "Button", 
				typeof(HoverRendererRectangleButton));
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private HoverRendererRectangleSlider UseOrFindOrBuildSlider() {
			if ( SliderRenderer != null ) {
				return SliderRenderer;
			}

			return  null;

			//TODO: 
			/*return RendererHelper.FindOrBuildRenderer<HoverRendererRectangleSlider>(
				gameObject.transform, SliderRendererPrefab, "Slider", 
				typeof(HoverRendererRectangleSlider));*/
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateButtonControl() {
			ButtonRenderer.RendererController = this;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void UpdateSliderControl() {
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.SizeXName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.SizeYName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.AlphaName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.ZeroValueName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.HandleValueName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.JumpValueName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.AllowJumpName, this);
			SliderRenderer.Controllers.Set(HoverRendererRectangleSlider.FillStartingPointName, this);

			SliderRenderer.HandleButton.Fill.Controllers.Set(
				HoverRendererFillRectangleFromCenter.HighlightProgressName, this);
			SliderRenderer.HandleButton.Fill.Controllers.Set(
				HoverRendererFillRectangleFromCenter.SelectionProgressName, this);
			SliderRenderer.HandleButton.Fill.Edge.Controllers.Set("GameObject.activeSelf", this);
			SliderRenderer.JumpButton.Fill.Controllers.Set(
				HoverRendererFillRectangleFromCenter.HighlightProgressName, this);
			SliderRenderer.JumpButton.Fill.Controllers.Set(
				HoverRendererFillRectangleFromCenter.SelectionProgressName, this);
			SliderRenderer.JumpButton.Fill.Edge.Controllers.Set("GameObject.activeSelf", this);

			SliderRenderer.HandleButton.Canvas.Label.Controllers.Set("Text.text", this);
			SliderRenderer.HandleButton.Canvas.IconOuter.Controllers.Set(
				HoverRendererIcon.IconTypeName, this);
			SliderRenderer.HandleButton.Canvas.IconInner.Controllers.Set(
				HoverRendererIcon.IconTypeName, this);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateButtonSettings(HoverItem pHoverItem) {
			HoverItemData data = pHoverItem.Data;
			ICheckboxItem checkboxData = (data as ICheckboxItem);
			IRadioItem radioData = (data as IRadioItem);
			IParentItem parentData = (data as IParentItem);
			IStickyItem stickyData = (data as IStickyItem);
			
			ButtonRenderer.SizeX = SizeX;
			ButtonRenderer.SizeY = SizeY;
			ButtonRenderer.Alpha = (data.IsEnabled ? 1 : DisabledAlpha);

			ButtonRenderer.LabelText = data.Label;

			if ( checkboxData != null ) {
				ButtonRenderer.IconOuterType = HoverRendererIcon.IconOffset.CheckOuter;
				ButtonRenderer.IconInnerType = (checkboxData.Value ?
					HoverRendererIcon.IconOffset.CheckInner : HoverRendererIcon.IconOffset.None);
			}
			else if ( radioData != null ) {
				ButtonRenderer.IconOuterType = HoverRendererIcon.IconOffset.RadioOuter;
				ButtonRenderer.IconInnerType = (radioData.Value ?
					HoverRendererIcon.IconOffset.RadioInner : HoverRendererIcon.IconOffset.None);
			}
			else if ( parentData != null ) {
				ButtonRenderer.IconOuterType = HoverRendererIcon.IconOffset.Parent;
				ButtonRenderer.IconInnerType = HoverRendererIcon.IconOffset.None;
			}
			else if ( stickyData != null ) {
				ButtonRenderer.IconOuterType = HoverRendererIcon.IconOffset.Sticky;
				ButtonRenderer.IconInnerType = HoverRendererIcon.IconOffset.None;
			}
			else {
				ButtonRenderer.IconOuterType = HoverRendererIcon.IconOffset.None;
				ButtonRenderer.IconInnerType = HoverRendererIcon.IconOffset.None;
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		private void UpdateSliderSettings(HoverItem pHoverItem) {
			ISliderItem data = (ISliderItem)pHoverItem.Data;
			HoverRendererCanvas handleCanvas = SliderRenderer.HandleButton.Canvas;
			HoverRendererIcon handleIconOuter = handleCanvas.IconOuter;
			HoverRendererIcon handleIconInner = handleCanvas.IconInner;

			SizeY = Mathf.Max(SizeY, SliderRenderer.HandleButton.SizeY);

			SliderRenderer.SizeX = SizeX;
			SliderRenderer.SizeY = SizeY;
			SliderRenderer.Alpha = (data.IsEnabled ? 1 : DisabledAlpha);

			handleCanvas.Label.TextComponent.text = data.GetFormattedLabel(data);
			handleIconOuter.IconType = HoverRendererIcon.IconOffset.Slider;
			handleIconInner.IconType = HoverRendererIcon.IconOffset.None;

			SliderRenderer.HandleValue = data.SnappedValue;
			SliderRenderer.FillStartingPoint = data.FillStartingPoint;
			SliderRenderer.ZeroValue = Mathf.InverseLerp(data.RangeMin, data.RangeMax, 0);
			SliderRenderer.AllowJump = data.AllowJump;
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateButtonSettings(HoverItemHighlightState pHighState) {
			ButtonRenderer.HighlightProgress = pHighState.MaxHighlightProgress;
			ButtonRenderer.ShowEdge = pHighState.IsNearestAcrossAllItemsForAnyCursor;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void UpdateSliderSettings(
									HoverItem pHoverItem, HoverItemHighlightState pHighState) {
			SliderItem data = (SliderItem)pHoverItem.Data;
			HoverItemHighlightState.Highlight? high = pHighState.NearestHighlight;
			float highProg = pHighState.MaxHighlightProgress;

			SliderRenderer.HandleButton.Fill.HighlightProgress = highProg;
			SliderRenderer.JumpButton.Fill.HighlightProgress = highProg;
			
			RendererHelper.SetActiveWithUpdate(SliderRenderer.HandleButton.Fill.Edge,
				pHighState.IsNearestAcrossAllItemsForAnyCursor);
			RendererHelper.SetActiveWithUpdate(SliderRenderer.JumpButton.Fill.Edge,
				pHighState.IsNearestAcrossAllItemsForAnyCursor);

			if ( high == null ) {
				data.HoverValue = null;
				SliderRenderer.JumpValue = -1;
				return;
			}
			
			float value = SliderRenderer.GetValueViaNearestWorldPosition(high.Value.NearestWorldPos);
			
			data.HoverValue = value;
			
			float snapValue = (float)data.SnappedHoverValue;
			//float easePower = (1-high.Value.Progress)*5+1; //gets "snappier" as you pull away
			float showValue = DisplayUtil.GetEasedValue(data.Snaps, value, snapValue, 3);
				
			SliderRenderer.JumpValue = showValue;
			
			if ( data.IsStickySelected ) {
				data.Value = value;
				SliderRenderer.HandleValue = showValue;
			}
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateButtonSettings(HoverItemSelectionState pSelState) {
			ButtonRenderer.SelectionProgress = pSelState.SelectionProgress;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateSliderSettings(HoverItemSelectionState pSelState) {
			float selProg = pSelState.SelectionProgress;
			
			SliderRenderer.HandleButton.Fill.SelectionProgress = selProg;
			SliderRenderer.JumpButton.Fill.SelectionProgress = selProg;
		}
		
	}

}
