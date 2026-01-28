using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LoggerSettings", menuName = "Custom/Utils/LoggerSettings")]
public class LoggerSettings : ScriptableObject
{
    public bool useColors = true;
    public bool showTimestamp = true;
    
    [Header("Filtros Activos")]
    public LogType enabledLogTypes = LogType.All;
}

[Flags]
public enum LogType
{
    None = 0,
    General = 1 << 0,
    Player = 1 << 1,
    Enemy = 1 << 2,
    System = 1 << 3,
    UI = 1 << 4,
    Audio = 1 << 5,
    Environment = 1 << 6,
    All = ~0
}