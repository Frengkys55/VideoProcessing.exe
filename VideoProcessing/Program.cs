/*
 * Application usage (MediaPlayer application)
 * VideoProcessing getframes VideoSource TargetFolder Height=480 FPS=30 Requester=MediaPlayer SessionID=12345
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace VideoProcessing
{
    class Program
    {
        /*
         * TODO
         * 1 Move every informations to ProgramDataClass.cs
         */
        #region Preparations
        static VideoProcessor.VideoProcessor processor = new VideoProcessor.VideoProcessor();
        static ProcessedVideoInformation videoInformation = new ProcessedVideoInformation();
        static VideoInformation processedVideoInformation;
        static UserInformation userInfo = new UserInformation();
        static Requester requester;
        //static LogInformation logs;
        static Command command;
        static ProcessingInformation processingInformation;
        static ApplicationConfiguration applicationConfiguration;
        static Result ExtractFrameResult;
        static int processID = 0;
        #endregion Preparations

        // The use of multithreading was canceled because there is no need in using it.
        #region Threading informations
        static Thread[] thread;
        static List<float>[] work;
        static List<int>[] frames;
        static Dictionary<int, int> threadWorkNumber;
        #endregion Threading informations

        #region Stopwatches
        Stopwatch totalProcessStopwatch = new Stopwatch();
        #endregion Stopwatches

        #region Other setups
        static bool printInformation = true;
        static bool writeLogs = true;
        static string logLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        static bool debug = false;
        static bool saveCommand = false;
        static string logData = string.Empty;

        static DateTime accessTime;
        #endregion Other setups

        static void Main(string[] args)
        {
            try
            {
                processingInformation.processingStartTime = DateTime.Now;
                #region Menu
                if (args.Length == 0)
                {
                    Console.WriteLine("The parameters must not empty.");
                    Console.WriteLine("USAGE: VideoProcessing.exe Command Input Output AdditionalParameter");
                    WriteHelp();

                    Console.WriteLine("Example:");
                    Console.WriteLine("Extracting using default informations.");
                    Console.WriteLine("VideoProcessing.exe \"C:\\SourceFolder\\SourceFile.mkv\" C:\\TargetFolder");
                    Console.WriteLine("Note: For now do not put \"\\\" at the end of the target folder because it will cause error...");
                    Console.WriteLine("\nCurrently the only command available is getframes.");
                    
                    return;
                }
                #endregion Menu

                #region Commands checker and validation
                if (args[0].ToLower() == "getframes")
                {
                    command = Command.ExtractFrame;
                    if (args[1].Contains("\""))
                    {
                        videoInformation.source = args[1];
                    }
                    else
                    {
                        videoInformation.source = "\"" + args[1] + "\"";
                    }
                    if (args[2].Contains("\""))
                    {
                        videoInformation.target = args[2];
                    }
                    else
                    {
                        videoInformation.target = "\"" + args[2] + "\"";
                    }
                }
                #endregion Commands checker and validation

                #region GetFrames additional parameters
                if (command == Command.ExtractFrame)
                {
                    #region Video width setup
                    foreach (var arg in args)
                    {
                        if (arg.Contains("width="))
                        {
                            videoInformation.width = Convert.ToInt32(arg.Remove(0, "width=".Length));
                        }
                        else
                        {
                            videoInformation.width = -1;
                        }
                    }
                    #endregion Video width setup

                    #region Video height setup
                    foreach (var item in args)
                    {
                        if (item.Contains("height="))
                        {
                            foreach (var item2 in item.Split('='))
                            {
                                if (!Int32.TryParse(item2, out videoInformation.height))
                                {
                                    videoInformation.height = -1;
                                }
                            }
                            break;
                        }
                        else
                        {
                            videoInformation.height = -1;
                        }
                    }
                    #endregion Video height setup

                    #region Audio process setup
                    foreach (var arg in args)
                    {
                        if (arg.ToLower().Contains("with_audio="))
                        {
                            if (arg.ToLower().Contains("false"))
                            {
                                videoInformation.withAudio = false;
                            }
                            else
                            {
                                videoInformation.withAudio = true;
                            }
                        }
                    }
                    #endregion Audio process setup

                    #region Frame rate request
                    foreach (var item in args)
                    {
                        if (item.ToLower().Contains("fps="))
                        {
                            foreach (var item2 in item.Split('='))
                            {
                                if (!Single.TryParse(item2, out videoInformation.fpsOverride))
                                {
                                    videoInformation.fpsOverride = -1;
                                }
                            }
                            break;
                        }
                        else
                        {
                            videoInformation.fpsOverride = -1;
                        }
                    }
                    #endregion Frame rate request

                    #region Request delete original
                    foreach (var arg in args)
                    {
                        if (arg.ToLower().Contains("delete_original="))
                        {
                            if (arg.ToLower().Contains("true"))
                            {
                                videoInformation.deleteOriginal = true;
                            }
                            else
                            {
                                videoInformation.deleteOriginal = false;
                            }
                        }
                    }
                    #endregion Request delete original
                }
                #endregion GetFrammes additional parameters

                #region Application debug setup
                foreach (var item in args)
                {
                    if (item.ToLower().Contains("printinfo=true"))
                    {
                        applicationConfiguration.displayInformations = true;
                        Console.WriteLine("Command:");

                        foreach (var arg in args)
                        {
                            Console.Write(arg + " ");
                        }
                    }
                    else
                    {
                        applicationConfiguration.displayInformations = false;
                    }
                }
                #endregion Application debug setup

                #region Request mode
                // Used to determine which application to use this application
                // This is required for some application like MediaPlayer application
                // so this application will also ask for users session ID for processing
                requester = Requester.Other;
                foreach (var item in args)
                {
                    if (item.ToLower().Contains("mediaplayer"))
                    {
                        requester = Requester.MediaPlayer;
                    }
                }
                if (requester == Requester.Other)
                {
                    //throw new ArgumentNullException("Requester is needed for now");
                }
                #endregion Request mode

                #region User information
                if (requester == Requester.MediaPlayer)
                {
                    foreach (var arg in args)
                    {
                        if (arg.ToLower().Contains("sessionid="))
                        {
                            userInfo.UserSessionID = arg.Remove(0, "sessionid=".Length);
                        }
                    }
                }
                #endregion User information

                #region Get User ID, Session ID, and Process ID
                #region User information
                if (requester == Requester.MediaPlayer)
                {
                    foreach (var arg in args)
                    {
                        if (arg.ToLower().Contains("sessionid="))
                        {
                            userInfo.UserSessionID = arg.Remove(0, "sessionid=".Length);
                        }
                    }
                }
                #endregion User information

                #region User ID
                if (requester == Requester.MediaPlayer)
                {
                    SQLClassPeralatan.MintaDataDatabase mintaDataDatabase = new SQLClassPeralatan.MintaDataDatabase("UserID", "SessionInfo", "SessionID", userInfo.UserSessionID, ConfigurationManager.AppSettings["DatabaseConnectionString"]);
                    userInfo.UserID = Convert.ToInt32(mintaDataDatabase.DataDiterima);
                    //userInfo.UserID = Convert.ToInt32(SQLClassPeralatan.Peralatan.MintaDataDatabase("MediaPlayerDatabase", "UserID", "SessionInfo", "SessionID", userInfo.UserSessionID, ConfigurationManager.AppSettings["DatabaseConnectionString"]));
                }
                #endregion User ID

                #region Process ID
                if (requester == Requester.MediaPlayer)
                {
                    processID = SQLClassPeralatan.Peralatan.NilaiTertinggi("MediaPlayerDatabase", "ProcessedVideoInfo", "ProcessID", "UserID", userInfo.UserID.ToString(), ConfigurationManager.AppSettings["DatabaseConnectionString"]);
                }
                #endregion Process ID
                #endregion Get user ID, Session ID, and Process ID

                ExtractFrameResult = ExtractFrames(videoInformation);

                if (ExtractFrameResult.result == FunctionStatus.Fail)
                {
                    string WriteLogLocation = ConfigurationManager.AppSettings["LogLocation"];
                    int fileNumber = Directory.GetFiles(WriteLogLocation).Length + 1;
                    if (logLocation.EndsWith("\\"))
                    {
                        WriteLogLocation = logLocation + "err" + fileNumber + ".log";
                    }
                    else
                    {
                        WriteLogLocation = logLocation + "\\" + "err" + fileNumber + ".txt";
                    }
                    System.IO.File.WriteAllText(WriteLogLocation, ExtractFrameResult.ErrorException.Message);

                    #region MediaPlayer status updater
                    if (requester == Requester.MediaPlayer)
                    {
                        string SQLCommand = "USE " + HelperClass.LoadDatabaseConfiguration().databaseName + ";";
                        SQLCommand += "UPDATE ProcessedVideoInfo SET VideoStatus=3";
                        SQLCommand += " WHERE UserID=" + userInfo.UserID + " AND ProcessID= " + processID + ";";
                        SQLClassPeralatan.Peralatan.UbahDataDatabase(SQLCommand, HelperClass.LoadDatabaseConfiguration().databaseConnectionString);
                    }
                    #endregion MediaPlayer status updater

                    if (applicationConfiguration.displayInformations == true)
                    {
                        //Console.ReadLine();
                    }
                }

                if (requester == Requester.MediaPlayer)
                {
                    HelperClass.WriteVideoActualEndFrame(userInfo, videoInformation, processID.ToString());
                    //userInfo.UserID = HelperClass.GetUserID(userInfo.UserSessionID).UserID;
                    HelperClass.VideoStatusUpdater(userInfo, videoInformation, processID.ToString(), VideoProcessStatus.Processed);
                }
                
                processingInformation.processingEndTime = DateTime.Now;
                HelperClass.GenerateLog(videoInformation, processedVideoInformation, processingInformation, userInfo, requester);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine(err.StackTrace);
                string WriteLogLocation = ConfigurationManager.AppSettings["LogLocation"];
                if (logLocation.EndsWith("\\"))
                {
                    WriteLogLocation = logLocation + "err" + Directory.GetFiles(WriteLogLocation).Length + 1 + ".log";
                }
                else
                {
                    WriteLogLocation = logLocation + "\\" + "err" + Directory.GetFiles(WriteLogLocation).Length + 1 + ".txt";
                }
                System.IO.File.WriteAllText(WriteLogLocation, err.Message);
                if (applicationConfiguration.displayInformations == true)
                {
                    //Console.ReadLine();
                }
                return;
            }
            
        }

        [Obsolete("Use ExtractFrames() instead", true)]
        static void GetFrames(bool multiThread = false, int numberOfThread = 2)
        {
            try
            {
                // Main process of extracting frames and audio
                //List<object> processedVideoInfo = processor.GetAndSaveFrames(input, output, startSecond, endSecond, withAudio, printInformation, writeLogs, logLocation, videoWidth, videoHeight);

                #region Information construction
                VideoProcessor.VideoInfo videoInfo = new VideoProcessor.VideoInfo
                {
                    SourceFile = videoInformation.source,
                    TargetFolder = videoInformation.target,
                    requestDeleteOriginal = videoInformation.deleteOriginal,
                    //height = videoHeight,
                    //width = videoWidth,
                    printInformation = true
                };
                if (videoInformation.fpsOverride != -1 || videoInformation.fpsOverride != 0)
                {
                    videoInfo.overrideFrameRate = true;
                    videoInfo.frameRate = 24;
                }
                VideoProcessor.ApplicationSettings settings = new VideoProcessor.ApplicationSettings
                {
                    useCustomApplication = false,
                };
                #endregion Information preparation

                try
                {
                    VideoProcessor.VideoLog log = processor.ExtractVideo(videoInfo, settings);

                }
                catch (Exception err)
                {
                    throw;
                }

                //if ((int)processedVideoInfo[0] != -1)
                //{
                //    logs.totalProcessingDuration = (TimeSpan)processedVideoInfo[1];
                //    if ((bool)processedVideoInfo[2])
                //    {
                //        logs.audioProcessingDuration = (TimeSpan)processedVideoInfo[3];
                //    }
                //    logs.videoProcessingDuration = (TimeSpan)processedVideoInfo[4];
                //}
            }
            catch (Exception err)
            {
                throw;
            }
        }

        static Result ExtractFrames(ProcessedVideoInformation informations)
        {
            #region Information preparation
            Result result = new Result();

            #region Video basic info
            VideoProcessor.VideoInfo videoInfo = new VideoProcessor.VideoInfo
            {
                SourceFile = videoInformation.source,
                TargetFolder = videoInformation.target,
                requestDeleteOriginal = videoInformation.deleteOriginal,
                height = videoInformation.height,
                width = videoInformation.width,
                withAudio = videoInformation.withAudio,
                printInformation = true
            };
            #endregion Video basic info

            #region Video FPS overriding
            if (videoInformation.fpsOverride > 0)
            {
                videoInfo.overrideFrameRate = true;
                videoInfo.frameRate = videoInformation.fpsOverride;
            }
            else
            {
                videoInfo.overrideFrameRate = false;
            }
            #endregion Video FPS overriding

            VideoProcessor.ApplicationSettings settings = new VideoProcessor.ApplicationSettings();
            if (ConfigurationManager.AppSettings["UseCustomApplication"] == "true")
            {
                settings.useCustomApplication = true;
                settings.FFmpegPath = ConfigurationManager.AppSettings["FFmpegLocation"];
                settings.FFProbePath = ConfigurationManager.AppSettings["FFProbeLocation"];
            }
            else
            {
                settings.useCustomApplication = false;
            }
            #endregion Information preparation

            try
            {
                if (applicationConfiguration.displayInformations)
                {
                    Console.WriteLine("Will now processing the video with this information");
                    Console.WriteLine("Source: " + videoInformation.source);
                    Console.WriteLine("Target: " + videoInformation.target);
                    Console.WriteLine("With audio: " + videoInformation.withAudio);
                    Console.WriteLine("Width: " + videoInformation.width);
                    Console.WriteLine("Height: " + videoInformation.height);
                    Console.WriteLine("FPS: " + videoInformation.fpsOverride);
                }
                VideoProcessor.VideoLog log = processor.ExtractVideo(videoInfo, settings);
                processingInformation.processedVideoDuration = log.resultVideoProcessDuration;
                processingInformation.processedAudioDuration = log.resultAudioProcessDuration;
                processingInformation.totalVideoDuration = log.resultTotalProcessDuration;

                result.result = FunctionStatus.Success;
                result.HasError = false;
                result.AdditionalInfomration = log;
            }
            catch (Exception err)
            {
                if (debug)
                {
                    Console.WriteLine("An error just occured.");
                    Console.WriteLine("The error is:");
                    Console.WriteLine(err.Message);
                    Console.WriteLine();
                    Console.WriteLine(err.StackTrace);
                    Console.ReadLine();

                    
                }
                result.result = FunctionStatus.Fail;
                result.HasError = true;
                result.ErrorMessage = err.Message;
                result.ErrorException = err;
            }
            return result;
        }
        
        static void WriteHelp()
        {
            Console.WriteLine("Basic command:");
            Console.WriteLine("GetFrames Input Output");
            Console.WriteLine();
            Console.WriteLine("Additional parameter:");
            Console.WriteLine("Width=value | Set specific width of video");
            Console.WriteLine("Height=value | Set specific height of video");
            Console.WriteLine("With_Audio=value | Include audio processing or not (Value is true or false)");
            Console.WriteLine("Delete_Original=value | Delete original file or not (Value is true or false)");
            Console.WriteLine("Debug=value | Run the application in debug mode (Value is true or false)");
        }
    }
}
