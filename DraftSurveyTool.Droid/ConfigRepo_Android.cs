using System;
using System.IO;
using Android.Content;
using Xamarin.Forms;
using DraftSurveyTool.Droid;
using Android.Graphics;

[assembly: Dependency(typeof(ConfigRepo_Android))]

namespace DraftSurveyTool.Droid
{    
    public class ConfigRepo_Android : Java.Lang.Object, IConfigRepo
    {
        public string emailAdr { get; set; }
        public string textAdr { get; set; }
        public string DBSecret { get; set; }
        public bool emailSel { get; set; }
        public bool textSel { get; set; }
        public bool DBSel { get; set; }
        public string ship { get; set; }
        public bool SciSel { get; set; }
        public string prec { get; set; }
        public string photoCount { get; set; }
        public string report { get; set; }
        public int imageWidth { get; set; }
        public int imageHeight { get; set; }

        public void SaveConfig()
        {
            try
            {
                var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var filePath = System.IO.Path.Combine(documentsPath, "emailAdr");
                System.IO.File.WriteAllText(filePath, emailAdr);
                filePath = System.IO.Path.Combine(documentsPath, "textAdr");
                System.IO.File.WriteAllText(filePath, textAdr);
                filePath = System.IO.Path.Combine(documentsPath, "emailSel");
                System.IO.File.WriteAllText(filePath, bool2Str(emailSel));
                filePath = System.IO.Path.Combine(documentsPath, "textSel");
                System.IO.File.WriteAllText(filePath, bool2Str(textSel));
                filePath = System.IO.Path.Combine(documentsPath, "DBSecret");
                System.IO.File.WriteAllText(filePath, DBSecret);
                filePath = System.IO.Path.Combine(documentsPath, "DBSel");
                System.IO.File.WriteAllText(filePath, bool2Str(DBSel));
                filePath = System.IO.Path.Combine(documentsPath, "ship");
                System.IO.File.WriteAllText(filePath, ship);
                filePath = System.IO.Path.Combine(documentsPath, "SciSel");
                System.IO.File.WriteAllText(filePath, bool2Str(SciSel));
                filePath = System.IO.Path.Combine(documentsPath, "prec");
                System.IO.File.WriteAllText(filePath, prec);
                filePath = System.IO.Path.Combine(documentsPath, "photoCount");
                System.IO.File.WriteAllText(filePath, photoCount);
                filePath = System.IO.Path.Combine(documentsPath, "report");
                System.IO.File.WriteAllText(filePath, report);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** SaveConfig failed: " + e.ToString());
            }
        }

        public void RestoreConfig()
        {
            try
            {
                var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var filePath = System.IO.Path.Combine(documentsPath, "emailAdr");
                emailAdr = System.IO.File.ReadAllText(filePath);
                filePath = System.IO.Path.Combine(documentsPath, "textAdr");
                textAdr = System.IO.File.ReadAllText(filePath);
                filePath = System.IO.Path.Combine(documentsPath, "emailSel");
                emailSel = str2Bool(System.IO.File.ReadAllText(filePath));
                filePath = System.IO.Path.Combine(documentsPath, "textSel");
                textSel = str2Bool(System.IO.File.ReadAllText(filePath));
                filePath = System.IO.Path.Combine(documentsPath, "DBSecret");
                DBSecret = System.IO.File.ReadAllText(filePath);
                filePath = System.IO.Path.Combine(documentsPath, "DBSel");
                DBSel = str2Bool(System.IO.File.ReadAllText(filePath));
                filePath = System.IO.Path.Combine(documentsPath, "ship");
                ship = System.IO.File.ReadAllText(filePath);
                filePath = System.IO.Path.Combine(documentsPath, "SciSel");
                SciSel = str2Bool(System.IO.File.ReadAllText(filePath));
                filePath = System.IO.Path.Combine(documentsPath, "prec");
                prec = System.IO.File.ReadAllText(filePath);
                filePath = System.IO.Path.Combine(documentsPath, "photoCount");
                photoCount = System.IO.File.ReadAllText(filePath);
                filePath = System.IO.Path.Combine(documentsPath, "report");
                report = System.IO.File.ReadAllText(filePath);
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** RestoreConfig failed. Reseting values: " + e.ToString());
                emailAdr = "";
                textAdr = "";
                emailSel = false;
                textSel = false;
                DBSecret = "";
                DBSel = false;
                ship = "";
                SciSel = false;
                prec = "1";
                photoCount = "0";
                report = "";
            }
        }

        string bool2Str(Boolean input)
        {
            if (input)
            {
                return "true";
            } else
            {
                return "false";
            }
        }
        Boolean str2Bool(String input)
        {
            if (input.Equals("true"))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public String ReadShipConfigFile(String ship)
        {
            try
            {
                String fileString = "";
                String fileName = ship + "_config.xml";
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = System.IO.Path.Combine(sdCardPath.ToString() + "/files", fileName);
                fileString = System.IO.File.ReadAllText(filePath);
                return fileString;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** ReadShipConfigFile failed: " + e.ToString());
                return "";
            }
        }
        public void WriteShipConfigFile(String ship, String data)
        {
            try
            {
                String fileName = ship + "_config.xml";
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = System.IO.Path.Combine(sdCardPath.ToString() + "/files", fileName);
                System.IO.File.WriteAllText(filePath, data);
                // System.Diagnostics.Debug.WriteLine("***** WriteShipConfigFile finished");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** WriteShipConfigFile failed: " + e.ToString());
            }
        }
        public String ReadFile(String fileName)
        {
            try
            {
                String fileString = "";
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = System.IO.Path.Combine(sdCardPath.ToString() + "/files", fileName);
                fileString = System.IO.File.ReadAllText(filePath);
                // System.Diagnostics.Debug.WriteLine("***** read draft report file: " + fileString);
                return fileString;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** ReadFile failed: " + e.ToString());
                return "";
            }
        }

        public ImageSource ReadShipImageFile(String ship)
        {
            try
            {
                String fileName = ship + "_graphic.png";
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var filePath = System.IO.Path.Combine(sdCardPath.ToString() + "/files", fileName);
                var imageFile = new Java.IO.File(filePath);
                Bitmap bitmap = BitmapFactory.DecodeFile(imageFile.AbsolutePath);
                imageWidth = bitmap.Width;
                imageHeight = bitmap.Height;
                // System.Diagnostics.Debug.WriteLine("******** ReadShipImageFile: image width, height: " + imageWidth + " " + imageHeight);

                ImageSource imgsrc;
                do
                {
                    // System.Diagnostics.Debug.WriteLine("Loop in imgsrc generation");
                    imgsrc = ImageSource.FromStream(() =>
                    {
                        MemoryStream ms = new MemoryStream();
                        bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
                        ms.Seek(0L, SeekOrigin.Begin);
                        return ms;
                    });
                } while (imgsrc == null);
                
                return imgsrc;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** ReadShipImageFile failed: " + e.ToString());
                return null;
            }
        }

        public String[] ReadFolder(int size, String path)
        {
            try
            {
                int cnt = 0;
                String[] fileNames = new String[size];
                var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
                var dirPath = System.IO.Path.Combine(sdCardPath.ToString() + "/files" + path, "");
                var dir = new DirectoryInfo(dirPath);
                foreach (var item in dir.GetFileSystemInfos())
                {
                    // System.Diagnostics.Debug.WriteLine("**** ReadFolder: Directory item: " + item.Name);
                    fileNames[cnt] = item.Name;
                    cnt++;
                    if (cnt == (size - 2))
                    {
                        break;
                    }
                }
                fileNames[cnt] = "";
                return fileNames;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** ReadFolder failed: " + e.ToString());
                return null;
            }            
        }
        public void LoadAsset(String fileName)
        {
            // System.Diagnostics.Debug.WriteLine("***** LoadAsset: " + fileName);
            try
            {
                FileStream fS;
                var destPath = Android.OS.Environment.ExternalStorageDirectory.Path + "/files/" + fileName;
                MainActivity.mainActivity.Assets.Open("DemoShips/" + fileName).CopyTo((fS = new FileStream(destPath, FileMode.OpenOrCreate)));
                fS.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("***** LoadAsset threw exception: " + e.ToString());
            }
        }
        public string[] GetAssetList()
        {
            return MainActivity.aL;
        }
        public int GetScreenWidth()
        {
            return MainActivity.screenWidth;
        }

        public int GetScreenHeight()
        {
            return MainActivity.screenHeight;
        }

        public float GetScreenDensity()
        {
            return MainActivity.density;
        }
        public void AndroidBack()
        {
            MainActivity.mainActivity.AndroidBack();
        }
        public void InvokeKB()
        {
            var i = new Intent(MainActivity.mainActivity, typeof(KBActivity));
            MainActivity.mainActivity.StartActivity(i);
        }
    }
}