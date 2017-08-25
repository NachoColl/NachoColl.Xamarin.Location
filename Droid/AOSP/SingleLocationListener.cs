using System;
using System.Threading.Tasks;
using Android.OS;
using System.Threading;
using System.Collections.Generic;
using Android.Runtime;


using Android.App;
using Android.Locations;
using Android.Content;
using System.Linq;

namespace NachoColl.Xamarin.Location.Droid.AOSP
{
    internal class SingleLocationListener
       : Java.Lang.Object, ILocationListener
    {
        
        // get-location timeout
        Action  _timeoutAction = null;
        Handler _handler = null;

        // for sending results
        TaskCompletionSource<global::Android.Locations.Location> _taskCompletionSource = new TaskCompletionSource<global::Android.Locations.Location>();
     
        public static async Task<global::Android.Locations.Location> GetLocation(long TimeoutInMillis = 300000, string Provider = "gps")
        {
            SingleLocationListener me = new SingleLocationListener();
            try {

                Globals.AOSP.LocationManager.RequestLocationUpdates(Provider, 0, 0, me, Looper.MainLooper);
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
              
            } catch (Exception e) { me._taskCompletionSource.TrySetException(e); }

            return await me._taskCompletionSource.Task;
        }

        #region ILocationListener
        public void OnLocationChanged(Android.Locations.Location location)
        {
            FinishListener();
            _taskCompletionSource.TrySetResult(location);
        }

        public void OnProviderDisabled(string provider)
        {
           
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }
        #endregion


        protected override void Dispose(bool disposing)
        {
            FinishListener();
            base.Dispose(disposing);
        }

        void FinishListener()
        {      
            Globals.AOSP.LocationManager.RemoveUpdates(this);    

            if (_handler != null && _timeoutAction!=null) {
                _handler.RemoveCallbacks(_timeoutAction);
            }
        }
    }
}
 