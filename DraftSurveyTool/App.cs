using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xamarin.Forms;
using Java.Util;
using System.Text.RegularExpressions;
using static Android.InputMethodServices.InputMethodService;
// using Android.Util;

namespace DraftSurveyTool
{
    public class CustomEntry : Entry { }
    public class CustomEntryStd : Entry { }
    public class CustomLabel : Label { }
    public class CustomLabelRed : Label { }

    public class App : Application
    {
        // AppKey & AppSecret are part of this app setup with Dropbox
        public static String AppKey = "";
        public static String AppSecret = "";
        String title1 = "Draft Tool";
        String configFile = "ConfigFile.txt";            // File that stores which report was last used for a given ship 
        String defaultReport = "REPORT1";   // 
        string reportName = "Draft_Report.xml";   // Draft report file name
        String tag = "DraftApp";
        static int maxDraftPts = 20;    // Max number of draft points allowed. Increasing uses more memory
        static int maxNumShips = 50;    // Max number of ships allowed. Increasing uses more memory
        static int maxReports = 50;     // Max number of reports per ship allowed. Increasing uses more memory
        int configButtonSize = 40;
        String[] dirList = new String[maxNumShips * 3];   // Choose larger of maxReports, maxNumShips * 3   IMPORTANT MANUAL UPDATE
        static int maxPrec = 3;
        // Used to allocate space for entry places and interior draft labels 
        float pixelPerChar = 20.0f;
        float pixelEntryScale = 2.5f;
        float boxDelta = 2.0f;
        float screenfit = 0.99f;    // fit room for screenwidth
        float screenfixed = 15f;
        double stepScale = 1.6;   // scaling factor for entry value minimun spacing
        public static int screenWidth, screenHeight, oldSW, oldSH;
        public float screenDensity;
        BoxView boxLeft, boxLeftL, boxLeftP, boxRight, boxRightL, boxRightP;
        public static Boolean rotateFlag;
        double dValue;
        int orient;
        String[] valueIDs = {"X", "Z", "LBP", "BEAM", "TOPLEFTX", "TOPLEFTZ", "BOTTOMRIGHTX", "BOTTOMRIGHTZ" };  // tags in config xml

        // App Objects
        Button btnShare, btnBar, btnConfig, btnMain, btnShips, btnUnits, btnPhoto, btnReport;
        Button btnRC, btnNS, btnNSDemo, btnRSL, btnSE, btnST, btnSDB;
        int imageWidth, imageHeight, reportCount, cIndex;
        int entryCount = 0;
        int entryFontNormal = 16;
        int entryFontSmall = 15;
        Button[] UB;
        Button[] RB = new Button[maxReports];
        Button[] DELBR = new Button[maxReports];
        Button[,] SB, DELBS;
        CustomEntry[] e = new CustomEntry[maxDraftPts];
        EventHandler<FocusEventArgs>[] eEH = new EventHandler<FocusEventArgs>[maxDraftPts];
        // CustomEntry eTemp = new CustomEntry();
        // EventHandler<FocusEventArgs> eTempEH = null;
        Double[] eVal = new Double[maxDraftPts];
        Boolean[] eTouched = new bool[maxDraftPts];
        String[] eId1 = new String[maxDraftPts];
        String[] eId2 = new String[maxDraftPts];
        String[] rStr = new String[maxDraftPts];
        Button[] DB = new Button[maxDraftPts];
        Dictionary<Guid, int> lookupDS, lookupDR;
        Dictionary<Guid, int> lookup = new Dictionary<Guid, int>();
        Label[] l = new Label[maxDraftPts];
        Label[] l2 = new Label[maxDraftPts];
        Double[] xVal = new Double[maxDraftPts];
        Double[] zVal = new Double[maxDraftPts];
        Double[] xLocA = new Double[maxDraftPts];
        Boolean[] iVal = new Boolean[maxDraftPts];
        BoxView [] bi1 = new BoxView[maxDraftPts];
        BoxView[] bi2 = new BoxView[maxDraftPts];
        Label lbH1, lbH2a, lbH2b, lbH3, lbBl1, lbBl2, lbBl3, lbBl4, lbEE, lbET, lbED, lbSP1, lbSP2;
        Label lUP, lUS, lUU, lbDH1, lbRH1, lbRE1, lbRH2, lbRH3, lbNS1, lbNS2, lbSH1, lbSE1;
        Label lConfig, lInterior, out1, outStat, lUnits, lUVal;
        Switch sEE, sET, sED, sUS;
        Entry eEa, eTN, eDS, eUP, eRpt;
        AbsoluteLayout topBlock, bottomBlock, shipBlock;
        StackLayout stackMain, stackMainL, stackMainP, stackBar, stackMainS, stackPicker, stackPickerL, stackPickerP;
        StackLayout stackInterior, stackUnits, stackUnitsL, stackUnitsP;
        StackLayout stackConfig, stackConfigL, stackConfigP, sTemp;
        StackLayout stackPhoto, stackPhotoL, stackPhotoP, stackReport, stackReportL, stackReportP;
        StackLayout stackShips, stackShipsL, stackShipsP, stackNS, stackShare;
        Image shipImage;
        Image[] iTemp;
        String cString, c2String, oString, ship, shipToDel, report, reportToDel, entryValueS, textData, emailData, DBClicked;
        String shipLoaded = "";
        String parameters = "";
        String[] dirArraySorted;
        Double LBP, Beam, leftBor, topBor, rightBor, botBor, TLX, TLZ, BRX, BRZ;
        Boolean ReadQuotedDone, result, entryEnabled;
        List<String> shipList = new List<String>();
        List<String> shipListPrev = new List<String>();
        List<String> dirListSorted = new List<String>();
        int shipCount, photoCount, entryIndex;
        String EOL = "\r\n";
        ScrollView scrollView;
        Boolean mainView, shipView, unitsView, configView, photoView, reportView, pickerView, shareView;
        // Boolean unitsChg = false;  // Indicates change in units, requires recalculation and display 
        CDisplayOptions CDO;
        GCUnits.hecLengthUnitConstants newUnits, oldUnits;
        Dictionary<String, GCUnits.hecLengthUnitConstants> unitTypesD;
        Dictionary<String, ImageSource> shipImages;
        Dictionary<String, int> shipImagesW, shipImagesH;
        GCUnits.hecLengthUnitConstants[] unitTypes;
        Regex rExp;
        ImageSource shipimgsrc;

        public App()
        {
            CDO = new CDisplayOptions();
            rExp = new Regex("^[a-zA-Z0-9_]*$");     // Valid characters for report name 
            SetupDBButtons();
            ResetStacks();
            stackUnitsL = null;
            stackUnitsP = null;
            stackConfigL = null;
            stackConfigP = null;
            stackShipsL = null;
            stackShipsP = null;
            stackPickerL = null;
            stackPickerP = null;
            boxRightL = null;
            boxRightP = null;
            boxLeftL = null;
            boxLeftP = null;

            ResetEHs();   // Hold Event Handlers (click detectors) for the entry cells so that they can be removed, saves memory 
            SetupEs();    // Creates draft entry cells one time, saves memory
            SetupSBs();   // Creates ship buttons one time, saves memory

            shipImage = new Image
            {
                Aspect = Aspect.AspectFit
            };

            // Buttons 
            btnShare = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Share",
                FontSize = 20
            };
            btnShare.IsEnabled = true;
            btnConfig = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Share Destinations",
                FontSize = 20
            };
            btnMain = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Return to Main Screen",
                FontSize = 20
            };
            btnShips = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Ship Selection Screen",
                FontSize = 20
            };
            btnUnits = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Unit Selection Screen",
                FontSize = 20
            };
            btnPhoto = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Draft Mark Photo",
                FontSize = 20
            };
            btnReport = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Select/Create Report",
                FontSize = 20
            };
            btnRC = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Create New Report",
                FontSize = 20
            };
            btnRSL = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Refresh Ship List",
                FontSize = 20
            };
            btnSE = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Email",
                FontSize = 20
            };
            btnST = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Text",
                FontSize = 20
            };
            btnSDB = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Dropbox",
                FontSize = 20
            };

            // Labels 
            lbH1 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Draft Surveyor",
                FontAttributes = FontAttributes.Bold,
                FontSize = 25
            };
            lbH2a = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "",
                FontSize = 23
            };
            lbH2b = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "",
                // BackgroundColor = Color.Black,
                // TextColor = Color.Black,
                FontSize = 20
            };
            lbH3 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "",
                FontSize = 20
            };
            lConfig = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Changes saved upon return to Main Screen",
                FontSize = 20
            };
            lInterior = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Enter Interior Draft Values Below",
                FontSize = 20
            };
            lbNS1 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "No Ship configurations Found",
                FontSize = 20
            };
            lbNS2 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Please connect to CargoMax Computer and download ship configurations. Then press 'OK'",
                FontSize = 20
            };
            lbSH1 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Select Share Destination",
                FontSize = 20
            };
            lbSE1 = new CustomLabelRed
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "No Share Destinations Setup",
                TextColor = Color.Red,
                FontSize = 20
            };

            btnNS = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "OK",
                FontSize = 20
            };
            btnNSDemo = new Button
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Load Demo Ships",
                FontSize = 20
            };

            lUnits = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Units: ",
                // TextColor = Color.Black,
                FontSize = 20
            };
            // Units selection screen
            lUVal = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "",
                FontSize = 20
            };
            lUP = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Precision (0 to " + maxPrec + " ): ",
            FontSize = 20
            };
            eUP = new CustomEntryStd
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Placeholder = CDO.Format.Draft2LengthPrec.ToString(),
                FontSize = 20,
                IsEnabled = true
            };
            lUS = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Scientific?    ",   // Set after CDisplayOptions object created 
                FontSize = 20
            };
            sUS = new Switch
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Scale = 1.5
            };
            lUU = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Select Units",  
                FontSize = 20
            };
            out1 = new CustomLabel
            {
                Text = "Result: ",
                HorizontalOptions = LayoutOptions.Start,
                FontSize = 20
            };
            outStat = new CustomLabelRed
            {
                Text = "",
                HorizontalOptions = LayoutOptions.Start,
                TextColor = Color.Red,
                FontSize = 20
            };
            lbBl1 = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = " ",
                FontSize = 20
            };
            lbBl2 = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = " ",
                FontSize = 20
            };
            lbBl3 = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = " ",
                FontSize = 20
            };
            lbBl4 = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = " ",
                FontSize = 20
            };
            lbEE = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Enable Email     ",
                FontSize = 20
            };
            sEE = new Switch
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Scale = 1.6
            };
            eEa = new CustomEntryStd
            {
                Keyboard = Keyboard.Email,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Placeholder = "Email Address",
                FontSize = 20,
                IsEnabled = true
            };
            lbET = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Enable Texting  ",
                FontSize = 20
            };
            sET = new Switch
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Scale = 1.6
            };
            eTN = new CustomEntryStd
            {
                Keyboard = Keyboard.Telephone,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Placeholder = "Text Number",
                FontSize = 20,
                IsEnabled = true
            };
            lbED = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Text = "Enable Dropbox ",
                FontSize = 20
            };
            sED = new Switch
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Scale = 1.6
            };
            eDS = new CustomEntryStd
            {
                Keyboard = Keyboard.Text,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Placeholder = "App Secret",
                FontSize = 20,
                IsEnabled = true,
                IsPassword = true
            };
            lbDH1 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Select Draft Mark for Photo",
                FontSize = 20
            };
            lbRH1 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Select Report Folder",
                FontSize = 20
            };
            lbRE1 = new CustomLabelRed
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "",
                TextColor = Color.Red,
                FontSize = 20
            };
            lbRH2 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Create New Report",
                FontSize = 20
            };
            lbRH3 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "(Valid are letters, digits, _)",
                FontSize = 18
            };
            eRpt = new CustomEntryStd
            {
                Keyboard = Keyboard.Text,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Placeholder = "Enter New Report Name",
                FontSize = 20,
                IsEnabled = true
            };
            // Ship selection menu objects 
            lbSP1 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Select Ship",
                FontSize = 20
            };
            lbSP2 = new CustomLabel
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = " ",
                FontSize = 20
            };


            // Array to hold supported unit values 
            unitTypes = new GCUnits.hecLengthUnitConstants[]
            {
                GCUnits.hecLengthUnitConstants.heclengthm,
                GCUnits.hecLengthUnitConstants.heclengthft,
                GCUnits.hecLengthUnitConstants.hecLengthYards
            };
            unitTypesD = new Dictionary<string, GCUnits.hecLengthUnitConstants>
            {
                {"m", GCUnits.hecLengthUnitConstants.heclengthm },
                {"ft", GCUnits.hecLengthUnitConstants.heclengthft },
                {"yd", GCUnits.hecLengthUnitConstants.hecLengthYards }
            };
            CreateUBs();    // Setup unit buttons
            SetupStackConfig();

            // Upper right hand corner options button
            btnBar = new Button();
            btnBar.Image = (FileImageSource)ImageSource.FromFile("baricon40x40.jpg");   // Upper right hand corner icon 
            btnBar.HeightRequest = 30;
            btnBar.WidthRequest = 40;
            btnBar.HorizontalOptions = LayoutOptions.End;
            btnBar.BackgroundColor = Color.Black;
            btnBar.BorderWidth = 0;
            btnBar.BorderRadius = 0;

            stackBar = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.End,
                // VerticalOptions = LayoutOptions.Start,
                HeightRequest = 30,
                WidthRequest = 40
                // HeightRequest = configButtonSize,
                // WidthRequest = configButtonSize,
                // Padding = 0,
                // Spacing = 0
            };
            stackBar.Children.Add(lbBl1);    // Forces button to right corner 
            stackBar.Children.Add(btnBar);

            // StackLayout for no ships (NS)
            stackNS = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 2,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    lbH1,
                    lbBl1,
                    lbNS1,
                    lbBl2,
                    lbNS2,
                    lbBl3,
                    btnNS,
                    lbBl4,
                    btnNSDemo
                }
            };

            // ScrollView allows scrolling of display. Options button not included
            scrollView = new ScrollView
            {
            };

            // Contains options button & all of the scrollView
            stackMainS = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.Start,
            };
            stackMainS.Children.Add(stackBar);
            stackMainS.Children.Add(scrollView);

            // MainPage is the main page for this framework
            MainPage = new ContentPage
            {
                Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5),       // Requested for IOS
                // Content = stackMainS
                Content = stackMainS
            };

            // Detects screen rotations - necessary to recreate display for good view, especially the ships because of absoluate layout
            // Detects other things beside screen rotation, so you must check
            MainPage.SizeChanged += (sender, args) =>
            {
                // System.Diagnostics.Debug.WriteLine("******** main page change");
                // Log.Info(tag, "******** main page change");
                // Check if there was a screen rotation 
                oldSW = screenWidth;
                screenDensity = DependencyService.Get<IConfigRepo>().GetScreenDensity();
                screenWidth = (int)(((float)DependencyService.Get<IConfigRepo>().GetScreenWidth()) / screenDensity);
                screenHeight = (int)(((float)DependencyService.Get<IConfigRepo>().GetScreenHeight()) / screenDensity);
                // System.Diagnostics.Debug.WriteLine("sw,sh = " + screenWidth + " " + screenHeight);
                if ((oldSW != screenWidth))
                {
                    // If screen rotation, then do the following redraw actions 
                    // System.Diagnostics.Debug.WriteLine("******** main page has rotation - reload");
                    SetupBoxes();
                    if (screenWidth > 400)
                    {  // Bigger font looks better for label, but only possible if enough width. 
                        lbH1.FontSize = 25;
                    }
                    else
                    {
                        lbH1.FontSize = 20;
                    }
                    // Select landscape or portrait background image. They are different. 
                    if (screenWidth > screenHeight)
                    {
                        MainPage.BackgroundImage = "drawable/background_land.png";
                    } else
                    {
                        MainPage.BackgroundImage = "drawable/background_port.png";
                    }

                    if (shipCount != 0)
                    {
                        // Prevents undesired keyboard appearance 
                        for (int i = 0; i < entryCount; i++)
                        {
                            e[i].Unfocus();
                        }         

                        // Determines what view you were showing. Regenerates that and shows it.
                        // Necessary to have consistently good display with rotation 
                        if (shipView)    // Viewing ship list 
                        {
                            SetupStackShips();
                            // ResetScrollView(stackShips);
                            scrollView.Content = null;
                            scrollView.Content = stackShips;
                            GC.Collect();
                        } else if (unitsView)
                        {
                            SetupStackUnits();
                            scrollView.Content = null;
                            scrollView.Content = stackUnits;
                        } else if (pickerView)   // Options menu 
                        {
                            SetupStackPicker();
                            scrollView.Content = null;
                            scrollView.Content = stackPicker;
                        } else if (photoView)
                        {
                            SetupStackPhoto();
                            scrollView.Content = null;
                            scrollView.Content = stackPhoto;
                        } else if (reportView)
                        {
                            SetupStackReport(lbRE1.Text);
                            scrollView.Content = null;
                            scrollView.Content = stackReport;
                        } else if (mainView)    // View of ship & draft entry values 
                        {
                            SetupStackMain();
                            scrollView.Content = null;
                            scrollView.Content = stackMain;
                        } else if (configView)   // share options setup view 
                        {
                            SetupStackConfig();
                            scrollView.Content = null;
                            scrollView.Content = stackConfig;
                        } else if (shareView)     // Appears when Share is clicked. User selects how to share 
                        {
                            SetupStackShare();
                            scrollView.Content = null;
                            scrollView.Content = stackShare;
                        }
                        GC.Collect();
                    }
                }
            };

            // Start operations
            // System.Diagnostics.Debug.WriteLine("******** App starting *******");
            screenDensity = DependencyService.Get<IConfigRepo>().GetScreenDensity();  // Some ship graphic calculation req scaling screen width/height values 
            screenWidth = (int) (((float) DependencyService.Get<IConfigRepo>().GetScreenWidth()) / screenDensity);
            screenHeight = (int) (((float) DependencyService.Get<IConfigRepo>().GetScreenHeight()) / screenDensity);
            SetupBoxes();
            // Setup one time, saves memory 
            shipImages = new Dictionary<String, ImageSource>();
            shipImagesH = new Dictionary<String, int>();
            shipImagesW = new Dictionary<String, int>();
            eDS.Text = AppSecret;
            if (screenWidth > 400)
            {
                lbH1.FontSize = 25;
            } else
            {
                lbH1.FontSize = 20;
            }
            if (screenWidth > screenHeight)
            {
                MainPage.BackgroundImage = "drawable/background_land.png";
            }
            else
            {
                MainPage.BackgroundImage = "drawable/background_port.png";
            }
            // Returns if folder exists. If not, creates it 
            Boolean HExists = DependencyService.Get<ISendEmail>().CreateFolder(folder, false, true);   // Create folder if it doesn't exist
            // If not, assume initial instation and load with demo ships from Assets
            if (!HExists)
            {
                LoadDemoShips();
            }
            entryEnabled = false;
            CDO.Format.Draft2LengthScientific = false;   // Scientific notation disabled
            newUnits = CDO.Units.Draft2Length;
            ReadShipList();   // Read list of all ship configuration/graphic files on device 
            ResetViews();     // All views boolean values reset
            if (shipCount == 0)
            {   // If no ships, notify uses to add ships before continuing 
                scrollView.Content = null;
                scrollView.Content = stackNS;
            } else
            {
                ReadConfig();     // Read app configuration data 
                ReadShipConfig();   // Read specific ship configuration data 
                SetupStackMain();   // Setup the main stack layout 
                scrollView.Content = null;
                scrollView.Content = stackMain;
                mainView = true;
                GC.Collect();    // Good time do garbage collect. Not sure this helps. 
            }

            // Setup various Event Handlers (Click events) 
            sUS.Toggled += (sender, args) =>
            {
                CDO.Format.Draft2LengthScientific = args.Value;
                // unitsChg = true;
            };
            // Precision entry 
            eUP.Completed += (sender, e) => {
                int val;
                var entry = sender as Entry;
                if (entry != null)
                {
                    if (!Int32.TryParse(entry.Text, out val))
                    {
                        val = 0;
                    }
                    if (val < 0)
                    {
                        val = 0;
                    }
                    if (val > maxPrec)
                    {
                        val = maxPrec;
                    }
                    entry.Text = val.ToString();
                    CDO.Format.Draft2LengthPrec = val;
                    // unitsChg = true;
                }
            };
            // Share button. Shares output
            btnShare.Clicked += (sender, args) =>
            {
                SetupStackShare();
                ResetViews();
                shareView = true;
                scrollView.Content = null;
                scrollView.Content = stackShare;
            };
            // Share by email, 3 step process 
            btnSE.Clicked += (sender, args) =>
            {
                String emailAdr = eEa.Text;
                String emailSubj = "Draft Report, Ship: " + ship + ", Report: " + report;
                Boolean stillOK = true;
                int fileCnt = 0;
                if (!DependencyService.Get<ISendEmail>().CreateEmail(emailAdr, emailSubj))   // Create email intent 
                {
                    stillOK = false;
                } else
                {
                    // Create list of all items in report folder, one report and 0 or more photos 
                    dirList = DependencyService.Get<IConfigRepo>().ReadFolder(maxReports, "/" + ship + "/" + report);
                    while (!(dirList[fileCnt].Equals("")))  // Get number of valid file names for sorting a new array of exact size
                    {
                        fileCnt++;
                    }
                    if (fileCnt > 0)
                    {
                        // Send items in sorted order 
                        dirListSorted.Clear();
                        for (int i = 0; i < fileCnt; i++)   // directory list of exact size for sorting
                        {
                            dirListSorted.Add(ship + "/" + report + "/" + dirList[i]);
                        }
                        dirListSorted.Sort();
                        // Send array of files to send
                        stillOK = DependencyService.Get<ISendEmail>().AttachEmail(dirListSorted);
                    }
                    if (stillOK)
                    {  // Send command to send email 
                        stillOK = DependencyService.Get<ISendEmail>().SendEmail();
                    }
                }                
                if (!stillOK)
                {
                    outStat.Text += "Email failed. ";
                }
                // Go back to main ship view 
                ResetViews();
                SetupStackMain();
                mainView = true;
                scrollView.Content = null;
                scrollView.Content = stackMain;
            };
            // Share by text 
            btnST.Clicked += (sender, args) =>
            {
                String textAdr = eTN.Text;
                SaveReport();  // Necessary to make sure textData is current 
                result = DependencyService.Get<ISendEmail>().SendText(textAdr, textData);
                // System.Diagnostics.Debug.WriteLine("Sending text, adr/data = " + textAdr + " ==== " + textData);
                if (!result)
                {
                    outStat.Text += "Text failed. ";
                }
                // Go back to main ship view 
                ResetViews();
                SetupStackMain();
                mainView = true;
                scrollView.Content = null;
                scrollView.Content = stackMain;
            };
            // Share by Dropbox 
            btnSDB.Clicked += (sender, args) =>
            {
                int fileCnt = 0;
                dirList = DependencyService.Get<IConfigRepo>().ReadFolder(maxReports, "/" + ship + "/" + report);
                while (!(dirList[fileCnt].Equals("")))  // Get number of valid file names for sorting a new array of exact size
                {
                    fileCnt++;
                }
                if (fileCnt > 0)
                {
                    dirListSorted.Clear();
                    for (int i = 0; i < fileCnt; i++)   // directory list of exact size for sorting
                    {
                        dirListSorted.Add(ship + "/" + report + "/" + dirList[i]);
                    }
                    dirListSorted.Sort();
                    // Send array of all files to upload to dropbox 
                    DependencyService.Get<ISendEmail>().WriteDBBlock(AppKey, eDS.Text, dirListSorted);
                }
                // Go back to main ship view 
                ResetViews();
                SetupStackMain();
                mainView = true;
                scrollView.Content = null;
                scrollView.Content = stackMain;
            };
            // Select upper right corner options button
            btnBar.Clicked += (sender, args) =>
            {
                if (shipCount != 0)     // Button only functions if there are ships 
                {   // Standard sequence to setup layout, reset other view flags, set this flag (for rotation), clear scrollView, set scrollView
                    SetupStackPicker();      // Setup options picker display
                    ResetViews();       // Clears all flags for other layouts 
                    pickerView = true;
                    scrollView.Content = null;       // Seems to reduce occurance of image artifacts 
                    scrollView.Content = stackPicker;
                }
            };
            // Select option to configure share location options  
            btnConfig.Clicked += (sender, args) =>
            {
                // ReadConfig();
                SetupStackConfig();      // Setup share configuration layout 
                ResetViews();
                configView = true;
                scrollView.Content = null;
                scrollView.Content = stackConfig;
            };
            // Select draft photo option 
            btnPhoto.Clicked += (sender, args) =>
            {
                SetupStackPhoto();
                ResetViews();
                photoView = true;
                scrollView.Content = null;
                scrollView.Content = stackPhoto;
            };
            // Select units options 
            btnUnits.Clicked += (sender, args) =>
            {
                eUP.Placeholder = CDO.Format.Draft2LengthPrec.ToString();    // placeholder view for precision is current precision 
                // System.Diagnostics.Debug.WriteLine("***** btnunits clicked, units/newunits are: " + CDO.Units.Draft2Length + " " + newUnits);
                newUnits = CDO.Units.Draft2Length;
                UpdateUnitBolds();    // Makes sure selected units buttion is bolded, others not. 
                SetupStackUnits();
                ResetViews();
                unitsView = true;
                scrollView.Content = null;
                scrollView.Content = stackUnits;
            };
            // Select main view (ship + draft entry) 
            btnMain.Clicked += (sender, args) =>
            {
                // btn.IsEnabled = true;
                outStat.Text = "";        // Disabled feature 
                WriteConfig();            // Configuration changes (email, text address, enables, etc) written 
                UpdateForNewUnits();      // Update values for new units, if changed 
                ReadShipConfig();         // Read ship configuration data, if new ship 
                SetupStackMain();
                ResetViews();
                mainView = true;
                scrollView.Content = null;
                scrollView.Content = stackMain;
            };
            // Select ship selection view 
            btnShips.Clicked += (sender, args) =>
            {
                ResetViews();
                shipView = true;
                SetupStackShips();
                // ResetScrollView(stackShips);
                scrollView.Content = null;
                scrollView.Content = stackShips;
            };
            // Read Ship List. Reads list of ships in folder 
            btnRSL.Clicked += (sender, args) =>
            {
                ReadShipList();    // Read list of ships in folder. Needs config and graphic files 
                ResetViews();
                if (shipCount == 0)
                {    // If no ships, show view no ships 
                    scrollView.Content = null;
                    scrollView.Content = stackNS;
                }
                else
                {  // Show updated list of ships to chooose
                    SetupStackShips();
                    shipView = true;
                    // ResetScrollView(stackShips);
                    scrollView.Content = null;
                    scrollView.Content = stackShips;
                }
            };
            // Select report view 
            btnReport.Clicked += (sender, args) =>
            {
                SetupStackReport("");     // Clears error message while showing reports 
                ResetViews();
                reportView = true;
                scrollView.Content = null;
                scrollView.Content = stackReport;
            };
            // Clicked button on no ships view to indicate ships are loaded 
            btnNS.Clicked += (sender, args) =>
            {
                ReadShipList();    // Get ship list from folder 
                GC.Collect();      // Good time to do garbage collection. Not certain this does anything. Literature statements also mixed. 
                if (shipCount != 0) 
                {   // If there are now ships in the folder 
                    ReadConfig();     // Read configuration data. Will tell last desired ship 
                    // WriteConfig();
                    shipLoaded = "";       // Indicate no previous ship. You must load current ship 
                    ReadShipConfig();      // Read ship configuration (draft point) information 
                    ResetViews();
                    if (shipCount == 1)
                    {
                        SetupStackMain();
                        mainView = true;
                        scrollView.Content = null;
                        scrollView.Content = stackMain;
                    }
                    else
                    {
                        SetupStackShips();
                        shipView = true;
                        // ResetScrollView(stackShips);
                        scrollView.Content = null;
                        scrollView.Content = stackShips;
                    }
                }
            };
            // Button to reload demo ships 
            btnNSDemo.Clicked += (sender, args) =>
            {
                LoadDemoShips();
                ReadShipList();
                if (shipCount != 0)
                {
                    ReadConfig();
                    // WriteConfig();
                    shipLoaded = "";
                    ReadShipConfig();
                    SetupStackMain();
                    ResetViews();
                    if (shipCount == 1)
                    {
                        mainView = true;
                        scrollView.Content = null;
                        scrollView.Content = stackMain;
                    }
                    else
                    {
                        SetupStackShips();
                        shipView = true;
                        // ResetScrollView(stackShips);
                        scrollView.Content = null;
                        scrollView.Content = stackShips;
                    }
                    GC.Collect();
                }
            };
            // Button to create report 
            btnRC.Clicked += (sender, args) =>
            {
                String reportInp = eRpt.Text.ToUpper();    // Get name of report. Report names are all upper case. Think Android automatically changes that. 
                if ((reportInp.Length != 0) && rExp.IsMatch(reportInp))    // Check for valid report file name 
                {
                    if (DuplicateReport(reportInp))      // Check for duplicate report name 
                    {
                        stackReportL = null;      // Required for apparent bug
                        stackReportP = null;
                        SetupStackReport("Duplicate Report Name");    // Error message 
                        scrollView.Content = null;
                        scrollView.Content = stackReport;
                    } else
                    {
                        reportCount++;
                        stackReportL = null;
                        stackReportP = null;
                        stackMainL = null;
                        stackMainP = null;
                        report = reportInp;     // Set to new report 
                        lbH2b.Text = report;
                        DependencyService.Get<ISendEmail>().CreateFolder(ship + "/" + report, true, true);    // Create report folder 
                        DependencyService.Get<ISendEmail>().WriteFile(report, ship + "/" + configFile);       // Set as default report for ship 
                        ClearEntryValues();       // Clear draft entry values 
                        SetupStackReport("");     // Setup report view 
                        ResetViews();
                        reportView = true;
                        scrollView.Content = null;
                        scrollView.Content = stackReport;
                    }
                } else
                {
                    stackReportL = null;   // Required for apparent bug
                    stackReportP = null;
                    SetupStackReport("Invalid Report Name");    // Error message for invalid report 
                    scrollView.Content = null;
                    scrollView.Content = stackReport;
                }
            };

            // Three toggle buttons to enable sharing by email, text, dropbox respectively. They control the enabling of the entry fields
            // The Dropbox entry field is not used. It is hard coded. When user specifies Dropbox, Android will ask user to authorize and log into the 
            //  Dropbox he/she wants to use. 
            sEE.Toggled += (sender, args) =>
            {
                eEa.IsEnabled = args.Value;
            };
            sET.Toggled += (sender, args) =>
            {
                eTN.IsEnabled = args.Value;
            };
            sED.Toggled += (sender, args) =>
            {
                eDS.IsEnabled = args.Value;
            };
        }

        void ReadConfig()  // requires a valid ship available. 
        {
            DependencyService.Get<IConfigRepo>().RestoreConfig();
            eEa.Text = DependencyService.Get<IConfigRepo>().emailAdr;
            eTN.Text = DependencyService.Get<IConfigRepo>().textAdr;
            sEE.IsToggled = DependencyService.Get<IConfigRepo>().emailSel;
            sET.IsToggled = DependencyService.Get<IConfigRepo>().textSel;
            // eDS.Text = DependencyService.Get<IConfigRepo>().DBSecret;
            eDS.Text = AppSecret;    // Dropbox App Secret is hard coded now. Not necessary 
            sED.IsToggled = DependencyService.Get<IConfigRepo>().DBSel;
            ship = DependencyService.Get<IConfigRepo>().ship;
            DBClicked = "";
            ResetStacks();
            lbH2a.Text = ship;
            // sUS.IsToggled = DependencyService.Get<IConfigRepo>().SciSel;
            // CDO.Format.Draft2LengthScientific = sUS.IsToggled;
            sUS.IsToggled = false;
            CDO.Format.Draft2LengthScientific = false;    // Scientific functionality is disabled. Not enough room to display 
            eUP.Placeholder = DependencyService.Get<IConfigRepo>().prec;
            CDO.Format.Draft2LengthPrec = Convert.ToInt32(eUP.Placeholder);
            photoCount = Convert.ToInt32(DependencyService.Get<IConfigRepo>().photoCount);   // count is appended to each photo file name to make unique
            // report = DependencyService.Get<IConfigRepo>().report;
            if (!ShipValid())
            {   // if ship read does not exist, use set to first ship 
                ship = shipList[0];
                lbH2a.Text = ship;
            }
            DependencyService.Get<ISendEmail>().CreateFolder(ship, true, true);  // only creates ship folder if doesn't exist
            ReadReportName();    // Load the current report values 

            eEa.IsEnabled = sEE.IsToggled;
            eTN.IsEnabled = sET.IsToggled;
            eDS.IsEnabled = sED.IsToggled;
        }

        void WriteConfig()
        {
            DependencyService.Get<IConfigRepo>().emailAdr = eEa.Text;
            DependencyService.Get<IConfigRepo>().textAdr = eTN.Text;
            DependencyService.Get<IConfigRepo>().emailSel = sEE.IsToggled;
            DependencyService.Get<IConfigRepo>().textSel = sET.IsToggled;
            DependencyService.Get<IConfigRepo>().DBSecret = eDS.Text;
            DependencyService.Get<IConfigRepo>().DBSel = sED.IsToggled;
            DependencyService.Get<IConfigRepo>().ship = ship;
            DependencyService.Get<IConfigRepo>().SciSel = sUS.IsToggled;
            DependencyService.Get<IConfigRepo>().prec = CDO.Format.Draft2LengthPrec.ToString();
            DependencyService.Get<IConfigRepo>().photoCount = photoCount.ToString();
            DependencyService.Get<IConfigRepo>().report = "";
            DependencyService.Get<IConfigRepo>().SaveConfig();
        }

        // Updates values for new units, if user changed 
        void UpdateForNewUnits()
        {
            if (!ship.Equals(shipLoaded))   // If user changed ship, no need to update
            { 
                return;
            }
            // unitsChg = false;
            oldUnits = CDO.Units.Draft2Length;   // present units that draft values are in 
            // Set CDisplayOptions for new units 
            CDO.Units.SetUnit(GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance, (int)newUnits);   
            if (newUnits != oldUnits)   // Update units if user changed them 
            {
                UpdateShipConfigNewUnits();
            }
            // This block may not be needed anymore
            // With old KB you could have entry values and displayed values not updated. This made sure those values got updated.
            // For draft entry, eVal[] holds the actual data value. e[].Text shows that which is displayed. 
            for (int i = 0; i < entryCount; i++)
            {
                // System.Diagnostics.Debug.WriteLine("****** UpdateForNewUnits processing i = " + i);
                if (eTouched[i])    // In past, it was set when entry made, but text field not formatted & eVal[] not calculated. 
                {
                    // System.Diagnostics.Debug.WriteLine("****** was touched");
                    // Calcuate expression, put value in dValue. Check if error
                    Boolean pass = CDO.bEvaluateUnitExpressionCustom(e[i].Text, (int)CDO.Units.Draft2Length, (int)CDO.Units.Draft2Length, ref dValue, CProperty.hecFeetFormat.hecFeetFtIn);
                    if (!pass)   // If invalid expression, assign to 0
                    {
                        dValue = 0;
                    }
                    // System.Diagnostics.Debug.WriteLine("****** result from bE: " + dValue);
                } else
                {
                    dValue = eVal[i];
                    // System.Diagnostics.Debug.WriteLine("****** result from eVal: " + dValue);
                }
                if (newUnits != oldUnits)    // Do units conversion if user changed them
                {    
                    dValue = CDO.GUnits.convert_migname(dValue, (int)oldUnits, (int)CDO.Units.Draft2Length);
                }
                if (dValue == 0)
                {
                    e[i].Text = "";
                } else
                {
                    e[i].Text = CDO.FormatUnit(dValue, (int)CDO.Units.Draft2Length, GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance, false);
                }
                if (e[i].Text.Length > 4)    // Adjust font down if entry field greater than 4 characters.
                {
                    e[i].FontSize = entryFontSmall;
                } else
                {
                    e[i].FontSize = entryFontNormal;
                }                       
                eVal[i] = dValue;
                // System.Diagnostics.Debug.WriteLine("Touched set to false, i = " + i);
                eTouched[i] = false;
            }
            // newUnits = CDO.Units.Draft2Length;    // Should have no effect 
        }
        // When user changes units the ship config file is updated to reflect the new units
        // Reads the ship config file xml. It copies the file back as it was, except the units line is changed to the new units
        void UpdateShipConfigNewUnits()
        {
            String t2Str;
            int idx1, idx2;
            oString = "";
            // Read the whole xml file into one long string 
            c2String = DependencyService.Get<IConfigRepo>().ReadShipConfigFile(ship);
            cIndex = 0;
            String line = ReadCLine();   // Read one line 
            while (line.Length != 0)   // If there is still another line 
            {
                if (line.Contains("units ") && line.Contains("value"))    // If the line is the units specifying line 
                {
                    // Replace the line with the new units line 
                    oString += "  <units value=\"" +
                        CDO.Units.UnitName(GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance) + "\" />";
                    // System.Diagnostics.Debug.WriteLine("***** UpdateShipConfigNewUnits line replaced: " + line);
                    // System.Diagnostics.Debug.WriteLine("***** UpdateShipConfigNewUnits new units: " + 
                    //   CDO.Units.UnitName(GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance));
                } else
                {
                    cString = line;
                    t2Str = GetNextElement();
                    // Check if one of the value variables "X", "Z", "LBP", "BEAM", "TOPLEFTX", "TOPLEFTZ", "BOTTOMRIGHTX", "BOTTOMRIGHTZ"
                    // These contain numeric values that must be adjusted for the new units 
                    if (valueIDs.Contains(t2Str.ToUpper()))
                    { // Find location of numeric values 
                        idx1 = line.IndexOf("\"");
                        idx2 = line.Substring(idx1 + 1).IndexOf("\"");
                        if ((idx1 != -1) && (idx2 != -1))
                        {
                            t2Str = line.Substring(idx1 + 1, idx2);
                            try
                            {
                                dValue = Convert.ToDouble(t2Str);
                            }
                            catch (Exception e)
                            {
                                dValue = 0.0;
                            }
                            // Convert value according to units change 
                            dValue = CDO.GUnits.convert_migname(dValue, (int)oldUnits, (int)CDO.Units.Draft2Length);
                            // Put the string with the new value in the xml output 
                            oString += line.Substring(0, idx1 + 1) + dValue + line.Substring(idx1 + 1 + idx2);
                        } else
                        {  // Line does not change 
                            oString += line;
                        }
                    } else
                    {   // Line does not change 
                        oString += line;
                    }
                }
                line = ReadCLine();
            }
            // Over write file with new xml with new units 
            DependencyService.Get<IConfigRepo>().WriteShipConfigFile(ship, oString);
            // System.Diagnostics.Debug.WriteLine("***** UpdateShipConfigNewUnits finished: " + oString);
        }
        // Read line from the file 
        String ReadCLine()
        {
            String oStr = "";
            String tStr;
            // Read until you get to the end of the line 
            while ((cIndex < c2String.Length) && (((tStr = c2String.Substring(cIndex,1)).Equals("\r")) || tStr.Equals("\n")))
            {
                oString += tStr;
                cIndex++;
            }
            while ((cIndex < c2String.Length) && (!((tStr = c2String.Substring(cIndex, 1)).Equals("\r")) && !tStr.Equals("\n")))
            {
                oStr += tStr;
                cIndex++;
            }
            return oStr;
        }
        void ReadShipConfig()
        {
            String rString, uString;
            if (ship.Equals(shipLoaded))
            {  // If the ship did not change, don't reload
                return;
            }
            entryEnabled = false;
            // unitsChg = false;
            shipLoaded = ship;
            // Create ship report folder, if it does not exist 
            DependencyService.Get<ISendEmail>().CreateFolder(ship, true, true);  // only creates folder if doesn't exist
            // Read ship config file that contains the name of the last report accessed for this ship 
            ReadReportName();

            // Remove previous Event Handlers (click detection) if they exist.
            // Documentation said this is necessary for garbage collection to eliminate unreferenced objects 
            // The EH is removed and set to null 
            if ((entryCount != 0) && (eEH[0] != null))
            {
                System.Diagnostics.Debug.WriteLine("***** Event Handlers removed");
                for (int i = 0; i < entryCount; i++)
                {
                    e[i].Focused -= eEH[i];
                    eEH[i] = null;
                }
            }

            entryCount = 0;
            // Reset default values
            LBP = 100;
            Beam = 50;
            TLX = 0;
            TLZ = 0;
            BRX = 0;
            BRZ = 0;
            // Reads the whole ship config xml file into a single string 
            cString = DependencyService.Get<IConfigRepo>().ReadShipConfigFile(ship);
            Boolean done = false;
            Boolean doingDraftPts = false;
            while (!done)
            {
                // Gets the next xml tag 
                rString = GetNextElement();
                if (rString.Length == 0)
                {
                    done = true;
                    break;
                }
                // Go based upon the xml tag 
                switch(rString.ToUpper())
                {
                    case "?XML":
                        // Presently, we're not using this information
                        break;

                    case "SHIP_CONFIGURATION":
                        // Presently this is assumed and not checked, since only ship configuration is provided.
                        // Future may require checking
                        break;

                    case "/SHIP_CONFIGURATION":
                        break;

                    case "NAME":
                        // This should be the same as the file name.
                        // Presently we do not verify this. 
                        break;

                    case "/NAME":
                        // This should be the same as the file name.
                        // Presently we do not verify this. 
                        break;

                    case "DATE_TIME":
                        // This is the time stamp for the configuration file
                        // We don't do anything with this 
                        break;

                    case "/DATE_TIME":
                        // This is the time stamp for the configuration file
                        break;

                    case "UNITS":
                        uString = ReadQuoted();   // Read the value in the double quotes. This is the units 
                        setUnits(uString);
                        break;

                    case "DRAFT_POINTS":     // Indicates that a draft point is being processed
                        doingDraftPts = true;
                        // System.Diagnostics.Debug.WriteLine("Processing draft points");
                        break;

                    case "/DRAFT_POINTS":     // Indicates done with processing draft point 
                        doingDraftPts = false;
                        // System.Diagnostics.Debug.WriteLine("Done processing draft points");
                        break;

                    case "POINT":    // Indicates a draft point is specified 
                        if (doingDraftPts)
                        {
                            ProcessDraftPt();   // Processes draft point xml through /POINT
                        }
                        break;

                    // Detect specific values 
                    case "LBP":
                        LBP = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("LBP: " + LBP);
                        break;

                    case "BEAM":
                        Beam = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("beam: " + Beam);
                        break;

                    case "TOPLEFTX":
                        TLX = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("TopLeftX: " + TLX);
                        break;

                    case "TOPLEFTZ":
                        TLZ = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("TopLeftZ: " + TLZ);
                        break;

                    case "BOTTOMRIGHTX":
                        BRX = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("BottomRightX: " + BRX);
                        break;

                    case "BOTTOMRIGHTZ":
                        BRZ = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("BottomRightZ: " + BRZ);
                        break;

                    default:
                        break;
                }
            }
            // This block is executed once the xml has been parsed. It adjusts some values 
            for (int i = 0; i < entryCount; i++)
            {
                if (iVal[i])   // If draft point is interior to ship, adjust away from edges 
                {
                    if (xVal[i] < -(0.95) * LBP / 2)
                        xVal[i] = (int) (-(0.95) * LBP / 2);
                    if (xVal[i] > (0.9) * LBP / 2)
                        xVal[i] = (int) ((0.9) * LBP / 2);
                    if (zVal[i] < -(0.95) * Beam / 2)
                        zVal[i] = (int) (-(0.95) * Beam / 2);
                    if (zVal[i] > (0.9) * Beam / 2)
                        zVal[i] = (int)((0.9) * Beam / 2);
                }
            }
            // Use default values for unspecified borders. Assume no border
            if (TLX == 0)
            {
                TLX = -LBP / 2;
            }
            if (TLZ == 0)
            {
                TLZ = -Beam / 2;
            }
            if (BRX == 0 )
            {
                BRX = LBP / 2;
            }
            if (BRZ == 0)
            {
                BRZ = Beam / 2;
            }
            leftBor = -(LBP / 2 + TLX);
            topBor = -(Beam / 2 + TLZ);
            rightBor = (BRX - LBP / 2);
            botBor = (BRZ - Beam / 2);
            // System.Diagnostics.Debug.WriteLine("l/t/r/b Bor: " + leftBor + " " + topBor + " " + rightBor + " " + botBor);

            // System.Diagnostics.Debug.WriteLine("All Touched set to false");
            // Values are initialized to 0.
            for (int i=0; i<entryCount; i++)
            {
                eVal[i] = 0;
                eTouched[i] = false;
            }
            // SetupStackPhoto();
            // If draft report already exists, read values in. 
            ReadDraftReport();
            entryEnabled = true;
            // System.Diagnostics.Debug.WriteLine("Done with ReadShipConfig");
        }

        // Gets the next tag 
        String GetNextElement()
        {
            int idx, idx1, idx2;
            String result;
            // Find left angled bracket
            if ((cString.Length == 0) || ((idx = cString.IndexOf("<")) == -1))
            {
                return "";
            }
            cString = cString.Substring(idx+1);
            // Eliminate any leading spaces
            while ((cString.Length != 0) && cString.Substring(0,1) == " ")
            {
                cString = cString.Substring(1);
            }
            if ((idx1 = cString.IndexOf(">")) == -1)
            {
                return "";
            }

            if (((idx2 = cString.IndexOf(" ")) != -1) && ((idx2 < idx1)))
            {
                result = cString.Substring(0, idx2);
                // Remaining values at the end of the tag line 
                parameters = cString.Substring(idx2, idx1 - idx2);
                cString = cString.Substring(idx1 + 1);
                // System.Diagnostics.Debug.WriteLine("element found: " + result);
                // System.Diagnostics.Debug.WriteLine("matching parms are: " + parameters);
                return result;
            }
            result = cString.Substring(0, idx1);
            cString = cString.Substring(idx1 + 1);
            parameters = "";
            // System.Diagnostics.Debug.WriteLine("element found: " + result);
            return result;
        }

        // Advances to end of angled ">" bracket 
        void Read2CloseBracket()
        {
            int idx = cString.IndexOf(">");
            if (idx >= 0)
            {
                cString = cString.Substring(idx+1);
            }
        }

        void ProcessDraftPt()
        {            
            Boolean done = false;
            double x = 0;
            double z = 0;
            Boolean interior = false;
            string name = "";
            string id1 = "";
            string id2 = "";
            string pString;
            while (!done)
            {
                // Read tag 
                pString = GetNextElement();
                // System.Diagnostics.Debug.WriteLine("Process draft value: '" + pString + "'");
                if (pString.Length == 0)
                {
                    done = true;
                    break;
                }
                // Process tag 
                switch (pString.ToUpper())
                {
                    case "NAME":
                        name = ReadQuoted();   // Obtain value from inside double quotes 
                        // System.Diagnostics.Debug.WriteLine("draft name: " + name);
                        done = ReadQuotedDone;   // Forward the done status from the ReadQuoted() routine 
                        break;

                    case "ID1":
                        id1 = ReadQuoted();
                        // System.Diagnostics.Debug.WriteLine("draft id1: " + id1);
                        done = ReadQuotedDone;
                        break;

                    case "ID2":
                        id2 = ReadQuoted();
                        // System.Diagnostics.Debug.WriteLine("draft id2: " + name);
                        done = ReadQuotedDone;
                        break;

                    case "X":
                        x = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("draft x: " + x);
                        done = ReadQuotedDone;
                        break;

                    case "Z":
                        z = ReadQuotedDouble();
                        // System.Diagnostics.Debug.WriteLine("draft z: " + z);
                        done = ReadQuotedDone;
                        break;

                    case "/POINT":
                        done = true;
                        break;

                    case "INTERIOR":    // Signals that draft point is interior in ship 
                        interior = true;
                        break;

                    case "":
                        done = true;
                        break;

                    default:
                        break;
                }
            }
            // Fill out draft entry cell
            eId1[entryCount] = id1;
            eId2[entryCount] = id2;
            e[entryCount].Placeholder = name;
            e[entryCount].FontSize = entryFontNormal;
            e[entryCount].WidthRequest = pixelEntryScale * pixelPerChar;
            e[entryCount].PlaceholderColor = Color.White;
            e[entryCount].Text = "";
            // Click handler             
            eEH[entryCount] = (sender, e) =>
            {
                var entry = sender as Entry;
                if (entry != null)
                {
                    // Store which entry was clicked. Must look up in dictionary. 
                    entryIndex = lookup[entry.Id];
                    // System.Diagnostics.Debug.WriteLine("e[] clicked event, entryEnabled/entryIndex = " + entryEnabled + 
                    //     " " + entryIndex);
                    // Run the custom keyboard activity. 
                    DependencyService.Get<IConfigRepo>().InvokeKB();
                }
            };
            // Add the event handler to respond to focus events, like clicking 
            e[entryCount].Focused += eEH[entryCount] as EventHandler<FocusEventArgs>;

            // Interior point requires putting label on ship with double boxes (black & white) to make label visible for any ship graphic
            // Also, a label & the entry value is required below the ship 
            if (interior)
            {
                l[entryCount] = new CustomLabel
                {   // Label to put on the ship
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Text = name,
                    FontSize = 16
                };
                l2[entryCount] = new CustomLabel
                {   // Label to put below the ship, next to entry 
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Text = name,
                    FontSize = 20
                };
                e[entryCount].Placeholder = "";
            }
            xVal[entryCount] = x;
            zVal[entryCount] = z;
            iVal[entryCount] = interior;
            entryCount++;
            // System.Diagnostics.Debug.WriteLine("New entry, count = " + entryCount);
            // System.Diagnostics.Debug.WriteLine("name/x/z/interior = " + name + " " + x + " " + z + " " + interior);
        }

        // Read value within quotes 
        String ReadQuoted()
        {
            int idx;
            if ((idx = parameters.IndexOf("\"")) == -1)
            {
                ReadQuotedDone = true;
                return "";
            }
            parameters = parameters.Substring(idx + 1);   // skip to char after "
            if ((idx = parameters.IndexOf("\"")) == -1)
            {
                ReadQuotedDone = true;
                return "";
            }
            ReadQuotedDone = false;
            return parameters.Substring(0, idx);  // fetch name between "'s 
        }

        // These routines read the quoted value and save the step of converting it to a specific type of number 
        int ReadQuotedInt()
        {
            int result;

            if (!(Int32.TryParse(ReadQuoted(), out result)))
            {
                result = 0;
            }
            return result;
        }

        double ReadQuotedDouble()
        {
            double result = 0.0;
            try
            {
                result = Convert.ToDouble(ReadQuoted());
            } catch (Exception e)
            {
                result = 0.0;
            }
            return result;
        }

        void ReadReportName()
        {
            // Get name of most recently access report for the ship
            // The name is stored in the configFile.txt file. 
            if (DependencyService.Get<ISendEmail>().FileExists(ship + "/" + configFile))
            {  // config file exists
                report = DependencyService.Get<IConfigRepo>().ReadFile(ship + "/" + configFile);
            }
            else
            {  // config file does not exist 
                // If configFile.txt does not exist, set to default report and write configFile.txt
                report = defaultReport;
                DependencyService.Get<ISendEmail>().WriteFile(report, ship + "/" + configFile);
            }
            // Set report and create folder if it does not exist
            lbH2b.Text = report;
            DependencyService.Get<ISendEmail>().CreateFolder(ship + "/" + report, true, true);  // only creates folder if doesn't exist
        }
        
        void ReadDraftReport()
        {
            // System.Diagnostics.Debug.WriteLine("ReadDraftReport start");
            string reportPath = ship + "/" + report + "/" + reportName;
            // Don't create. Check for existence 
            if (!DependencyService.Get<ISendEmail>().FileExists(reportPath))
            {
                return;  // report doesn't exist
            }
            // System.Diagnostics.Debug.WriteLine("ReadDraftReport after file exists");
            // Read draft report
            String rString;
            // Whole report read in as a string 
            cString = DependencyService.Get<IConfigRepo>().ReadFile(reportPath);
            Boolean done = false;
            while (!done)
            {
                // Get next xml tag 
                rString = GetNextElement();
                if (rString.Length == 0)
                {
                    done = true;
                    break;
                }
                switch (rString.ToUpper())
                {
                    case "?XML":
                        // Presently, we're not using this information
                        break;

                    case "DRAFT_REPORT":
                        // Presently this is assumed and not checked.
                        // Future may require checking
                        break;

                    case "/DRAFT_REPORT":
                        done = true;
                        break;

                    case "SHIP":
                        // This should be the same as the ship directory
                        // Presently we do not verify this. 
                        break;

                    case "/SHIP":
                        break;

                    case "REPORT":
                        // This should be the same as the report directory
                        // Presently we do not verify this. 
                        break;

                    case "/REPORT":
                        break;

                    case "DATE_TIME":
                        // This is the time stamp for the configuration file
                        break;

                    case "/DATE_TIME":
                        // This is the time stamp for the configuration file
                        break;

                    case "MEASUREMENT":
                        // System.Diagnostics.Debug.WriteLine("Processing draft measurement value");
                        ProcessDraftValue();
                        break;

                    case "":
                        done = true;
                        break;

                    default:
                        break;
                }
            }
        }

        void ProcessDraftValue()
        {
            Boolean donePDV = false;
            int idx = 0;
            string name = "";
            string entryName, inpValue;
            double value = 0;
            string pString;
            while (!donePDV)
            {
                pString = GetNextElement();
                // System.Diagnostics.Debug.WriteLine("Process draft value routine: '" + pString + "'");
                if (pString.Length == 0)
                {
                    donePDV = true;
                    break;
                }
                switch (pString.ToUpper())
                { 
                    case "NAME":
                        name = ReadBracketed();
                        // System.Diagnostics.Debug.WriteLine("draft value name: " + name);
                        break;

                    case "/NAME":
                        break;

                    case "VALUE":
                        inpValue = ReadBracketed();
                        if (inpValue.Length == 0)
                        {
                            value = 0;
                        } else
                        {
                            value = Double.Parse(inpValue);
                        }
                        // System.Diagnostics.Debug.WriteLine("draft value: " + value);
                        break;

                    case "/VALUE":
                        break;

                    case "/MEASUREMENT":
                        donePDV = true;
                        break;

                    case "":
                        donePDV = true;
                        break;

                    default:
                        break;
                }
            }
            // Put value in entry cell
            donePDV = false;
            // Find the e[] cell that corresponds to this draft point value 
            while (!donePDV && (idx < entryCount))
            {
                if (iVal[idx])
                {
                    entryName = l[idx].Text;
                }
                else
                {
                    entryName = e[idx].Placeholder;
                }
                if (entryName.Equals(name))
                {
                    // System.Diagnostics.Debug.WriteLine("match idx, name = " + idx + " " + name);
                    donePDV = true;
                    eTouched[idx] = false;
                    if (value == 0)
                    {
                        e[idx].Text = "";
                        eVal[idx] = 0;
                    } else
                    {
                        // If value convert from meters (xml report standard) to user specified 
                        dValue = CDO.GUnits.convert_migname(value, (int)GCUnits.hecLengthUnitConstants.heclengthm, (int)CDO.Units.Draft2Length);
                        e[idx].Text = CDO.FormatUnit(dValue, (int)CDO.Units.Draft2Length, GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance, false);
                        eVal[idx] = dValue;
                        eTouched[idx] = false;
                        if (e[idx].Text.Length > 4)   // Adjust font according to how many characters 
                        {
                            e[idx].FontSize = entryFontSmall;
                        }
                        else
                        {
                            e[idx].FontSize = entryFontNormal;
                        }
                    }
                    // System.Diagnostics.Debug.WriteLine("formated value = " + e[idx].Text);
                    // System.Diagnostics.Debug.WriteLine("value = " + eVal[idx]);
                }
                idx++;
            }
            // System.Diagnostics.Debug.WriteLine("name/value/idx = " + name + " " + value + " " + idx);
        }

        // Read text within angled brackets "< >"
        String ReadBracketed()
        {
            int idx;
            String result;
            // Find left angled bracket
            if ((cString.Length == 0) || ((idx = cString.IndexOf("<")) == -1))
            {
                return "";
            }
            result = cString.Substring(0, idx);
            cString = cString.Substring(idx);
            return result;
        }

        // Convert text unit values to HEC enum value 
        void setUnits(string uStr)
        {
            GCUnits.hecLengthUnitConstants units;
            switch (uStr)
            {
                case "m":
                    units = GCUnits.hecLengthUnitConstants.heclengthm;
                    break;
                case "ft":
                    units = GCUnits.hecLengthUnitConstants.heclengthft;
                    break;
                case "yd":
                    units = GCUnits.hecLengthUnitConstants.hecLengthYards;
                    break;
                default:
                    units = GCUnits.hecLengthUnitConstants.heclengthm;
                    break;
            }
            // CDO.Units.Draft2Length = units;
            CDO.Units.SetUnit(GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance, (int) units);
            newUnits = CDO.Units.Draft2Length;    // newUnits must always track the CDO units except when you will change
            // System.Diagnostics.Debug.WriteLine("Draft2Length starting units are: " + CDO.Units.Draft2LengthName);
        }

        // Place e[] value in ship top/bottom block if not interior 
        void PlaceEntryItems()
        {
            Boolean shownInterior = false;
            int i;
            Double xLoc, zLoc, tF;
            for (i = 0; i < entryCount; i++)
            { 
                // Calculate the x relative location for the draft point 
                xLoc = (float)((leftBor + xVal[i] + LBP / 2) / (leftBor + LBP + rightBor));
                if (xLoc < 0)
                    xLoc = 0;
                if (xLoc > 1.0)
                    xLoc = 1.0;
                xLocA[i] = xLoc;
            }
            // Space out entries that are too close. Do above image and then below
            SpaceEntries(-1);   // Space out entries (if needed) above image
            SpaceEntries(1);   // Space out entries (if needed) above image

            for (i = 0; i < entryCount; i++)
            {
                xLoc = xLocA[i];
                // Calculate z value (height position)
                zLoc = (float)(screenfit * ((screenWidth - screenfixed)/screenWidth) * ((topBor + zVal[i] + Beam / 2) / (topBor + Beam + botBor)));
                if (zLoc < 0f)
                    zLoc = 0f;
                if (zLoc > 1.0f)
                    zLoc = 1.0f;

                if (iVal[i]) // If internal, place graphics on the ship graphic 
                {
                    // Place the label on the ship graphic
                    bi1[i] = new BoxView
                    {
                        Color = Color.White
                    };
                    tF = l[i].Text.Length * pixelPerChar * 0.8f;   // Width required for text label 
                    if (l[i].Text.Length > 2)
                    {  // Special code to handle longer text length. Handled in two segments with different slope 
                        tF = 2 * pixelPerChar * 0.8f + (l[i].Text.Length - 2) * pixelPerChar / 4;
                    }
                    AbsoluteLayout.SetLayoutFlags(bi1[i],
                        AbsoluteLayoutFlags.PositionProportional);
                    AbsoluteLayout.SetLayoutBounds(bi1[i],
                        new Rectangle(xLoc, zLoc, tF + boxDelta, 34.0f));   // Set the size of the label

                    // Two boxes, one white, one black, one slightly larger than the other outlines label with any graphic 
                    bi2[i] = new BoxView
                    {
                        Color = Color.Black
                    };
                    AbsoluteLayout.SetLayoutFlags(bi2[i],
                        AbsoluteLayoutFlags.PositionProportional);
                    AbsoluteLayout.SetLayoutBounds(bi2[i],
                        new Rectangle(xLoc, zLoc, tF, 30.0f));

                    AbsoluteLayout.SetLayoutFlags(l[i], 
                        AbsoluteLayoutFlags.PositionProportional);
                    AbsoluteLayout.SetLayoutBounds(l[i],
                        new Rectangle(xLoc, zLoc, tF - boxDelta, 30.0f));

                    // Add black and white box and label
                    shipBlock.Children.Add(bi1[i]);
                    shipBlock.Children.Add(bi2[i]);
                    shipBlock.Children.Add(l[i]);

                    // Place Entry below graphic
                    if (!shownInterior)  // Adds interior label the first time 
                    {
                        stackMain.Children.Add(lInterior);
                        shownInterior = true;
                    }
                    stackInterior.Children.Add(new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Start,
                        // HeightRequest = 60.0,
                        Children =
                    {
                       l2[i], e[i]
                    }
                    });
                } else
                {    // Process non-interior draft point 
                    AbsoluteLayout.SetLayoutFlags(e[i],
                        AbsoluteLayoutFlags.XProportional);
                    /*
                    AbsoluteLayout.SetLayoutBounds(e[i],
                        new Rectangle(xLoc * screenfit - xLoc * screenfixed / screenWidth, 0f, pixelEntryScale*pixelPerChar, 30f));
                    */
                    // AbsoluteLayout.SetLayoutBounds(e[i],
                    //     new Rectangle(xLoc, 0f, pixelEntryScale*pixelPerChar * 1.2, 30f));
                    AbsoluteLayout.SetLayoutBounds(e[i],
                        new Rectangle(xLoc, 0f, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                    if (zVal[i] >= 0)  // Determine if draft point above or below ship image 
                    {
                        bottomBlock.Children.Add(e[i]);
                        // System.Diagnostics.Debug.WriteLine("Place entry, bottom, name/xLoc: " + e[i].Placeholder + " " + xLoc);
                    }
                    else
                    {
                        topBlock.Children.Add(e[i]);
                        // System.Diagnostics.Debug.WriteLine("Place entry, top, name/xLoc: " + e[i].Placeholder + " " + xLoc);
                    }
                }
            }
        }

        // Space entries out if too close 
        void SpaceEntries(int side)
        {
            int cnt, i, j, cycle;
            Double minVal = (0.5) * leftBor / (leftBor + LBP + rightBor);
            Double maxVal = 1 - (0.5) * rightBor / (leftBor + LBP + rightBor);
            // System.Diagnostics.Debug.WriteLine("minVal, maxVal: " + minVal + " " + maxVal);
            Double step = stepScale * pixelEntryScale * pixelPerChar / screenWidth;
            // System.Diagnostics.Debug.WriteLine("step value: " + step);
            Double moveLeft, moveRight, dist;
            Boolean done;
            int[] vIdx = new int[maxDraftPts];
            Double[] value = new Double[maxDraftPts];
            cnt = 0;
            // Put all values on selected side of ship in sorted order 
            for (i = 0; i < entryCount; i++)
            {
                if (!iVal[i] && ((Math.Sign(zVal[i]) + 0.5) * side > 0)) {
                    j = 0;
                    // Insert in sorted order 
                    while ((j < cnt) && (xLocA[i] >= value[j]))
                    {
                        j++;
                    }
                    // Shift existing values over 
                    for (int k = cnt-1; k >= j; k-- )
                    {
                        value[k + 1] = value[k];
                        vIdx[k + 1] = vIdx[k];
                    }
                    value[j] = xLocA[i];
                    vIdx[j] = i;
                    cnt++;
                }
            }
            // System.Diagnostics.Debug.WriteLine("***** side/cnt: " + side + " " + cnt);
            /*
            for (i = 0; i < cnt; i++)
            {
                // System.Diagnostics.Debug.WriteLine(vIdx[i] + "  " + value[i]);
            }
            */
            cycle = 0;
            done = false;
            // Do upto 10 cycles 
            // Scan through the points. If too close, separate both points part of the way. 
            // If small adjustments are made to both points, and the process is repeated then the points should be properly spaced out
            while (!done && (cycle < 10))
            {
                done = true;
                for (i = 0; i < cnt-1; i++)
                {
                    if ((value[i] + step) > value[i+1])
                    {   // If points are too close, shift both away by small amount 
                        dist = value[i] + step - value[i + 1];
                        done = false;
                        // Calculate projected moves. If too much, reduce 
                        moveLeft = Math.Min(dist / 2, value[i]- minVal);
                        if (i>0)
                        {
                            moveLeft = Math.Min(moveLeft, (value[i] - value[i - 1]) / 2);
                        }
                        moveRight = Math.Min(dist / 2, maxVal - value[i + 1]);
                        if (i < cnt-2)
                        {
                            moveRight = Math.Min(moveRight, (value[i + 2] - value[i + 1]) / 2);
                        }
                        value[i] -= moveLeft;
                        value[i+1] += moveRight;
                    }
                }
                cycle++;
            }
            // System.Diagnostics.Debug.WriteLine("Cycle = " + cycle);
            /*
            for (i = 0; i < cnt; i++)
            {
                // System.Diagnostics.Debug.WriteLine(vIdx[i] + "  " + value[i]);
            }
            */
            // move points back to array 
            for (i = 0; i < cnt; i++)
            {
                xLocA[vIdx[i]] = value[i];
            }
        }

        // Read folder and build list of ships 
        void ReadShipList()
        {
            // Make copy of ship list. This is to see if it changed. If not, we won't rebuild the ship layout 
            CloneShipList();
            shipList.Clear();
            shipCount = 0;
            int fileCnt = 0;
            int idx, idx1, idx2;
            // Read list of files from folder 
            dirList = DependencyService.Get<IConfigRepo>().ReadFolder(maxNumShips*3, "");
            while (!(dirList[fileCnt].Equals("")))  // Get number of valid file names for sorting a new array of exact size
            {
                fileCnt++;
            }
            // Create sorted list 
            dirArraySorted = new String[fileCnt];
            for (int i = 0; i < fileCnt; i++)   // directory list of exact size for sorting
            {
                // System.Diagnostics.Debug.WriteLine("found file: " + dirList[i]);
                dirArraySorted[i] = dirList[i];
            }
            // Sort list 
            Array.Sort(dirArraySorted);
            idx = 0;
            // Look for consecutive base names with _config & _graphic suffixes. This indicates a ship file 
            while ((idx < fileCnt-1) && (shipCount < maxNumShips))
            {
                idx1 = dirArraySorted[idx].IndexOf("_config");
                idx2 = dirArraySorted[idx+1].IndexOf("_graphic");
                if ((idx1 != -1) && (idx1 == idx2) && (dirArraySorted[idx].Substring(0,idx1) == dirArraySorted[idx+1].Substring(0,idx2)) &&
                    (dirArraySorted[idx].Substring(idx1+1).ToUpper().Equals("CONFIG.XML")) && (dirArraySorted[idx+1].Substring(idx1+1).ToUpper().Equals("GRAPHIC.PNG")))
                {
                    shipList.Add(dirArraySorted[idx].Substring(0, idx1));
                    // System.Diagnostics.Debug.WriteLine("****** Found ship name: " + dirArraySorted[idx].Substring(0, idx1));
                    shipCount++;
                }
                idx++;                
            }

            if (!ShipListsMatch())
            {    // If new ship list is different than previous, then we recreate the ship list view 
                stackShipsL = null;
                stackShipsP = null;
            }

            // System.Diagnostics.Debug.WriteLine("Ship count: " + shipCount);
        }

        void SetupStackPhoto()
        {
            // Setup the photo stack layout
            // The following code block of code has been added to most stacklayout setup routines
            // The purpose is to reduce memory usage which increases performance after usage & reduces crashes 
            // Two stack layouts are created. One for landscape and one for portrait. 
            // These are saved and reused. If some event causes them to be invalid, the variables are set to null, forcing regeneration
            // The approach of saving layouts helps with performance and crashes a lot. However, sometimes layout artifacts occur. 
            // Some code has been added to reduce the artifacts, but they still occur with frequent rotation. 
            if (screenWidth > screenHeight)
            {
                stackPhoto = stackPhotoL;
            }
            else
            {
                stackPhoto = stackPhotoP;
            }
            if (stackPhoto != null)
            {
                // System.Diagnostics.Debug.WriteLine("****** SetupStack - use existing photo");
                ResetPhotoBolds();   // Clear bold from last photo 
                for (int i =0; i < entryCount; i++)
                {   // Set bold for last photo 
                    if (DB[i].Text.Equals(DBClicked))
                    {
                        DB[i].TextColor = Color.Yellow;
                    }
                }
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** SetupStack - make new photo");


            // Create ship stack layout. Some inital settings 
            String tStr;
            stackPhoto = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 2,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };
            // Add header labels 
            stackPhoto.Children.Add(lbH1);
            stackPhoto.Children.Add(lbDH1);
            stackPhoto.Children.Add(lbBl1);
            // Add a button for each draft point 
            for (int i = 0; i < entryCount; i++)
            {
                DB[i].HorizontalOptions = LayoutOptions.CenterAndExpand;
                DB[i].FontSize = 20;
                DB[i].TextColor = Color.White;
                if (iVal[i])
                {
                    tStr = l[i].Text;
                }
                else
                {
                    tStr = e[i].Placeholder;
                }
                DB[i].Text = tStr;
                stackPhoto.Children.Add(DB[i]);
            }
            stackPhoto.Children.Add(lbBl2);
            stackPhoto.Children.Add(btnMain);

            // Part of the code that handles the landscape and portrait layouts
            // Save the layout according if it is portrait or landscape 
            if (screenWidth > screenHeight)
            {
                stackPhotoL = stackPhoto;
            }
            else
            {
                stackPhotoP = stackPhoto;
            }
        }

        // Setup report view layout 
        void SetupStackReport(String errMsg)
        {
            int count1 = 0;
            int i = 0;
            Boolean done = false;
            lbRE1.Text = errMsg;

            if ((stackReportL == null) && (stackReportP == null))
            {
                // If neither setup then the buttons are not setup 
                reportCount = 0;
                eRpt.Text = "";                
                lookupDR = new Dictionary<Guid, int>();  // Lookup dictionary to match to button number 
                // Get the list of all the reports 
                dirList = DependencyService.Get<IConfigRepo>().ReadFolder(maxNumShips * 2, "/" + ship);
                while (dirList[count1].Length != 0)
                {
                    count1++;
                }
                
                while (!done)
                {
                    if (dirList[i].Length == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        if (!dirList[i].Equals(configFile))
                        {   // If not configFile.txt then it is a report folder 
                            RB[reportCount] = new Button();
                            RB[reportCount].HorizontalOptions = LayoutOptions.StartAndExpand;
                            RB[reportCount].FontSize = 20;
                            RB[reportCount].Text = dirList[i];
                            if (report.Equals(dirList[i]))
                            {    // Make selected report have a yellow button 
                                RB[reportCount].TextColor = Color.Yellow;
                            }
                            RB[reportCount].Clicked += (sender, args) =>
                            {   // Button to select report 
                                var button = sender as Button;
                                if (button != null)
                                {
                                    report = button.Text;
                                    lbH2b.Text = report;
                                    // Update configFile.txt with name of current report 
                                    DependencyService.Get<ISendEmail>().WriteFile(report, ship + "/" + configFile);
                                    // ResetReportBolds();
                                    // button.TextColor = Color.Yellow;
                                    // Clear error and report name entry field 
                                    eRpt.Text = "";
                                    lbRE1.Text = "";
                                    ClearEntryValues();    // Clear previous draft values 
                                    ReadDraftReport();     // Read draft report, load values 
                                    // btnShare.IsEnabled = true;
                                    outStat.Text = "";
                                    WriteConfig();
                                    UpdateForNewUnits();    // Update for current units 
                                    ReadShipConfig();
                                    // stackMainL = null;    // required for apparent bug
                                    // stackMainP = null;
                                    // SetupStackMain();
                                    ResetViews();
                                    SetupStackMain();
                                    mainView = true;
                                    scrollView.Content = null;
                                    scrollView.Content = stackMain;
                                }
                            };
                            // Configure delete button 
                            DELBR[reportCount] = new Button();
                            DELBR[reportCount].HorizontalOptions = LayoutOptions.EndAndExpand;
                            DELBR[reportCount].FontSize = 20;
                            DELBR[reportCount].TextColor = Color.Red;
                            DELBR[reportCount].Text = "Delete";
                            lookupDR.Add(DELBR[reportCount].Id, reportCount);   // Setup lookup table to link button with report number.
                            DELBR[i].Clicked += async (sender, args) =>
                            {
                                var button = sender as Button;
                                if (button != null)
                                {
                                    reportToDel = RB[lookupDR[button.Id]].Text;   // Use lookup table to get report name to delete 
                                    var answer = await MainPage.DisplayAlert("Confirm Deletion", "Do you want to delete report: " + reportToDel, "Yes", "Cancel");
                                    if (answer)
                                    {
                                        // Deleting report requires reseting report view layout 
                                        stackReportL = null;
                                        stackReportP = null;
                                        reportCount--;
                                        DeleteReport(reportToDel);    // Delete report folder 
                                        if (report.Equals(reportToDel))
                                        {  // if selected report is deleted choose default report 
                                            report = defaultReport;
                                            DependencyService.Get<ISendEmail>().WriteFile(report, ship + "/" + configFile);
                                            lbH2b.Text = report;
                                            // Switching reports requires clearing all draft entries & reading report file 
                                            ClearEntryValues();
                                            ReadDraftReport();
                                        }
                                        SetupStackReport("");     // Setup new report stack layout 
                                        ResetViews();
                                        reportView = true;
                                        scrollView.Content = null;
                                        scrollView.Content = stackReport;
                                    }
                                }
                            };
                            reportCount++;
                        }
                    }
                    i++;
                }
            }
            // Reset all buttons back to white. Then set selected to yellow (bold) 
            ResetReportBolds();
            for (i = 0; i < reportCount; i++)
            {
                if (RB[i].Text.Equals(report))
                {
                    RB[i].TextColor = Color.Yellow;
                }
            }

            if (screenWidth > screenHeight)
            {
                stackReport = stackReportL;
            }
            else
            {
                stackReport = stackReportP;
            }
            if (stackReport != null)
            {
                // System.Diagnostics.Debug.WriteLine("****** SetupStack - use existing report");
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** SetupStack - make new report");

            // Setup stck layout and headers 
            stackReport = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 2,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };
            stackReport.Children.Add(lbH1);
            stackReport.Children.Add(lbRH1);
            stackReport.Children.Add(lbBl1);

            // Add button for each report 
            for (i = 0; i < reportCount; i++)
            {
                if (RB[i].Text == defaultReport)
                { // Handle default report (Report1) and other reports differently. You cannot delete default report.
                    sTemp = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        HeightRequest = 60.0,
                        Children =
                        {
                            RB[i]
                        }
                    };
                }
                else
                {
                    sTemp = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        HeightRequest = 60.0,
                        Children =
                        {
                            RB[i], DELBR[i]
                        }
                    };
                }
                stackReport.Children.Add(sTemp);
            }
            // Add new report creation and other labels, buttons 
            // stackReport.Children.Add(lbBl2);
            stackReport.Children.Add(lbRE1);
            if (reportCount < maxReports)
            {
                stackReport.Children.Add(lbRH2);
                stackReport.Children.Add(lbRH3);
                stackReport.Children.Add(eRpt);
                stackReport.Children.Add(btnRC);
            }
            stackReport.Children.Add(lbBl3);
            stackReport.Children.Add(btnMain);

            if (screenWidth > screenHeight)
            {
                stackReportL = stackReport;
            }
            else
            {
                stackReportP = stackReport;
            }
        }

        void ClearEntryValues()
        {
            // Clears all the draft entry for the presently loaded ship 
            // System.Diagnostics.Debug.WriteLine("***** inside clearentryvalues, entrycount =  " + entryCount);
            for (int idx = 0; idx < entryCount; idx++)
            {
                e[idx].Text = "";
                eTouched[idx] = false;
                eVal[idx] = 0;
            }
        }

        // Determines if inpReport name is already used 
        Boolean DuplicateReport(String inpReport)
        {
            Boolean match = false;
            int idx = 0;
            while ((idx < reportCount) && !match)
            {
                if (inpReport.Equals(RB[idx].Text))
                {
                    match = true;
                }
                idx++;
            }
            return match;
        }

        // Sets up 
        void SetupStackMain()
        {
            GC.Collect();   // Need to free up memory. Not sure this does anything. 
            lUVal.Text = CDO.Units.Draft2LengthName;
            if (screenWidth > screenHeight)
            {
                stackMain = stackMainL;
            }
            else
            {
                stackMain = stackMainP;
            }
            if (stackMain != null)
            {
                // System.Diagnostics.Debug.WriteLine("****** SetupStack - use existing main");
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** SetupStack - make new main");

            // Builds a dictionary of ship images so that the bitmap must be read in only once. 
            if (shipImages.ContainsKey(ship))
            {
                // System.Diagnostics.Debug.WriteLine("****** main contains key");
                shipimgsrc = shipImages[ship];
                imageHeight = shipImagesH[ship];
                imageWidth = shipImagesW[ship];
            } else
            {
                // System.Diagnostics.Debug.WriteLine("****** main does not contain key");
                shipimgsrc = DependencyService.Get<IConfigRepo>().ReadShipImageFile(ship);
                imageHeight = DependencyService.Get<IConfigRepo>().imageHeight;
                imageWidth = DependencyService.Get<IConfigRepo>().imageWidth;
                shipImages.Add(ship, shipimgsrc);
                shipImagesH.Add(ship, imageHeight);
                shipImagesW.Add(ship, imageWidth);
            }
            // System.Diagnostics.Debug.WriteLine("****** main past keys");
            shipImage.Source = shipimgsrc;
            // Set the ship size to match available screen width. Height is set to keep same aspect ratio 
            float shipImageHeight = screenWidth * imageHeight / imageWidth;
            // Absolute block necessary to place draft labels on ship layout 
            shipBlock = new AbsoluteLayout
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                HeightRequest = shipImageHeight
            };

            // top & bottom blocks are for non-interior draft points 
            topBlock = new AbsoluteLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Start
            };
            bottomBlock = new AbsoluteLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Start
            };

            // The boxLeft and boxRight appear to be necessary to get the blocks to stretch the whole width of the screen and result in 
            //  proper placement of the draft points
            topBlock.Children.Add(boxLeft);
            topBlock.Children.Add(boxRight);
            bottomBlock.Children.Add(boxLeft);
            bottomBlock.Children.Add(boxRight);
            // shipBlock.Children.Add(boxLeft);
            // shipBlock.Children.Add(boxRightS);

            // System.Diagnostics.Debug.WriteLine("====== sw,sh,sd, iw,ih,sih " + screenWidth + " " + screenHeight +
            //      " " + screenDensity + " " + imageWidth + " " + imageHeight + " " + shipImageHeight);

            // Ship image width & height are set to fill width and keep same aspect ratio. 
            AbsoluteLayout.SetLayoutFlags(shipImage,
                AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(shipImage,
                new Rectangle(0f, 0f, screenWidth * screenfit - screenfixed, shipImageHeight * screenfit));
            shipBlock.Children.Add(shipImage);

            stackInterior = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Start,
                Spacing = 2,
                // VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };

            stackMain = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 2,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };
            // Add headers including ship and report 
            stackMain.Children.Add(lbH1);
            stackMain.Children.Add(lbH2a);
            stackMain.Children.Add(lbH2b);
            // stackMain.Children.Add(lbH3);   //  Displays how many draft points exist. Eliminate for now. 
            stackMain.Children.Add(lbBl1);
            stackMain.Children.Add(topBlock);
            stackMain.Children.Add(shipBlock);
            stackMain.Children.Add(bottomBlock);
            // stackMain.Children.Add(lbBl2);
            // Add the units label and units value
            stackMain.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Start,
                Children =
                {
                    lUnits, lUVal
                }
            });
            // ReadShipConfig();
            PlaceEntryItems();    // Places draft entries in the proper location in the top & bottom blocks 
            stackMain.Children.Add(stackInterior);
            lbH3.Text = "Enter up to " + entryCount + " Draft Values";     // Not used now
            stackMain.Children.Add(lbBl3);
            stackMain.Children.Add(outStat);
            stackMain.Children.Add(btnShare);

            if (screenWidth > screenHeight)
            {
                stackMainL = stackMain;
            }
            else
            {
                stackMainP = stackMain;
            }
        }

        // Units stack layout view 
        void SetupStackUnits()
        {
            ResetUnitBolds();   // Eliminate yellow color on units buttons. Selected one will be set to yellow
            if (screenWidth > screenHeight)
            {
                stackUnits = stackUnitsL;
            }
            else
            {
                stackUnits = stackUnitsP;
            }

            if (stackUnits != null)
            {   // If the stack layout already exists, simply make the selected units button yellow
                // System.Diagnostics.Debug.WriteLine("****** SetupStack - use existing units");
                for (int i = 0; i < unitTypesD.Count; i++)
                {
                    if (newUnits == unitTypes[i])
                    {
                        UB[i].TextColor = Color.Yellow;
                    }
                }
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** SetupStack - make new units");
            stackUnits = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };
            stackUnits.Children.Add(lbH1);
            stackUnits.Children.Add(lbBl1);
            // stackUnits.Children.Add(pickerUnits);
            stackUnits.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Start,
                Children =
                {
                    lUP, eUP
                }
            });
            stackUnits.Children.Add(lbBl2);
            stackUnits.Children.Add(lUU);
            for (int i = 0; i < unitTypesD.Count; i++)
            {
                if (newUnits == unitTypes[i])
                {
                    UB[i].TextColor = Color.Yellow;
                }
                stackUnits.Children.Add(UB[i]);
            }
            stackUnits.Children.Add(lbBl2);
            stackUnits.Children.Add(btnMain);

            if (screenWidth > screenHeight)
            {
                stackUnitsL = stackUnits;
            }
            else
            {
                stackUnitsP = stackUnits;
            }
        }

        void ResetShipBolds()
        {
            for (int i = 0; i < shipCount; i++)
            {
                SB[0, i].TextColor = Color.White;
                SB[1, i].TextColor = Color.White;
            }
        }
        void ResetUnitBolds()
        {
            for (int i = 0; i < unitTypesD.Count; i++)
            {
                UB[i].TextColor = Color.White;
            }
        }
        void ResetPhotoBolds()
        {
            for (int i = 0; i < entryCount; i++)
            {
                DB[i].TextColor = Color.White;
            }
        }
        void ResetReportBolds()
        {
            for (int i = 0; i < reportCount; i++)
            {
                RB[i].TextColor = Color.White;
            }
        }
        void UpdateUnitBolds()
        {
            for (int i = 0; i < unitTypesD.Count; i++)
            {
                if (newUnits == unitTypes[i])
                {
                    UB[i].TextColor = Color.Yellow;
                } else
                {
                    UB[i].TextColor = Color.White;
                }
            }
        }
        void ResetViews()
        {
            unitsView = false;
            configView = false;
            photoView = false;
            reportView = false;
            mainView = false;
            shipView = false;
            pickerView = false;
            shareView = false;
        }
        void SetupStackConfig()
        {
            if (screenWidth > screenHeight)
            {
                stackConfig = stackConfigL;
            }
            else
            {
                stackConfig = stackConfigP;
            }
            stackConfig = null;      // bypassing the saving stack feature due to screen artifacts
            if (stackConfig != null)
            {
                // System.Diagnostics.Debug.WriteLine("****** SetupStack - use existing config");
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** SetupStack - make new config");

            stackConfig = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 2,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };
            stackConfig.Children.Add(lbH1);
            stackConfig.Children.Add(lbBl1);
            stackConfig.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    lbEE, sEE
                }
            });
            stackConfig.Children.Add(eEa);
            stackConfig.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    lbET, sET
                }
            });
            stackConfig.Children.Add(eTN);
            stackConfig.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    lbED, sED
                }
            });
            // stackConfig.Children.Add(eDS);
            stackConfig.Children.Add(lbBl2);
            stackConfig.Children.Add(lbBl3);
            stackConfig.Children.Add(btnMain);

            if (screenWidth > screenHeight)
            {
                stackConfigL = stackConfig;
            }
            else
            {
                stackConfigP = stackConfig;
            }
        }
        // Setup stack layout view for upper-right button menu
        void SetupStackPicker()
        {
            if (screenWidth > screenHeight)
            {
                stackPicker = stackPickerL;
            }
            else
            {
                stackPicker = stackPickerP;
            }
            stackPicker = null;   // bypassing the saving stack feature due to screen artifacts
            if (stackPicker != null)
            {
                // System.Diagnostics.Debug.WriteLine("****** SetupStack - use existing picker");
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** SetupStack - make new picker");

            stackPicker = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Children = {
                    lbH1,
                    lbBl1,
                    btnConfig,
                    btnShips,
                    btnUnits,
                    btnPhoto,
                    btnReport,
                    lbBl2,
                    btnMain,
                    lbBl3,
                    lConfig
                }
            };

            if (screenWidth > screenHeight)
            {
                stackPickerL = stackPicker;
            }
            else
            {
                stackPickerP = stackPicker;
            }
        }
        void SetupStackShare()
        {
            int cnt = 0;   // Count of number of valid share options 
            stackShare = null;
            stackShare = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Children = {
                    lbH1,
                    lbBl1,
                    lbSH1,
                    lbBl2
                }
            };
            if (sEE.IsToggled && (eEa.Text.Length > 0))   // email 
            {
                stackShare.Children.Add(btnSE);
                stackShare.Children.Add(eEa);
                cnt++;
            }
            if (sET.IsToggled && (eTN.Text.Length > 0))     // text 
            {
                stackShare.Children.Add(btnST);
                stackShare.Children.Add(eTN);
                cnt++;
            }
            if (sED.IsToggled)        // Dropbox 
            {
                stackShare.Children.Add(btnSDB);
                // stackShare.Children.Add(eDS);
                cnt++;
            }
            if (cnt == 0)      // If no valid share options, give error message 
            {
                stackShare.Children.Add(lbSE1);
                stackShare.Children.Add(lbBl3);
                stackShare.Children.Add(btnConfig);
            }
            stackShare.Children.Add(lbBl4);
            stackShare.Children.Add(btnMain);
        }
        void SetupStackShips()
        {
            GC.Collect();       // Good time to garbage collecct, but not this helps 
            stackShips = null;    // Eliminates some layout artifacts 
            // Determine if portrait or landscape. Choose the correct ship stack layout for the given layout
            if (screenWidth > screenHeight)
            {
                stackShips = stackShipsL;
                orient = 0;    
            } else
            {
                stackShips = stackShipsP;
                orient = 1;
            }
            // stackShips = null;       // Enable to bypass the saving stack feature due to screen artifacts. Uses more memory, but reduces artifacts
            if (stackShips != null)     // Determine if stack was setup or not. If so, set correct ship button yellow. The rest white. then return.
            {
                // System.Diagnostics.Debug.WriteLine("****** SetupStackShips - use existing");
                // If stack layout view exits, simply make the selected ship button yellow
                ResetShipBolds();

                for (int j = 0; j < shipCount; j++)
                {
                    if (SB[orient, j].Text.Equals(ship))
                    {
                        SB[orient, j].TextColor = Color.Yellow;
                    }
                }
                return;
            }
            // Otherwise, you must create the ship stack layout
            // System.Diagnostics.Debug.WriteLine("****** SetupStackShips - make new");
            // Create ship stack layout
            ImageSource iSource;
            int i, k;
            // Variables used to adjust button text font size (and button size) to fit screen width 
            int maxNameLen = 0;
            int minRatio = 25;
            int buttonFont = 20;
            double delButWidth = 120;
            double butHeight = 40;
            double shipScale = 4;
            for (i = 0; i < shipCount; i++)   // Get the largest ship name - used to set button size 
            {
                maxNameLen = Math.Max(maxNameLen, shipList[i].Length);
            }
            // button size adjustment factor. 6 is for the word "delete" 
            k = screenWidth / (maxNameLen + 6);
            if ( k < minRatio )
            {
                buttonFont = 20 * k / minRatio;
                shipScale = 4 * minRatio / k;
                delButWidth = 120 * k / minRatio;
            }
            // System.Diagnostics.Debug.WriteLine("****  SetupStackShips, maxNameLen/k/buttonFont/shipScale/delButWidth: " + maxNameLen + " " 
            //      + " " + k + " " + buttonFont + " " + shipScale + " " + delButWidth);

            stackShips = null;
            stackShips = new StackLayout
            {
                // HorizontalOptions = LayoutOptions.FillAndExpand,
                // Spacing = 2,
                // VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical
            };
            stackShips.Children.Add(lbH1);
            stackShips.Children.Add(lbSP1);
            stackShips.Children.Add(lbBl1);

            // Setup ship buttons. Two sets of buttons. One for portrait; one for landscape 
            for (i = 0; i < shipCount; i++)
            {
                // SB[i].HeightRequest = butHeight;
                SB[orient, i].FontSize = buttonFont;
                SB[orient, i].Text = shipList[i];
                if (ship == SB[orient, i].Text)
                {
                    SB[orient, i].TextColor = Color.Yellow;
                } else
                {
                    SB[orient, i].TextColor = Color.White;
                }                
                DELBS[orient, i].FontSize = buttonFont;
                // DELBS[i].HeightRequest = butHeight;
                // DELBS[i].WidthRequest = delButWidth;

                // System.Diagnostics.Debug.WriteLine("**** Before ship button check");
                // Ship images are stored in dictionary to reduce bitmap processing 
                if (shipImages.ContainsKey(SB[orient, i].Text))
                {
                    // System.Diagnostics.Debug.WriteLine("**** Contains key");
                    iSource = shipImages[SB[orient, i].Text];
                }
                else
                {
                    // System.Diagnostics.Debug.WriteLine("**** Does not Contain key");
                    iSource = DependencyService.Get<IConfigRepo>().ReadShipImageFile(SB[orient, i].Text);
                    int ih = DependencyService.Get<IConfigRepo>().imageHeight;
                    int iw = DependencyService.Get<IConfigRepo>().imageWidth;
                    shipImages.Add(SB[orient, i].Text, iSource);
                    shipImagesH.Add(SB[orient, i].Text, ih);
                    shipImagesW.Add(SB[orient, i].Text, iw);
                }
                // System.Diagnostics.Debug.WriteLine("**** Past Contains key");

                // iSource = DependencyService.Get<IConfigRepo>().ReadShipImageFile(SB[i].Text);
                
                Image iTemp = new Image();
                iTemp.Source = iSource;
                double iW, iH;
                iW = iTemp.Width;
                iH = iTemp.Height;
                iTemp.WidthRequest = screenWidth / shipScale;
                iTemp.HeightRequest = screenWidth / shipScale * (iH / iW);
                
                SB[orient, i].HeightRequest = butHeight;
                DELBS[orient, i].HeightRequest = butHeight;
                // Create row of three items; button, ship thumbnail, delete button 
                sTemp = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Fill,
                    Children =
                    {
                       SB[orient, i], iTemp, DELBS[orient, i]
                    }
                };
                sTemp.HeightRequest = Math.Max(40, iTemp.Height);
                // System.Diagnostics.Debug.WriteLine("**** setupstackships, shipname, iheight: " + shipList[i] + " " + iTemp.Height);
                stackShips.Children.Add(sTemp);
            }
            stackShips.Children.Add(lbBl2);
            stackShips.Children.Add(btnRSL);
            stackShips.Children.Add(lbBl3);
            stackShips.Children.Add(btnMain);

            // Save the stacklayout in portrait or landscape object reference for future use
            if (screenWidth > screenHeight)
            {
                stackShipsL = stackShips;
            }
            else
            {
                stackShipsP = stackShips;
            }
        }
        // Copies demo ships setup in Assets into folder 
        void LoadDemoShips()
        {
            String[] aL = new String[maxNumShips * 2 + 2];
            aL = DependencyService.Get<IConfigRepo>().GetAssetList();
            // System.Diagnostics.Debug.WriteLine("***** Reading demo ships from assets");
            for (int i = 0; i < aL.Length; i++)
            {
                // System.Diagnostics.Debug.WriteLine("File #" + (i + 1) + ": " + aL[i]);
                DependencyService.Get<IConfigRepo>().LoadAsset(aL[i]);
            }
        }
        void DeleteShip(String inpShip)
        {
            DependencyService.Get<ISendEmail>().DeleteShip(inpShip);
        }
        void DeleteReport(String inpReport)
        {
            DependencyService.Get<ISendEmail>().DeleteReport(ship + "/" + inpReport);
        }
        Boolean ShipValid()
        {
            Boolean result = false;
            int idx = 0;
            while ((idx < shipCount) && !result)
            {
                if (ship.Equals(shipList[idx]))
                {
                    result = true;
                }
                idx++;
            }
            return result;
        }
        // Provides formatted version of date string. Not presently used. Could be used in file name, as it was in the past. 
        string dateFormatter(String date)
        {
            string output = "";
            int mon = 0;
            int day = 0;
            int year = 0;
            int hour = 0;
            int min = 0;
            int sec = 0;
            int i;
            if ((i = date.IndexOf("/")) >= 0) {
                mon = Int32.Parse(date.Substring(0, i));
                date = date.Substring(i + 1);
            }
            if ((i = date.IndexOf("/")) >= 0)
            {
                day = Int32.Parse(date.Substring(0, i));
                date = date.Substring(i + 1);
            }
            if ((i = date.IndexOf(" ")) >= 0)
            {
                year = Int32.Parse(date.Substring(0, i));
                date = date.Substring(i + 1);
            }
            if ((i = date.IndexOf(":")) >= 0)
            {
                hour = Int32.Parse(date.Substring(0, i));
                date = date.Substring(i + 1);
                if (date.Contains("PM"))
                {
                    hour += 12;
                }
            }
            if ((i = date.IndexOf(":")) >= 0)
            {
                min = Int32.Parse(date.Substring(0, i));
                date = date.Substring(i + 1);
            }
            if ((i = date.IndexOf(" ")) >= 0)
            {
                sec = Int32.Parse(date.Substring(0, i));
            }
            output = year.ToString("0000") + mon.ToString("00") + day.ToString("00") + hour.ToString("00") +
                min.ToString("00") + sec.ToString("00");
            // System.Diagnostics.Debug.WriteLine("******      date/time in file name format: " + output);
            return output;
        }
        // Called by main activity when it receives notificadtion that the Android "back" button has been pressed.
        // If on the main ship view, then Android back is executed which leaves the app.
        // Otherwise, control is returned to the main ship view 
        public void NotifyBackChg()
        {
            // System.Diagnostics.Debug.WriteLine("*****  NotifyBackChg called");
            if (mainView)
            {
                DependencyService.Get<IConfigRepo>().AndroidBack();
            } else
            {
                // btnShare.IsEnabled = true;
                outStat.Text = "";
                WriteConfig();
                UpdateForNewUnits();
                ReadShipConfig();
                SetupStackMain();
                ResetViews();
                mainView = true;
                scrollView.Content = null;
                scrollView.Content = stackMain;
            }
        }
        public void Unfocus()
        {
            e[entryIndex].Unfocus();
        }
        // Called upon completion of the draft entry activity using special keyboard. The entered value is returned as the string 
        public void UpdateValue(string value)
        {
            // Treat the entry string as an entry and process 
            entryValueS = value;
            // System.Diagnostics.Debug.WriteLine("***** UpdateValue, entryIndex/value.length/value: " + entryIndex + " " + value.Length + " " + value);
            e[entryIndex].Unfocus();
            e[entryIndex].Text = value;
            // btnShare.IsEnabled = true;
            outStat.Text = "";
            Boolean pass = CDO.bEvaluateUnitExpressionCustom(e[entryIndex].Text, (int)CDO.Units.Draft2Length, (int)CDO.Units.Draft2Length, ref dValue, CProperty.hecFeetFormat.hecFeetFtIn);
            if ((dValue == 0) || (!pass) || (value.Length == 0))
            {
                e[entryIndex].Text = "";
                dValue = 0;
            }
            else
            {
                e[entryIndex].Text = CDO.FormatUnit(dValue, (int)CDO.Units.Draft2Length, GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance, false);
                if (e[entryIndex].Text.Length > 4)
                {
                    e[entryIndex].FontSize = entryFontSmall;
                }
                else
                {
                    e[entryIndex].FontSize = entryFontNormal;
                }
                // System.Diagnostics.Debug.WriteLine("TOUched set to false, lookup " + lookup[e[entryIndex].Id]);
            }
            eTouched[lookup[e[entryIndex].Id]] = false;
            eVal[lookup[e[entryIndex].Id]] = dValue;
            SaveReport();    // Report updated and saved for each entry 
        }
        void SaveReport()
        {
            GC.Collect();
            DateTime now = DateTime.Now.ToLocalTime();

            emailData = "";
            emailData += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + EOL;
            emailData += "<draft_report>" + EOL;
            emailData += "  <ship>" + ship + "</ship>" + EOL;
            emailData += "  <report>" + report + "</report>" + EOL;
            emailData += "  <date_time>" + now.ToString() + "</date_time>" + EOL;

            textData = "HDraft;";
            textData += "" + ship + ";";
            textData += "{";
            String tStr;
            for (int i = 0; i < entryCount; i++)     //  Process each draft point into xml output file 
            {
                if (iVal[i])
                {
                    tStr = l[i].Text;
                }
                else
                {
                    tStr = e[i].Placeholder;
                }
                if (eTouched[i])     // With the present scheme this is always false 
                {
                    Boolean pass = CDO.bEvaluateUnitExpressionCustom(e[i].Text, (int)CDO.Units.Draft2Length, (int)CDO.Units.Draft2Length, ref dValue, CProperty.hecFeetFormat.hecFeetFtIn);
                    if (!pass)
                    {
                        dValue = 0;
                        e[i].Text = "";
                    }
                    else
                    {
                        e[i].Text = CDO.FormatUnit(dValue, (int)CDO.Units.Draft2Length, GCUnits.hecUnitInstanceConstants.hecDraft2UnitInstance, false);
                    }
                    if (e[i].Text.Length > 4)
                    {
                        e[i].FontSize = entryFontSmall;
                    }
                    else
                    {
                        e[i].FontSize = entryFontNormal;
                    }
                    eVal[i] = dValue;
                    eTouched[i] = false;
                    // System.Diagnostics.Debug.WriteLine("******* touched set to false, i = " + i);
                }
                else
                {
                    dValue = eVal[i];
                }
                if (!(CDO.Units.Draft2Length == GCUnits.hecLengthUnitConstants.heclengthm))  // results sent in meters
                {
                    dValue = CDO.GUnits.convert_migname(dValue, (int)CDO.Units.Draft2Length, (int)GCUnits.hecLengthUnitConstants.heclengthm);
                }
                if (dValue == 0)
                {
                    rStr[i] = "";
                }
                else
                {
                    rStr[i] = dValue.ToString();
                }
                emailData += "  <measurement>" + EOL;
                emailData += "    <name>" + tStr + "</name>" + EOL;
                emailData += "    <value>" + rStr[i] + "</value>" + EOL;
                emailData += "    <id1>" + eId1[i] + "</id1>" + EOL;
                emailData += "    <id2>" + eId2[i] + "</id2>" + EOL;
                emailData += "  </measurement>" + EOL;
                textData += "" + tStr + ",";
            }
            emailData += "</draft_report>" + EOL;
            textData = textData.Substring(0, textData.Length - 1) + "};";
            textData += "{";
            for (int i = 0; i < entryCount - 1; i++)
            {
                textData += "" + rStr[i] + ",";
            }
            textData += "" + rStr[entryCount - 1] + "}";

            outStat.Text = "";
            // outStat.Text = "Please connect to CargoMax computer with USB. ";
            DependencyService.Get<ISendEmail>().WriteFile(emailData, ship + "/" + report + "/" + reportName);
        }
        void CloneShipList()
        {
            shipListPrev.Clear();
            for (int i = 0; i < shipList.Count; i++)
            {
                shipListPrev.Add(shipList[i]);
            }
        }
        // Determine if existing (prev) ship list matches present.
        // This is necessary to see if the ship stack view must be recreated 
        Boolean ShipListsMatch()
        {
            Boolean result = true;
            if ((shipListPrev == null) || (shipList.Count != shipListPrev.Count))
            {
                return false;
            }
            for (int i = 0; i < shipList.Count; i++)
            {
                if (!shipList[i].Equals(shipListPrev[i]))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
        void ResetStacks()
        {
            stackMainL = null;
            stackMainP = null;
            stackReportL = null;
            stackReportP = null;
            stackPhotoL = null;
            stackPhotoP = null;
        }
        // Create Units buttons 
        void CreateUBs()
        {
            UB = new Button[unitTypesD.Count];
            for (int i = 0; i < unitTypesD.Count; i++)
            {
                UB[i] = new Button();
                UB[i].HorizontalOptions = LayoutOptions.CenterAndExpand;
                UB[i].FontSize = 20;
                UB[i].Text = CDO.gUnits.LengthName(unitTypes[i]);
                UB[i].Clicked += (sender, args) =>
                {
                    var button = sender as Button;
                    if (button != null)
                    {
                        ResetUnitBolds();
                        button.TextColor = Color.Yellow;
                        newUnits = unitTypesD[button.Text];
                        // unitsChg = true;
                        // btnShare.IsEnabled = true;
                        outStat.Text = "";
                        WriteConfig();
                        UpdateForNewUnits();
                        ReadShipConfig();
                        SetupStackMain();
                        ResetViews();
                        mainView = true;
                        scrollView.Content = null;
                        scrollView.Content = stackMain;
                    }
                };
            }
        }

        // Setup Draft point Photo buttions 
        void SetupDBButtons()
        {
            for (int i = 0; i < maxDraftPts; i++)
            {
                DB[i] = new Button();
                DB[i].Clicked += (sender, args) =>
                {
                    var button = sender as Button;
                    if (button != null)
                    {
                        String photoName;
                        photoName = "H_P" + photoCount.ToString("0000") + "_" + ship + "_" + report + "_" +
                            button.Text.Replace(" ", "");
                        // System.Diagnostics.Debug.WriteLine("***** photo name is: " + ship + "/" + report + "/" + photoName);
                        DependencyService.Get<ISendEmail>().TakeAPicture(ship + "/" + report + "/" + photoName);
                        photoCount = (photoCount + 1) % 10000;   // Appending the count keeps all photos unique 
                        ResetPhotoBolds();
                        button.TextColor = Color.Yellow;
                        DBClicked = button.Text;
                    }
                };
            }
        }
        // Reset draft entry point event handlers. This was recommended by Xamarin to allow garbage collection and reduce memory usage 
        void ResetEHs()
        {
            for (int i = 0; i < maxDraftPts; i++)
            {
                eEH[i] = null;
            }
        }
        // Draft entry buttons are only setup once by this routine. This is to reduce memory usage 
        void SetupEs()
        {
            for (int i = 0; i < maxDraftPts; i++)
            {
                e[i] = new CustomEntry();
                lookup.Add(e[i].Id, i);
            }
        }
        void SetupBoxes()
        {
            if (screenWidth > screenHeight)
            {
                boxRight = boxRightL;
                boxLeft = boxLeftL;
            }
            else
            {
                boxRight = boxRightP;
                boxLeft = boxLeftP;
            }
            if (boxRight != null)
            {
                // System.Diagnostics.Debug.WriteLine("****** Boxes - use existing");
                return;
            }
            // System.Diagnostics.Debug.WriteLine("****** Boxes - use new");

            boxLeft = new BoxView();
            boxRight = new BoxView();
            AbsoluteLayout.SetLayoutFlags(boxLeft,
                AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(boxLeft,
                new Rectangle(0f, 0f, 5f, 30f));
            AbsoluteLayout.SetLayoutFlags(boxRight,
                AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(boxRight,
                new Rectangle(screenWidth, 0f, 1f, 30f));

            if (screenWidth > screenHeight)
            {
                boxRightL = boxRight;
                boxLeftL = boxLeft;
            }
            else
            {
                boxRightP = boxRight;
                boxLeftP = boxLeft;
            }
        }
        // Setup ship buttons once to reduce memory usage 
        void SetupSBs()
        {
            lookupDS = new Dictionary<Guid, int>();
            SB = new Button[2, maxNumShips];
            DELBS = new Button[2, maxNumShips];
            // iTemp = new Image[maxNumShips];
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < maxNumShips; i++)
                {
                    if (j == 0)
                    {
                        // iTemp[i] = new Image
                        /*
                        {
                            // Aspect = Aspect.Fill
                        };
                        */
                    }
                    SB[j, i] = new Button();
                    SB[j, i].HorizontalOptions = LayoutOptions.StartAndExpand;
                    SB[j, i].Clicked += (sender, args) =>
                    {
                        var button = sender as Button;
                        if (button != null)
                        {
                            // Selects the new ship and does all setup 
                            ResetShipBolds();
                            button.TextColor = Color.Yellow;
                            ship = button.Text;
                            DBClicked = "";
                            ResetStacks();
                            lbH2a.Text = ship;
                            if (!ship.Equals(shipLoaded))
                            {
                                shipLoaded = "";
                            }
                            // btnShare.IsEnabled = true;
                            WriteConfig();
                            UpdateForNewUnits();
                            ReadShipConfig();
                            SetupStackMain();
                            ResetViews();
                            mainView = true;
                            scrollView.Content = null;
                            scrollView.Content = stackMain;
                        }
                    };
                    // Setup the ship delete button 
                    DELBS[j, i] = new Button();
                    DELBS[j, i].HorizontalOptions = LayoutOptions.EndAndExpand;
                    DELBS[j, i].TextColor = Color.Red;
                    // DELBS[i].HeightRequest = butHeight;
                    DELBS[j, i].Text = "Delete";
                    // DELBS[i].WidthRequest = delButWidth;
                    lookupDS.Add(DELBS[j, i].Id, i);
                    DELBS[j, i].Clicked += async (sender, args) =>
                    {
                        var button = sender as Button;
                        if (button != null)
                        {
                            shipToDel = SB[orient, lookupDS[button.Id]].Text;
                            var answer = await MainPage.DisplayAlert("Confirm Deletion", "Do you want to delete ship: " + shipToDel, "Yes", "Cancel");
                            if (answer)
                            {
                                DeleteShip(shipToDel);
                                shipList.Remove(shipToDel);
                                shipCount--;
                                stackShipsL = null;
                                stackShipsP = null;
                                if (shipCount == 0)
                                {
                                    ResetStacks();
                                    WriteConfig();
                                    ResetViews();
                                    scrollView.Content = null;
                                    scrollView.Content = stackNS;
                                }
                                else
                                {
                                    if (ship.Equals(shipToDel)) 
                                    {  // If selected ship deleted, select first ship 
                                        ship = shipList[0];
                                        DBClicked = "";
                                        ResetStacks();
                                        lbH2a.Text = ship;
                                        shipLoaded = "";
                                        DependencyService.Get<ISendEmail>().CreateFolder(ship, true, true);  // only creates folder if doesn't exist
                                        ReadReportName();  // Gets report for that ship, or created blank folder
                                        WriteConfig();
                                        ReadShipConfig();
                                    }
                                    SetupStackShips();
                                    ResetViews();
                                    shipView = true;
                                    scrollView.Content = null;
                                    scrollView.Content = stackShips;
                                    GC.Collect();
                                }
                            }
                        }
                    };
                }
            }
        }
        // This routine was used to reset the scrollview. This was done to reduce layout artifacts.
        // The screen did not redraw correctly so it is not presently used. 
        void ResetScrollView(Layout layout)
        {
            stackMainS.Children.Remove(scrollView);
            scrollView = null;
            scrollView = new ScrollView
            {
            };
            scrollView.Content = layout;
            stackMainS.Children.Add(scrollView);
        }
        // If the Android Dropbox code returns an error, this routine is called to report the error to the user 
        public void ReportDBError()
        {
            outStat.Text += "Dropbox Write failed. ";
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            System.Diagnostics.Debug.WriteLine("****  onStart called");
        }        

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            System.Diagnostics.Debug.WriteLine("****  onSleep called");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            System.Diagnostics.Debug.WriteLine("**** onResume called");
        }
    }
}
