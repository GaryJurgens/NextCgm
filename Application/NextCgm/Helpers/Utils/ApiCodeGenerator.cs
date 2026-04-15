namespace NextCgm.Helpers.Utils
{
    public class ApiCodeGenerator
    {
        // nightscout uses a 8 character code that is a combination of letters and numbers, where the first 4 characters are letters and the last 4 characters are numbers. This is used for generating unique codes for the API endpoints, such as for creating new containers, etc. The code is generated using a random number generator, and is stored in the database for later use. The code is also used for mapping to the host machine, and for identifying the container in the system.

        // and in the Xdip connection string
        public static string GenerateCode()
        {
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";

            // Using Span with stackalloc for high-performance memory management
            Span<char> code = stackalloc char[8];

            for (int i = 0; i < 4; i++)
            {
                code[i] = letters[Random.Shared.Next(letters.Length)];
            }

            for (int i = 4; i < 8; i++)
            {
                code[i] = numbers[Random.Shared.Next(numbers.Length)];
            }

            return new string(code);
        }
    }
}