namespace TWinService;
using System;
using WinService;

public static class TEnvironmentHelper
{
    private const string TestVar = "ENV_MANAGER_TEST_KEY";

    public static void Run()
    {
        Console.WriteLine("=== EnvManager Test Start ===");

        // 1. Clean up before test
        EnvironmentHelper.Remove(TestVar, EnvScope.Process);
        EnvironmentHelper.Remove(TestVar, EnvScope.User);

        // 2. Process scope test
        Console.WriteLine("\n[Process scope]");
        EnvironmentHelper.Set(TestVar, "PROCESS_VALUE", EnvScope.Process);

        Console.WriteLine("Read(Process): " +
            EnvironmentHelper.Get(TestVar, EnvScope.Process));

        Console.WriteLine("Read(User): " +
            EnvironmentHelper.Get(TestVar, EnvScope.User));

        // 3. User scope test
        Console.WriteLine("\n[User scope]");
        EnvironmentHelper.Set(TestVar, "USER_VALUE", EnvScope.User);

        Console.WriteLine("Read(User): " +
            EnvironmentHelper.Get(TestVar, EnvScope.User));

        // 4. Overwrite test
        Console.WriteLine("\n[Overwrite]");
        EnvironmentHelper.Set(TestVar, "USER_VALUE_2", EnvScope.User);

        Console.WriteLine("Read(User): " +
            EnvironmentHelper.Get(TestVar, EnvScope.User));

        // 5. Remove test
        Console.WriteLine("\n[Remove]");
        EnvironmentHelper.Remove(TestVar, EnvScope.User);

        Console.WriteLine("Read(User): " +
            (EnvironmentHelper.Get(TestVar, EnvScope.User) ?? "<null>"));

        Console.WriteLine("\n=== EnvManager Test End ===");
    }
}
