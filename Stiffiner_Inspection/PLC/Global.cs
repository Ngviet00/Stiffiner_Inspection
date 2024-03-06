namespace Stiffiner_Inspection
{
    public class Global
    {
        public static ControlPLC controlPLC = new ControlPLC();
        public enum eSampleStatus
        {
            OK = 1,
            NG = 2,
            EMPTY = 3
        }
    }
}
