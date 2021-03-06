﻿using System;
using System.Reactive.Subjects;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Shiny.Logging;

namespace Shiny.Locations
{
    [BroadcastReceiver(
        Name = "com.shiny.locations.GpsBroadcastReceiver",
        Exported = true
    )]
    [IntentFilter(new []
    {
        "com.shiny.locations.GpsBroadcastReceiver.ACTION_PROCESS"
    })]
    public class GpsBroadcastReceiver : BroadcastReceiver
    {
        public const string INTENT_ACTION = "com.shiny.locations.GpsBroadcastReceiver.ACTION_PROCESS";
        public static IObservable<IGpsReading> WhenReading() => readingSubject;
        static readonly Subject<IGpsReading> readingSubject = new Subject<IGpsReading>();


        public override void OnReceive(Context context, Intent intent)
        {
            if (!intent.Action.Equals(INTENT_ACTION))
                return;

            var result = LocationResult.ExtractResult(intent);
            if (result == null)
                return;

            try
            {
                var gpsDelegate = ShinyHost.Resolve<IGpsDelegate>();
                foreach (var location in result.Locations)
                {
                    var reading = new GpsReading(location);
                    readingSubject.OnNext(reading);
                    gpsDelegate?.OnReading(reading);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}