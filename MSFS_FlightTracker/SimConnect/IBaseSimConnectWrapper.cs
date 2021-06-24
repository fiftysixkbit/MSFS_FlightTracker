using System;

namespace MSFS_FlightTracker
{
    interface IBaseSimConnectWrapper
    {
        int GetUserSimConnectWinEvent();
        void ReceiveSimConnectMessage();
        void SetWindowHandle(IntPtr _hWnd);
        void Disconnect();
    }
}
