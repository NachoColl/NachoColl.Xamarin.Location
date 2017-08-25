# NachoColl.Xamarin.Location
Just a Location library for Xamarin.

# How to Install

# How to use it

For now I just published the Android library:

```
try {
   Android.Locations.Location location = 
      await new NachoColl.Xamarin.Location.Droid.CustomLocationManager().GetSingleLocation();
}catch{
 // you need to catch (e.g. TimeOut will trigger an Exception).
}
```
