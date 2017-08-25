using System;
using System.Threading.Tasks;

using Android.Content;
using Android.App;
using System.Linq;
using System.Collections;

namespace NachoColl.Xamarin.Location.Droid
{
    internal static class Globals
    {
        public static class AOSP
        {
            static Android.Locations.LocationManager _locationManager;
            public static Android.Locations.LocationManager LocationManager
            {
                get {
                    if (_locationManager == null)
                        _locationManager = (Android.Locations.LocationManager)Application.Context.GetSystemService(Context.LocationService);

                    return _locationManager;
                }
            }
            public static string[] LocationProviders => LocationManager.GetProviders(enabledOnly: false).ToArray();
            public static string[] LocationPriorities = { "passive", "network", "gps" }; // we need providers in order...
            public static Hashtable LocationPrioritiesAndTimeout = new Hashtable {
                    { "passive", 20000},
                    { "network", 60000 },
                    { "gps", 300000 }
                };
        }

        public static class GooglePlayServices
        {
            public static bool IsGooglePlayServicesInstalled = Android.Gms.Common.GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == Android.Gms.Common.ConnectionResult.Success;
            public static int[,] LocationPrioritiesAndTimeout = new int[4, 2] {
                    { Android.Gms.Location.LocationRequest.PriorityNoPower, 10000},
                    { Android.Gms.Location.LocationRequest.PriorityLowPower, 30000 },
                    { Android.Gms.Location.LocationRequest.PriorityBalancedPowerAccuracy, 60000 },
                    { Android.Gms.Location.LocationRequest.PriorityHighAccuracy, 300000 }
                };
        }
    }


    public class CustomLocationManager
    {

        public async Task<global::Android.Locations.Location> GetSingleLocation(int TimeoutInMillis = 300000, int GoodEnoughAccuracyInMeters = 50, int GoodEnoughTimeLapseInMillis = 30000)
        {

            if (Globals.GooglePlayServices.IsGooglePlayServicesInstalled) {
                try {
                    return await GetGooglePlayServicesSingleLocation(TimeoutInMillis, Android.Gms.Location.LocationRequest.PriorityHighAccuracy, GoodEnoughAccuracyInMeters, GoodEnoughTimeLapseInMillis);
                } catch {
                    return await GetAOSPSingleLocation(TimeoutInMillis, GoodEnoughAccuracyInMeters, GoodEnoughTimeLapseInMillis);
                }             
            } else 

            return await GetAOSPSingleLocation(TimeoutInMillis, GoodEnoughAccuracyInMeters, GoodEnoughTimeLapseInMillis);

        }

        // privates

        // for sending results
        TaskCompletionSource<global::Android.Locations.Location> _taskCompletionSource = new TaskCompletionSource<global::Android.Locations.Location>();
        async Task<global::Android.Locations.Location> GetGooglePlayServicesSingleLocation(int TimeoutInMillis = 300000, int Priority = Android.Gms.Location.LocationRequest.PriorityHighAccuracy, int GoodEnoughAccuracyInMeters = 50, int GoodEnoughTimeLapseInMillis = 30000)
        {
            try {
                global::Android.Locations.Location currentLocation = null;

                for (int i = 0; i < Globals.GooglePlayServices.LocationPrioritiesAndTimeout.Length; i++) {
                    global::Android.Locations.Location newLocation = null;
                    int priority = Globals.GooglePlayServices.LocationPrioritiesAndTimeout[i, 0],
                        timeout = Globals.GooglePlayServices.LocationPrioritiesAndTimeout[i, 1];
                    try {
                        newLocation = await GooglePlayServices.SingleLocationListener.GetLocation(TimeoutInMillis: Priority == priority ? TimeoutInMillis : timeout, Priority: priority);
                    } catch (Exception e) {
                        Console.WriteLine(e?.Message);
                    }

                    if (newLocation != null && (currentLocation == null || IsBetterLocation(currentLocation, newLocation, GoodEnoughTimeLapseInMillis))) {
                        currentLocation = newLocation;
                    }

                    if (currentLocation != null && (priority == Priority || IsGoodEnough(currentLocation, GoodEnoughAccuracyInMeters, GoodEnoughTimeLapseInMillis)))
                        break;
                }

                if (currentLocation != null)
                    _taskCompletionSource.TrySetResult(currentLocation);
                else
                    this._taskCompletionSource.TrySetException(new Exception("no location found"));


            } catch (Exception e) {
                this._taskCompletionSource.TrySetException(e);
            }
            return await this._taskCompletionSource.Task;
        }
        async Task<global::Android.Locations.Location> GetAOSPSingleLocation(int TimeoutInMillis = 300000, int GoodEnoughAccuracyInMeters = 50, int GoodEnoughTimeLapseInMillis = 30000)
        {

            try {
                global::Android.Locations.Location currentLocation = null;

         
                for (int i = 0; i < Globals.AOSP.LocationPriorities.Length; i++) {
                    string provider = Globals.AOSP.LocationPriorities[i];
                    if (!Globals.AOSP.LocationProviders.Contains(provider))
                        continue;
                    global::Android.Locations.Location newLocation = null;
                    try {                      
                        int timeout = provider == "gps" ? TimeoutInMillis : (int)Globals.AOSP.LocationPrioritiesAndTimeout[provider];
                        try {
                            newLocation = Globals.AOSP.LocationManager.GetLastKnownLocation(provider);
                        } catch { }
                        newLocation = await AOSP.SingleLocationListener.GetLocation(TimeoutInMillis: timeout, Provider: provider);
                    } catch (Exception e) {
                        Console.WriteLine(e?.Message);
                    }

                    if (newLocation != null && (currentLocation == null || IsBetterLocation(currentLocation, newLocation, GoodEnoughTimeLapseInMillis))) {
                        currentLocation = newLocation;
                    }

                    if (currentLocation != null && IsGoodEnough(currentLocation, GoodEnoughAccuracyInMeters, GoodEnoughTimeLapseInMillis))
                        break;
                }

                if (currentLocation != null)
                    _taskCompletionSource.TrySetResult(currentLocation);
                else
                    this._taskCompletionSource.TrySetException(new Exception("no location found"));


            } catch (Exception e) {
                this._taskCompletionSource.TrySetException(e);
            }
            return await this._taskCompletionSource.Task;
        }


        static bool IsGoodEnough(global::Android.Locations.Location Location, int GoodEnoughAccuracyInMeters, int GoodEnoughTimeLapseInMillis)
        {
            long currentTimeMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            return ((Location.HasAccuracy && Location.Accuracy < GoodEnoughAccuracyInMeters) && ((currentTimeMillis - Location.Time) <= GoodEnoughTimeLapseInMillis));
        }

        // from https://github.com/jamesmontemagno/GeolocatorPlugin/blob/master/src/Geolocator.Plugin.Android/GeolocationUtils.cs ^_^
        static bool IsBetterLocation(global::Android.Locations.Location CurrentLocation, global::Android.Locations.Location NewLocation, int GoodEnoughTimeLapseInMillis = 30000)
        {
         
            if (NewLocation == null)
                return false;

            // time
            var timeDelta = NewLocation.Time - CurrentLocation.Time;
            var isSignificantlyNewer = timeDelta > GoodEnoughTimeLapseInMillis;
            var isSignificantlyOlder = timeDelta < -(2*GoodEnoughTimeLapseInMillis);
            var isNewer = timeDelta > 0;

            // time is most important parameter. Any GoodEnoughTimeLapseInMillis means you must get that one.
            if (isSignificantlyNewer)
                return true;

            if (isSignificantlyOlder)
                return false;

            // if both locations are under GoodEnoughTimeLapseInMillis, get the most balanced time/accuracy.

            // accuracy
            var accuracyDelta = (int)(NewLocation.Accuracy - CurrentLocation.Accuracy);
            var isLessAccurate = accuracyDelta > 0;
            var isMoreAccurate = accuracyDelta < 0;
            var isSignificantlyLessAccurate = accuracyDelta > 200;
            
            // provider
            var isFromSameProvider = IsSameProvider(CurrentLocation.Provider, NewLocation.Provider);

            if (isMoreAccurate)
                return true;

            if (isNewer && !isLessAccurate)
                return true;

            if (isNewer && !isSignificantlyLessAccurate && isFromSameProvider)
                return true;

            return false;

        }
        static bool IsSameProvider(string provider1, string provider2)
        {
            if (provider1 == null)
                return provider2 == null;

            return provider1.Equals(provider2);
        }
    }
}