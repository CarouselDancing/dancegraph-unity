using UnityEngine;


namespace DanceGraph
{
    // A static class that converts between the server's time and the client's time
    
    public class TimeConverter
    {
        // Time offset in microseconds
        static ulong offset = 0;
        
        public static ulong GetTimeOffset() {
            return offset;
        }

        // Offset in seconds, as a float. Used for audiosource
        public static double GetDoubleOffset() {
            return ((double)offset) / 1000000.0f;
        }
        
        public static ulong LocalToServerTime(ulong ltime) {
            return ltime - offset;
        }

        public static ulong ServerToLocalTime(ulong stime) {
            return stime + offset;
        }

        public static double LocalToServerDoubleTime(double ltime) {
            return ltime - GetDoubleOffset();
        }

        public static double ServerToLocalDoubleTime(double stime) {
            return stime + GetDoubleOffset();
        }

        public static void SetTimeOffset(ulong off) {
            offset = off;
        }

    };

}
