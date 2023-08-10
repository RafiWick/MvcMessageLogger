namespace MvcMessageLogger.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool LoggedIn { get; set; } = true;
        public List<User> Followers { get; set; } = new List<User>();
        public List<User> Following { get; set; } = new List<User>();
        public List<Message> Messages { get; } = new List<Message>();

        public string Encrypt(string password)
        {
            var key = new Dictionary<char, int>
            {
                {'a', 1},
                {'b', 2},
                {'c', 3},
                {'d', 4},
                {'e', 5},
                {'f', 6},
                {'g', 7},
                {'h', 8},
                {'i', 9},
                {'j', 10},
                {'k', 11},
                {'l', 12},
                {'m', 13},
                {'n', 14},
                {'o', 15},
                {'p', 16},
                {'q', 17},
                {'r', 18},
                {'s', 19},
                {'t', 20},
                {'u', 21},
                {'v', 22},
                {'w', 23},
                {'x', 24},
                {'y', 25},
                {'z', 26},
                {'1', 27},
                {'2', 28},
                {'3', 29},
                {'4', 30},
                {'5', 31},
                {'6', 32},
                {'7', 33},
                {'8', 34},
                {'9', 35},
                {'0', 36},
                {'!', 37},
                {'@', 38},
                {'#', 39},
                {'$', 40},
                {'%', 41},
                {'^', 42},
                {'&', 43},
                {'*', 44},
                {'(', 45},
                {')', 46},
                {'-', 47},
                {'_', 48},
                {'=', 49},
                {'+', 50},
                {'A', 51},
                {'B', 52},
                {'C', 53},
                {'D', 54},
                {'E', 55},
                {'F', 56},
                {'G', 57},
                {'H', 58},
                {'I', 59},
                {'J', 60},
                {'K', 61},
                {'L', 62},
                {'M', 63},
                {'N', 64},
                {'O', 65},
                {'P', 66},
                {'Q', 67},
                {'R', 68},
                {'S', 69},
                {'T', 70},
                {'U', 71},
                {'V', 72},
                {'W', 73},
                {'X', 74},
                {'Y', 75},
                {'Z', 76},
                {'`', 77},
                {'~', 78},
                {'[', 79},
                {']', 80},
                {'{', 81},
                {'}', 82},
                {'|', 83},
                {';', 84},
                {':', 85},
                {'/', 86},
                {'?', 87},
                {'.', 88},
                {'>', 89},
                {',', 90},
                {'<', 91},
            };
            int length = password.Length;
            int sum = 0;
            for(int i = 0; i < length; i++)
            {
                sum += i * (key[password[i]]);
            }
            return sum.ToString();
        }

        public bool PasswordCheck(string password)
        {
            var returnBool = false;
            var check = Encrypt(password);
            if (check == Password) returnBool = true;
            return returnBool;
        }
    }
}
