using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace travelus.Utility
{
    public static class SD
    {
        public const string DefaultTourImage = "defaulttourimage.png";
        public const string ManagerUser = "Manager";
        public const string EmployeeUser = "Employee";
        public const string HelpDeskUser = "HelpDesk";
        public const string CustomerEndUser = "Customer";

        public const string ssBookingCartCount = "ssCartCount";

        public const string StatusSubmitted = "Submitted";
        public const string StatusInProcess = "Preparing Tour";
        public const string StatusReady = "Tour has been arranged";
        public const string StatusCompleted = "Tour Attendance confirmed";
        public const string StatusCancelled = "Cancelled";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";








        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

    }
}
