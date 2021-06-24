using System;

namespace MSFS_FlightTracker.Utils
{
    class Utils
    {
        public static Uri GetPlaneImageFileFromHeading(double heading)
        {
            string filenameDeg = "360";

            if (heading >= 2.5 && heading < 7.5)
            {
                filenameDeg = "005";
            }
            if (heading >= 7.5 && heading < 12.5)
            {
                filenameDeg = "010";
            }
            else if (heading >= 12.5 && heading < 17.5)
            {
                filenameDeg = "015";
            }
            else if (heading >= 17.5 && heading < 22.5)
            {
                filenameDeg = "020";
            }
            else if (heading >= 22.5 && heading < 27.5)
            {
                filenameDeg = "025";
            }
            else if (heading >= 27.5 && heading < 32.5)
            {
                filenameDeg = "030";
            }
            else if (heading >= 32.5 && heading < 37.5)
            {
                filenameDeg = "035";
            }
            else if (heading >= 37.5 && heading < 42.5)
            {
                filenameDeg = "040";
            }
            else if (heading >= 42.5 && heading < 47.5)
            {
                filenameDeg = "045";
            }
            else if (heading >= 47.5 && heading < 52.5)
            {
                filenameDeg = "050";
            }
            else if (heading >= 52.5 && heading < 57.5)
            {
                filenameDeg = "055";
            }
            else if (heading >= 57.5 && heading < 62.5)
            {
                filenameDeg = "060";
            }
            else if (heading >= 62.5 && heading < 67.5)
            {
                filenameDeg = "065";
            }
            else if (heading >= 67.5 && heading < 72.5)
            {
                filenameDeg = "070";
            }
            else if (heading >= 72.5 && heading < 77.5)
            {
                filenameDeg = "075";
            }
            else if (heading >= 77.5 && heading < 82.5)
            {
                filenameDeg = "080";
            }
            else if (heading >= 82.5 && heading < 87.5)
            {
                filenameDeg = "085";
            }
            else if (heading >= 87.5 && heading < 92.5)
            {
                filenameDeg = "090";
            }
            else if (heading >= 92.5 && heading < 97.5)
            {
                filenameDeg = "095";
            }
            else if (heading >= 97.5 && heading < 102.5)
            {
                filenameDeg = "100";
            }
            else if (heading >= 102.5 && heading < 107.5)
            {
                filenameDeg = "105";
            }
            else if (heading >= 107.5 && heading < 112.5)
            {
                filenameDeg = "110";
            }
            else if (heading >= 112.5 && heading < 117.5)
            {
                filenameDeg = "115";
            }
            else if (heading >= 117.5 && heading < 122.5)
            {
                filenameDeg = "120";
            }
            else if (heading >= 122.5 && heading < 127.5)
            {
                filenameDeg = "125";
            }
            else if (heading >= 127.5 && heading < 132.5)
            {
                filenameDeg = "130";
            }
            else if (heading >= 132.5 && heading < 137.5)
            {
                filenameDeg = "135";
            }
            else if (heading >= 137.5 && heading < 142.5)
            {
                filenameDeg = "140";
            }
            else if (heading >= 142.5 && heading < 147.5)
            {
                filenameDeg = "145";
            }
            else if (heading >= 147.5 && heading < 152.5)
            {
                filenameDeg = "150";
            }
            else if (heading >= 152.5 && heading < 157.5)
            {
                filenameDeg = "155";
            }
            else if (heading >= 157.5 && heading < 162.5)
            {
                filenameDeg = "160";
            }
            else if (heading >= 162.5 && heading < 167.5)
            {
                filenameDeg = "165";
            }
            else if (heading >= 167.5 && heading < 172.5)
            {
                filenameDeg = "170";
            }
            else if (heading >= 172.5 && heading < 177.5)
            {
                filenameDeg = "175";
            }
            else if (heading >= 177.5 && heading < 182.5)
            {
                filenameDeg = "180";
            }
            else if (heading >= 182.5 && heading < 187.5)
            {
                filenameDeg = "185";
            }
            else if (heading >= 187.5 && heading < 192.5)
            {
                filenameDeg = "190";
            }
            else if (heading >= 192.5 && heading < 197.5)
            {
                filenameDeg = "195";
            }
            else if (heading >= 197.5 && heading < 202.5)
            {
                filenameDeg = "200";
            }
            else if (heading >= 202.5 && heading < 207.5)
            {
                filenameDeg = "205";
            }
            else if (heading >= 207.5 && heading < 212.5)
            {
                filenameDeg = "210";
            }
            else if (heading >= 212.5 && heading < 217.5)
            {
                filenameDeg = "215";
            }
            else if (heading >= 217.5 && heading < 222.5)
            {
                filenameDeg = "220";
            }
            else if (heading >= 222.5 && heading < 227.5)
            {
                filenameDeg = "225";
            }
            else if (heading >= 227.5 && heading < 232.5)
            {
                filenameDeg = "230";
            }
            else if (heading >= 232.5 && heading < 237.5)
            {
                filenameDeg = "235";
            }
            else if (heading >= 237.5 && heading < 242.5)
            {
                filenameDeg = "240";
            }
            else if (heading >= 242.5 && heading < 247.5)
            {
                filenameDeg = "245";
            }
            else if (heading >= 247.5 && heading < 252.5)
            {
                filenameDeg = "250";
            }
            else if (heading >= 252.5 && heading < 257.5)
            {
                filenameDeg = "255";
            }
            else if (heading >= 257.5 && heading < 262.5)
            {
                filenameDeg = "260";
            }
            else if (heading >= 262.5 && heading < 267.5)
            {
                filenameDeg = "265";
            }
            else if (heading >= 267.5 && heading < 272.5)
            {
                filenameDeg = "270";
            }
            else if (heading >= 272.5 && heading < 277.5)
            {
                filenameDeg = "275";
            }
            else if (heading >= 277.5 && heading < 282.5)
            {
                filenameDeg = "280";
            }
            else if (heading >= 282.5 && heading < 287.5)
            {
                filenameDeg = "285";
            }
            else if (heading >= 287.5 && heading < 292.5)
            {
                filenameDeg = "290";
            }
            else if (heading >= 292.5 && heading < 297.5)
            {
                filenameDeg = "295";
            }
            else if (heading >= 297.5 && heading < 302.5)
            {
                filenameDeg = "300";
            }
            else if (heading >= 302.5 && heading < 307.5)
            {
                filenameDeg = "305";
            }
            else if (heading >= 307.5 && heading < 312.5)
            {
                filenameDeg = "310";
            }
            else if (heading >= 312.5 && heading < 317.5)
            {
                filenameDeg = "315";
            }
            else if (heading >= 317.5 && heading < 322.5)
            {
                filenameDeg = "320";
            }
            else if (heading >= 322.5 && heading < 327.5)
            {
                filenameDeg = "325";
            }
            else if (heading >= 327.5 && heading < 332.5)
            {
                filenameDeg = "330";
            }
            else if (heading >= 332.5 && heading < 337.5)
            {
                filenameDeg = "335";
            }
            else if (heading >= 337.5 && heading < 342.5)
            {
                filenameDeg = "340";
            }
            else if (heading >= 342.5 && heading < 347.5)
            {
                filenameDeg = "345";
            }
            else if (heading >= 347.5 && heading < 352.5)
            {
                filenameDeg = "350";
            }
            else if (heading >= 352.5 && heading < 357.5)
            {
                filenameDeg = "355";
            }
            else if (heading >= 357.5 || heading < 2.5)
            {
                filenameDeg = "360";
            }

            return new Uri("pack://application:,,,/MSFS_FlightTracker;component/Images/ic_airplane_" + filenameDeg + ".png");
        }
    }
}
