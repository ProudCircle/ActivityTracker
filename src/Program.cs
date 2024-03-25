class Program {
    private static string TOKEN;
    static void Main(string[] args) {
        TOKEN = Environment.GetEnvironmentVariable("TOKEN") ?? string.Empty;
        Console.WriteLine("Token: " + TOKEN);
    }
}