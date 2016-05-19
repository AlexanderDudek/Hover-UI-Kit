﻿using System.Collections.Generic;
using Hover.Common.Layouts;
using Hover.Common.Utils;
using UnityEngine;
using System.Linq;

namespace Hover.Common.Items {

	/*================================================================================================*/
	[ExecuteInEditMode]
	[RequireComponent(typeof(TreeUpdater))]
	public class HoverLayoutGroup : MonoBehaviour, ISettingsController, ITreeUpdateable {

		public ISettingsControllerMap Controllers { get; private set; }
		
		protected readonly List<IRectangleLayoutElement> vChildElements;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		protected HoverLayoutGroup() {
			Controllers = new SettingsControllerMap();
			vChildElements = new List<IRectangleLayoutElement>();
		}

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Start() {
			//do nothing...
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public virtual void TreeUpdate() {
			FillChildItemsList();
		}

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		protected virtual void FillChildItemsList() {
			vChildElements.Clear();

			foreach ( Transform childTx in gameObject.transform ) {
				IRectangleLayoutElement childElem = childTx.GetComponent<IRectangleLayoutElement>();

				if ( childElem == null ) {
					continue;
				}

				vChildElements.Add(childElem);
			}
		}

	}

}