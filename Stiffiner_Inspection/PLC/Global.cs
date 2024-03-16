namespace Stiffiner_Inspection
{
    public static class Global
    {
        public static ControlPLC controlPLC = new ControlPLC();

        public enum eSampleStatus
        {
            OK = 1,
            NG = 2,
            EMPTY = 3
        }

        public static int valuePLC { get; set; } = 4;

        public static int resetPLC { get; set; } = -1;
    }
}
