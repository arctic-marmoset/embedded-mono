using System.Runtime.InteropServices;

static class Native
{
    [DllImport("__Internal", EntryPoint = "set_exit_code")]
    public static extern void SetExitCode(uint code);
}
