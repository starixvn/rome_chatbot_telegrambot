using TelegramBot.Shared.Constants;

namespace TelegramBot.Modules.AutoTransfer.Helpers
{
    public static class TransferConditionHelper
    {
        private const int indexOfWeeklyCondition = 7;

        public static List<int> GetAutoTransferScheduleFromTransferOption(string transferOption, int roleId)
        {
            var allDates = new List<int> { 2, 3, 4, 5, 6, 7, 1 };

            if (roleId == (int)CustomerLevel.Super)
            {
                return allDates;
            }

            var transferrableDates = new List<int>();
            if (string.IsNullOrWhiteSpace(transferOption))
            {
                return transferrableDates;
            }

            char[] arrchar = transferOption.ToCharArray();
            if (arrchar.Length == 0)
            {
                return transferrableDates;
            }

            var isDaily = arrchar[indexOfWeeklyCondition] == '0';
            if (isDaily)
            {
                return allDates;
            }

            for (int i = 1; i < arrchar.Length; i++)
            {
                var dateIndex = i % indexOfWeeklyCondition;
                if (arrchar[dateIndex] == '1')
                {
                    transferrableDates.Add(dateIndex + 1);
                }
            }

            return transferrableDates;
        }
    }
}