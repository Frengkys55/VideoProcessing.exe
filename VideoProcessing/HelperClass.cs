using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using SQLClassPeralatan;

namespace VideoProcessing
{
    static class HelperClass
    {
        public static LogInformation GenerateLog(ProcessedVideoInformation videoInfo, VideoInformation processedVideoInformation, ProcessingInformation processingInfo, UserInformation userInfo, Requester requester)
        {
            #region Preparation
            DatabaseConfiguration configuration = LoadDatabaseConfiguration();
            LogInformation logInformation = new LogInformation();
            FFmpegConfiguration applicationConfiguration = LoadApplicationConfiguration();
            #endregion Preparation

            #region FFmpeg's log
            if (applicationConfiguration.useCustomFFmpeg)
            {
                logInformation.FFmpegLocation = applicationConfiguration.FFmpegLocation;
            }
            else
            {
                logInformation.FFmpegLocation = string.Empty;
            }
            #endregion FFmpeg's log

            #region FFProbe's log
            if (applicationConfiguration.useCustomFFProbe)
            {
                logInformation.FFProbeLocation = applicationConfiguration.FFProbeLocation;
            }
            else
            {
                logInformation.FFProbeLocation = string.Empty;
            }
            #endregion FFProbe's log

            #region Request time
            logInformation.requestTime = processingInfo.processingStartTime;
            logInformation.finishedTime = processingInfo.processingEndTime;
            #endregion Request time

            #region UserID
            string userID = string.Empty;
            if (Peralatan.PeriksaDataDatabase(userInfo.UserSessionID, "SessionID", configuration.databaseName, configuration.userTableName, configuration.databaseConnectionString))
            {
                userID = Peralatan.MintaDataDatabase(configuration.databaseName, "UserID", configuration.userTableName, "SessionID", userInfo.UserSessionID, configuration.databaseConnectionString);
            }
            logInformation.userID = userID;
            #endregion UserID

            #region Processed video location
            logInformation.processedVideoLocation = videoInfo.target;
            #endregion Processed video location

            #region Delete original file
            logInformation.isDeleteWhenCompleteRequested = videoInfo.deleteOriginal;
            #endregion Delete original file

            #region Processing duration
            logInformation.totalProcessingDuration = processingInfo.totalVideoDuration;
            logInformation.audioProcessingDuration = processingInfo.processedAudioDuration;
            logInformation.videoProcessingDuration = processingInfo.processedVideoDuration;
            #endregion Processing duration

            #region Video information
            logInformation.originalVideoWidth = processedVideoInformation.videoHeight;
            logInformation.originalVideoHeight = processedVideoInformation.videoHeight;
            logInformation.videoDuration = processedVideoInformation.videoDuration;
            logInformation.scaledVideoWidth = videoInfo.width;
            logInformation.scaledVideoHeight = videoInfo.height;
            
            #endregion Video information
            return logInformation;
        }

        #region Database worker
        public static void DatabaseWriter(LogInformation informations)
        {
            DatabaseConfiguration databaseConfiguration = LoadDatabaseConfiguration();

            throw new NotImplementedException("Function is not yet implemented!");
        }

        public static void DatabaseUpdater(LogInformation informations)
        {
            DatabaseConfiguration databaseConfiguration = LoadDatabaseConfiguration();   
            
            throw new NotImplementedException("Function is not yet implemented!");
        }

        public static void WriteLogToDatabase(LogInformation information)
        {
            DatabaseConfiguration configuration = LoadDatabaseConfiguration();
            string SQLCommand = "USE " + configuration.databaseName + ";";
            SQLCommand += "INSERT INTO " + configuration.tableName + " (";
            SQLCommand += "UserID, " +
                "RequestTime, " +
                "ProcessedVideoLocation, " +
                "IsDeleteWhenCompleteRequested, " +
                "IsOriginalFileDeleted, " +
                "TotalProcessingDuration, " +
                "AudioProcessingDuration, " +
                "VideoProcessingDuration, " +
                "OriginalVideoWidth, " +
                "OriginalVideoHeight, " +
                "OriginalFrameRate, " +
                "FPSRequest, " +
                "Scaled, " +
                "ScaledVideoWidth, " +
                "ScaledVideoHeight, " +
                "WithAudio, " +
                "VideoStartFrame, " +
                "VideoEndFrame, " +
                "VideoDuration, " +
                "VideoStatus, " +
                "ErrorCode, " +
                "ErrorMessage, " +
                "ProgramArguments, " +
                "FFmpegLocation, " +
                "FFProbeLocation, " +
                " VALUES " +
                information.userID + ", " +
                information.requestTime + ", " +
                //information.finishedTime + ", " +
                information.processedVideoLocation + ", ";
            if (information.isDeleteWhenCompleteRequested)
            {
                SQLCommand += "1, ";
            }
            else
            {
                SQLCommand += "0, ";
            }

            SQLCommand += information.isOriginalFileDeleted + ", " +
                information.originalVideoWidth + ", " +
                information.originalVideoHeight + ", " +
                information.originalFrameRate + ", " +
                information.scaled + ", " +
                information.withAudio + ", " +
                information.scaledVideoWidth + ", " +
                information.scaledVideoHeight + ", " +
                information.totalProcessingDuration.TotalSeconds + ", " +
                information.videoStartFrame + ", " +
                information.videoEndFrame + ", " +
                information.videoDuration + ", " +
                information.status + ", 0, " +
                information.errorMessage + ", " +
                information.programArguments + ", " +
                information.FFmpegLocation + ", " +
                information.FFProbeLocation + ";";

            Peralatan.TambahKeDatabase(SQLCommand, configuration.databaseConnectionString);
            throw new NotImplementedException("Function is not completed yet");
        }
        
        public static DatabaseConfiguration LoadDatabaseConfiguration()
        {
            DatabaseConfiguration configuration = new DatabaseConfiguration();
            configuration.databaseConnectionString = ConfigurationManager.AppSettings["DatabaseConnectionString"];
            configuration.databaseName = ConfigurationManager.AppSettings["DatabaseName"];
            configuration.tableName = ConfigurationManager.AppSettings["TableName"];
            configuration.userTableName = ConfigurationManager.AppSettings["UserTableName"];
            return configuration;
        }

        public static UserInformation GetUserID(string SessionID)
        {
            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            string connectionString = ConfigurationManager.AppSettings["DatabaseConnectionString"];
            UserInformation userInfo = new UserInformation();

            string receivedValue = Peralatan.MintaDataDatabase("MediaPlayerDatabase", "UserID", "SessionInfo", "SessionID", SessionID, connectionString);
            userInfo.UserID = Convert.ToInt32(receivedValue);
            userInfo.UserSessionID = SessionID;

            #region Old worker
            //MintaDataDatabase mintaDataDatabase = new MintaDataDatabase("UserID", "SessionInfo", "SessionID", SessionID, connectionString);
            //userInfo.UserID = Convert.ToInt32(mintaDataDatabase.DataDiterima);
            //userInfo.UserSessionID = SessionID;
            #endregion Old worker

            return userInfo;
        }

        public static Result WriteVideoActualEndFrame(UserInformation userInfo, ProcessedVideoInformation processedVideoInfo, string processID)
        {
            #region Preparation
            Result result = new Result();
            string sourceFileName = Path.GetFileNameWithoutExtension(processedVideoInfo.source.Replace("\"", string.Empty));
            string targetFolderName = processedVideoInfo.target.Replace("\"", string.Empty);
            #endregion Preapration

            #region Path recreation
            //if (processedVideoInfo.target.EndsWith("\\"))
            //{
            //    targetFolderName += processedVideoInfo.target.Replace("\"", string.Empty) + sourceFileName;
            //}
            //else
            //{
            //    targetFolderName += processedVideoInfo.target.Replace("\"", string.Empty) + "\\" + sourceFileName;
            //}
            #endregion Path recreation

            int totalFile = Directory.GetFiles(targetFolderName, "*.jpg").Length;

            #region Updater
            string SQLCommand = "USE MediaPlayerDatabase;";
            SQLCommand += "UPDATE ProcessedVideoInfo SET VideoActualEndFrame=" + totalFile.ToString() + " WHERE ProcessID=" + processID + ";";
            if (Peralatan.UbahDataDatabase(SQLCommand, ConfigurationManager.AppSettings["DatabaseConnectionString"]))
            {
                result.result = FunctionStatus.Success;
            }
            else
            {
                result.result = FunctionStatus.Fail;
                result.HasError = true;
                result.ErrorMessage = Peralatan.PesanKesalahan;
            }
            #endregion Updater

            return result;
        }


        #endregion Database worker

        #region Loader
        public static FFmpegConfiguration LoadApplicationConfiguration()
        {
            FFmpegConfiguration configuration = new FFmpegConfiguration();

            #region FFmpeg configuration
            if (ConfigurationManager.AppSettings["UseCustomFFmpeg"] == "true")
            {
                configuration.useCustomFFmpeg = true;
                if (ConfigurationManager.AppSettings["FFmpegLocation"] != string.Empty)
                {
                    configuration.FFmpegLocation = ConfigurationManager.AppSettings["FFmpegLocation"];
                }
                else
                {
                    configuration.useCustomFFmpeg = false;
                }
            }
            else
            {
                configuration.useCustomFFmpeg = false;
            }
            #endregion FFmpeg configuration

            #region FFProbe configuration
            if (ConfigurationManager.AppSettings["UseCustomFFProbe"] == "true")
            {
                configuration.useCustomFFProbe = true;

                if (ConfigurationManager.AppSettings["FFProbeLocation"] != string.Empty)
                {
                    configuration.FFProbeLocation = ConfigurationManager.AppSettings["FFProbeLocation"];
                }
                else
                {
                    configuration.useCustomFFProbe = false;
                }
            }
            else
            {
                configuration.useCustomFFProbe = false;
            }
            #endregion FFProbe configuration

            return configuration;
        }

        public static VideoInformation LoadVideoInfo(string videoLocation)
        {
            #region Preparation
            VideoInformation information = new VideoInformation();
            VideoProcessor.VideoProcessor processor = new VideoProcessor.VideoProcessor();
            string temporaryVideoLocation = string.Empty;
            #endregion Preparation

            if (!videoLocation.Contains("\""))
            {
                temporaryVideoLocation = "\"" + videoLocation + "\"";
            }
            else
            {
                temporaryVideoLocation = videoLocation;
            }

            //VideoProcessor.VideoInformation processedVideoInformation = processor.ReadVideoInfo(temporaryVideoLocation);
            information.videoDuration = processor.GetTotalDuration(videoLocation);
            information.videoWidth = -1;
            information.videoHeight = -1;
            return information;
        }
        #endregion Loader

        public static Result VideoStatusUpdater(UserInformation userInfo, ProcessedVideoInformation videoInfo, string processID, VideoProcessStatus status)
        {
            Result result = new Result();
            string connectionString = ConfigurationManager.AppSettings["DatabaseConnectionString"];

            byte[] videoSourceInBytes = Encoding.UTF8.GetBytes(videoInfo.source);

            string SQLCommand = "USE MediaPlayerDatabase;";
            SQLCommand += "UPDATE ProcessedVideoInfo SET VideoStatus=" + ((int)status).ToString();
            SQLCommand += " WHERE UserID=" + userInfo.UserID + " AND ProcessID= " + processID + ";";

            if (Peralatan.UbahDataDatabase(SQLCommand, connectionString))
            {
                result.result = FunctionStatus.Success;
            }
            else
            {
                result.result = FunctionStatus.Fail;
                result.ErrorMessage = Peralatan.PesanKesalahan;
            }
            return result;
        }

        public static int GetVideoActualEndFrame(UserInformation userInfo, ProcessedVideoInformation videoInfo, string processID)
        {
            /*
             * How it works
             * 
             * Count the number of images in the folder
             */

            return Directory.GetFiles(videoInfo.target, "*.jpg").Length;
        }

    }
}
