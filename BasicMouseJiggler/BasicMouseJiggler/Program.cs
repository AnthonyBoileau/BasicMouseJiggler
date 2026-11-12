using System.Runtime.InteropServices;
using System.Text;

#region WindowsImports
// Native Windows actions imports (user32.dll)
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);

[DllImport("user32.dll", CharSet = CharSet.Auto)]
static extern uint MapVirtualKey(uint uCode, uint uMapType);

[DllImport("user32.dll", CharSet = CharSet.Auto)]
static extern int GetKeyNameText(int lParam, StringBuilder lpString, int nSize);

[DllImport("user32.dll", SetLastError = true)]
static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
#endregion

#region MouseEvents
// Mouse event actions
const int INPUT_MOUSE = 0;
const uint MOUSEEVENTF_MOVE = 0x0001;
#endregion

// Microsoft Virtual-Key Codes: https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
const int SELECTED_KEY = 0x75; // 0x75 = F6
const uint MAPVK_VK_TO_VSC = 0x00;

bool isActive = false;

Console.Title = "Basic mouse jiggler";
Console.WriteLine("=== Basic mouse jiggler ===");
Console.WriteLine($"Click [{GetKeyName(SELECTED_KEY)}] to ENABLE / DISABLE.");
Console.WriteLine("---------------------------------------------");

DateTime nextJiggleTime = DateTime.UtcNow; // Used to track elapsed time. Using Thread.Sleep blocked the user input

while (true)
{
    if ((GetAsyncKeyState(SELECTED_KEY) & 0x8000) != 0)
    {
        isActive = !isActive; // Switching the mouse jiggler state

        Console.ForegroundColor = isActive ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"[STATUS] Basic mouse jiggler: {(isActive ? "ENABLED" : "DISABLED")}");
        Console.ResetColor();

        // Trigger the first jiggle immediately upon enabling
        if (isActive)
            nextJiggleTime = DateTime.UtcNow;

        // Anti-spam: waits for the user to release the key to prevent looping
        while ((GetAsyncKeyState(SELECTED_KEY) & 0x8000) != 0)
            Thread.Sleep(10);
    }

    // If the mouse jiggler is active and if 30 seconds have passed since the last jiggle, the cursor will move automatically 1 pixel to the right, then 1 pixel left
    if (isActive && DateTime.UtcNow >= nextJiggleTime)
    {
        Jiggle(1, 0);
        Thread.Sleep(50);
        Jiggle(-1, 0);

        // Schedule the next jiggle 30 seconds from now
        nextJiggleTime = DateTime.UtcNow.AddSeconds(30);
    }

    // Short sleep keeps the hotkey responsive while keeping CPU usage near 0%
    Thread.Sleep(20);
}

#region Functions
string GetKeyName(uint virtualKeyCode)
{
    // Mapping the virtual key code to a scan code
    uint scanCode = MapVirtualKey(virtualKeyCode, MAPVK_VK_TO_VSC);
    if (scanCode == 0)
        return "Unknown";

    int lParam = (int)(scanCode << 16);

    // Getting the string name
    StringBuilder buffer = new StringBuilder(128);
    int result = GetKeyNameText(lParam, buffer, buffer.Capacity);

    return result > 0 ? buffer.ToString() : "Unknown";
}

static void Jiggle(int dx, int dy)
{
    INPUT[] inputs = new INPUT[1];
    inputs[0].type = INPUT_MOUSE;
    inputs[0].mi.dx = dx;
    inputs[0].mi.dy = dy;
    inputs[0].mi.dwFlags = MOUSEEVENTF_MOVE;

    SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
}
#endregion

#region Structs
[StructLayout(LayoutKind.Sequential)]
struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
struct INPUT
{
    public int type;
    public MOUSEINPUT mi;
}
#endregion