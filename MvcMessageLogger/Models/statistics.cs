using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;

namespace MvcMessageLogger.Models
{
    public class Statistics
    {
        private MvcMessageLoggerContext _context { get; set; }
        public Statistics(MvcMessageLoggerContext context)
        {
            _context = context;
        }
        public List<string> UsersByNumberOfMessages()
        {
            var ListOfUsers = _context.Users.Include(user => user.Messages);
            var orderedListOfUsers = ListOfUsers.OrderByDescending(user => user.Messages.Count());
            var orderedListOfNames = orderedListOfUsers.Select(user => $"{user.Name}: {user.Messages.Count()} messages").ToList();
            return orderedListOfNames;
        }
        public List<string> MostCommonWords(User? user)
        {
            var returnList = new List<string>();
            var wordList = new List<string>();

            if (user != null)
            {
                foreach (Message message in user.Messages)
                {
                    wordList.AddRange((message.Content.ToLower()).Split(" "));
                }
            }
            else
            {
                var listOfMessages = new List<Message>();
                foreach (User mUser in _context.Users.Include(user => user.Messages))
                {
                    listOfMessages.AddRange(mUser.Messages);
                }
                foreach (Message message in listOfMessages)
                {
                    wordList.AddRange((message.Content.ToLower()).Split(" "));
                }
            }
            returnList = TenMostFrequent(wordList);
            return returnList;
        }

        public List<string> TenMostFrequent(List<string> wordList)
        {
            for (int i = 0; i < wordList.Count(); i++)
            {
                while (wordList[i].Contains('.') || wordList[i].Contains(',') || wordList[i].Contains('!') || wordList[i].Contains('?') || wordList[i].Contains(':'))
                {
                    wordList[i] = wordList[i].Remove(wordList[i].Length - 1);
                }
            }
            var returnList = new List<string>();
            var countedWords = new Dictionary<string, int>();
            foreach (string word in wordList)
            {
                if (countedWords.ContainsKey(word.ToLower()))
                {
                    countedWords[word] += 1;
                }
                else
                {
                    countedWords.Add(word, 1);
                }
            }
            var orderedCountedWords = countedWords.OrderByDescending(d => d.Value);
            foreach (KeyValuePair<string, int> word in orderedCountedWords)
            {
                if (returnList.Count() >= 10) break;
                string w = "";
                w += word.Key;
                //w += "-";
                //w += word.Value.ToString();
                w += " ";
                returnList.Add(w);
            }
            return returnList;
        }
        public List<string> AllMostCommonWords()
        {
            var returnList = new List<string>();
            string line = "Top 10 most used words by all users:";
            returnList.Add(line);
            line = "";
            var wordList = MostCommonWords(null);
            int w = wordList.Count();
            for (int i = 0; i < w; i++)
            {
                line += ((i + 1).ToString() + ":");
                line += wordList[i];
                line += " ";
            }
            returnList.Add(line);
            returnList.Add("-----------------------------------------------------");
            returnList.Add("Most Common Words Used by User");
            var users = _context.Users.Include(u => u.Messages);
            foreach (User user in users)
            {
                line = $"{user.Name}:";
                wordList = MostCommonWords(user);
                w = wordList.Count();
                for (int i = 0; i < w; i++)
                {
                    line += $" ({i + 1}): {wordList[i]}";
                }
                returnList.Add(line);
            }


            return returnList;
        }

        public List<string> BusiestHour()
        {
            var returnList = new List<string>();
            var allMessages = new List<Message>();
            foreach (User user in _context.Users.Include(u => u.Messages))
            {
                allMessages.AddRange(user.Messages);
            }
            var countedHours = new Dictionary<DateTime, int>();
            foreach (Message message in allMessages)
            {
                DateTime hour = new DateTime(message.CreatedAt.Year,
                    message.CreatedAt.Month, message.CreatedAt.Day, message.CreatedAt.Hour, 0, 0);
                if (countedHours.ContainsKey(hour))
                {
                    countedHours[hour] += 1;
                }
                else
                {
                    countedHours.Add(hour, 1);
                }
            }
            var orderedCountedHours = countedHours.OrderByDescending(d => d.Value);
            foreach (KeyValuePair<DateTime, int> hour in orderedCountedHours)
            {
                string h = "";
                if (hour.Key.Hour > 12)
                {
                    h = (hour.Key.Hour - 12).ToString() + " PM";
                }
                else if (hour.Key.Hour == 12)
                {
                    h = (hour.Key.Hour).ToString() + " PM";
                }
                else if (hour.Key.Hour > 0)
                {
                    h = (hour.Key.Hour).ToString() + " AM";
                }
                else
                {
                    h = "12 AM";
                }
                returnList.Add($"the hour with the most posts is {h} on {hour.Key.Month}/{hour.Key.Day}/{hour.Key.Year}");
                break;
            }
            return returnList;
        }
    }
}

