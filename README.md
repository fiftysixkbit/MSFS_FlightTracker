# MSFS Flight Tracker Tool
Generate maps and charts in real-time for your flights:

![Map Light](Assets/Screenshots/map_light.png)
![Chart Light](Assets/Screenshots/chart_light.png)

Also in dark mode:
![Chart Dark](Assets/Screenshots/chart_dark.png)

This project uses the SimConnect SDK to retrieve various flight statistics from the simulator.  A map is displayed using OpenStreetMap and WPF Map Control (https://www.codeproject.com/Articles/87944/WPF-Map-Control-using-openstreetmap-org-Data).  Charts are generated in real-time using Live Charts (https://github.com/beto-rodriguez/LiveCharts2).

# Installation Instructions
1. Install .NET 4.7.2 if needed - download here: https://dotnet.microsoft.com/download/dotnet-framework/net472
2. Download and extract the zip file from the releases section of this project: https://github.com/fiftysixkbit/MSFS_FlightTracker/releases
3. Launch MSFS_FlightTracker.exe to start the tool

# Usage
1. Launch Microsoft Flight Simulator and load a flight
2. Click on "Connect" and verify that the tool can connect to the sim
3. When ready to track, click "Start Tracking".  Tracking can be paused or reset.
