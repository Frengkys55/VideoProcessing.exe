using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoProcessing
{

    /*
     * This class is used to store informations from a video and other functions
     * I build this class just to make this code looks cleaner (Program.cs is very messy).
     */


    #region Informations

    #region Log information
    public struct LogInformation
    {
        public string userID;
        public DateTime requestTime;
        public DateTime finishedTime;
        public string videoLocation;
        public string processedVideoLocation;
        public bool isDeleteWhenCompleteRequested;
        public bool isOriginalFileDeleted;
        public TimeSpan totalProcessingDuration;
        public TimeSpan audioProcessingDuration;
        public TimeSpan videoProcessingDuration;
        public int originalVideoWidth;
        public int originalVideoHeight;
        public float originalFrameRate;
        public bool overrideFrameRate;
        public float requestedFrameRate;
        public bool scaled;
        public int scaledVideoWidth;
        public int scaledVideoHeight;
        public bool withAudio;
        public int videoStartFrame;
        public int videoEndFrame;
        public TimeSpan videoDuration;
        public bool debugging;
        public bool hasError;
        public string errorMessage;
        public bool videoProcessed;
        public VideoProcessStatus status;
        public string FFmpegLocation;
        public string FFProbeLocation;
        public string programArguments;
    }

    [Obsolete("Backup. Use \"LogInformation\" instead", true)]
    public struct LogInformation2
    {
        public string userID;
        public DateTime requestTime;
        public string videoLocation;
        public string processedVideoLocation;
        public bool isDeleteWhenCompleteRequested;
        public bool isOriginalFileDeleted;
        public TimeSpan totalProcessingDuration;
        public TimeSpan audioProcessingDuration;
        public TimeSpan videoProcessingDuration;
        public int originalVideoWidth;
        public int originalVideoHeight;
        public float originalFrameRate;
        public bool overrideFrameRate;
        public float requestedFrameRate;
        public bool scaled;
        public int scaledVideoWidth;
        public int scaledVideoHeight;
        public bool withAudio;
        public int videoStartFrame;
        public int videoEndFrame;
        public TimeSpan videoDuration;
        public bool debugging;
        public bool hasError;
        public string errorMessage;
        public bool videoProcessed;
        public VideoProcessStatus status;
    }
    #endregion Log information

    #region Video information
    public struct ProcessedVideoInformation
    {
        public string source;
        public string target;
        public int width;
        public int height;
        public bool withAudio;
        public float fpsOverride;
        public bool deleteOriginal;
    }
    
    public struct VideoInformation
    {
        public int videoWidth;
        public int videoHeight;
        public TimeSpan videoDuration;
    }
    #endregion Video information

    #region User information
    public struct UserInformation
    {
        public int UserID;
        public string UserSessionID;
    }
    #endregion User information

    #region Processing information
    public struct ProcessingInformation
    {
        public DateTime processingStartTime;
        public DateTime processingEndTime;
        public TimeSpan totalVideoDuration;
        public TimeSpan processedVideoDuration;
        public TimeSpan processedAudioDuration;
    }

    public struct Result
    {
        public FunctionStatus result;
        public bool HasError;
        public string ErrorMessage;
        public Exception ErrorException;
        public dynamic AdditionalInfomration;
    }
    #endregion Processing information

    #endregion Informations

    #region Status
    public enum VideoProcessStatus
    {
        InProcess = 1,
        Processed = 2,
        Failed = 3,
        Canceled = 4
    }

    public enum Requester
    {
        MediaPlayer,
        Other
    }

    public enum FunctionStatus
    {
        Fail,
        Success
    }

    #endregion Staus

    #region Configurations
    public struct DatabaseConfiguration
    {
        public string databaseConnectionString;
        public string databaseName;
        public string tableName;

        public string userTableName;
    }

    public struct FFmpegConfiguration
    {
        public bool useCustomFFmpeg;
        public bool useCustomFFProbe;
        public string FFmpegLocation;
        public string FFProbeLocation;
    }

    public struct ApplicationConfiguration
    {
        public bool displayInformations;
    }
    #endregion Configurations

    #region Commands
    public enum Command
    {
        ExtractFrame,
        Other
    }
    #endregion Comamnds
}
