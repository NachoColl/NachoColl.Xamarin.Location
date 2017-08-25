using System;
using System.Threading.Tasks;
using Android.OS;
using System.Threading;
using System.Collections.Generic;
using Android.Runtime;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Gms.Location;

using Android.App;

namespace NachoColl.Xamarin.Location.Droid.GooglePlayServices
{
    internal class SingleLocationListener
       : Java.Lang.Object, Android.Gms.Location.ILocationListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
  
        // to get location ...
        GoogleApiClient _googleApiClient = null;
        LocationRequest _locationRequest = null;

        // get-location timeout
        Action  _timeoutAction = null;
        Handler _handler = null;

        // send results
        TaskCompletionSource<global::Android.Locations.Location> _taskCompletionSource = new TaskCompletionSource<global::Android.Locations.Location>();
     
        public static async Task<global::Android.Locations.Location> GetLocation(long TimeoutInMillis = 300000, int Priority = LocationRequest.PriorityHighAccuracy)
        {
            SingleLocationListener me = new SingleLocationListener();
            try {           
                if (Globals.GooglePlayServices.IsGooglePlayServicesInstalled) {
                   
                    me._locationRequest = new LocationRequest();
                    me._locationRequest.SetPriority(Priority);
                    me._locationRequest.SetInterval(1000);
                    me._locationRequest.SetFastestInterval(500);

                    me._googleApiClient = new GoogleApiClient.Builder(Application.Context).AddApi(LocationServices.API).AddConnectionCallbacks(me).AddOnConnectionFailedListener(me).Build();

                    if (me._googleApiClient != null) {
                        me._googleApiClient.Connect();

                        me._handler = new Handler(Looper.MainLooper);
                        if (me._handler != null) {
                            me._timeoutAction = () => {
                                try {
                                    me.FinishListener();
                                } finally {
                                    me._taskCompletionSource.TrySetException(new Exception("SingleLocationListener Timeout"));
                                }
                            };
                            me._handler.PostDelayed(me._timeoutAction, TimeoutInMillis);
                        }
                    } else {
                        me._taskCompletionSource.TrySetException(new Exception("Unexpected error while calling GoogleApiClient.Builder(...).Build()"));
                    }
                } else
                    me._taskCompletionSource.TrySetException(new Exception("GooglePlayService is not available!"));
            } catch (Exception e) { me._taskCompletionSource.TrySetException(e); }

            return await me._taskCompletionSource.Task;
        }

        /* GoogleApiClient.IConnectionCallbacks */
        public void OnConnected(Bundle connectionHint)
        {
            LocationServices.FusedLocationApi.RequestLocationUpdates(
                   _googleApiClient, _locationRequest, this);
        }

        public void OnConnectionSuspended(int cause)
        {
            FinishListener();
            _taskCompletionSource.TrySetException(new Exception("GoogleApiClient OnConnectionSuspended"));
            
        }


        /* GoogleApiClient.IOnConnectionFailedListener */
        public void OnConnectionFailed(ConnectionResult result)
        {
            FinishListener();
            _taskCompletionSource.TrySetException(new Exception("GoogleApiClient OnConnectionFailed"));
            
        }

        /* Android.Gms.Location.ILocationListener  */
        public void OnLocationChanged(global::Android.Locations.Location location)
        {        
            FinishListener();
            _taskCompletionSource.TrySetResult(location);
        }

        protected override void Dispose(bool disposing)
        {
            FinishListener();
            base.Dispose(disposing);
        }

        void FinishListener()
        {
            if (_googleApiClient != null && _googleApiClient.IsConnected) {
                _googleApiClient.UnregisterConnectionCallbacks(this);
                _googleApiClient.Disconnect();
            }

            if (_handler != null && _timeoutAction!=null) {
                _handler.RemoveCallbacks(_timeoutAction);
            }
        }
    }
}
 