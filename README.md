# NachoColl.Xamarin.Location

Just a **Single Location** listener library for Xamarin. 

This library will only give you a single location each time you call the method GetSingleLocation(). If you need to listen for locations continuously, you must check for another git ^_^.

*For now I just published the Android library. Will update the iOS version soon ...*

**Android Notes:**

The library will try to use GooglePlayServices to get the location if it is available, or the original AOSP location library if Google libraries are not installed on the device.

**iOS Notes:**

*TO-DO*


# How to Install

*Working on the Nuget package...*


# How to use it

Call GetSingleLocation() with optional parameters:

- int TimeoutInMillis = 300000, if timeout is reached, an exception is thrown. 
- int GoodEnoughAccuracyInMeters = 50, used to speed-up/use less resources if possible.
- int GoodEnoughTimeLapseInMillis = 30000, same.

I've tested the default values and calling GetSingleLocation() should be good enough in most scenarios.

```cs
location = await new NachoColl.Xamarin.Location.Droid.CustomLocationManager().GetSingleLocation();
```


To use it correctly you must check the Location permission before calling and also catch for the related exceptions that may be triggered (e.g. Time Out).

```cs
Android.Locations.Location location = null;
try {

    if (CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location).Result 
         == PermissionStatus.Granted) { 
         
        location = await new NachoColl.Xamarin.Location.Droid.CustomLocationManager().GetSingleLocation();
    }
    return location;
} catch { 
    return null;
}
```
