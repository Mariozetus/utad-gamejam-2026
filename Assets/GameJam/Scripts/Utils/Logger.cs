using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Logger
{
    private static LoggerSettings _settings;

    private static void LoadSettings()
    {
        if(!_settings)
        {
            _settings = Resources.Load<LoggerSettings>("LoggerSettings");

            if(!_settings)
            {
                Debug.LogWarning("Logger: No se encontraron LoggerSettings en Resources. Usando configuración por defecto.");
                _settings = ScriptableObject.CreateInstance<LoggerSettings>();
            }
        }
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    [HideInCallstack]
    public static void Log(string message, LogType logType = LogType.General, UnityEngine.Object context = null, bool includeFrameInfo = false)
    {
        FormatAndLog(message, logType, context, includeFrameInfo, LogLevel.Log);
    }
    
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    [HideInCallstack]   
    public static void Warning(string message, LogType logType = LogType.General, UnityEngine.Object context = null, bool includeFrameInfo = false)
    {
        FormatAndLog(message, logType, context, includeFrameInfo, LogLevel.Warning);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    [HideInCallstack]
    public static void Error(string message, LogType logType = LogType.General, UnityEngine.Object context = null, bool includeFrameInfo = false)
    {
        FormatAndLog(message, logType, context, includeFrameInfo, LogLevel.Error);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    [HideInCallstack]
    public static void Assert(bool condition, string message, UnityEngine.Object context = null)
    {
        if (!condition)
        {
            Error($"ASSERT FAILED: {message}", LogType.System, context);
        }
    }

    private static void FormatAndLog(string message, LogType logType, UnityEngine.Object context, bool includeFrameInfo, LogLevel level)
    {
        LoadSettings();

        if ((_settings.enabledLogTypes & logType) == 0)
            return;

        string timeStamp = _settings.showTimestamp ? $"[{DateTime.Now:HH:mm:ss}] " : "";
        string frameInfo = includeFrameInfo ? $"[F:{Time.frameCount}]" : "";
        string typeName = logType.ToString();
       
        if(_settings.useColors)
        {
            string color = GetColorByLevel(level);
            message = $"<color={color}>{message}</color>";
        }

        string formattedMessage = $"{timeStamp}{frameInfo}[{typeName}] {message}";

        switch (level)
        {
            case LogLevel.Log:
                Debug.Log(formattedMessage, context);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(formattedMessage, context);
                break;
            case LogLevel.Error:
                Debug.LogError(formattedMessage, context);
                break;
        }
    }

    private static string GetColorByLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Log => "white",
            LogLevel.Warning => "yellow",
            LogLevel.Error => "red",
            _ => "white"
        };
    }

    private enum LogLevel { Log, Warning, Error }
}


