# NachoColl.Xamarin.Location

Just a **Single Location** listener library for Xamarin. 

This library will only give you a single location each time you call the method GetSingleLocation(). If you need to listen for locations continuously, you must check for another git ^_^.

# How to Install

# How to use it

For now I just published the Android library you can use by calling:

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
