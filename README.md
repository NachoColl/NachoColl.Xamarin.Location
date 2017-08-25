# NachoColl.Xamarin.Location

Just a **Single Location** listener library for Xamarin. 

This library will only give you a single location each time you call the method GetSingleLocation(). If you need to listen for locations continuously, you must check for another git ^_^.

*For now I just published the Android library. Will update the iOS version soon ...*

# How to Install

*Working on the Nuget package...*


# How to use it

Call GetSingleLocation() with optional parameters:

- int TimeoutInMillis = 300000, if timeout is reached, an exception is thrown. 
- int GoodEnoughAccuracyInMeters = 50, used to speed-up/use less resources if possible.
- int GoodEnoughTimeLapseInMillis = 30000, same.

I've tested the default values and calling GetSingleLocation() should be good enough in most scenarios.

```
location = await new NachoColl.Xamarin.Location.Droid.CustomLocationManager().GetSingleLocation();
```


To use it correctly you must check the Location permission before calling and also check for the related exceptions that may be triggered.

```
Android.Locations.Location location = null;
try {

    if (CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location).Result 
         == PermissionStatus.Granted) {            
        location = await new NachoColl.Xamarin.Location.Droid.CustomLocationManager().GetSingleLocation();
        if(location!=null)
            LogFile.WriteLogEntry(string.Format("Lat: {0}, Lon: {1}, Accuracy: {2}, Timestamp: {3}", location.Latitude,
                  location.Longitude, location.Accuracy, Timestamp.FromUnixTimeStamp(location.Time).ToUniversalTime()));
    }
    return location;
} catch (Exception e) {  // you need to catch (e.g. TimeOut will trigger an Exception).
    LogFile.WriteLogEntry(e);
    return null;
}
```
