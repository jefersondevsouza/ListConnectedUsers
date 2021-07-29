using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ConsoleApplicationListUsers
{
    class Program
    {
        [DllImport("wtsapi32.dll")]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        static extern Int32 WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
            [MarshalAs(UnmanagedType.U4)] Int32 Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
            IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType
        }

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        public static string StatusToString(WTS_CONNECTSTATE_CLASS connectedStatus)
        {
            switch (connectedStatus)
            {
                case WTS_CONNECTSTATE_CLASS.WTSActive:
                    return "Ativo";
                case WTS_CONNECTSTATE_CLASS.WTSConnected:
                    return "Conectado";
                case WTS_CONNECTSTATE_CLASS.WTSConnectQuery:
                    return "Conectado Query";
                case WTS_CONNECTSTATE_CLASS.WTSShadow:
                    return "Sombra";
                case WTS_CONNECTSTATE_CLASS.WTSDisconnected:
                    return "Desconectado";
                case WTS_CONNECTSTATE_CLASS.WTSIdle:
                    return "Idle";
                case WTS_CONNECTSTATE_CLASS.WTSListen:
                    return "Ouvindo";
                case WTS_CONNECTSTATE_CLASS.WTSReset:
                    return "Resetado";
                case WTS_CONNECTSTATE_CLASS.WTSDown:
                    return "Derrubado";
                case WTS_CONNECTSTATE_CLASS.WTSInit:
                    return "Inicializado";
                default:
                    return "Desconhecido";
            }

        }

        static void Main(string[] args)
        {
            ListUsers(Environment.MachineName);
            Console.ReadKey();
        }

        public static void ListUsers(string serverName)
        {
            IntPtr serverHandle = IntPtr.Zero;
            List<string> resultList = new List<string>();
            serverHandle = WTSOpenServer(serverName);

            try
            {
                IntPtr sessionInfoPtr = IntPtr.Zero;
                IntPtr userPtr = IntPtr.Zero;
                IntPtr domainPtr = IntPtr.Zero;
                Int32 sessionCount = 0;
                Int32 retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref sessionInfoPtr, ref sessionCount);
                Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                IntPtr currentSession = sessionInfoPtr;
                uint bytes = 0;

                if (retVal != 0)
                {
                    for (int i = 0; i < sessionCount; i++)
                    {
                        WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)currentSession, typeof(WTS_SESSION_INFO));
                        currentSession += dataSize;

                        WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out bytes);
                        WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out bytes);

                        Console.WriteLine("Domain and User: " + Marshal.PtrToStringAnsi(domainPtr) + "\\" + Marshal.PtrToStringAnsi(userPtr) + " - Status = " + StatusToString(si.State));

                        WTSFreeMemory(userPtr);
                        WTSFreeMemory(domainPtr);
                    }

                    WTSFreeMemory(sessionInfoPtr);
                }
            }
            finally
            {
                WTSCloseServer(serverHandle);
            }

        }

    }
}

