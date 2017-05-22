using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using Dropbox.CoreApi.Android;
using Dropbox.CoreApi.Android.Session;

using Xamarin.Forms;
using DraftSurveyTool.Droid;
using System.Threading;
using Android.Provider;
using Java.Util;

[assembly: Dependency(typeof(SendEmail_Android))]

namespace DraftSurveyTool.Droid
{
    // This routine sends email and other similar functions for the Android build 
    public class SendEmail_Android : Java.Lang.Object, ISendEmail
    {
        String emailType = "text/plain";   // "message/rfc822";
        static public String DBFileNamePath;
        static public Boolean DBAuthStart = false;
        static public Boolean DBAuthResumed = false;
        static public Intent email;
        static public DropboxApi dropboxApi;
        static Boolean DBStillOK = true;
        static Boolean DBInProgress = false;
        static List<String> DBPaths;
        static int DBIndex;
        static String GlbAppKey, GlbAppSecret;

        // ConnectivityManager connectivityManager = 
        //     (ConnectivityManager) Forms.Context.GetSystemService(Context.ConnectivityService);

        public SendEmail_Android() { }

        // Called to initially create the Android email Intent 
        public Boolean CreateEmail(String emailAdr, String emailSubj)
        {
            String emailBody = "This email contains the " + emailSubj;
            try
            {
                email = new Intent(Android.Content.Intent.ActionSendMultiple);
                email.PutExtra(Android.Content.Intent.ExtraEmail, new String[] { emailAdr });
                email.PutExtra(Android.Content.Intent.ExtraSubject, emailSubj);
                email.PutExtra(Android.Content.Intent.ExtraText, emailBody);
                email.SetType(emailType);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** CreateEmail failed: " + e.ToString());
                return false;
            }
            return true;
        }

        // Called to attach email file (draft report & photos) to email. An array of paths is sent. 
        public Boolean AttachEmail(List<String> filePaths)
        {
            List<Android.OS.IParcelable> uris = new List<Android.OS.IParcelable>();
            var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
            try
            {
                for (int i = 0; i < filePaths.Count; i++)
                {
                    var filePath = Path.Combine(sdCardPath.ToString() + "/files", filePaths[i]);
                    var file = new Java.IO.File(filePath);
                    file.SetReadable(true, false);
                    uris.Add(Android.Net.Uri.FromFile(file));
                }
                email.PutParcelableArrayListExtra(Intent.ExtraStream, uris);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** attachEmail failed: " + e.ToString());
                return false;
            }
            return true;
        }
        // Execute the send email Intent
        public Boolean SendEmail()
        {
            try
            {
                Forms.Context.StartActivity(email);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** sendEmail failed: " + e.ToString());
                return false; 
            }
            return true;
        }

        public Boolean SendText(String textAdr, String textData)
        {
            // System.Diagnostics.Debug.WriteLine("***** Text address/data " + textAdr + " " + textData);
            try
            {
                Android.Telephony.SmsManager.Default.SendTextMessage(textAdr, null, textData, null, null);
            }
            catch  (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** Texting failed: " + e.ToString());
                return false;
            }
            return true;
        }

        // Write the given string to the given file. 
        public void WriteFile(String writeData, string fileNameInput)
        {
            try
            {
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = Path.Combine(sdCardPath.ToString() + "/files", fileNameInput);
                System.IO.File.WriteAllText(filePath, writeData);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** Write file failed: " + e.ToString());
            }
        }

        public void WriteDBBlock(String AppKey, String AppSecret, List<String> fileNamePaths)
        {
            if (DBInProgress)
            {
                return;
            }
            GlbAppKey = AppKey;
            GlbAppSecret = AppSecret;
            DBInProgress = true;
            DBStillOK = true;
            DBPaths = fileNamePaths;
            DBIndex = 0;
            if (!WriteDropbox(GlbAppKey, GlbAppSecret, DBPaths[DBIndex]))
            {
                DBStillOK = false;
            }
        }

        static public Boolean WriteDropbox(String AppKey, String AppSecret, String fileNamePath)
        {
            DBFileNamePath = fileNamePath;
            try
            {
                // If the authorizing process has already been started
                if (DBAuthStart)
                {   // Writes to Dropbox must be a thread. Android reuqirement
                    ThreadPool.QueueUserWorkItem(o => writeDB());
                }

                // If the authorizing process has not been done yet
                if (!DBAuthStart)
                {
                    DBAuthStart = true;
                    AppKeyPair appKeys = new AppKeyPair(AppKey, AppSecret);
                    AndroidAuthSession session = new AndroidAuthSession(appKeys);
                    dropboxApi = new DropboxApi(session);
                    (dropboxApi.Session as AndroidAuthSession).StartOAuth2Authentication(MainActivity.mainContext);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** WriteDropbox failed: " + e.ToString());
                System.Diagnostics.Debug.WriteLine("***** DBAuthStart: " + DBAuthStart);
                return false;
            }
            return true;
        }

        static public Boolean writeDB()
        {
            try
            {
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = Path.Combine(sdCardPath.ToString() + "/files", DBFileNamePath);
                // System.Diagnostics.Debug.WriteLine("***** writeDB DBFileNamePath: " + DBFileNamePath);
                using (var input = File.OpenRead(filePath))
                {
                    // Gets the local file and upload it to Dropbox
                    dropboxApi.PutFileOverwrite("/files" + "/" + DBFileNamePath, input, input.Length, null);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** writeDB failed: " + e.ToString());
                DBStillOK = false;
            }
            DBIndex++;
            if (DBIndex >= DBPaths.Count)
            {
                DBInProgress = false;
                if (!DBStillOK)
                {
                    MainActivity.app.ReportDBError();
                }
            } else
            {
                if (!WriteDropbox(GlbAppKey, GlbAppSecret, DBPaths[DBIndex]))
                {
                    DBStillOK = false;
                }
            }
            return true;
        }

        public void TakeAPicture(String draftFileName)
        {
            String fileName;
            try
            {
                fileName = draftFileName + ".jpg";
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = Path.Combine(sdCardPath.ToString() + "/files", fileName);
                var file = new Java.IO.File(filePath);
                Intent intent = new Intent(MediaStore.ActionImageCapture);
                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
                Forms.Context.StartActivity(intent);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** TakeAPicture failed: " + e.ToString());
            }
        }

        // only creates folder if doesn't exist. result = true if exists
        public Boolean CreateFolder(String folder, Boolean addH, Boolean create)
        {
            try
            {
                String header = "/";
                if (addH)
                {
                    header = "/files/";
                }
                Boolean result = true;
                var dir = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + header + folder + "/");
                // System.Diagnostics.Debug.WriteLine("***** checking if folder exists: " + folder);
                if (!dir.Exists())
                {
                    // System.Diagnostics.Debug.WriteLine("***** doesn't exist");
                    result = false;
                    if (create)
                    {
                        // System.Diagnostics.Debug.WriteLine("***** creating now");
                        dir.Mkdirs();
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** CreateFolder failed: " + e.ToString());
                return false;
            }
        }

        // only creates folder if doesn't exist. result = true if exists
        public Boolean FileExists(String file)
        {
            try
            {
                Boolean result = true;
                var fileP = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/files/" + file);
                // System.Diagnostics.Debug.WriteLine("***** checking if file exists: " + file);
                if (!fileP.Exists())
                {
                    // System.Diagnostics.Debug.WriteLine("***** doesn't exist");
                    result = false;
                }
                return result;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** FileExists failed: " + e.ToString());
                return false;
            }
        }

        public void DeleteShip(String inpShip)
        {
            try
            {
                var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/files/" + inpShip + "/";
                var dir = new Java.IO.File(path);
                // System.Diagnostics.Debug.WriteLine("***** DeleteShip: directory exists: " + dir.Exists());
                if (dir.Exists())
                {   // Android Java Unix command will delete the whole ship directory which includes all reports 
                    String deleteCmd = "rm -r " + path;
                    Java.Lang.Runtime runtime = Java.Lang.Runtime.GetRuntime();
                    try
                    {
                        runtime.Exec(deleteCmd);
                    }
                    catch (IOException e)
                    {
                        System.Diagnostics.Debug.WriteLine("***** DeleteShip rm -r threw exception: " + e.ToString());
                    }
                }
                // dir.Delete();
                var fileP = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
                    + "/files/" + inpShip + "_config.xml");
                fileP.Delete();
                fileP = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
                    + "/files/" + inpShip + "_graphic.png");
                fileP.Delete();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** DeleteShip failed: " + e.ToString());
            }            
        }
        public void DeleteReport(String inpReportPath)
        {
            var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/files/" + inpReportPath + "/";
            var dir = new Java.IO.File(path);
            // System.Diagnostics.Debug.WriteLine("***** DeleteReport: directory exists: " + dir.Exists());
            if (dir.Exists())
            {   // Android Java Unix command will delete the whole report directory 
                String deleteCmd = "rm -r " + path;
                Java.Lang.Runtime runtime = Java.Lang.Runtime.GetRuntime();
                try
                {
                    runtime.Exec(deleteCmd);
                    while (dir.Exists())
                    {
                        // System.Diagnostics.Debug.WriteLine("***** Waiting for directory deletion");
                    }
                }
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine("***** DeleteReport rm -r threw exception: " + e.ToString());
                }
            }
        }
    }
}