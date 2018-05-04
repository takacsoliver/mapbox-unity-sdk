﻿namespace Mapbox.Unity.Ar
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System;
	using UnityARInterface;
	using Mapbox.Unity.Location;
	using Mapbox.Unity.Map;

	public class ARLocalizationStrategy : ComputeARLocalizationStrategy
	{

		ARInterface.CustomTrackingState _trackingState;
		ARInterface _arInterface;
		bool _isTrackingGood, _setUserHeading;
		float _planePosOnY = -.5f;

		public override event Action<Alignment> OnLocalizationComplete;

		private void Start()
		{
			_arInterface = ARInterface.GetInterface();
			_trackingState = new ARInterface.CustomTrackingState();
			ARInterface.planeAdded += GetPlaneCoords;
			ARInterface.planeRemoved += GetPlaneCoords;
		}

		public override void ComputeLocalization(CentralizedARLocator centralizedARLocator)
		{
			var currentLocation = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation;
			var aligment = new Alignment();

			// Checking if tracking is good, do nothing on location. Other than check if the newly calculated heading is better.
			Debug.Log("Tracking state: " + CheckTracking());

			if (CheckTracking())
			{

				Unity.Utilities.Console.Instance.Log(string.Format("YPlaneCoords: {0}", _planePosOnY)
					, "red"
				);

				var mapPos = centralizedARLocator.CurrentMap.transform.position;
				var newPos = new Vector3(mapPos.x, _planePosOnY, mapPos.z);
				aligment.Position = newPos;

				if (currentLocation.IsUserHeadingUpdated && !_setUserHeading)
				{
					aligment.Rotation = currentLocation.UserHeading;
					_setUserHeading = true;
				}
				else
				{
					aligment.Rotation = centralizedARLocator.CurrentMap.transform.eulerAngles.y;

				}

				OnLocalizationComplete(aligment);

				return;
			}

			// If tracking is bad then use GPS to align map.
			// TODO : Add mapmatching to the equation.
			var geoPos = centralizedARLocator.CurrentMap.GeoToWorldPosition(currentLocation.LatitudeLongitude);
			var geoAndPlanePos = new Vector3(geoPos.x, _planePosOnY, geoPos.z);
			aligment.Position = geoAndPlanePos;
			//aligment.Rotation = currentLocation.IsUserHeadingUpdated ? currentLocation.UserHeading : currentLocation.DeviceOrientation;
			aligment.Rotation = currentLocation.DeviceOrientation;
			OnLocalizationComplete(aligment);
		}

		void GetPlaneCoords(BoundedPlane plane)
		{
			_planePosOnY = plane.center.y;
		}

		bool CheckTracking()
		{

			if (_arInterface.GetTrackingState(ref _trackingState))
			{
				Unity.Utilities.Console.Instance.Log(
				string.Format(
					"ARTracking State: {0}"
						, _trackingState
				)
				, "blue"
			);

				Debug.Log((_trackingState));

				if (_trackingState == ARInterface.CustomTrackingState.Good)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}

}

