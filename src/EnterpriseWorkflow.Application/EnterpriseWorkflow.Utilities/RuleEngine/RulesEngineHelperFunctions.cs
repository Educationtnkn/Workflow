using System;
using System.Collections.Generic;
using System.Linq;

namespace AppFW.Utilities.RuleEngine
{
    public static class DateUtils
    {
        public static bool OlderThanDays(object dateObj, int days)
        {
            // TryParse the dateObj to a DateTime
            if (!TryParse(dateObj, out var date))
            {
                return false;
            }

            // Check if the date is older than 'days' days
            return (DateTime.UtcNow - date).TotalDays > days;
        }

        public static bool WithinLastDays(object dateObj, int days)
        {
            // TryParse the dateObj to a DateTime
            if (!TryParse(dateObj, out var date))
            {
                return false;
            }

            // Check if the date is within the last 'days' days
            return (DateTime.UtcNow - date).TotalDays <= days;
        }

        public static bool Before(object dateObj, string compareDateStr)
        {
            // TryParse the dateObj to a DateTime
            if (!TryParse(dateObj, out var date))
            {
                return false;
            }

            // TryParse the compareDateStr to a DateTime
            if (!DateTime.TryParse(compareDateStr, out var compare))
            {
                return false;
            }

            // Compare the two dates
            return date < compare;
        }

        public static bool After(object dateObj, string compareDateStr)
        {
            // TryParse the dateObj to a DateTime
            if (!TryParse(dateObj, out var date))
            {
                return false;
            }

            // TryParse the compareDateStr to a DateTime
            if (!DateTime.TryParse(compareDateStr, out var compare))
            {
                return false;
            }

            // Compare the two dates
            return date > compare;
        }

        public static bool BetweenDates(object dateObj, string startStr, string endStr)
        {
            // TryParse the dateObj to a DateTime
            if (!TryParse(dateObj, out var date))
            {
                return false;
            }

            // TryParse the startDateStr and endDateStr to DateTime
            if (!DateTime.TryParse(startStr, out var start) || !DateTime.TryParse(endStr, out var end))
            {
                return false;
            }

            // Check if the date is between the start and end dates
            return date >= start && date <= end;
        }

        private static bool TryParse(object dateObj, out DateTime result)
        {
            // Initialize the result to default
            result = default;

            // Check if the dateObj is null or cannot be parsed to a DateTime
            return dateObj is not null && DateTime.TryParse(dateObj.ToString(), out result);
        }


    }

    public static class CollectionUtils
    {
        public static bool ContainsAny(object collection, string valList)
        {
            // If either the collection or the value list is null or empty, return false
            var (items, checks) = Parse(collection, valList);

            if (items is null || checks is null)
            {
                return false;
            }

            // Check if any item in the collection is contained in the checks list
            return items.Any(items => checks.Contains(items?.ToString() ?? string.Empty));
        }

        public static bool ContainsAll(object collection, string valList)
        {
            // If either the collection or the value list is null or empty, return false
            var (items, checks) = Parse(collection, valList);

            if (items is null || checks is null)
            {
                return false;
            }

            // Create a HashSet of the items in the collection for efficient lookups
            var itemSet = items.Select(i => i?.ToString() ?? string.Empty).ToHashSet();

            // Check if all items in the checks list are contained in the itemSet
            return checks.All(itemSet.Contains);
        }

        public static bool AnyMatch(object collection, string substring)
        {
            // If either the collection or the substring is null or empty, return false
            if (collection is null || string.IsNullOrEmpty(substring))
            {
                return false;
            }

            // Check if the collection is an IEnumerable of objects
            if (collection is not IEnumerable<object> list)
            {
                return false;
            }

            return list.Any(item => item?.ToString()?.Contains(substring, StringComparison.OrdinalIgnoreCase) == true);
        }

        public static bool AllMatch(object collection, string substring)
        {
            // If either the collection or the substring is null or empty, return false
            if (collection is null || string.IsNullOrEmpty(substring))
            {
                return false;
            }

            // Check if the collection is an IEnumerable of objects
            if (collection is not IEnumerable<object> list)
            {
                return false;
            }

            // Materialize the list to avoid multiple enumerations
            var enumerated = list.ToList();

            // Ensure that the collection is not empty and that all items contain the substring (case-insensitive)
            return enumerated.Count > 0 && enumerated.All(item => item?.ToString()?.Contains(substring, StringComparison.OrdinalIgnoreCase) == true);
        }

        private static (List<object>? items, HashSet<string>? checks) Parse(object collection, string valList)
        {
            // If either the collection or the value list is null or empty, return nulls
            if (collection is null || String.IsNullOrEmpty(valList))
            {
                return (null, null);
            }

            if (collection is not IEnumerable<object> list)
            {
                return (null, null);
            }

            // Split the value list by commas, trim whitespace, and create a HashSet for efficient lookups
            var checks = valList.Split(',').Select(v => v.Trim()).ToHashSet();
            return (list.ToList(), checks);
        }
    }
}


