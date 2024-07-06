// MyMediaPlaybackService.cs
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Graphics.Drawable;
using CommunityToolkit.Maui.Converters;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using RudymentaryMobile;
//namespace RudymentaryMobile;
[Service]
public class MyMediaPlaybackService : Service
{
    private const string NotificationChannelId = "MyForegroundServiceChannel";
    private const int NotificationId = 1;

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        //string isCloseCommand = intent.GetStringExtra("closeApp");
        //if (isCloseCommand == "toBeClosed")
        //{
        //    StopForeground(true);
        //   StopSelf();
        //    
        //    return StartCommandResult.Sticky;
        //}
        string title = intent.GetStringExtra("songTitle");
        string artist = intent.GetStringExtra("songArtists");
        string album = intent.GetStringExtra("albumName");
        var stream = typeof(MainPage).Assembly.GetManifestResourceStream("RudymentaryMobile.Resources.Images.rudymentarynb_ic.png");
        byte[] rudymentaryLogoByte = new byte[stream.Length];
        stream.Read(rudymentaryLogoByte, 0, (int)stream.Length);
        Bitmap referenceSuccess = BitmapFactory.DecodeByteArray(rudymentaryLogoByte, 0, rudymentaryLogoByte.Length);
        var referenceICSuccess = IconCompat.CreateWithBitmap(referenceSuccess);

        //Bitmap rudymentaryImage = BitmapFactory.DecodeByteArray(rudymentaryImageByteData, 0, rudymentaryImageByteData.Length);

        //bool byteDataExists = rudymentaryLogoByteArray == null;
        // Process.KillProcess(Process.MyPid());

        /*
        var clickIntentToLaunch = new Intent(this, typeof(MainActivity));
        int clickIntentToLaunchID = 1;
        var pendingClickIntentToLaunch = PendingIntent.GetActivity(this, 0, clickIntentToLaunch, 0);
        var iconResourceId = Android.Resource.Drawable.IcMediaPlay;
        */

        //Intent stopIntent = new Intent(this, typeof(MyMediaPlaybackService));
        //stopIntent.SetAction("STOP_SERVICE"); // Custom action for stopping the service
        //stopIntent.PutExtra("closeApp", "toBeClosed");
        //PendingIntent stopPendingIntent = PendingIntent.GetService(this, 0, stopIntent, 0);
        //var playButtonAction = new NotificationCompat.Builder(Android.Resource.Drawable.IcMediaPlay, "Play");
        CreateNotificationChannel();
        if (album == "Download")
        {
            var notification = new NotificationCompat.Builder(this, NotificationChannelId)
                //.SetContentIntent(pendingClickIntentToLaunch)
                .SetCategory(Notification.CategoryTransport)

                //.AddAction(Android.Resource.Drawable.IcMenuCloseClearCancel, "Stop", stopPendingIntent)
                .SetSmallIcon(referenceICSuccess)
                .SetColor(-43449)
                .SetContentTitle(title)
                .SetContentText(artist)
            //.AddAction()
                .Build();



            StartForeground(NotificationId, notification);
            return StartCommandResult.Sticky;
        }
        else
        {
            byte[] albumImageByteData = intent.GetByteArrayExtra("albumArt");

            Bitmap albumImage = BitmapFactory.DecodeByteArray(albumImageByteData, 0, albumImageByteData.Length);
            var notification = new NotificationCompat.Builder(this, NotificationChannelId)
                //.SetContentIntent(pendingClickIntentToLaunch)
                .SetCategory(Notification.CategoryTransport)

                .SetSmallIcon(referenceICSuccess)
                .SetColor(-43449)
                .SetContentTitle(title)
                .SetContentText(artist)

                .SetStyle(new NotificationCompat.BigPictureStyle()
                    .BigPicture(albumImage)
                    .ShowBigPictureWhenCollapsed(false))
            //.AddAction()
                .Build();



            StartForeground(NotificationId, notification);
            return StartCommandResult.Sticky;
        }
        return StartCommandResult.Sticky;

    }

    // Other methods and logic as needed

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(NotificationChannelId, "Foreground Service", NotificationImportance.High);
            var notificationManager = GetSystemService(NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    public override IBinder OnBind(Intent intent) => null;
}